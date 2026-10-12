using System;
using System.IO;
using UnityEngine;

public class ProjectileDefinition
{
    public readonly ProjectileStats stats;
    public readonly GameObject prefab;
    private readonly Func<float, float, Projectile> creator;

    public ProjectileDefinition(ProjectileStats projectileStats, string projectilePrefabPath, Func<float, float, Projectile> projectileCreator)
    {
        stats = projectileStats;
        prefab = Resources.Load<GameObject>(projectilePrefabPath);
        if (prefab == null)
            throw new InvalidOperationException($"Failed registering plant with given name '{projectileStats.DisplayName}': prefab at '{projectilePrefabPath}' was not found");
        ProjectileActor actor = prefab.GetComponent<ProjectileActor>();
        if (actor == null)
            throw new InvalidDataException($"Failed registering plant with given name '{projectileStats.DisplayName}': The prefab at path {projectilePrefabPath} does not contain a {actor.GetType().Name} component");
        creator = projectileCreator;
    }

    public Projectile Create(float xPos, float yPos)
    {
        return creator(xPos, yPos);
    }
}
