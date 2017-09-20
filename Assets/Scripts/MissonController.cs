using UnityEngine;
using System.Collections;

public class MissonController : MonoBehaviour
{
    public float levelDuration = 60.0f;
    public CrystalPack[] missionGoals;

    private CrystalPack[] currentValues;

    public float LevelDuration
    {
        get { return levelDuration; }
    }

	void Start()
    {
        for (int i = 0; i < missionGoals.Length; i++)
        {
            for (int j = i + 1; j < missionGoals.Length; j++)
            {
                if (missionGoals[i].type == missionGoals[j].type)
                    throw new UnityException("Duplicate goal in mission " + this.gameObject.name);
            }
        }

        currentValues = new CrystalPack[missionGoals.Length];
        for (int i = 0; i < currentValues.Length; i++)
        {
            currentValues[i] = new CrystalPack();
            currentValues[i].type = missionGoals[i].type;
            currentValues[i].amount = 0;
        }
	}
	
    public void ApplyCrystals(Crystal type, uint amount)
    {
        for (int i = 0; i < currentValues.Length; i++)
        {
            if (currentValues[i].type == type)
            {
                currentValues[i].amount = amount;
                break;
            }
        }
    }

    public string GetGoalString(Crystal type)
    {
        for (int i = 0; i < missionGoals.Length; i++)
        {
            if (missionGoals[i].type == type)
            {
                return "" + missionGoals[i].amount;
            }
        }
        return "";
    }
}
