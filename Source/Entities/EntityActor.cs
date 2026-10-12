using System;
using System.Collections.Generic;
using UnityEngine;

public enum ActorFace
{
    Right,
    Left,
    Below,
    Above,
    Center,
    PlantShooterMouth,
    PlantShooterLowMouth,
    PlantShooterMidMouth,
}
public abstract class EntityActor : MonoBehaviour
{
    public enum LaneRenderLayer
    {
        Tile = -1,
        Plant = 0,
        Zombie = 1,
    }
    [SerializeField] protected List<Sprite> sprites = new List<Sprite>();
    private static Sprite FrozenEffectSprite { get { return GameManager.Instance.FrozenEffectSprite; } }
    private SpriteRenderer _effect_overlay;
    private SpriteRenderer EffectOverlay
    {
        get
        {
            if(_effect_overlay == null)
            {
                GameObject effect = new GameObject();
                effect.transform.SetParent(gameObject.transform);
                effect.transform.localPosition = Vector3.zero;
                effect.AddComponent<SpriteRenderer>();
                _effect_overlay = effect.GetComponent<SpriteRenderer>();
            }

            return _effect_overlay;
        }
    }
    private SpriteRenderer _sprite_Renderer;
    protected SpriteRenderer spriteRenderer
    {
        get
        {
            if (_sprite_Renderer == null)
                _sprite_Renderer = GetComponent<SpriteRenderer>();
            if (_sprite_Renderer == null)
                Debug.LogError("Plant prefabs need to own a Sprite Renderer Component");
            return _sprite_Renderer;
        }
    }

    public virtual void SetLane(int lane, LaneRenderLayer layer)
    {
        spriteRenderer.sortingOrder = Enum.GetValues(typeof(LaneRenderLayer)).Length * (lane*10) + ((int)layer);
        EffectOverlay.sortingOrder = spriteRenderer.sortingOrder + 1;
    }
    public virtual void UpdatePosition(float xPos, float yPos)
    {
        transform.position = GameManager.Instance.currentWorld.TileCoordsToWorldPos(xPos, yPos);
    }

    public void UpdateVisualState(int StateIndex)
    {
        if (StateIndex >= sprites.Count || StateIndex < 0)
            Debug.LogError($"{gameObject.name}.UpdateVisualState({StateIndex}) called, with only {sprites.Count} sprites in the list. Have you added the sprites in the prefab in the correct order?");
        if (sprites[StateIndex] == null)
            Debug.LogError($"\"{gameObject.name}\" isMissing sprite at index {StateIndex} from the sprites list, was trying to be set");
        spriteRenderer.sprite = sprites[StateIndex];
    }

    public void FlipHorizontal(bool flipped)
    {
        spriteRenderer.flipX = flipped;
    }

    public void SetSlowedTint(bool value)
    {
        if (value)
            AddTint(GameManager.Instance.StatusTint_Frozen);
        else
            RemoveTint(GameManager.Instance.StatusTint_Frozen);
    }
    public void SetHypnotizedTint(bool value)
    {
        if (value)
            AddTint(GameManager.Instance.StatusTint_Hypnotized);
        else
            RemoveTint(GameManager.Instance.StatusTint_Hypnotized);
    }
    protected void AddTint(Color color)
    {
        if (color.r == 0 || color.g == 0 || color.b == 0 || color.a == 0)
            Debug.Log($"The tint {color} has one of their values set to 0, which is destructive, and thus irreversible, so it is not allowed");
        else
            spriteRenderer.color *= color;
    }

    protected void RemoveTint(Color color)
    {
        if (color.r == 0 || color.g == 0 || color.b == 0 || color.a == 0)
            Debug.Log($"The tint {color} has one of their values set to 0, which is destructive, and thus irreversible, so it is not allowed");
        else
            spriteRenderer.color *= new Color(1 / color.r, 1 / color.g, 1 / color.b, 1 / color.a);
    }

    public void SetFrozenEffect(bool value)
    {
        if (value)
            AddEffectOverlay(FrozenEffectSprite);
        else
            RemoveEffectOverlay();
    }

    protected void AddEffectOverlay(Sprite overlaySprite)
    {
        EffectOverlay.sprite = overlaySprite;
    }

    protected void RemoveEffectOverlay()
    {
        EffectOverlay.sprite = null;
    }

    #region debug

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.purple;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
    private void Update()
    {
        Debug.DrawLine(transform.position, transform.position + new Vector3((spriteRenderer.flipX ? 1 : -1) * (GameManager.Instance.currentWorld.geometry.cellWidth * GameManager.ZombieTileBackReach), 0), Color.green);
    }

    #endregion

    #region projectiles
    public static (float, float) AdjustWorldPosWithFace((float, float) Pos, ActorFace face)
    {
        switch (face)
        {
            case ActorFace.Right:
                Pos.Item1 += 0.9f;
                Pos.Item2 += 0.5f;
                break;
            case ActorFace.PlantShooterMouth:
                Pos.Item1 += 0.9f;
                Pos.Item2 += 0.2f;
                break;
            case ActorFace.Center:
                Pos.Item1 += 0.5f;
                Pos.Item2 += 0.5f;
                break;
            case ActorFace.PlantShooterLowMouth:
                Pos.Item1 += 0.9f;
                Pos.Item2 += 0.8f;
                break;
            case ActorFace.PlantShooterMidMouth:
                Pos.Item1 += 0.9f;
                Pos.Item2 += 0.5f;
                break;
        }
        return Pos;
    }
    #endregion
}
