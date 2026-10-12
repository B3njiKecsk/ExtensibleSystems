using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ProjectileRegistry
{
    //Since ProjectileType is an Enum, this could be an array. However, we can afford it due to the amount of projectiles being only a few hundred at most and it pays off since adding a new projectile is harder to mess up, array would need them to be in the same order as in the enum
    private static readonly Dictionary<ProjectileType, ProjectileDefinition> registry = new();

    public static void RegisterProjectiles()
    {
        if (registry.Count > 0)
            throw new InvalidOperationException($"{nameof(ProjectileRegistry)} has already been initialized.");


        registry[ProjectileType.LawnMower] = new ProjectileDefinition(
            LawnMower.stats,
            GameManager.ProjectilesPrefabsPath + "LawnMower",
            (float xPos, float yPos) => new LawnMower(xPos, yPos));

        registry[ProjectileType.Pea] = new ProjectileDefinition(
            Pea.stats,
            GameManager.ProjectilesPrefabsPath + "Pea",
            (float xPos, float yPos) => new Pea(xPos, yPos));

        registry[ProjectileType.FrozenPea] = new ProjectileDefinition(
            FrozenPea.stats,
            GameManager.ProjectilesPrefabsPath + "FrozenPea",
            (float xPos, float yPos) => new FrozenPea(xPos, yPos));

        registry[ProjectileType.Puff] = new ProjectileDefinition(
            Puff.stats,
            GameManager.ProjectilesPrefabsPath + "Puff",
            (float xPos, float yPos) => new Puff(xPos, yPos));

        registry[ProjectileType.LongPuff] = new ProjectileDefinition(
            LongPuff.stats,
            GameManager.ProjectilesPrefabsPath + "Puff",//intentionally reusing Puff prefab for scaredyshroom's puff
            (float xPos, float yPos) => new LongPuff(xPos, yPos));

    }

    private static ProjectileDefinition GetDefinition(ProjectileType projectileType)
    {
        return registry[projectileType];
    }

    public static Projectile Create(ProjectileType type, float xPos, float yPos)
    {
        return GetDefinition(type).Create(xPos, yPos);
    }
    public static GameObject GetGameObject(ProjectileType type)
    {
        return GetDefinition(type).prefab;
    }
    public static ProjectileStats GetStats(ProjectileType type)
    {
        return GetDefinition(type).stats;
    }

    public static ProjectileType[] GetAllProjectileTypes()
    {
        return ((ProjectileType[])Enum.GetValues(typeof(ProjectileType))) //gives an array from the PlantType Enum
            .Where(p => p != ProjectileType.NullProjectile)               //excludes NullPlant, since its not a plant
            .ToArray();
    }
}
