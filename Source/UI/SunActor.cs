using UnityEngine;

public class SunActor : CollectibleActor
{
    protected override float timeToCollect => GameManager.collectibleCollectTime;
    protected override float lifetime => GameManager.sunLifetime;
    protected override Vector3 destination => sunHome;

    public Sprite SunLarge, SunSmall;
    int amount;
    Vector3 sunHome = new Vector3(GameManager.sunHomeX, GameManager.sunHomeY);
    float speed;
    float? targetY;

    public void SetUpSun(bool smallSprite, int sunAmount, float fallSpeed, float targetYPos)
    {
        SetUpSun(smallSprite, sunAmount);

        targetY = targetYPos;
        speed = fallSpeed;
    }
    public void SetUpSun(bool smallSprite, int sunAmount)
    {
        spriteRenderer.sprite = smallSprite ? SunSmall : SunLarge;
        amount = sunAmount;
    }
    

    protected override void OnClicked()
    {
        GameManager.Instance.levelManager.CollectSun(amount);
    }

    private void Update()
    {
        if (targetY == null)
            return;

        if (transform.position.y <= targetY)
            return;

        transform.Translate(0,-speed*Time.deltaTime,0);
        if (transform.position.y < targetY)
            transform.position = new Vector3(transform.position.x, targetY.Value, transform.position.z);
    }
}
