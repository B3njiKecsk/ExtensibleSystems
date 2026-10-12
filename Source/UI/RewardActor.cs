using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum RewardType
{
    SeedPacket,
    MoneyBag,
    Shovel,
    Note1,
    Almanac,
    Note2,
    ShopKeys,
    Note3,
    Taco,
    Note4,
    WateringCan,
    Note5,
    Note6,
}
public struct LevelReward
{
    public RewardType rewardType;
    public PlantType? rewardPlant;
    public string description;
    public string displayName;
}
public class RewardActor : CollectibleActor
{
    protected override float timeToCollect => 3f;

    protected override float lifetime => -1;

    protected override Vector3 destination => Vector3.zero;
    private LevelReward reward;

    [SerializeField] List<Sprite> sprites = new();

    public void Setup(LevelReward reward)
    {
        this.reward = reward;
        if(reward.rewardType == RewardType.SeedPacket)
        {
            if (reward.rewardPlant == PlantType.NullPlant)
                throw new InvalidOperationException("The reward for this level is a seed packet, but no plant was defined");

            spriteRenderer.sprite = PlantRegistry.GetSeedSprite(reward.rewardPlant.Value);

            Debug.Log($"Before, name was {reward.displayName}");
            if(reward.displayName == null)
                this.reward.displayName = PlantRegistry.GetStats(reward.rewardPlant.Value).DisplayName;
            if(reward.description == null)
                this.reward.description = PlantRegistry.GetStats(reward.rewardPlant.Value).Description;

            Debug.Log($"Now the name became {reward.displayName} since {reward.rewardPlant.Value} has that name");
        }
        else
            spriteRenderer.sprite = getSprite(reward.rewardType);

        GetComponent<BoxCollider2D>().size = spriteRenderer.sprite.bounds.size;
        GetComponent<BoxCollider2D>().offset = spriteRenderer.sprite.bounds.center;
    }

    private Sprite getSprite(RewardType reward)
    {
        switch(reward)
        {
            case RewardType.Note1:
            case RewardType.Note2:
            case RewardType.Note3:
            case RewardType.Note4:
            case RewardType.Note5:
            case RewardType.Note6:
                return sprites[0];
            case RewardType.Shovel:
                return sprites[1];
            case RewardType.Almanac:
                return sprites[2];
            case RewardType.ShopKeys:
                return sprites[3];
            case RewardType.Taco:
                return sprites[4];
            case RewardType.WateringCan:
                return sprites[5];
            case RewardType.MoneyBag: 
                return sprites[6];
        }
        throw new InvalidOperationException($"The reward of type {reward} is not associated with a sprite. If it should, add it in the list in {nameof(getSprite)}");
    }

    protected override void OnClicked()
    {
        GameManager.Instance.levelManager.EndLevel();
        spriteRenderer.color = Color.white;
    }
    protected override void OnDeath(bool reasonIsTimeout)
    {
        Debug.Log("NOW name is " + reward.displayName);
        GameManager.Instance.levelManager.DisplayReward(reward);
        LevelUI.SetRewardSprite(spriteRenderer.sprite);
    }

    private float time = 0;
    private float duration = 1f;
    private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] Color blinkingColor;
    private void Update()
    {
        if (collecting)
            return;

        time += Time.deltaTime;
        float t = Mathf.PingPong(time / duration, 1f);

        float curvedT = curve.Evaluate(t);

        spriteRenderer.color = Color.Lerp(Color.white, blinkingColor, curvedT);
    }

    
}
