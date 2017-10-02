using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;
using UnityEngine.Advertisements;
using System.Collections;
using System.Collections.Generic;
using SmartLocalization;
using GoogleMobileAds;
using GoogleMobileAds.Api;

public class GameController : MonoBehaviour
{
  public MissionController missionController;
  public Text startTimer;
  public Text gameTimer;
  public RectTransform gameTimerPanel;
  public float startTimerDuration = 3.0f;

  public SceneController sceneController;

  public delegate void OnGameTickEvent(float dt);
  public event OnGameTickEvent OnGameTick;

  public GameObject ingameUI;

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

  private float runElapsedTime = 0.0f;
  private float playElapsedTime = 0.0f;

  // Mission panel.
  public GameObject missionPanel;
  private bool needShowMissionPanel = true;
  private bool needHideMissionPanel = false;
  private float missionPanelInitialPositionY = 0;
  private float missionPanelShowHideTime;
  private RectTransform missionPanelRectTransform;
  private bool missionStarted = false;

  // Menu panel.
  public GameObject menuPanel;
  private bool needShowMenuPanel = false;
  private bool needHideMenuPanel = false;
  private float menuPanelInitialPositionY = 0;
  private float menuPanelShowHideTime;
  private RectTransform menuPanelRectTransform;

  // Menu completion panel.
  public GameObject missionCompletionPanel;
  private bool needShowMissionCompletionPanel = false;
  private bool needHideMissionCompletionPanel = false;
  private float missionCompletionPanelInitialPositionY = 0;
  private float missionCompletionPanelShowHideTime;
  private RectTransform missionCompletionPanelRectTransform;
  private bool missionCompleted = false;
  private bool missionCompletionPanelMoveUp = true;
  private float missionCompletionPanelDownPos = -808;
  public AudioClip missionCompletedAudio;
  public AudioClip missionFailedAudio;

  public GameObject helpPanel;
  public Button openHelpButton;
  public Text helpText1;
  public Text helpText2;
  public Text helpTextFormulas;

  private CrystalStock crystalStock = null;

  public CrystalStock Stock
  {
    get { return crystalStock; }
  }

  public bool IsPlaying
  {
    get { return gameStarted && !gameFinished; }
  }

  public bool IsPlayingOrPreparing
  {
    get { return (gameStarted && !gameFinished) || ingameUI.activeSelf; }
  }

  public bool IsGameStarted
  {
    get { return gameStarted; }
  }

  private BannerView bannerView;

  public void Awake()
  {
    Persistence.Load();
  }

	public void Start()
  {
    SmartCultureInfo systemLanguage = LanguageManager.Instance.GetDeviceCultureIfSupported();
    if (systemLanguage != null)
      LanguageManager.Instance.ChangeLanguage(systemLanguage);
  
    ingameUI.SetActive(false);

    menuPanel.SetActive(false);
    menuPanelRectTransform = menuPanel.GetComponent<RectTransform>();
    menuPanelInitialPositionY = menuPanelRectTransform.localPosition.y;

    missionPanel.SetActive(true);
    missionPanelRectTransform = missionPanel.GetComponent<RectTransform>();
    missionPanelInitialPositionY = missionPanelRectTransform.localPosition.y;

    missionCompletionPanel.SetActive(false);
    missionCompletionPanelRectTransform = missionCompletionPanel.GetComponent<RectTransform>();
    missionCompletionPanelInitialPositionY = missionCompletionPanelRectTransform.localPosition.y;

    helpText1.text = LanguageManager.Instance.GetTextValue("Help_1");
    helpText2.text = LanguageManager.Instance.GetTextValue("Help_2");
    helpTextFormulas.text = LanguageManager.Instance.GetTextValue("Help_Formulas");

    if (Math.Abs(Camera.main.aspect - 4.0f / 3.0f) <= 0.1f)
    {
      var newPos = Camera.main.transform.position - 7.0f * Camera.main.transform.forward;
      Camera.main.transform.SetPositionAndRotation(newPos, Camera.main.transform.rotation);
    }

    RequestBanner();
	}

