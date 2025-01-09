using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

// Example: We'll use "Ray" from UnityEngine
using Ray = UnityEngine.Ray;
using RaycastHit = Unity.Physics.RaycastHit; // DOTS Physics version

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct PlayerAimAndShootSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // 1. Early out if there's no main camera
        if (Camera.main == null) return;

        // 2. Get DOTS Physics world for raycasting
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        // 3. Create an ECB for structural changes (spawning projectiles)
        var ecb = SystemAPI
            .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        float deltaTime = SystemAPI.Time.DeltaTime;

        // 4. Query for our player data, transform, and spawner
        foreach (var (playerData, transform, spawner)
                 in SystemAPI.Query<RefRW<PlayerData>, RefRW<LocalTransform>, RefRO<PlayerProjectileSpawner>>())
        {
            // 4A. Build a UnityEngine Ray from the mouse position
            Ray unityRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Convert that to a DOTS Physics "RaycastInput"
            float3 start = unityRay.origin;
            float3 end = unityRay.origin + unityRay.direction * 1000f;

            // 4B. Define a collision filter for layer 28
            var filter = new CollisionFilter
            {
                BelongsTo = ~0u,        // The ray itself "belongs" to all layers
                CollidesWith = 1u << 28,  // We only want to collide with layer 29
                GroupIndex = 0
            };

            RaycastInput rayInput = new RaycastInput
            {
                Start = start,
                End = end,
                Filter = filter
            };

            // 4C. We'll define a default forward direction if the raycast fails
            float3 forward = math.forward(transform.ValueRO.Rotation);
            // or (0,0,1) if you prefer

            // 4D. Cast the ray in DOTS Physics
            if (physicsWorld.CastRay(rayInput, out RaycastHit rayHit))
            {
                // 4E. If we hit something on layer 29, compute aim direction
                float3 hitPoint = rayHit.Position;
                float3 aimDir = hitPoint - transform.ValueRO.Position;

                // Flatten the direction so we only rotate around Y
                aimDir.y = 0f;

                if (math.lengthsq(aimDir) > 0.0001f)
                {
                    // Rotate the player to face the aim direction
                    quaternion newRot = quaternion.LookRotationSafe(aimDir, math.up());
                    transform.ValueRW.Rotation = newRot;

                    // Update 'forward' to match aim direction
                    forward = math.normalize(aimDir);
                }
            }

            // 5. Shooting (with a cooldown)
            playerData.ValueRW.TimeToNextShoot -= deltaTime;
            if (Input.GetKey(KeyCode.Mouse0) && playerData.ValueRO.TimeToNextShoot <= 0f)
            {
                // Reset shooting timer
                playerData.ValueRW.TimeToNextShoot = playerData.ValueRO.FireRate;

                // 5A. Spawn the projectile via ECB
                Entity projectile = ecb.Instantiate(spawner.ValueRO.ProjectilePrefab);

                // 5B. Position & rotation
                float3 spawnPos = transform.ValueRO.Position + forward * 1f;
                quaternion spawnRot = quaternion.LookRotationSafe(forward, math.up());

                // 5C. Apply transform
                ecb.SetComponent(projectile, LocalTransform.FromPositionRotation(spawnPos, spawnRot));

                // 5D. Give the projectile some speed + lifetime
                ecb.AddComponent(projectile, new Speed { value = 10f });
                ecb.AddComponent(projectile, new LimitedLifeTime { TimeRemaining = 5f });

                // 5E. Optionally add a DamageOverride
                ecb.AddComponent(projectile, new DamageOverride
                {
                    Value = playerData.ValueRO.Damage
                });
            }
        }
    }
}

// A simple override to store projectile damage
public struct DamageOverride : IComponentData
{
    public float Value;
}
