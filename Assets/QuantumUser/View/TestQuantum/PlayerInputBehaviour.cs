using System;
using Quantum;
using UnityEngine;
using Photon.Deterministic;

namespace TestQuantum
{
    public class PlayerInputBehaviour : QuantumEntityViewComponent
    {
        [SerializeField] private Transform _cameraPivot;
        [SerializeField] private Transform _cameraHandle;
        [SerializeField] private Vector2 _lookSensitivity = new Vector2(0.3f, 0.3f);
        [SerializeField] private FPVector2 _pitchClamp = new (0, 45);
        
        private DispatcherSubscription _pollInputSubscription;
        private PlayerInputActions _playerInputActions;
        private Quantum.Input _input;
        private Camera _camera;
        private bool _isLocal;

        private void Awake()
        {
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Enable();
        }

        private void OnDestroy()
        {
            _playerInputActions.Disable();
        }
        
        public override void OnActivate(Frame frame)
        {
            var playerLink = GetPredictedQuantumComponent<PlayerLink>();
            _isLocal = Game.PlayerIsLocal(playerLink.PlayerRef);
            
            if (!_isLocal) return;
            
            _pollInputSubscription = QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
            
            _camera = Camera.main;
        }

        public override void OnDeactivate()
        {
            if (!_isLocal) return;
            
            QuantumCallback.Unsubscribe(_pollInputSubscription);
        }
        
        private void PollInput(CallbackPollInput callback)
        {
            callback.SetInput(_input, DeterministicInputFlags.Repeatable);
        }
        
        public override void OnUpdateView()
        {
            if (!_isLocal) return;
            
            UpdateMovementInput();

#if UNITY_EDITOR
            UpdateLevelInput();
#endif
        }

        private void UpdateMovementInput()
        {
            var lookValue = _playerInputActions.Player.Look.ReadValue<Vector2>() * _lookSensitivity;
            var lookRotationDelta = new Vector2(-lookValue.y, lookValue.x);
            
            _input.LookRotation = ClampLookRotation(_input.LookRotation + lookRotationDelta.ToFPVector2());
            _input.MoveDirection = _playerInputActions.Player.Move.ReadValue<Vector2>().ToFPVector2();
            _input.Jump = _playerInputActions.Player.Jump.IsPressed();
        }

        private void UpdateLevelInput()
        {
            if (_playerInputActions.Player.Previous.triggered)
            {
                QuantumRunner.Default.Game.SendCommand(new ChangeLevelCommand { LevelsCount = -1 } );
            }
            else if (_playerInputActions.Player.Next.triggered)
            {
                QuantumRunner.Default.Game.SendCommand(new ChangeLevelCommand { LevelsCount = 1 } );
            }
        }
        
        public override void OnLateUpdateView()
        {
            if (!_isLocal) return;
            
            _cameraPivot.rotation = Quaternion.Euler(_input.LookRotation.ToUnityVector2());
            _camera.transform.SetPositionAndRotation(_cameraHandle.position, _cameraHandle.rotation);
        }
        
        private FPVector2 ClampLookRotation(FPVector2 lookRotation)
        {
            lookRotation.X = FPMath.Clamp(lookRotation.X, _pitchClamp.X, _pitchClamp.Y);
            return lookRotation;
        }
    }
}