  private void RequestBanner()
  {
    #if UNITY_ANDROID
      string adUnitId = "ca-app-pub-8904882368983998/3748761996";
      string appId = "ca-app-pub-8904882368983998~4680190977";
      string gameId = "1557938";
    #elif UNITY_IPHONE
      string adUnitId = "ca-app-pub-8904882368983998/9863023444";
      string appId = "ca-app-pub-8904882368983998~8304198062";
      string gameId = "1557937";
    #else
      string adUnitId = "unexpected_platform";
      string appId = "unexpected_platform";
      string gameId = "unexpected_platform";
    #endif

    MobileAds.Initialize(appId);

    if (Advertisement.isSupported)
      Advertisement.Initialize(gameId, false);

    //Persistence.gameConfig.donated = true;
    //Persistence.Save();

    if (!Persistence.gameConfig.donated)
    {
      bannerView = new BannerView(adUnitId, AdSize.Banner, 0, 0);
      AdRequest request = new AdRequest.Builder().Build();
      this.bannerView.OnAdLeavingApplication += this.HandleAdLeftApplication;
      bannerView.LoadAd(request);

      var p = gameTimerPanel.localPosition;
      gameTimerPanel.localPosition = new Vector3(320.0f, p.y, p.z);
    }
  }

  public void HandleAdLeftApplication(object sender, EventArgs args)
  {
    // Pause app.
    if (IsPlayingOrPreparing && !menuPanel.activeSelf)
      OnMenuClicked();
  }

  public void DestroyBanner()
  {
    if (bannerView != null)
    {
      bannerView.Destroy();
      var p = gameTimerPanel.localPosition;
      gameTimerPanel.localPosition = new Vector3(0.0f, p.y, p.z);
    }
  }

  public void ClearGameTickSubscribers()
  {
    if (OnGameTick != null)
    {
      var delegatesList = OnGameTick.GetInvocationList();
      foreach (var d in delegatesList)
        OnGameTick -= (d as OnGameTickEvent);
    }
  }

  private void InitGameplay()
  {
    ingameUI.SetActive(true);
    runTimestamp = Time.time;
    textOnStart = new string[] { "", "3", "2", "1", LanguageManager.Instance.GetTextValue("StartTimerGo") };
    startTimer.gameObject.SetActive(true);
    startTimer.text = textOnStart[0];
    gameTimer.text = FormatTime(missionController.LevelDuration);

    crystalStock = new CrystalStock();
    crystalStock.OnStockAmountChanged = OnStockAmountChanged;

    sceneController.RestartScene();

    Crystal[] types = new Crystal[] { Crystal.Purple, Crystal.Yellow, Crystal.Cian };
    for (int i = 0; i < types.Length; i++)
      UpdateCrystalsText(types[i], 0);
  }

