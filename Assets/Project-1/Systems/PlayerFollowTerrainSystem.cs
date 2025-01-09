using System.Collections;
using System.Collections.Generic;
//using UnityEngine;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct PlayerFollowTerrainSystem : ISystem
{
    private Entity _playerEntity;

    public void OnCreate(ref SystemState state)
    {
        // Initialization logic, such as finding or caching player entity, can go here
    }

    public void OnUpdate(ref SystemState state)
    {
        // 1) Try to find the player entity if it hasn't been found yet
        if (_playerEntity == Entity.Null)
        {
            foreach (var (pData, ent) in SystemAPI.Query<RefRO<PlayerData>>().WithEntityAccess())
            {
                _playerEntity = ent;
                break;
            }
            if (_playerEntity == Entity.Null) return; // No player found, exit
        }

        // 2) Retrieve the PhysicsWorld singleton to perform raycasting
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        // 3) Read the player's current LocalTransform
        if (!SystemAPI.HasComponent<LocalTransform>(_playerEntity)) return;
        LocalTransform playerXform = SystemAPI.GetComponent<LocalTransform>(_playerEntity);

        // 4) Define the ray start and end positions
        // Start the raycast slightly above the player
        float3 rayStart = playerXform.Position + new float3(0, 10f, 0);
        // End the raycast far below the terrain
        float3 rayEnd = playerXform.Position + new float3(0, -100f, 0);

        // 5) Build the RaycastInput

        CollisionFilter pathLayerFilter = new CollisionFilter
        {
            BelongsTo = ~0u,            // Allow this ray to "belong to" all layers
            CollidesWith = 1u << 28,    // Only collide with layer 28
            GroupIndex = 0              // Default group
        };


        RaycastInput input = new RaycastInput
        {
            Start = rayStart,
            End = rayEnd,
            Filter = pathLayerFilter // Adjust this for your terrain collision layer if necessary
        };



        // 6) Perform the raycast and check if it hits something
        if (physicsWorld.CastRay(input, out RaycastHit hit))
        {

            // 7) Update the player's position to the hit position
            float3 newPos = hit.Position;

            // Optional: Add an offset to account for the player's height or pivot point
            // Adjust this based on your character's height or pivot offset
            float playerHeight = 2.0f; // Replace with your character's height if known
            newPos.y += playerHeight / 2f;

            // 8) Update the player's LocalTransform with the new position
            playerXform.Position = newPos;

            // 9) Write back the updated transform to the player entity
            SystemAPI.SetComponent(_playerEntity, playerXform);
        }




    }
}
