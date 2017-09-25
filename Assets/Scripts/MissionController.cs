using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SmartLocalization;
using System.Linq;

public class MissionController : MonoBehaviour
{
  public float levelDuration = 60.0f;

  public GameObject ingameYellowGoal;
  public GameObject ingameCianGoal;
  public GameObject ingamePurpleGoal;

  public GameController gameController;

  public Text missionNameText;
  public Toggle goalToggle1;
  public Toggle goalToggle2;
  public Toggle goalToggle3;
  public Button startMissionButton;

  public Sprite yellowCrystal;
  public Sprite cianCrystal;
  public Sprite purpleCrystal;

  public Text tutorialText;

  public Planet planet1;
  public Planet planet2;
  public Planet planet3;

  private Text ingameYellowGoalText;
  private Text ingameCianGoalText;
  private Text ingamePurpleGoalText;
  private GameObject ingameYellowGoalCheck;
  private GameObject ingameCianGoalCheck;
  private GameObject ingamePurpleGoalCheck;

  public class MissionDetails
  {
    public uint yellowCrystalsCount = 0;
    public uint cianCrystalsCount = 0;
    public uint purpleCrystalsCount = 0;
    public float[] planetsSpeeds = new float[]{ 20.0f, 20.0f, 20.0f };
    public float spaceshipSpeed = 5.0f;

    public MissionDetails(uint _yellowCrystalsCount, uint _cianCrystalsCount, uint _purpleCrystalsCount,
                          float[] _planetsSpeeds, float _spaceshipSpeed)
    {
      yellowCrystalsCount = _yellowCrystalsCount;
      cianCrystalsCount = _cianCrystalsCount;
      purpleCrystalsCount = _purpleCrystalsCount;
      planetsSpeeds = _planetsSpeeds;
      spaceshipSpeed = _spaceshipSpeed;
    }
  }

  private MissionDetails[] missions = new MissionDetails[]
  {
    new MissionDetails(1 /*yellow*/, 0 /*cian*/, 0 /*purple*/, new float[]{ 3, 4, 5 } /*planets speed*/, 5 /*ship speed*/), // Mission 1
    new MissionDetails(0 /*yellow*/, 1 /*cian*/, 0 /*purple*/, new float[]{ 10, 12, 20 } /*planets speed*/, 5 /*ship speed*/), // Mission 2
    new MissionDetails(0 /*yellow*/, 0 /*cian*/, 1 /*purple*/, new float[]{ 10, 13, 20 } /*planets speed*/, 5 /*ship speed*/), // Mission 3
    new MissionDetails(1 /*yellow*/, 1 /*cian*/, 0 /*purple*/, new float[]{ 10, 13, 20 } /*planets speed*/, 5 /*ship speed*/), // Mission 4
    new MissionDetails(0 /*yellow*/, 1 /*cian*/, 1 /*purple*/, new float[]{ 10, 13, 20 } /*planets speed*/, 5 /*ship speed*/), // Mission 5
    new MissionDetails(1 /*yellow*/, 0 /*cian*/, 1 /*purple*/, new float[]{ 10, 13, 20 } /*planets speed*/, 5 /*ship speed*/), // Mission 6
    new MissionDetails(1 /*yellow*/, 1 /*cian*/, 1 /*purple*/, new float[]{ 10, 13, 20 } /*planets speed*/, 5 /*ship speed*/), // Mission 7
  };

  private int currentMissionIndex = 0;
  private uint currentYellowCrystalsCount = 0;
  private uint currentCianCrystalsCount = 0;
  private uint currentPurpleCrystalsCount = 0;

  private uint tutorialStep = 0;

  public float LevelDuration
  {
    get { return levelDuration; }
  }

	void Start()
  {
    ingameYellowGoalText = FindInGameGoalText(ingameYellowGoal);
    ingameCianGoalText = FindInGameGoalText(ingameCianGoal);
    ingamePurpleGoalText = FindInGameGoalText(ingamePurpleGoal);
    ingameYellowGoalCheck = FindInGameGoalCheck(ingameYellowGoal);
    ingameCianGoalCheck = FindInGameGoalCheck(ingameCianGoal);
    ingamePurpleGoalCheck = FindInGameGoalCheck(ingamePurpleGoal);

    startMissionButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("MissionStartButton");

    // TODO: read index from config
    InitMission(0);
	}

