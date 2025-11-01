using Quantum;
using UnityEngine;
using UnityEngine.Scripting;

namespace TestQuantum
{
    [Preserve]
    public unsafe class PlayerSystem : SystemMainThreadFilter<PlayerSystem.Filter>, 
        ISignalOnPlayerAdded, ISignalOnPlayerRemoved, ISignalOnMapChanged
    {
        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* PlayerLink;
        }
        
        public override void Update(Frame f, ref Filter filter)
        {
            PullCommands(f, ref filter);
        }
        
        private void PullCommands(Frame f, ref Filter filter)
        {
            var command = f.GetPlayerCommand(filter.PlayerLink->PlayerRef);
            switch (command)
            {
                case ChangeLevelCommand nextLevelCommand:
                    nextLevelCommand.Execute(f);
                    break;
            }
        }
        
        public void OnPlayerAdded(Frame f, PlayerRef player, bool isFirstTime)
        {
            SpawnPlayer(f, player);
        }

        public void OnPlayerRemoved(Frame f, PlayerRef playerRef)
        {
            foreach (var pair in f.GetComponentIterator<PlayerLink>())
            {
                if (pair.Component.PlayerRef != playerRef) continue;

                f.Destroy(pair.Entity);
            }
        }
        
        private void SpawnPlayer(Frame f, PlayerRef playerRef)
        {
            var config = f.FindAsset(f.RuntimeConfig.GameConfig);
            var playerEntity = f.Create(config.PlayerPrototype);

            f.AddOrGet<PlayerLink>(playerEntity, out var playerLink);
            playerLink->PlayerRef = playerRef;
            
            f.Signals.ResetPlayerPosition(playerEntity, playerLink);
        }

        public void OnMapChanged(Frame f, AssetRef<Map> previousMap)
        {
            var filter = f.Filter<PlayerLink>();
            while (filter.NextUnsafe(out var playerEntity, out var playerLink))
            {
                f.Signals.ResetPlayerPosition(playerEntity, playerLink);
            }
        }
    }
}