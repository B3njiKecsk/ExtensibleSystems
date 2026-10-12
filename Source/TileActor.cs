using UnityEngine;

public class TileActor : EntityActor
{
    public void SetRandomSprite(System.Random random)
    {
        UpdateVisualState(random.Next() % (sprites.Count - 1));
    }
    public void Remove()
    {
        Debug.Log("Destroying gameobject");
        Destroy(gameObject);
    }
}
