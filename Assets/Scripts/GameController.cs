using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public GameObject startMenu;
    public GameObject gameMenu;
    public GameObject gameOverMenu;
    public CanvasGroup loadingScreen;

    public Transform playerTransform;


    public Menu gameState;

    public enum Menu{Start, Game, Loading, GameOver}

    private void Awake()
    {
        
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
            SwitchMenu(Menu.Start);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SwitchMenu(Menu.Start);
    }

    // Switch between the different menus by Menu enum
    private void SwitchMenu(Menu menu)
    {
        switch (menu)
        {
            case Menu.Start:
                startMenu.SetActive(true);
                gameMenu.SetActive(false);
                gameOverMenu.SetActive(false);
                break;
            case Menu.Game:
                startMenu.SetActive(false);
                gameMenu.SetActive(true);
                gameOverMenu.SetActive(false);
                break;
            case Menu.GameOver:
                startMenu.SetActive(false);
                gameMenu.SetActive(false);
                gameOverMenu.SetActive(true);
                break;
        }
        gameState = menu;
    }

    //Switch gamestate and animate loading Screen
    public void SwitchGameState(Menu menu, bool animate, bool switchLevel = false)
    {
        StartCoroutine(AnimateLoadingScreen(menu, animate, switchLevel));
    }

    //Animate loading Screen
    IEnumerator AnimateLoadingScreen(Menu menu, bool animate, bool switchLevel = false)
    {
        float time = 0;
        loadingScreen.gameObject.SetActive(true);
        float animationDuration = 0.5f;
        if (animate)
        {
            while (time <= animationDuration)
            {
                time += Time.deltaTime;
                float value = Mathf.Lerp(0f, 1f, time / animationDuration);
                loadingScreen.alpha = value;
                yield return null;
            }
            loadingScreen.alpha = 1f;
            
            if (switchLevel)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                SwitchMenu(menu);
                // Extra time needed to load the dungeon
                yield return new WaitForSeconds(1f);
            }
            else
            {
                // Wait for a little while
                yield return new WaitForSeconds(0.25f);
                SwitchMenu(menu);
            }
            time = 0;
            while (time <= animationDuration)
            {
                time += Time.deltaTime;
                float value = Mathf.Lerp(1f, 0f, time / animationDuration);
                loadingScreen.alpha = value;
                yield return null;
            }
            loadingScreen.alpha = 0f;
            loadingScreen.gameObject.SetActive(false);
        }
        else
        {
            SwitchMenu(menu);
        }
        yield return null;
    }

    public void StartGame()
    {
        SwitchGameState(Menu.Game, true);
    }

    public void RestartGame()
    {
        SwitchGameState(Menu.Start, true, true);
    }

    public void NextLevel()
    {
        PersistentData.instance.DungeonLevel++;
        SwitchGameState(Menu.Game, true, true);
    }
}
