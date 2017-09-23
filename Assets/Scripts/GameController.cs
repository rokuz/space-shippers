using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SmartLocalization;

public class GameController : MonoBehaviour
{
    public MissonController missionController;
    public Text startTimer;
    public Text gameTimer;
    public float startTimerDuration = 3.0f;

    public delegate void OnGameTickEvent(float dt);
    public event OnGameTickEvent OnGameTick;

    public Text redCrystals;
    public Text greenCrystals;
    public Text blueCrystals;
    public Text yellowCrystals;
    public Text purpleCrystals;
    public Text cianCrystals;

    public RectTransform redCrystalsProgress;
    public RectTransform greenCrystalsProgress;
    public RectTransform blueCrystalsProgress;
    public RectTransform yellowCrystalsProgress;
    public RectTransform purpleCrystalsProgress;
    public RectTransform cianCrystalsProgress;

    public GameObject redCrystalsWaitingTank;
    public GameObject greenCrystalsWaitingTank;
    public GameObject blueCrystalsWaitingTank;
    public GameObject yellowCrystalsWaitingTank;
    public GameObject purpleCrystalsWaitingTank;
    public GameObject cianCrystalsWaitingTank;

    private bool gameStarted = false;
    private float runTimestamp;
    private string[] textOnStart;

    private float playTimestamp;
    private bool gameFinished = false;

    private CrystalStock crystalStock = new CrystalStock();

    public CrystalStock Stock
    {
        get { return crystalStock; }
    }

    public bool IsPlaying
    {
        get { return gameStarted && !gameFinished; }
    }

	public void Start()
    {
        runTimestamp = Time.time;
        textOnStart = new string[] { "", "3", "2", "1", LanguageManager.Instance.GetTextValue("StartTimerGo") };
        gameTimer.text = FormatTime(missionController.LevelDuration);

        crystalStock.OnStockAmountChanged += OnStockAmountChanged;

        Crystal[] types = new Crystal[] { Crystal.Purple, Crystal.Yellow, Crystal.Cian };
        for (int i = 0; i < types.Length; i++)
            UpdateCrystalsText(types[i], 0);
	}

	public void Update()
    {
        if (!gameStarted)
        {
            float delta = Time.time - runTimestamp;
            if (delta <= startTimerDuration)
            {
                PrepareToPlay(delta);
            }
            else
            {
                gameStarted = true;
                playTimestamp = Time.time;
                startTimer.gameObject.SetActive(false);
            }
        }
        else
        {
            float delta = Time.time - playTimestamp;
            if (delta <= missionController.LevelDuration)
            {
                if (OnGameTick != null)
                    OnGameTick(Time.deltaTime);
                
                gameTimer.text = FormatTime(missionController.LevelDuration - delta);
            }
            else
            {
                gameTimer.text = "00:00";
                gameFinished = true;
                //game over
            }
        }
	}

    public void OnCrystalsAmountChanged(Crystal crystal, uint amount)
    {
        if (gameFinished)
            return;

        missionController.ApplyCrystals(crystal, amount);
        UpdateCrystalsText(crystal, amount);
    }

    public void OnProgress(Crystal crystal, float progress)
    {
        progress = Mathf.Clamp(progress, 0.0f, 1.0f);
        var progressRectTransform = FindCrystalsProgress(crystal);
        progressRectTransform.sizeDelta = new Vector2(100.0f * progress, 10.0f);

    }

    public void OnWaiting(Crystal crystal, bool isWaiting)
    {
        var obj = FindWaitingTank(crystal);
        obj.SetActive(isWaiting);
    }

    private void OnStockAmountChanged(Crystal crystal, uint amount)
    {
        OnCrystalsAmountChanged(crystal, amount);
    }

    private void UpdateCrystalsText(Crystal crystal, uint amount)
    {
        //string goalStr = missionController.GetGoalString(crystal);
        Text text = FindCrystalsText(crystal);
        text.text = "" + amount;
    }

    private void PrepareToPlay(float dt)
    {
        float d = Mathf.Clamp(dt, 0.0f, startTimerDuration) / startTimerDuration;
        int index = (int)(d * textOnStart.Length);
        if (index < 0) index = 0;
        if (index > textOnStart.Length - 1) index = textOnStart.Length - 1;

        startTimer.text = textOnStart[index];

        float p = d * textOnStart.Length - index;
        float s = 1.0f + p;
        startTimer.transform.localScale = new Vector3(s, s, s);
        float a = 1.0f - p;
        startTimer.color = new Color(1.0f, 1.0f, 1.0f, a);
    }

    private string FormatTime(float seconds)
    {
        string m = Mathf.Floor(seconds / 60.0f).ToString("00");
        string s = ((int)(seconds) % 60).ToString("00");
        return m + ":" + s;
    }

    private Text FindCrystalsText(Crystal crystal)
    {
        switch (crystal)
        {
            case Crystal.Red:
                return redCrystals;
            case Crystal.Green:
                return greenCrystals;
            case Crystal.Blue:
                return blueCrystals;
            case Crystal.Yellow:
                return yellowCrystals;
            case Crystal.Purple:
                return purpleCrystals;
            case Crystal.Cian:
                return cianCrystals;
        }
        return null;
    }

    private RectTransform FindCrystalsProgress(Crystal crystal)
    {
        switch (crystal)
        {
            case Crystal.Red:
                return redCrystalsProgress;
            case Crystal.Green:
                return greenCrystalsProgress;
            case Crystal.Blue:
                return blueCrystalsProgress;
            case Crystal.Yellow:
                return yellowCrystalsProgress;
            case Crystal.Purple:
                return purpleCrystalsProgress;
            case Crystal.Cian:
                return cianCrystalsProgress;
        }
        return null;
    }

    private GameObject FindWaitingTank(Crystal crystal)
    {
        switch (crystal)
        {
            case Crystal.Red:
                return redCrystalsWaitingTank;
            case Crystal.Green:
                return greenCrystalsWaitingTank;
            case Crystal.Blue:
                return blueCrystalsWaitingTank;
            case Crystal.Yellow:
                return yellowCrystalsWaitingTank;
            case Crystal.Purple:
                return purpleCrystalsWaitingTank;
            case Crystal.Cian:
                return cianCrystalsWaitingTank;
        }
        return null;
    }
}
