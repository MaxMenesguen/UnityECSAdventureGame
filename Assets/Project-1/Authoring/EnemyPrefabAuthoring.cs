using Unity.Entities;
using UnityEngine;

// This goes on the actual enemy prefab so that it bakes a tag onto itself.
public class EnemyPrefabAuthoring : MonoBehaviour
{
    // any fields you want for the enemy
}

public class EnemyPrefabBaker : Baker<EnemyPrefabAuthoring>
{
    public override void Bake(EnemyPrefabAuthoring authoring)
    {
        // This "e" is the entity created for *this prefab* 
        // while converting the prefab in its own scope
        Entity e = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent<EnemyTag>(e);
    }
}
