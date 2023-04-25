using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This class saves the current Dungeon level
// This could be made using LootLocker progressions as well, but for the sake of simplicity, it's using PlayerPrefs
public class PersistentData : MonoBehaviour
{
    public static PersistentData instance;
    private int dungeonLevel = 1;

    public UIController uiController;

    public int DungeonLevel
    {
        get
        {
            return dungeonLevel;
        }
        set
        {
            dungeonLevel = value;
            PlayerPrefs.SetInt("DungeonLevel", dungeonLevel);
            uiController.dungeonLevel.text = dungeonLevel.ToString();
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            DungeonLevel = PlayerPrefs.GetInt("DungeonLevel", dungeonLevel);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
