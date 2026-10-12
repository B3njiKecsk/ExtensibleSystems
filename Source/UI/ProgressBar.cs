using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    private int stepN, flagN, stepIndex;
    [SerializeField] private RectTransform FlagsPivot;
    [SerializeField] private GameObject FlagPrefab;
    private Slider bar;
    private List<RectTransform> Flags = new();
    private void Awake()
    {
        bar = GetComponent<Slider>();
    }

    public void Initialize(int waves, int flags)
    {
        Debug.Log($"Initialized with {waves} waves, and {flags} flags");
        stepN = waves;
        flagN = flags;

        RectTransform flagTransform = Instantiate(FlagPrefab, FlagsPivot).GetComponent<RectTransform>();
        Flags.Add(flagTransform);

        float spaceBetweenFlags = FlagsPivot.rect.width / flagN;
        for (int i = 1; i < flagN; i++)
        {
            flagTransform = Instantiate(FlagPrefab, FlagsPivot).GetComponent<RectTransform>();
            flagTransform.anchoredPosition += new Vector2(spaceBetweenFlags * i, 0);
            Flags.Add(flagTransform);
        }
        
    }

    public void Show()
    {
        
    }

    public void nextStep()
    {
        if(!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
            return;
        }

        stepIndex++;
        bar.value = (float)stepIndex / (stepN-1);

        //if we stepped on a flag, raise the flag of that index
        if ((stepIndex + 1) % (stepN / flagN) == 0)
            RaiseFlag(((stepIndex + 1) / (stepN / flagN))-1);
    }

    private void RaiseFlag(int flag)
    {
        Debug.Log($"flag: {flag}; stepIndex: {stepIndex}; flagN: {flagN}; stepN: {stepN}");
        Flags[^(flag+1)].anchoredPosition += new Vector2(0, 25);
    }
}
