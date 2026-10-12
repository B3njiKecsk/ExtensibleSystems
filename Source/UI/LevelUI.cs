using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using Button = UnityEngine.UI.Button;

public class LevelUI : MonoBehaviour
{
    [SerializeField] GameObject SunCounterValue;
    [SerializeField] GameObject CoinCounter;
    [SerializeField] private TMP_Text levelCount;
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private GameObject shovelSlot;
    [SerializeField] GameObject Camera;
    [SerializeField] GameObject Canvas;
    [SerializeField] GameObject LetsRockButton;
    [SerializeField] float CoincounterDisappearTime;
    [SerializeField] Vector3 CameraPosition_Lawn, CameraPosition_Street;
    [SerializeField] string currencyPrefix = "€";
    [SerializeField] private GameObject LevelRewardOverlay;
    [SerializeField] private TMP_Text LevelRewardDescription;
    [SerializeField] private TMP_Text LevelRewardName;
    [SerializeField] private Image levelRewardSprite;
    public static LevelUI Instance;
    private TMP_Text coinCounterText;
    private TMP_Text sunCounterText;
    public ProgressBar ProgressBar => progressBar;
    public Camera cam => Camera.GetComponent<Camera>();

    private void Awake()
    {
        levelCount.text = "";
        Instance = this;
        GameManager.RegisterLevelUI(Instance);

        coinCounterText = CoinCounter.GetComponentInChildren<TMP_Text>();
        sunCounterText = SunCounterValue.GetComponent<TMP_Text>();
    }
    public void StartLevel(int worldIndex, int levelIndex, int waves, int flags)
    {
        levelCount.text = $"Level {worldIndex + 1}-{levelIndex + 1}";
        progressBar.Initialize(waves, flags);
        if(GameManager.Instance.ShovelIsUnlocked)
            shovelSlot.SetActive(true);
    }
    public void UpdateSunCounter(int value)
    {
        sunCounterText.text = value.ToString();
        Hotbar.UpdateSunCount(value);
    }
    private Coroutine coinCounterCoroutine;
    public void UpdateCoinCounter(int value)
    {
        CoinCounter.SetActive(true);
        coinCounterText.text = $"{currencyPrefix}{value}";

        if (coinCounterCoroutine != null)
            StopCoroutine(coinCounterCoroutine);

        coinCounterCoroutine = StartCoroutine(HideCoinCounterAfterDelay());
    }

    private IEnumerator HideCoinCounterAfterDelay()
    {
        yield return new WaitForSeconds(CoincounterDisappearTime);

        CoinCounter.SetActive(false);
        coinCounterCoroutine = null;
    }

    public void MoveCameraToLawn()
    {
        Camera.transform.position = CameraPosition_Lawn;
    }

    public void MoveCameraToStreet()
    {
        Camera.transform.position = CameraPosition_Street;
    }
    public GameObject GetCanvas()
    {
        return Canvas;
    }

    public void LetsRockButtonHit()
    {
        ChooseYourSeeds.LetsRock();
    }

    public void SetButtonLetsRockEnabled(bool value)
    {
        LetsRockButton.GetComponent<Button>().interactable = value;
    }

    public void RemoveLetsRockButton()
    {
        LetsRockButton.SetActive(false);
    }

    public void WarnPlantChoice(string warning)
    {
        Debug.LogWarning(warning+ "\nDo you wish to continue?");
    }

    public void ShowReward(LevelReward reward)
    {
        LevelRewardOverlay.SetActive(true);
        LevelRewardDescription.text = reward.description;
        LevelRewardName.text = reward.displayName;
    }

    public static void SetRewardSprite(Sprite sprite)
    {
        Instance.levelRewardSprite.sprite = sprite;
        Instance.levelRewardSprite.SetNativeSize();
    }

    public void NextLevelButton()
    {
        GameManager.Instance.NextLevel();
    }
}
