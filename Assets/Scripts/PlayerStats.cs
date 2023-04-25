using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;


public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    // Keys for LootLocker progression
    string experienceProgressionKey = "experience";
    string missionProgressionKey = "mission";

    public UIController uiController;

    // Player stats
    private ulong playerLevel = 1;
    private ulong playerMaxHealth = 0;
    private int playerHealth = 0;
    private ulong playerDamage = 0;
    private ulong playerCurrentXP = 0;
    private ulong playerPreviousLevelXP = 0;
    private ulong playerNextLevelXP = 0;

    // Mission
    private ulong slayedSkeletons = 0;
    private ulong slayedSkeletonsGoal = 0;

    public WorldText popUpText;

    public ulong PlayerMaxHealth
    {
        get { return playerMaxHealth; }
    }

    public ulong PlayerDamage
    {
        get { return GetPlayerDamage(); }
    }

    public int PlayerHealth
    {
        get { return playerHealth; }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Delete player prefs
    [ContextMenu("Delete Player Prefs")]
    public void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        // Outout to log in red color
        Debug.Log("<color=red>Player Prefs Deleted</color>");
    }
    private void Start()
    {
        // Start a guest session
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            // If the session was started successfully
            if (response.success)
            {
                // New player; register the progression
                if (response.seen_before == false)
                {
                    // Register the experience so that we now it exists
                    LootLockerSDKManager.RegisterPlayerProgression(experienceProgressionKey, (response) =>
                    {
                        if (response.success)
                        {
                            Debug.Log("XP Progression registered");
                            // If the next step is null(last step of progression), set the next treshhold to the amount of points instead
                            ulong nextTreshold = response.next_threshold == null ? response.points : (ulong)response.next_threshold;
                            UpdateAllPlayerStats(response.points, response.step, response.previous_threshold, nextTreshold);
                        }
                        else
                        {
                            Debug.Log("XP Progression registration failed;" + response.Error);
                        }
                    });
                    // Register the mission progression so that we now it exists
                    LootLockerSDKManager.RegisterPlayerProgression(missionProgressionKey, (response) =>
                    {
                        if (response.success)
                        {
                            uiController.UpdateSlayedSkeletons(response.points, (ulong)response.next_threshold);
                        }
                        else
                        {
                            Debug.Log("Mission progression registration failed;" + response.Error);
                        }
                    });
                }
                else
                {
                    // Player has played before; get the progression
                    // We only have two progressions in this game, so we specify that
                    int amountOfProgressions = 2;
                    LootLockerSDKManager.GetPlayerProgressions(amountOfProgressions, "", (response) =>
                    {
                        if (response.success)
                        {
                            // Update both progressions
                            for (int i = 0; i < response.items.Count; i++)
                            {
                                string progressionKey = response.items[i].progression_key;
                                // If the progression is the experience progression
                                if (progressionKey == experienceProgressionKey)
                                {
                                    // Update the player stats
                                    // If the next step is null(last step of progression), set the next treshhold to the amount of points instead
                                    ulong nextTreshold = response.items[i].next_threshold == null ? response.items[i].points : (ulong)response.items[i].next_threshold;
                                    UpdateAllPlayerStats(response.items[i].points, response.items[i].step, response.items[i].previous_threshold, nextTreshold);

                                }
                                // If the progression is the mission progression
                                else if (progressionKey == missionProgressionKey)
                                {
                                    // Update the mission stats
                                    // We can cast this to just an ulong instead of an ulong? since we know that this progression never has a null next threshold
                                    UpdateSlayedSkeletons(response.items[i].points, (ulong)response.items[i].next_threshold);

                                }
                            }

                            Debug.Log("Progressions retrieved");
                        }
                        else
                        {
                            Debug.Log("Progression retrieval failed;" + response.Error);
                        }
                    });
                }
            }
        });
    }

    public void UpdateAllPlayerStats(ulong xp, ulong level, ulong previousLevelXP, ulong nextLevelXP)
    {
        playerCurrentXP = xp;
        playerLevel = level;
        playerPreviousLevelXP = previousLevelXP;
        playerNextLevelXP = nextLevelXP;
        UpdateXPAndLevel(playerCurrentXP, playerLevel, playerPreviousLevelXP, playerNextLevelXP);

        playerMaxHealth = GetMaxHealth();
        playerHealth = (int)playerMaxHealth;
        UpdateHealth();

        playerDamage = GetPlayerDamage();
    }

    public void UpdateHealth()
    {
        uiController.UpdateHealthbar(playerHealth, playerMaxHealth);
    }

    public void UpdateXPAndLevel(ulong xp, ulong level, ulong previousLevelXP, ulong nextLevelXP)
    {
        playerCurrentXP = xp;
        playerLevel = level;
        playerNextLevelXP = nextLevelXP;
        playerPreviousLevelXP = previousLevelXP;
        playerMaxHealth = GetMaxHealth();
        uiController.UpdateHealthbar(playerHealth, playerMaxHealth);
        uiController.UpdateXPAndLevel(playerCurrentXP, playerLevel, playerPreviousLevelXP, playerNextLevelXP);
    }

    public ulong GetMaxHealth()
    {
        // Simple exponential formula for calculating playerMaxHealth
        return (ulong)(10 + (playerLevel * playerLevel * 0.8f) * 10);
    }

    public ulong GetPlayerDamage()
    {
        // Simple exponential formula for calculating damage
        return (ulong)(1 + (playerLevel * playerLevel * 0.8f));
    }

    public void AddXP(int pointsToAdd)
    {
        LootLockerSDKManager.AddPointsToPlayerProgression(experienceProgressionKey, (ulong)pointsToAdd, (response) =>
        {
            if (response.success)
            {
                // Spawn XP popup prefab
                Instantiate(popUpText, transform.position + Vector3.up * 2f, Quaternion.identity).SetText("+" + pointsToAdd + "XP", Color.blue, 3f);
                ulong playerPreviousXP = playerCurrentXP;
                playerCurrentXP = response.points;
                // If we are at max level, set next level xp to our points instead, since we can not go higher
                ulong nextTreshold = response.next_threshold == null ? response.points : (ulong)response.next_threshold;
                UpdateXPAndLevel(response.points, response.step, response.previous_threshold, nextTreshold);
                if (playerPreviousXP > response.next_threshold)
                {
                    // Leveled up!
                    // Spawn VFX
                    Instantiate(popUpText, transform.position + Vector3.up * 2f, Quaternion.identity).SetText("Level Up!", Color.blue, 5f);

                    // Set new health
                    playerMaxHealth = GetMaxHealth();
                    playerHealth = (int)playerMaxHealth;
                    UpdateHealth();
                }

            }
        });
    }

    public void SlaySkeleton()
    {
        // You will always slay one skeleton per time
        int pointsToAdd = 1;
        LootLockerSDKManager.AddPointsToPlayerProgression(missionProgressionKey, (ulong)pointsToAdd, (response) =>
        {
            if (response.success)
            {
                ulong previousSlayedSkeletons = slayedSkeletons;
                slayedSkeletons = response.points;
                // Check if the next threshold exists, if not, it means we've reached our mission goal
                if (response.next_threshold == null)
                {
                    // Mission was cleared!
                    // Award extra XP
                    AddXP(100);

                    // Reset the mission
                    LootLockerSDKManager.ResetPlayerProgression(missionProgressionKey, (response) =>
                    {
                        if (response.success)
                        {
                            slayedSkeletons = 0;
                            Debug.Log("Mission reset");
                            UpdateSlayedSkeletons(response.points, (ulong)response.next_threshold);
                        }
                        else
                        {
                            Debug.Log("Mission reset failed;" + response.Error);
                        }
                    });
                }
                else
                {
                    // Since the progression can be null, we need to do this seperately
                    // If the progression was null, the progression was reset in the above if-statement
                    UpdateSlayedSkeletons(response.points, (ulong)response.next_threshold);
                }
            }
        });
    }

    void UpdateSlayedSkeletons(ulong slayedSkeletons, ulong slayedSkeletonsGoal)
    {
        this.slayedSkeletonsGoal = slayedSkeletonsGoal;
        this.slayedSkeletons = slayedSkeletons;
        uiController.UpdateSlayedSkeletons(slayedSkeletons, slayedSkeletonsGoal);
    }

    public void TakeDamage(ulong damage)
    {
        playerHealth -= (int)damage;
        uiController.UpdateHealthbar(playerHealth, playerMaxHealth);
    }
}
