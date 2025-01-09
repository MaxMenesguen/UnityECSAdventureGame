using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct PlayerInputSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (playerData, transform, spawner, entity)
                 in SystemAPI.Query<RefRW<PlayerData>, RefRW<LocalTransform>, RefRO<PlayerProjectileSpawner>>()
                     .WithEntityAccess())
        {
            // 1. Handle movement
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            float3 move = new float3(horizontal, 0, vertical) * playerData.ValueRO.MoveSpeed * deltaTime;
            transform.ValueRW.Position += move;


        }

    }
}

