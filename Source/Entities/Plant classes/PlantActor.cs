using UnityEngine;

public class PlantActor : EntityActor
{
    [SerializeField]
    private Sprite SeedPacketSprite;

    private void Awake()
    {
        if(sprites.Count>0)
            spriteRenderer.sprite = sprites[0];
        if (SeedPacketSprite == null)
        {
            Debug.LogWarning($"{gameObject.name} doesnt have a seed packet sprite set");
            SeedPacketSprite = Resources.Load<GameObject>("Prefabs/Plants/Peashooter").GetComponent<PlantActor>().GetSeedPacketSprite();
        }
            
    }
    public Sprite GetSeedPacketSprite()
    {
        return SeedPacketSprite;
    }
    public Sprite GetMainSprite()
    {
        return spriteRenderer.sprite;
    }

    public override void UpdatePosition(float xPos, float yPos)
    {
    }
}
