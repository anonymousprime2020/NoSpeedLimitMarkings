using Game;
using Game.Rendering;
using Unity.Entities;
using Unity.Jobs;

namespace NoSpeedLimitMarkings.Systems
{
    /// <summary>
    /// Clears the near-camera bit only in vanilla's transient pre-culling
    /// output. It changes no simulation or serialized component.
    /// </summary>
    public partial class SpeedLimitMarkingRenderSystem : GameSystemBase
    {
        private PreCullingSystem m_PreCullingSystem;
        private SpeedLimitMarkingTrackingSystem m_TrackingSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_PreCullingSystem = World.GetOrCreateSystemManaged<PreCullingSystem>();
            m_TrackingSystem =
                World.GetOrCreateSystemManaged<SpeedLimitMarkingTrackingSystem>();
        }

        protected override void OnUpdate()
        {
            var settings = Mod.Settings;
            if (settings == null ||
                !settings.HideSpeedLimitMarkings ||
                m_TrackingSystem.TargetInstanceCount == 0)
            {
                return;
            }

            var cullingData = m_PreCullingSystem.GetUpdatedData(
                readOnly: false,
                out JobHandle dependencies);
            dependencies.Complete();

            for (var index = 0; index < cullingData.Length; index++)
            {
                var item = cullingData[index];
                if (!m_TrackingSystem.IsCurrentTargetInstance(item.m_Entity))
                {
                    continue;
                }

                item.m_Flags &= ~PreCullingFlags.NearCamera;
                cullingData[index] = item;
            }

            // The write happened synchronously after all prior readers/writers were
            // completed, so downstream rendering can depend on an empty handle.
            m_PreCullingSystem.AddCullingDataWriter(default(JobHandle));
        }
    }
}

