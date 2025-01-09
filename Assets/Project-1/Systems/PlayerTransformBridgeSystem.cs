using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class PlayerTransformBridgeSystem : SystemBase
{
    private ECSPlayerBridge _bridge;
    private Entity _playerEntity;

    protected override void OnCreate()
    {
        // Initial attempt to find ECSPlayerBridge
        _bridge = Object.FindObjectOfType<ECSPlayerBridge>();
        if (_bridge == null)
        {
            Debug.LogWarning("ECSPlayerBridge not found during OnCreate. Will attempt to find it dynamically during runtime.");
        }
    }

    protected override void OnUpdate()
    {
        // Dynamically check for ECSPlayerBridge if it's still null
        if (_bridge == null)
        {
            _bridge = Object.FindObjectOfType<ECSPlayerBridge>();
            if (_bridge != null)
            {
                Debug.Log($"Dynamically found ECSPlayerBridge at runtime: {_bridge.name}");
            }
            else
            {
                Debug.LogError("ECSPlayerBridge is still null. Camera follow will not work.");
                return;
            }
        }

        // Retry logic: Find the player entity if it's not already set
        if (_playerEntity == Entity.Null)
        {
            Entities.WithAll<PlayerData>().ForEach((Entity entity) =>
            {
                _playerEntity = entity;
                Debug.Log($"Player entity found: {_playerEntity}");
            }).WithoutBurst().Run();

            if (_playerEntity == Entity.Null)
            {
                Debug.LogWarning("Player entity is still null. Retrying next frame...");
                return;
            }
        }

        // Fallback: Dynamically create the player entity if not found
        if (_playerEntity == Entity.Null)
        {
            Debug.LogWarning("Player entity not found. Creating a new one.");
            _playerEntity = EntityManager.CreateEntity(
                typeof(PlayerData),
                typeof(LocalTransform)
            );

            // Set default transform
            EntityManager.SetComponentData(_playerEntity, new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            });

            Debug.Log($"Player entity dynamically created: {_playerEntity}");
        }

        // Ensure the player has a LocalTransform
        if (EntityManager.HasComponent<LocalTransform>(_playerEntity))
        {
            var playerTransform = EntityManager.GetComponentData<LocalTransform>(_playerEntity);

            // Update ECSPlayerBridge position and rotation
            _bridge.transform.position = playerTransform.Position;
            _bridge.transform.rotation = playerTransform.Rotation;

            Debug.Log($"ECSPlayerBridge updated to position: {_bridge.transform.position}, rotation: {_bridge.transform.rotation}");
        }
        else
        {
            Debug.LogError("Player entity does not have a LocalTransform component.");
        }
    }
}