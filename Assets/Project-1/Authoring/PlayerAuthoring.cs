using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
public class PlayerAuthoring : MonoBehaviour
{
    public GameObject ProjectilePrefab;
    public float MoveSpeed = 5f;
    public float FireRate = 0.5f;
    public float BaseDamage = 5f;
    public float Health = 100f;
    public float HealthRegenPerSec = 1f;

    class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity playerEntity = GetEntity(TransformUsageFlags.Dynamic);

            // Basic player data
            AddComponent(playerEntity, new PlayerData
            {
                MoveSpeed = authoring.MoveSpeed,
                FireRate = authoring.FireRate,
                Damage = authoring.BaseDamage,
                TimeToNextShoot = authoring.FireRate
            });

            // Reference to projectile prefab
            AddComponent(playerEntity, new PlayerProjectileSpawner
            {
                ProjectilePrefab = GetEntity(authoring.ProjectilePrefab, TransformUsageFlags.Dynamic)
            });

            // Health so we can kill the player
            AddComponent(playerEntity, new Health
            {
                Current = 100,
                Max = 100
            });

            AddComponent(playerEntity, new HealthRegen()
            {
                PointPerSec = authoring.HealthRegenPerSec
            });
        }
    }
}

public struct PlayerData : IComponentData
{
    public float MoveSpeed;
    public float FireRate;
    public float Damage;
    public float TimeToNextShoot;
}

public struct PlayerProjectileSpawner : IComponentData
{
    public Entity ProjectilePrefab;
}

