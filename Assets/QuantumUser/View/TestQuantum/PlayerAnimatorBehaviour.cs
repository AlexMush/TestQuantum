using Quantum;
using UnityEngine;

namespace TestQuantum
{
    public class PlayerAnimatorBehaviour : QuantumEntityViewComponent
    {
        private readonly int _speedParamId = Animator.StringToHash("Speed");
        private readonly int _motionSpeedParamId  = Animator.StringToHash("MotionSpeed");
        private readonly int _groundedParamId = Animator.StringToHash("Grounded");
        private readonly int _jumpParamId = Animator.StringToHash("Jump");
        
        private Animator _animator;

        public override void OnActivate(Frame frame)
        {
            _animator = GetComponent<Animator>();
            _animator.SetFloat(_motionSpeedParamId, 1f);
        }

        public override void OnUpdateView()
        {
            var kcc = GetPredictedQuantumComponent<KCC>();
            var movement = GetPredictedQuantumComponent<Movement>();

            _animator.SetFloat(_speedParamId, kcc.RealSpeed.AsFloat);
            _animator.SetBool(_jumpParamId, movement.IsJumping);
            _animator.SetBool(_groundedParamId, kcc.IsGrounded);
        }
        
        private void OnFootstep() { }

        private void OnLand() { }
    }
}
