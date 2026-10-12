using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class SeedPacketHandler : MonoBehaviour
{
    [SerializeField] private Sprite DefaultUnselectedSeedSprite;
    [SerializeField] private Color NoSelectedSeedColor, SelectedSeedColor;
    [SerializeField] private TMP_Text SunCostText;
    [SerializeField] private GameObject RechargeOverlay;

    public PlantType plant {  get; private set; }
    private int HotbarSlot;
    private bool IsHotbarPacket => HotbarSlot >= 0;
    private Image image;
    private bool chosen = false;
    private void Awake()
    {
        image = GetComponent<Image>();
    }
    public void Initialize(PlantType plantType, int HotbarSlot = -1)
    {
        this.HotbarSlot = HotbarSlot;
        plant = plantType;
        SetSpriteToPlant(plant);
    }

    public void OnClick()
    {
        if (GameManager.playing)
            ClickedDuringGameplay();
        else
            ClickedDuringCYS();
    }

    #region choose your seeds
    private void ClickedDuringCYS()
    {
        //clicked on empty or already chosen packet
        if (plant == PlantType.NullPlant || chosen)
            return;

        //clicked on hotbar packet
        if (IsHotbarPacket)
            ChooseYourSeeds.DeselectPlantFromSlot(HotbarSlot, plant);
        //clicked a packet from all seeds
        else
            ChooseYourSeeds.SelectPlant(plant);
    }
    private void SetSpriteToPlant(PlantType plant)
    {
        if(plant == PlantType.NullPlant)
        {
            image.sprite = DefaultUnselectedSeedSprite;
            image.color = NoSelectedSeedColor;
            SunCostText.text = "";
        }

        else
        {
            image.sprite = PlantRegistry.GetSeedSprite(plant);
            image.color = Color.white;
            SunCostText.text = PlantRegistry.GetStats(plant).SunCost.ToString();
        }

        image.SetNativeSize();
    }

    public void SetChosen(bool value)
    {
        chosen = value;
        image.color = value ? SelectedSeedColor : Color.white;
    }
    public void SetPlant(PlantType plant)
    {
        this.plant = plant;
        plantRechargeTime = plant == PlantType.NullPlant?0: PlantRegistry.GetStats(plant).RechargeTime;
        SetSpriteToPlant(plant);
    }
    #endregion

    #region gameplay
    
    private float timeToRecharge = 0;
    private bool haveEnoughSun;
    private bool isReady;
    private float plantRechargeTime;
    
    public void UpdateSunAvailable(int TotalSun)
    {
        haveEnoughSun = TotalSun >= PlantRegistry.GetStats(plant).SunCost;
        UpdateIsReady();
    }
    public void StartGame()
    {
        UpdateSunAvailable(GameManager.startingSun);
        if (PlantRegistry.GetStats(plant).RechargeTime > Plant.Recharge.Fast)
            Planted();
        UpdateIsReady();
        ApplyReadyVisual();
    }
    public void PassTime(float deltaTime)
    {
        if (timeToRecharge >= 0)
        {
            timeToRecharge -= deltaTime;
            UpdateRechargeVisual();
        }
        UpdateIsReady();
    }
    private void UpdateIsReady()
    {
        bool isReady_Condition = timeToRecharge <= 0 && haveEnoughSun;

        if (isReady == isReady_Condition)
            return;
        isReady = !isReady;
        ApplyReadyVisual();
    }
    private void ApplyReadyVisual()
    {
        image.color = isReady ? Color.white : SelectedSeedColor;
    }
    private void UpdateRechargeVisual()
    {
        //float height = image.GetComponent<RectTransform>().rect.height;
        RechargeOverlay.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1-(timeToRecharge / plantRechargeTime));
        RechargeOverlay.GetComponent<RectTransform>().offsetMin = Vector2.zero;
    }
    private void ClickedDuringGameplay()
    {
        if (isReady)
            SelectPlantFromHotbar();
        else
            PlantNotReady();
        return;
    }
    private void SelectPlantFromHotbar()
    {
        Hotbar.SelectPlant(HotbarSlot+1);
        image.color = SelectedSeedColor;
    }
    public void DeselectPlantFromHotbar()
    {
        if(isReady)
            image.color = Color.white;
    }
    private void PlantNotReady()
    {
        //temp
        Debug.Log($"plant is not ready; timeToRecharge: {timeToRecharge}; enoughSun:{haveEnoughSun}");
    }
    public void Planted()
    {
        timeToRecharge = plantRechargeTime;
        UpdateIsReady();
    }
    #endregion
}
