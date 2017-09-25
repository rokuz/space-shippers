using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
  public GameController gameController;
  public Button resumeButton;
  public Button restartButton;
  public Button donateButton;

	void Start()
  {
		
	}
	
	void Update()
  {
    resumeButton.enabled = gameController.IsPlaying;
    restartButton.enabled = gameController.IsPlaying;
	}
}
