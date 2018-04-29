using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;
using SmartLocalization;

public class OtherGames : MonoBehaviour
{
  public Button spellbinderDuelsButton;
  public Text spellbinderDuelsText;
  public Text otherGamesText;

	void Start()
  {
    var lang = LanguageManager.Instance;
    otherGamesText.text = lang.GetTextValue("Menu_OtherGames");

    spellbinderDuelsText.text = lang.GetTextValue("SB_desc");
    spellbinderDuelsButton.GetComponentInChildren<Text>().text = lang.GetTextValue("Button_Details");
	}
  
  public void OnSpellbinderDuelsClicked()
  {
    Analytics.CustomEvent("Details_SpellbinderDuels");
#if UNITY_ANDROID
    Application.OpenURL("https://play.google.com/store/apps/details?id=com.rokuz.spellbinder");
#elif UNITY_IPHONE
    Application.OpenURL("https://itunes.apple.com/us/app/spellbinder-duels/id1303026915");
#endif
  }
}
