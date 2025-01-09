using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct EnemySpawnSystem : ISystem
{
    private Unity.Mathematics.Random _random;
    private float _timer;

    public void OnCreate(ref SystemState state)
    {
        _random = new Unity.Mathematics.Random(0xABC123U);
    }

    public void OnUpdate(ref SystemState state)
    {
        // We'll find any entity that has EnemySpawnerData
        // (You can decide if you have exactly one such entity or multiple)
        foreach (var (spawnerData, entity)
                 in SystemAPI.Query<RefRO<EnemySpawnerData>>()
                     .WithEntityAccess())
        {
            float spawnRate = spawnerData.ValueRO.SpawnRate;
            Entity enemyPrefab = spawnerData.ValueRO.EnemyPrefab;

            // If it's still Entity.Null, we can't spawn
            if (enemyPrefab == Entity.Null) continue;

            // We do a simple "spawn X enemies per second" approach:
            float dt = SystemAPI.Time.DeltaTime;
            _timer += dt;

            // If you only want to spawn once, remove this approach
            if (_timer >= (1f / spawnRate))
            {
                _timer = 0f;

                // We'll find the player to get its position:
                // (assuming exactly 1 player)
                Entity playerEntity = Entity.Null;
                float3 playerPos = float3.zero;

                foreach (var (playerData, playerXform)
                         in SystemAPI.Query<RefRO<PlayerData>, RefRO<LocalTransform>>())
                {
                    playerEntity = entity;
                    playerPos = playerXform.ValueRO.Position;
                    break;
                }
                if (playerEntity == Entity.Null) return;

                // Create an ECB
                var ecb = SystemAPI
                    .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged);

                // Spawn e.g. 2 enemies at once
                int spawnCount = 2;
                for (int i = 0; i < spawnCount; i++)
                {
                    float radius = _random.NextFloat(10f, 30f);
                    float angle = _random.NextFloat(0f, math.PI * 2f);

                    float3 offset = new float3(
                        math.cos(angle) * radius,
                        0f,
                        math.sin(angle) * radius
                    );

                    float3 spawnPos = playerPos + offset;

                    // Instantiate from the prefab we got from spawnerData
                    Entity enemy = ecb.Instantiate(enemyPrefab);

                    // Place them at the new position
                    ecb.SetComponent(enemy, LocalTransform.FromPositionRotation(
                        spawnPos,
                        quaternion.identity
                    ));
                }
            }
        }
    }
}
