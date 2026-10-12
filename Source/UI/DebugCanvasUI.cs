using System;
using TMPro;
using UnityEngine;

public class DebugCanvasUI : MonoBehaviour
{
    public TMP_Text NextWaveTimer;
    public TMP_Text ZombiesOnScreen;
    public TMP_Text CurrentWave;
    private void Awake()
    {
        GameManager.debug = this;
    }
    public void UpdateTimer(float newVal)
    {
        NextWaveTimer.text = "Time before next wave: " + newVal.ToString("F2") + "s";
    }
    public void UpdateZombiesOnScreen(int newVal)
    {
        ZombiesOnScreen.text = "Zombies on screen: " + newVal;
    }

    public void UpdateWave(int currentWave, int levelWaves)
    {
        CurrentWave.text = "Current wave: " + currentWave + "/" + levelWaves;
    }
}
