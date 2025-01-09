using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct EnemyFollowTerrainSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        // For all enemies that have LocalTransform and EnemyTag
        foreach (var (transform, entity)
                 in SystemAPI.Query<RefRW<LocalTransform>>()
                             .WithAll<EnemyTag>()
                             .WithEntityAccess())
        {
            float3 pos = transform.ValueRO.Position;

            float3 rayStart = pos + new float3(0, 10f, 0);
            float3 rayEnd = pos + new float3(0, -100f, 0);

            CollisionFilter terrainFilter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << 28, // layer 28 for terrain, for instance
                GroupIndex = 0
            };

            RaycastInput input = new RaycastInput
            {
                Start = rayStart,
                End = rayEnd,
                Filter = terrainFilter
            };

            if (physicsWorld.CastRay(input, out RaycastHit hit))
            {
                float3 newPos = hit.Position;
                float enemyHeight = 2f; // depends on your model pivot
                newPos.y += enemyHeight / 2f;

                LocalTransform newXform = transform.ValueRO;
                newXform.Position = newPos;
                transform.ValueRW = newXform;
            }
        }
    }
}
