using Unity.Entities;

namespace NoSpeedLimitMarkings.Systems
{
    internal readonly struct RemovedCandidate
    {
        public RemovedCandidate(Entity prefabEntity, int originalIndex)
        {
            PrefabEntity = prefabEntity;
            OriginalIndex = originalIndex;
        }

        public Entity PrefabEntity { get; }
        public int OriginalIndex { get; }
    }
}

