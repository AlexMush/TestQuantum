using Photon.Deterministic;
using Quantum;
using UnityEngine;
using UnityEngine.Scripting;

namespace TestQuantum
{
    [Preserve]
    public unsafe class LevelSystem : SystemSignalsOnly, ISignalChangeLevel
    {
        public override void OnInit(Frame f)
        {
            var levelEntity = f.Create();
            f.Add<Level>(levelEntity);
        }

        public void ChangeLevel(Frame f, int levelsCount)
        {
            var filter = f.Filter<Level>();
            if (!filter.NextUnsafe(out var levelEntityRef, out var level))
            {
                Debug.LogError("MapSystem NextLevel :: no level component found");
                return;
            }
            
            var gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            level->CurrentLevel = FPMath.Clamp(levelsCount, 0, gameConfig.Maps.Count - 1);
            f.MapAssetRef = gameConfig.Maps[level->CurrentLevel];
        }
    }
}