  private void InitMission(int index)
  {
    currentMissionIndex = index;
    currentYellowCrystalsCount = 0;
    currentCianCrystalsCount = 0;
    currentPurpleCrystalsCount = 0;

    ingameYellowGoal.SetActive(missions[index].yellowCrystalsCount != 0);
    ingameCianGoal.SetActive(missions[index].cianCrystalsCount != 0);
    ingamePurpleGoal.SetActive(missions[index].purpleCrystalsCount != 0);
    ingameYellowGoalText.text = "" + missions[index].yellowCrystalsCount;
    ingameCianGoalText.text = "" + missions[index].cianCrystalsCount;
    ingamePurpleGoalText.text = "" + missions[index].purpleCrystalsCount;

    ingameYellowGoalCheck.SetActive(false);
    ingameCianGoalCheck.SetActive(false);
    ingamePurpleGoalCheck.SetActive(false);

    var lang = LanguageManager.Instance;
    missionNameText.text = lang.GetTextValue("MissionNameTitle") + " " + (currentMissionIndex + 1);

    Toggle[] toggles = new Toggle[] { goalToggle1, goalToggle2, goalToggle3 };
    int currentToggle = 0;
    if (missions[index].yellowCrystalsCount != 0)
    {
      toggles[currentToggle].gameObject.SetActive(true);
      toggles[currentToggle].GetComponentInChildren<Text>().text = lang.GetTextValue("MissionGoal") + " " + missions[index].yellowCrystalsCount;
      FindCrystalImage(toggles[currentToggle].gameObject).sprite = yellowCrystal;
      currentToggle++;
    }
    if (missions[index].cianCrystalsCount != 0)
    {
      toggles[currentToggle].gameObject.SetActive(true);
      toggles[currentToggle].GetComponentInChildren<Text>().text = lang.GetTextValue("MissionGoal") + " " + missions[index].cianCrystalsCount;
      FindCrystalImage(toggles[currentToggle].gameObject).sprite = cianCrystal;
      currentToggle++;
    }
    if (missions[index].purpleCrystalsCount != 0)
    {
      toggles[currentToggle].gameObject.SetActive(true);
      toggles[currentToggle].GetComponentInChildren<Text>().text = lang.GetTextValue("MissionGoal") + " " + missions[index].purpleCrystalsCount;
      FindCrystalImage(toggles[currentToggle].gameObject).sprite = purpleCrystal;
      currentToggle++;
    }
    for (int i = currentToggle; i < toggles.Length; i++)
      toggles[i].gameObject.SetActive(false);

    Planet[] planets = new Planet[] { planet1, planet2, planet3 };
    int[] indices = (from i in new int[] { 0, 1, 2 } orderby Random.value select i).ToArray();
    float[] orbitRadiuses = new float[] { 8.0f, 15.0f, 22.0f };
    for (int i = 0; i < indices.Length; i++)
    {
      var planet = planets[indices[i]];
      planet.PlanetOrbit.ApplyRadius(orbitRadiuses[i]);
      planet.speed = missions[currentMissionIndex].planetsSpeeds[i];

      if (currentMissionIndex == 0 && planet.GetComponent<CrystalFactory>().crystalType == Crystal.Blue)
        planet.startPosition = 0.0f;

      planet.ApplyInitialTransform();
    }

    if (currentMissionIndex == 0)
    {
      tutorialStep = 0;
      FindPlanet(Crystal.Red).tutorialPointer.SetActive(false);
      FindPlanet(Crystal.Green).tutorialPointer.SetActive(false);
      tutorialText.gameObject.SetActive(true);
      tutorialText.text = lang.GetTextValue("Tutorial_1");
      tutorialStep++;
    }
    else
    {
      tutorialText.gameObject.SetActive(false);
      tutorialStep = 0;
    }
  }

