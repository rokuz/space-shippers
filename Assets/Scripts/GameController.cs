using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SmartLocalization;
using GoogleMobileAds;
using GoogleMobileAds.Api;

public class GameController : MonoBehaviour
{
  public MissionController missionController;
  public Text startTimer;
  public Text gameTimer;
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

    RequestBanner();
	}

  private void RequestBanner()
  {
    #if UNITY_ANDROID
      string adUnitId = "ca-app-pub-8904882368983998/3748761996";
      string appId = "ca-app-pub-8904882368983998~4680190977";
    #elif UNITY_IPHONE
      string adUnitId = "";
      string appId = "";
    #else
      string adUnitId = "unexpected_platform";
      string appId = "unexpected_platform";
    #endif

    MobileAds.Initialize(appId);
    bannerView = new BannerView(adUnitId, AdSize.Banner, 0, 0);
    AdRequest request = new AdRequest.Builder().Build();

    this.bannerView.OnAdLoaded += this.HandleAdLoaded;
    this.bannerView.OnAdFailedToLoad += this.HandleAdFailedToLoad;
    this.bannerView.OnAdLeavingApplication += this.HandleAdLeftApplication;

    bannerView.LoadAd(request);
  }

  public void HandleAdLoaded(object sender, EventArgs args)
  {

  }

  public void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
  {

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
      bannerView.Destroy();
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
        if (!gameFinished)
        {
          gameFinished = true;
          missionController.missionFailed = true;

          crystalStock = new CrystalStock();
          crystalStock.OnStockAmountChanged = OnStockAmountChanged;
          sceneController.RestartScene();
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

  public void NextMission()
  {
    gameStarted = false;
    gameFinished = false;
    missionCompleted = false;
    missionController.SetupNextMission();
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
      var y = Mathf.Lerp(0.0f, missionCompletionPanelInitialPositionY, Mathf.Clamp((Time.time - missionCompletionPanelShowHideTime) / 0.3f, 0.0f, 1.0f));
      missionCompletionPanelRectTransform.localPosition = new Vector3(missionCompletionPanelRectTransform.localPosition.x, y,
                                                                      missionCompletionPanelRectTransform.localPosition.z);

      if (y == missionCompletionPanelInitialPositionY)
      {
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
    InitGameplay();
  }

  public void OnMissionCompleted()
  {
    if (missionCompleted)
      return;

    missionCompleted = true;

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
      RestartMission();
    else
      NextMission();

    if (missionCompletionPanel.activeSelf && !needHideMissionCompletionPanel)
    {
      needShowMissionCompletionPanel = false;
      needHideMissionCompletionPanel = true;
      missionCompletionPanelShowHideTime = Time.time;
    }
  }
}
