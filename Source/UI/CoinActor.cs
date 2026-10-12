using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CoinActor : CollectibleActor
{
    /*
    const float MinDistanceToTarget = 0.1f;

    [SerializeField]
    List<Sprite> sprites;
    SpriteRenderer spriteRenderer;
    CoinType type;
    [SerializeField]
    Vector3 coinCounter;
    [SerializeField]
    float CoinSpeedMultiplier;
    [SerializeField]
    float coinLifetime = 15f;
    float speed;
    bool collecting = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void SetUpCoin(CoinType coinType)
    {
        this.type = coinType;
        SetCoinSprite(coinType);
    }

    private void SetCoinSprite(CoinType coinType)
    {
        switch (coinType)
        {
            case CoinType.SilverCoin:
                spriteRenderer.sprite = sprites[0];
                break;
            case CoinType.GoldCoin:
                spriteRenderer.sprite = sprites[1];
                break;
            case CoinType.Diamond:
                spriteRenderer.sprite = sprites[2];
                break;
            default: throw new InvalidOperationException($"Tried summoning a cointype that has not been setup, mainly {coinType}");
        }
    }

    float age = 0f;
    public void Update()
    {
        if (collecting)
        {
            transform.position = Vector3.MoveTowards(transform.position, coinCounter, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, coinCounter) < MinDistanceToTarget)
            {
                collecting = false;
                KillCoinAnimation();
            }
        }
        else
            age = age + Time.deltaTime;
        if (age > coinLifetime)
            EndCoinLife();
    }

    private void CollectCoinAnimation()
    {
        collecting = true;
        speed = Vector3.Distance(transform.position, coinCounter) * CoinSpeedMultiplier;
    }
    private void KillCoinAnimation()
    {
        Destroy(gameObject);
    }

    public void OnMouseDown()
    {
        CollectCoinAnimation();
        GameManager.Instance.levelManager.CollectCoin((int)type);
    }
    private void EndCoinLife()
    {
        KillCoinAnimation();
    }
    */

    const float MinDistanceToTarget = 0.1f;

    [SerializeField]
    List<Sprite> sprites;
    CoinType type;
    [SerializeField]
    Vector3 coinCounter;

    protected override float timeToCollect => GameManager.collectibleCollectTime;
    protected override float lifetime => GameManager.coinLifetime;
    protected override Vector3 destination => coinCounter;

    public void SetUpCoin(CoinType coinType)
    {
        type = coinType;
        SetCoinSprite(coinType);
    }

    private void SetCoinSprite(CoinType coinType)
    {
        switch (coinType)
        {
            case CoinType.SilverCoin:
                spriteRenderer.sprite = sprites[0];
                break;
            case CoinType.GoldCoin:
                spriteRenderer.sprite = sprites[1];
                break;
            case CoinType.Diamond:
                spriteRenderer.sprite = sprites[2];
                break;
            default: throw new InvalidOperationException($"Tried summoning a cointype that has not been setup, mainly {coinType}");
        }
    }
    protected override void OnClicked()
    {
        GameManager.Instance.levelManager.CollectCoin((int)type);
    }
}
