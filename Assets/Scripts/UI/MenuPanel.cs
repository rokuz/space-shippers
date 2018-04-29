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
  public Button otherGamesButton;
  public Text menuText;

	void Start()
  {
    var lang = LanguageManager.Instance;
    menuText.text = lang.GetTextValue("Menu_Title");
    resumeButton.GetComponentInChildren<Text>().text = lang.GetTextValue("Menu_Resume");
    restartButton.GetComponentInChildren<Text>().text = lang.GetTextValue("Menu_Restart");
    otherGamesButton.GetComponentInChildren<Text>().text = lang.GetTextValue("Menu_OtherGames");
	}
  
	void Update()
  {
    resumeButton.interactable = gameController.IsPlayingOrPreparing;
    restartButton.interactable = gameController.IsGameStarted;
	}
  
  public void OnRestartClicked()
  {
    gameController.RestartMission();
    gameController.OnMenuClicked();
  }
}
