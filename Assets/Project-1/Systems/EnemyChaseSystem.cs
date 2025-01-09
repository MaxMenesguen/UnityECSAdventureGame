using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct EnemyChaseSystem : ISystem
{
    private Entity _player;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _player = Entity.Null;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 1) find player (once)
        if (_player == Entity.Null)
        {
            foreach (var (pData, lt, ent)
                     in SystemAPI.Query<RefRO<PlayerData>, RefRO<LocalTransform>>()
                                 .WithEntityAccess())
            {
                _player = ent;
                break;
            }
            if (_player == Entity.Null) return;
        }

        float3 playerPos = float3.zero;
        if (SystemAPI.HasComponent<LocalTransform>(_player))
        {
            playerPos = SystemAPI.GetComponent<LocalTransform>(_player).Position;
        }

        float dt = SystemAPI.Time.DeltaTime;

        // 2) For each enemy (EnemyTag), move them horizontally toward the player
        foreach (var (enemyXform, entity)
                 in SystemAPI.Query<RefRW<LocalTransform>>()
                             .WithAll<EnemyTag>()
                             .WithEntityAccess())
        {
            float3 enemyPos = enemyXform.ValueRO.Position;

            float3 dir = playerPos - enemyPos;
            dir.y = 0f;
            float distSq = math.lengthsq(dir);
            if (distSq > 0.001f)
            {
                dir = math.normalize(dir);
                float speed = 3f; // you can read from a Speed component if you prefer
                enemyPos += dir * speed * dt;
            }

            LocalTransform newXform = enemyXform.ValueRO;
            newXform.Position = enemyPos;

            // optional: face the player
            if (distSq > 0.001f)
            {
                newXform.Rotation = quaternion.LookRotationSafe(dir, math.right());
                //turn on the z axis
                newXform.Rotation = quaternion.LookRotationSafe(dir, math.down());

            }

            enemyXform.ValueRW = newXform;
        }
    }
}