  public void SetupNextMission()
  {
    int nextMission = currentMissionIndex + 1;
    InitMission(nextMission);
  }

  public void RestartMission()
  {
    InitMission(currentMissionIndex);
  }
	
  public void ApplyCrystals(Crystal type, uint amount)
  {
    if (type == Crystal.Yellow)
      currentYellowCrystalsCount += amount;
    else if (type == Crystal.Cian)
      currentCianCrystalsCount += amount;
    else if (type == Crystal.Purple)
      currentPurpleCrystalsCount += amount;

    if (currentMissionIndex == 0 && tutorialStep == 1 && type == Crystal.Red && amount > 0)
    {
      tutorialText.text = LanguageManager.Instance.GetTextValue("Tutorial_2");
      FindPlanet(Crystal.Red).tutorialPointer.SetActive(true);
      tutorialStep++;
    }

    if (currentYellowCrystalsCount >= missions[currentMissionIndex].yellowCrystalsCount &&
        missions[currentMissionIndex].yellowCrystalsCount != 0)
    {
      ingameYellowGoalCheck.SetActive(true);
    }
    if (currentCianCrystalsCount >= missions[currentMissionIndex].cianCrystalsCount &&
        missions[currentMissionIndex].cianCrystalsCount != 0)
    {
      ingameCianGoalCheck.SetActive(true);
    }
    if (currentPurpleCrystalsCount >= missions[currentMissionIndex].purpleCrystalsCount &&
        missions[currentMissionIndex].purpleCrystalsCount != 0)
    {
      ingamePurpleGoalCheck.SetActive(true);
    }

    if (currentYellowCrystalsCount >= missions[currentMissionIndex].yellowCrystalsCount &&
        currentCianCrystalsCount >= missions[currentMissionIndex].cianCrystalsCount &&
        currentPurpleCrystalsCount >= missions[currentMissionIndex].purpleCrystalsCount)
    {
      gameController.OnMissionCompleted();
    }
  }

  public void OnSelectPlanet(Planet planet)
  {
    if (currentMissionIndex == 0 && tutorialStep == 2)
    {
      if (planet != null && planet.gameObject.GetComponent<CrystalFactory>().crystalType == Crystal.Red)
      {
        tutorialText.text = LanguageManager.Instance.GetTextValue("Tutorial_3");
        FindPlanet(Crystal.Red).tutorialPointer.SetActive(false);
        FindPlanet(Crystal.Green).tutorialPointer.SetActive(true);
        tutorialStep++;
      }
    }
    else if (currentMissionIndex == 0 && tutorialStep == 3)
    {
      tutorialText.gameObject.SetActive(false);
      FindPlanet(Crystal.Green).tutorialPointer.SetActive(false);
      tutorialStep = 0;
    }
  }

  public float GetSpaceshipSpeed()
  {
    return missions[currentMissionIndex].spaceshipSpeed;
  }

  private Text FindInGameGoalText(GameObject ingameUiObject)
  {
    string kObjectName = "MissionGoalText";
    foreach (Text t in ingameUiObject.GetComponentsInChildren<Text>(true)) 
    {
      if (t.gameObject.name == kObjectName)
        return t;
    }
    return null;
  }

  private GameObject FindInGameGoalCheck(GameObject ingameUiObject)
  {
    string kObjectName = "MissionGoalCheck";
    foreach (Image t in ingameUiObject.GetComponentsInChildren<Image>(true)) 
    {
      if (t.gameObject.name == kObjectName)
        return t.gameObject;
    }
    return null;
  }

  private Image FindCrystalImage(GameObject obj)
  {
    string kObjectName = "CrystalImage";
    foreach (Image t in obj.GetComponentsInChildren<Image>(true)) 
    {
      if (t.gameObject.name == kObjectName)
        return t;
    }
    return null;
  }

  private Planet FindPlanet(Crystal crystalType)
  {
    Planet[] planets = new Planet[] { planet1, planet2, planet3 };
    foreach (var planet in planets)
    {
      if (planet.gameObject.GetComponent<CrystalFactory>().crystalType == crystalType)
        return planet;
    }
    return null;
  }
}
