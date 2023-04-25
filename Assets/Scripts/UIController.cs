using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI dungeonLevel;
    public TextMeshProUGUI playerHealth;
    public Image playerHealthBar;
    public Image playerHealthBarOverlay;
    public TextMeshProUGUI playerLevel;
    public TextMeshProUGUI playerXP;
    public Image XPBar;
    public TextMeshProUGUI slayedSkeletonsText;
    public Image slayedSkeletonsBar;

    public AnimationCurve barAnimationCurve;

    public void UpdateXPAndLevel(ulong currentXP, ulong level, ulong previousLevelXP, ulong nextLevelXP)
    {
        playerLevel.text = level.ToString();
        StartCoroutine(AnimateXPBar(currentXP, previousLevelXP, nextLevelXP));
    }

    public void UpdateSlayedSkeletons(ulong amount, ulong total)
    {
        slayedSkeletonsText.text = (total - amount).ToString();
        slayedSkeletonsBar.fillAmount = (float)amount / (float)total;
    }

    IEnumerator AnimateXPBar(ulong currentXP, ulong previousLevelXP, ulong nextLevelXP)
    {
        float t = 0f;
        float startValue = XPBar.fillAmount;
        float endValue = Mathf.Lerp(0f, 1f, (float)(currentXP - previousLevelXP) / (float)(nextLevelXP - previousLevelXP));
        float duration = 0.5f;
        ulong startXPValue = nextLevelXP - previousLevelXP;
        // Check if we are at max level (no xp given)
        if ((startXPValue != 0) && (nextLevelXP - currentXP) != 0)
        {
            while (t < duration)
            {
                t += Time.deltaTime;
                XPBar.fillAmount = Mathf.Lerp(startValue, endValue, barAnimationCurve.Evaluate(t / duration));
                // Get the text animation based on the fill amount of the image
                playerXP.text = Mathf.CeilToInt(Mathf.Lerp((int)startXPValue, 0, XPBar.fillAmount)).ToString();
                yield return null;
            }
        }
        else
        {
            yield return null;
        }
        playerXP.text = (nextLevelXP - currentXP).ToString();
        XPBar.fillAmount = endValue;
    }

    // Animate healthbar
    public void UpdateHealthbar(int health, ulong maxHealth)
    {
        playerHealth.text = health.ToString() + "/" + maxHealth;
        playerHealthBar.fillAmount = (float)health / (float)maxHealth;
        StartCoroutine(AnimateHealthbar(health, maxHealth));
    }

    // Animate healthbar with coroutine
    IEnumerator AnimateHealthbar(int health, ulong maxHealth)
    {
        float currentHealth = playerHealthBar.fillAmount;
        float targetHealth = (float)health / (float)maxHealth;
        float time = 0f;
        float duration = 0.25f;
        while (time <= duration)
        {
            time += Time.deltaTime;
            float animationCurveValue = barAnimationCurve.Evaluate(time / duration);
            playerHealthBar.fillAmount = Mathf.Lerp(currentHealth, targetHealth, animationCurveValue);
            yield return null;
        }
        time = 0f;
        while (time <= duration)
        {
            time += Time.deltaTime;
            float animationCurveValue = barAnimationCurve.Evaluate(time / duration);
            playerHealthBarOverlay.fillAmount = Mathf.Lerp(currentHealth, targetHealth, animationCurveValue);
            // Set player health text to the value of the healthbar
            playerHealth.text = Mathf.RoundToInt(playerHealthBar.fillAmount * PlayerStats.instance.PlayerMaxHealth).ToString() + "/" + PlayerStats.instance.PlayerMaxHealth.ToString();
            yield return null;
        }


        playerHealth.text = health.ToString() + "/" + PlayerStats.instance.PlayerMaxHealth.ToString();
        playerHealthBar.fillAmount = (float)health / (float)PlayerStats.instance.PlayerMaxHealth;
    }

    //Add 10 XP to player
    public void AddXP(int xpToAdd)
    {
        PlayerStats.instance.AddXP(xpToAdd);
    }

}
