using Photon.Deterministic;
using Quantum;

namespace TestQuantum
{
    public class ChangeLevelCommand : DeterministicCommand
    {
        public int LevelsCount;
        
        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref LevelsCount);
        }
        
        public void Execute(Frame f)
        {
            f.Signals.ChangeLevel(LevelsCount);
        }
    }
}