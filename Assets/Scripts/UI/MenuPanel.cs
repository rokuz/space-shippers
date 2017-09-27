using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SmartLocalization;

public class MenuPanel : MonoBehaviour
{
  public GameController gameController;
  public Button resumeButton;
  public Button restartButton;
  public Button donateButton;
  public Text menuText;

  public Purchaser purchaser;

	void Start()
  {
    purchaser.OnDonated += OnDonateCompleted;

    var lang = LanguageManager.Instance;
    menuText.text = lang.GetTextValue("Menu_Title");
    resumeButton.GetComponentInChildren<Text>().text = lang.GetTextValue("Menu_Resume");
    restartButton.GetComponentInChildren<Text>().text = lang.GetTextValue("Menu_Restart");
    UpdateDonateButton();
	}

  private void UpdateDonateButton()
  {
    var lang = LanguageManager.Instance;
    if (purchaser.IsDonated)
    {
      donateButton.GetComponentInChildren<Text>().text = lang.GetTextValue("Menu_Thanks");
      donateButton.enabled = false;
      gameController.DestroyBanner();
    }
    else
    {
      string val = purchaser.GetDonateAmount();
      donateButton.GetComponentInChildren<Text>().text = lang.GetTextValue("Menu_Donate") + (val.Length != 0 ? ("\n" + val) : "");
      donateButton.enabled = true;
    }
  }
	
	void Update()
  {
    resumeButton.interactable = gameController.IsPlayingOrPreparing;
    restartButton.interactable = gameController.IsGameStarted;
	}

  public void OnDonateButtonClicked()
  {
    if (!purchaser.IsDonated)
      purchaser.Donate();
  }

  public void OnDonateCompleted(bool success)
  {
    UpdateDonateButton();
    if (success)
    {
      gameController.DestroyBanner();
      Persistence.gameConfig.donated = true;
      Persistence.Save();
    }
  }

  public void OnRestartClicked()
  {
    gameController.RestartMission();
    gameController.OnMenuClicked();
  }
}
