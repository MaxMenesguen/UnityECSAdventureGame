using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct EnemyPlayerCollisionSystem : ISystem
{
    // We'll store component lookups for PlayerData, EnemyTag, Health.
    private ComponentLookup<PlayerData> _playerDataLookup;
    private ComponentLookup<EnemyTag>   _enemyTagLookup;
    private ComponentLookup<Health>     _healthLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Make sure we have a PhysicsWorld
        state.RequireForUpdate<SimulationSingleton>();

        // Create the lookups
        _playerDataLookup = state.GetComponentLookup<PlayerData>(true);
        _enemyTagLookup   = state.GetComponentLookup<EnemyTag>(true);
        _healthLookup     = state.GetComponentLookup<Health>(false);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Update them each frame so they're valid
        _playerDataLookup.Update(ref state);
        _enemyTagLookup.Update(ref state);
        _healthLookup.Update(ref state);

        // Get the Physics simulation
        var simulation = SystemAPI.GetSingleton<SimulationSingleton>();

        // Create and schedule our collision job
        var job = new EnemyPlayerCollisionJob
        {
            PlayerData   = _playerDataLookup,
            EnemyTag     = _enemyTagLookup,
            HealthLookup = _healthLookup
        };

        // Schedule the job
        state.Dependency = job.Schedule(simulation, state.Dependency);
    }

    [BurstCompile]
    private struct EnemyPlayerCollisionJob : ICollisionEventsJob
    {
        [ReadOnly] public ComponentLookup<PlayerData> PlayerData;
        [ReadOnly] public ComponentLookup<EnemyTag>   EnemyTag;
        public ComponentLookup<Health>                HealthLookup;

        public void Execute(CollisionEvent collisionEvent)
        {
            Entity a = collisionEvent.EntityA;
            Entity b = collisionEvent.EntityB;

            bool isPlayerA = PlayerData.HasComponent(a);
            bool isPlayerB = PlayerData.HasComponent(b);
            bool isEnemyA  = EnemyTag.HasComponent(a);
            bool isEnemyB  = EnemyTag.HasComponent(b);

            // If A is enemy and B is player
            if (isEnemyA && isPlayerB)
            {
                DamagePlayer(b);
            }
            // If B is enemy and A is player
            else if (isEnemyB && isPlayerA)
            {
                DamagePlayer(a);
            }
        }

        private void DamagePlayer(Entity player)
        {
            if (HealthLookup.HasComponent(player))
            {
                var hp = HealthLookup[player];
                hp.Current -= 10f; // example damage
                HealthLookup[player] = hp;
            }
        }
    }
}
