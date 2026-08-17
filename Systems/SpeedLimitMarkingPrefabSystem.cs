using Colossal.Serialization.Entities;
using Game;
using Game.Buildings;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Tools;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace NoSpeedLimitMarkings.Systems
{
    /// <summary>
    /// Removes only speed-limit candidates from the exact lane-markings placeholder
    /// before the vanilla SecondaryObjectSystem chooses objects to spawn.
    /// </summary>
    public partial class SpeedLimitMarkingPrefabSystem : GameSystemBase
    {
        private const string PlaceholderName = "LaneMarkings Placeholder";
        private const int RestoreBatchSize = 1024;

        private readonly List<RemovedCandidate> m_RemovedCandidates =
            new List<RemovedCandidate>();
        private readonly HashSet<Entity> m_TargetPrefabs =
            new HashSet<Entity>();

        private PrefabSystem m_PrefabSystem;
        private EntityQuery m_RestoreOwnerQuery;
        private NativeArray<Entity> m_RestoreOwners;
        private Entity m_PlaceholderEntity = Entity.Null;
        private int m_RestoreOwnerIndex;
        private bool m_NeedsResolve = true;
        private bool m_IsSuppressed;
        private bool m_NeedsFullCleanup = true;

        internal bool HasTargetPrefabs => m_TargetPrefabs.Count > 0;

        internal bool NeedsFullCleanup => m_NeedsFullCleanup;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_RestoreOwnerQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<Game.Objects.SubObject>()
                },
                Any = new[]
                {
                    ComponentType.ReadOnly<Edge>(),
                    ComponentType.ReadOnly<Node>()
                },
                None = new[]
                {
                    ComponentType.ReadOnly<Building>(),
                    ComponentType.ReadOnly<PrefabData>(),
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<Temp>()
                }
            });

            ClearRuntimeState();
        }

        protected override void OnGameLoaded(Context serializationContext)
        {
            base.OnGameLoaded(serializationContext);

            CancelOwnerRefresh();
            if (!IsPlaceholderValid())
            {
                ClearRuntimeState();
            }
            else
            {
                m_NeedsResolve = false;
                m_NeedsFullCleanup = true;
            }
        }

        protected override void OnDestroy()
        {
            RestoreForModUnload();
            CancelOwnerRefresh();
            ClearRuntimeState();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            var settings = Mod.Settings;
            if (settings == null || !EnsurePlaceholder())
            {
                return;
            }

            if (settings.HideSpeedLimitMarkings)
            {
                CancelOwnerRefresh();
                SuppressCandidates();
                return;
            }

            if (m_IsSuppressed && RestoreCandidates())
            {
                BeginOwnerRefresh();
            }

            ProcessOwnerRefresh();
        }

        public void RestoreForModUnload()
        {
            try
            {
                if (World == null || !World.IsCreated)
                {
                    return;
                }

                if (m_IsSuppressed && RestoreCandidates())
                {
                    RefreshAllOwnersImmediately();
                }
            }
            catch (Exception exception)
            {
                Mod.Log.Warn($"Failed to restore lane-marking candidates: {exception}");
            }
        }

        internal bool IsTargetPrefab(Entity prefabEntity)
        {
            return m_TargetPrefabs.Contains(prefabEntity);
        }

        internal void CompleteFullCleanup()
        {
            m_NeedsFullCleanup = false;
        }

        internal void RequestFullCleanup()
        {
            m_NeedsFullCleanup = true;
        }

        private bool EnsurePlaceholder()
        {
            if (!m_NeedsResolve && IsPlaceholderValid())
            {
                return true;
            }

            CancelOwnerRefresh();
            ClearRuntimeState();

            var id = new PrefabID(nameof(StaticObjectPrefab), PlaceholderName);
            if (!m_PrefabSystem.TryGetPrefab(id, out PrefabBase prefab) ||
                !m_PrefabSystem.TryGetEntity(prefab, out Entity placeholderEntity) ||
                !EntityManager.Exists(placeholderEntity) ||
                !EntityManager.HasBuffer<PlaceholderObjectElement>(placeholderEntity))
            {
                return false;
            }

            m_PlaceholderEntity = placeholderEntity;
            m_NeedsResolve = false;
            m_NeedsFullCleanup = true;
            Mod.Log.Info($"Resolved {PlaceholderName} to {placeholderEntity}");
            return true;
        }

        private bool IsPlaceholderValid()
        {
            return m_PlaceholderEntity != Entity.Null &&
                   EntityManager.Exists(m_PlaceholderEntity) &&
                   EntityManager.HasBuffer<PlaceholderObjectElement>(m_PlaceholderEntity);
        }

        private void SuppressCandidates()
        {
            var candidates = EntityManager.GetBuffer<PlaceholderObjectElement>(
                m_PlaceholderEntity);
            var speedLimitMask = TrafficSignData.GetTypeMask(TrafficSignType.SpeedLimit);
            var removedCount = 0;

            for (var index = candidates.Length - 1; index >= 0; index--)
            {
                var candidateEntity = candidates[index].m_Object;
                if (!EntityManager.Exists(candidateEntity) ||
                    !EntityManager.HasComponent<TrafficSignData>(candidateEntity))
                {
                    continue;
                }

                var trafficSign = EntityManager.GetComponentData<TrafficSignData>(candidateEntity);
                if ((trafficSign.m_TypeMask & speedLimitMask) == 0)
                {
                    continue;
                }

                if (m_TargetPrefabs.Add(candidateEntity))
                {
                    m_RemovedCandidates.Add(
                        new RemovedCandidate(candidateEntity, index));

                    if (Mod.Settings.DetailedLogging)
                    {
                        Mod.Log.Info(
                            $"Suppressing {m_PrefabSystem.GetPrefabName(candidateEntity)} " +
                            $"(speed {trafficSign.m_SpeedLimit}, entity {candidateEntity})");
                    }
                }

                candidates.RemoveAt(index);
                removedCount++;
            }

            m_IsSuppressed = true;
            if (removedCount > 0)
            {
                m_NeedsFullCleanup = true;
                Mod.Log.Info(
                    $"Suppressed {removedCount} speed-marking candidate(s); " +
                    $"tracking {m_TargetPrefabs.Count} prefab(s)");
            }
        }

        private bool RestoreCandidates()
        {
            if (!IsPlaceholderValid())
            {
                m_NeedsResolve = true;
                return false;
            }

            var candidates = EntityManager.GetBuffer<PlaceholderObjectElement>(
                m_PlaceholderEntity);

            m_RemovedCandidates.Sort(
                (left, right) => left.OriginalIndex.CompareTo(right.OriginalIndex));

            var restoredCount = 0;
            foreach (var removed in m_RemovedCandidates)
            {
                if (!EntityManager.Exists(removed.PrefabEntity) ||
                    ContainsCandidate(candidates, removed.PrefabEntity))
                {
                    continue;
                }

                var insertIndex = Math.Max(0, Math.Min(removed.OriginalIndex, candidates.Length));
                candidates.Insert(
                    insertIndex,
                    new PlaceholderObjectElement(removed.PrefabEntity));
                restoredCount++;
            }

            m_IsSuppressed = false;
            m_NeedsFullCleanup = false;
            Mod.Log.Info($"Restored {restoredCount} speed-marking candidate(s)");
            return true;
        }

        private static bool ContainsCandidate(
            DynamicBuffer<PlaceholderObjectElement> candidates,
            Entity candidateEntity)
        {
            for (var index = 0; index < candidates.Length; index++)
            {
                if (candidates[index].m_Object == candidateEntity)
                {
                    return true;
                }
            }

            return false;
        }

        private void BeginOwnerRefresh()
        {
            CancelOwnerRefresh();
            m_RestoreOwners = m_RestoreOwnerQuery.ToEntityArray(Allocator.Persistent);
            m_RestoreOwnerIndex = 0;
            Mod.Log.Info($"Queued {m_RestoreOwners.Length} road owner(s) for marking restoration");
        }

        private void ProcessOwnerRefresh()
        {
            if (!m_RestoreOwners.IsCreated)
            {
                return;
            }

            var end = Math.Min(m_RestoreOwnerIndex + RestoreBatchSize, m_RestoreOwners.Length);
            using (var commandBuffer = new EntityCommandBuffer(Allocator.Temp))
            {
                for (; m_RestoreOwnerIndex < end; m_RestoreOwnerIndex++)
                {
                    QueueUpdatedIfEligible(
                        commandBuffer,
                        m_RestoreOwners[m_RestoreOwnerIndex]);
                }

                commandBuffer.Playback(EntityManager);
            }

            if (m_RestoreOwnerIndex >= m_RestoreOwners.Length)
            {
                Mod.Log.Info("Finished rebuilding vanilla speed markings");
                CancelOwnerRefresh();
            }
        }

        private void RefreshAllOwnersImmediately()
        {
            using (var owners = m_RestoreOwnerQuery.ToEntityArray(Allocator.Temp))
            using (var commandBuffer = new EntityCommandBuffer(Allocator.Temp))
            {
                for (var index = 0; index < owners.Length; index++)
                {
                    QueueUpdatedIfEligible(commandBuffer, owners[index]);
                }

                commandBuffer.Playback(EntityManager);
            }
        }

        private void QueueUpdatedIfEligible(
            EntityCommandBuffer commandBuffer,
            Entity ownerEntity)
        {
            if (!EntityManager.Exists(ownerEntity) ||
                EntityManager.HasComponent<Deleted>(ownerEntity) ||
                EntityManager.HasComponent<Temp>(ownerEntity) ||
                EntityManager.HasComponent<Updated>(ownerEntity))
            {
                return;
            }

            commandBuffer.AddComponent<Updated>(ownerEntity);
        }

        private void CancelOwnerRefresh()
        {
            if (m_RestoreOwners.IsCreated)
            {
                m_RestoreOwners.Dispose();
            }

            m_RestoreOwnerIndex = 0;
        }

        private void ClearRuntimeState()
        {
            m_PlaceholderEntity = Entity.Null;
            m_RemovedCandidates.Clear();
            m_TargetPrefabs.Clear();
            m_NeedsResolve = true;
            m_IsSuppressed = false;
            m_NeedsFullCleanup = true;
        }
    }
}

