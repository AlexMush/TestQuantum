using Quantum;
using UnityEngine;
using UnityEngine.Scripting;

namespace TestQuantum
{
    [Preserve]
    public unsafe class CollisionSystem : SystemSignalsOnly, ISignalOnTriggerEnter3D
    {
        public void OnTriggerEnter3D(Frame f, TriggerInfo3D info)
        {
            if (!f.Has<PlayerLink>(info.Other)) return;

            if (!f.Has<FinishTrigger>(info.Entity)) return;
            
            f.Signals.ChangeLevel(1);
        }
    }
}