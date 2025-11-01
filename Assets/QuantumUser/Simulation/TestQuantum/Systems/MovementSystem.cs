using Quantum;
using UnityEngine;
using UnityEngine.Scripting;
using Photon.Deterministic;

namespace TestQuantum
{
    [Preserve]
    public unsafe class MovementSystem : SystemMainThreadFilter<MovementSystem.Filter>, ISignalResetPlayerPosition
    {
        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* PlayerLink;
            public Transform3D* Transform;
            public Movement* Movement;
            public KCC* KCC;
        }
        
        public override void Update(Frame f, ref Filter filter)
        {
            if (!filter.KCC->IsActive)
            {
                return;
            }
            
            var kcc = filter.KCC;
            var movement = filter.Movement;
            var config = f.FindAsset(f.RuntimeConfig.GameConfig);
            var input = f.GetPlayerInput(filter.PlayerLink->PlayerRef);
            
            if (filter.Transform->Position.Y < config.MinPlayerPositionY)
            {
                ResetPlayerPosition(f, filter.Entity, filter.PlayerLink);
                return;
            }
            
            if (movement->IsJumping && kcc->IsGrounded)
            {
                movement->IsJumping = false;
            }

            var lookRotation = FPQuaternion.Euler(0, input->LookRotation.Y, 0);
            var moveDirection = lookRotation * new FPVector3(input->MoveDirection.X, 0, input->MoveDirection.Y);
            if (moveDirection != default)
            {
                var currentRotation = kcc->Data.TransformRotation;
                var targetRotation = FPQuaternion.LookRotation(moveDirection);
                var nextRotation = FPQuaternion.Lerp(currentRotation, targetRotation, movement->RotationSpeed * f.DeltaTime);

                kcc->SetLookRotation(nextRotation);
                
                UpdatePlayerCollisions(f, filter);
            }
            
            kcc->SetInputDirection(moveDirection);
            
            movement->IsJumping = input->Jump.WasPressed && kcc->IsGrounded;
            if (movement->IsJumping)
            {
                kcc->Jump(FPVector3.Up * filter.Movement->JumpForce);
            }
        }

        public void ResetPlayerPosition(Frame f, EntityRef playerEntity, PlayerLink* playerLink)
        {
            var playerRef = playerLink->PlayerRef;
            var playerActorId = f.PlayerToActorId(playerRef);
            if (playerActorId == null)
            {
                Debug.LogError("PlayerSystem SpawnPlayer :: player is not found");
                return;           
            }
            
            var config = f.FindAsset(f.RuntimeConfig.GameConfig);
            var spawnPositionId = playerActorId.Value % config.PlayerSpawnPositions.Length;
            var playerSpawnPosition = config.PlayerSpawnPositions[spawnPositionId];
            
            var transform = f.Unsafe.GetPointer<Transform3D>(playerEntity);
            transform->Position = playerSpawnPosition;
        }

        private void UpdatePlayerCollisions(Frame f, Filter filter)
        {
            var collisions = f.ResolveList(filter.KCC->Collisions);
            foreach (var collision in collisions)
            {
                var entity = collision.GetEntity();
                if (!entity.IsValid) continue;
                
                if (!f.Unsafe.TryGetPointer<PhysicsBody3D>(entity, out var body)) continue;
                
                var transform = f.Unsafe.GetPointer<Transform3D>(entity);
                var direction = (transform->Position - filter.Transform->Position).Normalized;
                
                body->AddForce(direction * filter.Movement->PushForce);
            }
        }
    }
}