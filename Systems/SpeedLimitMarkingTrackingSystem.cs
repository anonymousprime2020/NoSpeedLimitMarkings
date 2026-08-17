using Colossal.Serialization.Entities;
using Game;
using Game.Common;
using Game.Objects;
using Game.Prefabs;
using Game.Tools;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace NoSpeedLimitMarkings.Systems
{
    /// <summary>
    /// Tracks exact generated instances whose prefab was classified through the
    /// lane-markings placeholder. Entity identities remain in memory only.
    /// </summary>
    public partial class SpeedLimitMarkingTrackingSystem : GameSystemBase
    {
        private const int PruneInterval = 128;

        private readonly HashSet<Entity> m_TargetInstances =
            new HashSet<Entity>();

        private EntityQuery m_AllSecondaryObjects;
        private EntityQuery m_CreatedSecondaryObjects;
        private SpeedLimitMarkingPrefabSystem m_MarkingPrefabSystem;
        private int m_PruneCountdown;

        internal int TargetInstanceCount => m_TargetInstances.Count;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_AllSecondaryObjects = CreateSecondaryQuery(requireCreated: false);
            m_CreatedSecondaryObjects = CreateSecondaryQuery(requireCreated: true);
            m_MarkingPrefabSystem =
                World.GetOrCreateSystemManaged<SpeedLimitMarkingPrefabSystem>();
            m_PruneCountdown = PruneInterval;
        }

        protected override void OnGameLoaded(Context serializationContext)
        {
            base.OnGameLoaded(serializationContext);

            m_TargetInstances.Clear();
            m_MarkingPrefabSystem.RequestFullCleanup();
            m_PruneCountdown = PruneInterval;
        }

        protected override void OnDestroy()
        {
            RestoreForModUnload();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            var settings = Mod.Settings;
            if (settings == null)
            {
                return;
            }

            if (!settings.HideSpeedLimitMarkings)
            {
                RestoreForModUnload();
                return;
            }

            if (!m_MarkingPrefabSystem.HasTargetPrefabs)
            {
                RestoreForModUnload();
                return;
            }

            if (m_MarkingPrefabSystem.NeedsFullCleanup)
            {
                TrackMatchingInstances(m_AllSecondaryObjects);
                m_MarkingPrefabSystem.CompleteFullCleanup();
            }
            else
            {
                TrackMatchingInstances(m_CreatedSecondaryObjects);
            }

            m_PruneCountdown--;
            if (m_PruneCountdown <= 0)
            {
                PruneInvalidInstances();
                m_PruneCountdown = PruneInterval;
            }
        }

        public void RestoreForModUnload()
        {
            try
            {
                if (World == null || !World.IsCreated || m_TargetInstances.Count == 0)
                {
                    return;
                }

                using (var commandBuffer = new EntityCommandBuffer(Allocator.Temp))
                {
                    foreach (var entity in m_TargetInstances)
                    {
                        if (!EntityManager.Exists(entity) ||
                            EntityManager.HasComponent<Deleted>(entity) ||
                            EntityManager.HasComponent<BatchesUpdated>(entity))
                        {
                            continue;
                        }

                        commandBuffer.AddComponent<BatchesUpdated>(entity);
                    }

                    commandBuffer.Playback(EntityManager);
                }

                var restoredCount = m_TargetInstances.Count;
                m_TargetInstances.Clear();
                Mod.Log.Info($"Released {restoredCount} pavement speed marking(s) for rendering");
            }
            catch (Exception exception)
            {
                Mod.Log.Warn($"Failed to restore pavement speed-marking rendering: {exception}");
            }
        }

        internal bool IsCurrentTargetInstance(Entity entity)
        {
            return m_TargetInstances.Contains(entity) && IsStillTargetInstance(entity);
        }

        private bool IsStillTargetInstance(Entity entity)
        {
            if (!EntityManager.Exists(entity) ||
                EntityManager.HasComponent<Deleted>(entity) ||
                !EntityManager.HasComponent<PrefabRef>(entity))
            {
                return false;
            }

            var prefab = EntityManager.GetComponentData<PrefabRef>(entity).m_Prefab;
            return m_MarkingPrefabSystem.IsTargetPrefab(prefab);
        }

        private EntityQuery CreateSecondaryQuery(bool requireCreated)
        {
            var all = requireCreated
                ? new[]
                {
                    ComponentType.ReadOnly<Secondary>(),
                    ComponentType.ReadOnly<Owner>(),
                    ComponentType.ReadOnly<PrefabRef>(),
                    ComponentType.ReadOnly<Created>()
                }
                : new[]
                {
                    ComponentType.ReadOnly<Secondary>(),
                    ComponentType.ReadOnly<Owner>(),
                    ComponentType.ReadOnly<PrefabRef>()
                };

            return GetEntityQuery(new EntityQueryDesc
            {
                All = all,
                None = new[]
                {
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<Temp>()
                }
            });
        }

        private void TrackMatchingInstances(EntityQuery query)
        {
            var addedCount = 0;
            using (var entities = query.ToEntityArray(Allocator.Temp))
            using (var commandBuffer = new EntityCommandBuffer(Allocator.Temp))
            {
                for (var index = 0; index < entities.Length; index++)
                {
                    var entity = entities[index];
                    var prefab = EntityManager.GetComponentData<PrefabRef>(entity).m_Prefab;
                    if (!m_MarkingPrefabSystem.IsTargetPrefab(prefab) ||
                        !m_TargetInstances.Add(entity))
                    {
                        continue;
                    }

                    if (!EntityManager.HasComponent<BatchesUpdated>(entity))
                    {
                        commandBuffer.AddComponent<BatchesUpdated>(entity);
                    }

                    addedCount++;
                }

                commandBuffer.Playback(EntityManager);
            }

            if (addedCount > 0 && Mod.Settings.DetailedLogging)
            {
                Mod.Log.Info($"Tracked {addedCount} pavement speed marking(s) for culling");
            }
        }

        private void PruneInvalidInstances()
        {
            m_TargetInstances.RemoveWhere(entity => !IsStillTargetInstance(entity));
        }
    }
}

