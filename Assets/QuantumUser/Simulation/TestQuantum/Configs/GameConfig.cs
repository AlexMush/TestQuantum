using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;

namespace TestQuantum
{
    public class GameConfig : AssetObject
    {
        public List<Map> Maps = new();
        public FPVector3[] PlayerSpawnPositions;
        public AssetRef<EntityPrototype> PlayerPrototype;
        public FP MinPlayerPositionY = -15;
    }
}