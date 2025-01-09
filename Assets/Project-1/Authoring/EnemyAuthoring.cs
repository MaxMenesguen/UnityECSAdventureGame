using Unity.Entities;
using UnityEngine;

public class EnemyAuthoring : MonoBehaviour
{
    public GameObject EnemyPrefab;  // The actual prefab to spawn (Drag in Inspector)
    public float SpawnRate = 1.0f;  // How many enemies per second
    public float speed = 5.0f;      // Some speed data
}

public class EnemyAuthoringBaker : Baker<EnemyAuthoring>
{
    public override void Bake(EnemyAuthoring authoring)
    {
        Entity spawnerEntity = GetEntity(TransformUsageFlags.Dynamic);
        Entity prefabEntity = GetEntity(authoring.EnemyPrefab, TransformUsageFlags.Dynamic);

        AddComponent(spawnerEntity, new EnemySpawnerData
        {
            EnemyPrefab = prefabEntity,
            SpawnRate = authoring.SpawnRate
        });
    }
}

// The spawner data (attached to the spawner entity)
public struct EnemySpawnerData : IComponentData
{
    public Entity EnemyPrefab;
    public float SpawnRate;
}

// The tag that identifies an entity as an Enemy
public struct EnemyTag : IComponentData { }