	public void Update()
  {
    bool uiResult = UpdateMissionPanel();
    uiResult = UpdateMenuPanel() || uiResult;
    uiResult = UpdateMissionCompletionPanel() || uiResult;
    if (uiResult)
      return;

    if (!missionStarted)
      return;
    if (this.Paused)
      return;
    if (gameFinished)
      return;
    
    if (!gameStarted)
    {
      if (!openHelpButton.gameObject.activeSelf)
        openHelpButton.gameObject.SetActive(true);

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
        if (openHelpButton.gameObject.activeSelf)
        {
          openHelpButton.gameObject.SetActive(false);
          helpPanel.gameObject.SetActive(false);
        }

        gameTimer.text = "00:00";
        if (!gameFinished)
        {
          gameFinished = true;
          missionController.missionFailed = true;

          var audio = GetComponent<AudioSource>();
          audio.volume = 0.5f;
          audio.clip = missionFailedAudio;
          audio.Play();

          crystalStock = new CrystalStock();
          crystalStock.OnStockAmountChanged = OnStockAmountChanged;
          sceneController.RestartScene();

          Analytics.CustomEvent("MissionFailed", new Dictionary<string, object>
          {
            { "mission", missionController.CurrentMission },
          });
        }
        if (!missionCompletionPanel.activeSelf && !needShowMissionCompletionPanel)
        {
          missionController.UpdateMissionCompletionPanel();
          needShowMissionCompletionPanel = true;
          needHideMissionCompletionPanel = false;
          missionCompletionPanelShowHideTime = Time.time;
        }
      }
    }
	}

  public bool Paused
  {
    get { return menuPanel.activeSelf; }
  }

  public void RestartMission()
  {
    gameStarted = false;
    gameFinished = false;
    missionCompleted = false;
    missionController.RestartMission();
    InitGameplay();
  }

  private bool UpdateMissionPanel()
  {
    if (needShowMissionPanel)
    {
      if (!missionPanel.activeSelf)
      {
        missionPanel.SetActive(true);
        missionPanelShowHideTime = Time.time;
      }

      var y = Mathf.Lerp(missionPanelInitialPositionY, 0.0f, Mathf.Clamp((Time.time - missionPanelShowHideTime) / 0.5f, 0.0f, 1.0f));
      missionPanelRectTransform.localPosition = new Vector3(missionPanelRectTransform.localPosition.x, y,
        missionPanelRectTransform.localPosition.z);

      if (y == 0.0)
        needShowMissionPanel = false;

      return true;
    }
    else if (needHideMissionPanel)
    {
      var y = Mathf.Lerp(0.0f, missionPanelInitialPositionY, Mathf.Clamp((Time.time - missionPanelShowHideTime) / 0.3f, 0.0f, 1.0f));
      missionPanelRectTransform.localPosition = new Vector3(missionPanelRectTransform.localPosition.x, y,
        missionPanelRectTransform.localPosition.z);

      if (y == missionPanelInitialPositionY)
      {
        missionPanel.SetActive(false);
        needHideMissionPanel = false;
      }
      return true;
    }
    return false;
  }

  private bool UpdateMissionCompletionPanel()
  {
    if (needShowMissionCompletionPanel)
    {
      if (!missionCompletionPanel.activeSelf)
      {
        missionCompletionPanel.SetActive(true);
        missionCompletionPanelShowHideTime = Time.time;
      }

      var y = Mathf.Lerp(missionCompletionPanelInitialPositionY, 0.0f, Mathf.Clamp((Time.time - missionCompletionPanelShowHideTime) / 0.5f, 0.0f, 1.0f));
      missionCompletionPanelRectTransform.localPosition = new Vector3(missionCompletionPanelRectTransform.localPosition.x, y,
                                                                      missionCompletionPanelRectTransform.localPosition.z);

      if (y == 0.0)
        needShowMissionCompletionPanel = false;

      return true;
    }
    else if (needHideMissionCompletionPanel)
    {
      var up = missionCompletionPanelMoveUp ? missionCompletionPanelInitialPositionY : missionCompletionPanelDownPos;
      var y = Mathf.Lerp(0.0f, up, Mathf.Clamp((Time.time - missionCompletionPanelShowHideTime) / 0.3f, 0.0f, 1.0f));
      missionCompletionPanelRectTransform.localPosition = new Vector3(missionCompletionPanelRectTransform.localPosition.x, y,
                                                                      missionCompletionPanelRectTransform.localPosition.z);

      if (y == missionCompletionPanelInitialPositionY || y == missionCompletionPanelDownPos)
      {
        missionCompletionPanelRectTransform.localPosition = new Vector3(missionCompletionPanelRectTransform.localPosition.x,
                                                                        missionCompletionPanelInitialPositionY,
                                                                        missionCompletionPanelRectTransform.localPosition.z);
        missionCompletionPanel.SetActive(false);
        needHideMissionCompletionPanel = false;
      }
      return true;
    }
    return false;
  }

  private bool UpdateMenuPanel()
  {
    if (needShowMenuPanel)
    {
      if (!menuPanel.activeSelf)
      {
        menuPanel.SetActive(true);
        menuPanelShowHideTime = Time.time;
        if (!gameStarted)
          runElapsedTime = Time.time - runTimestamp;
        else
          playElapsedTime = Time.time - playTimestamp;
      }

      var y = Mathf.Lerp(menuPanelInitialPositionY, 0.0f, Mathf.Clamp((Time.time - menuPanelShowHideTime) / 0.5f, 0.0f, 1.0f));
      menuPanelRectTransform.localPosition = new Vector3(menuPanelRectTransform.localPosition.x, y,
        menuPanelRectTransform.localPosition.z);

      if (y == 0.0)
        needShowMenuPanel = false;

      return true;
    }
    else if (needHideMenuPanel)
    {
      var y = Mathf.Lerp(0.0f, menuPanelInitialPositionY, Mathf.Clamp((Time.time - menuPanelShowHideTime) / 0.3f, 0.0f, 1.0f));
      menuPanelRectTransform.localPosition = new Vector3(menuPanelRectTransform.localPosition.x, y,
        menuPanelRectTransform.localPosition.z);

      if (y == menuPanelInitialPositionY)
      {
        menuPanel.SetActive(false);
        needHideMenuPanel = false;
        if (!gameStarted)
          runTimestamp = Time.time - runElapsedTime;
        else
          playTimestamp = Time.time - playElapsedTime;
      }
      return true;
    }
    return false;
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

  public void OnStartMission()
  {
    needHideMissionPanel = true;
    needShowMissionPanel = false;
    missionStarted = true;
    missionPanelShowHideTime = Time.time;

    gameStarted = false;
    gameFinished = false;
    missionCompleted = false;
    missionController.RestartMission();
    InitGameplay();

    if (missionController.CurrentMission == 1)
    {
      OnMenuClicked();
      helpPanel.gameObject.SetActive(true);
    }
  }

  public void OnMissionCompleted()
  {
    if (missionCompleted)
      return;

    missionCompleted = true;

    if (openHelpButton.gameObject.activeSelf)
    {
      openHelpButton.gameObject.SetActive(false);
      helpPanel.gameObject.SetActive(false);
    }

    Analytics.CustomEvent("MissionCompleted", new Dictionary<string, object>
    {
      { "mission", missionController.CurrentMission },
      { "timeInSeconds", (int)(Time.time - playTimestamp) }
    });

    var audio = GetComponent<AudioSource>();
    audio.volume = 0.5f;
    audio.clip = missionCompletedAudio;
    audio.Play();

    if (menuPanel.activeSelf && !needHideMenuPanel)
    {
      needShowMenuPanel = false;
      needHideMenuPanel = true;
      menuPanelShowHideTime = Time.time;
    }

    if (!needShowMissionCompletionPanel)
    {
      needShowMissionCompletionPanel = true;
      needHideMissionCompletionPanel = false;
      missionCompletionPanelShowHideTime = Time.time;
    }

    if (!gameFinished)
    {
      gameFinished = true;
      missionController.missionFailed = false;
      missionController.UpdateMissionCompletionPanel();

      crystalStock = new CrystalStock();
      crystalStock.OnStockAmountChanged = OnStockAmountChanged;
      sceneController.RestartScene();
    }
  }

  public void OnMenuClicked()
  {
    if (!menuPanel.activeSelf)
    {
      if (missionPanel.activeSelf && !needShowMissionPanel && !needHideMissionPanel)
      {
        needHideMissionPanel = true;
        needShowMissionPanel = false;
        missionPanelShowHideTime = Time.time;
      }
      else if (missionCompletionPanel.activeSelf && (missionCompleted || missionController.missionFailed) &&
               !needHideMissionCompletionPanel)
      {
        missionCompletionPanelMoveUp = true;
        needShowMissionCompletionPanel = false;
        needHideMissionCompletionPanel = true;
        missionCompletionPanelShowHideTime = Time.time;
      }
      needShowMenuPanel = true;
      needHideMenuPanel = false;
      menuPanelShowHideTime = Time.time;
    }
    else if (!needHideMenuPanel && !needShowMenuPanel)
    {
      if (!missionPanel.activeSelf && !missionStarted && !missionCompleted)
      {
        needHideMissionPanel = false;
        needShowMissionPanel = true;
        missionPanelShowHideTime = Time.time;
      }
      else if (!missionCompletionPanel.activeSelf && missionCompleted)
      {
        needShowMissionCompletionPanel = true;
        needHideMissionCompletionPanel = false;
        missionCompletionPanelShowHideTime = Time.time;
      }
      needShowMenuPanel = false;
      needHideMenuPanel = true;
      menuPanelShowHideTime = Time.time;
    }
  }

  public void OnNextMission()
  {
    if (missionController.missionFailed)
    {
      RestartMission();
    }
    else
    {
      missionController.SetupNextMission();
      missionCompleted = false;
      missionStarted = false;
      if (!missionPanel.activeSelf)
      {
        needHideMissionPanel = false;
        needShowMissionPanel = true;
        missionPanelShowHideTime = Time.time;
      }

      if (!Persistence.gameConfig.donated)
      {
        ShowOptions options = new ShowOptions();
        Advertisement.Show("video", options);
      }
    }

    if (missionCompletionPanel.activeSelf && !needHideMissionCompletionPanel)
    {
      missionCompletionPanelMoveUp = false;
      needShowMissionCompletionPanel = false;
      needHideMissionCompletionPanel = true;
      missionCompletionPanelShowHideTime = Time.time;
    }
  }

  public void OnOpenHelp()
  {
    if (helpPanel.activeSelf)
    {
      helpPanel.gameObject.SetActive(false);
    }
    else
    {
      helpPanel.gameObject.SetActive(true);
      Analytics.CustomEvent("HelpOpened", new Dictionary<string, object>
      {
        { "mission", missionController.CurrentMission }
      });
    }
  }

  public void OnCloseHelp()
  {
    this.openHelpButton.onClick.Invoke();
  }
}
