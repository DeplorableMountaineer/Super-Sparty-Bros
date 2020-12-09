using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// include UI namespace so can reference UI elements

// include so we can manipulate SceneManager

public class GameManager : MonoBehaviour
{
    // static reference to game manager so can be called from other scripts directly (not just through gameobject component)
    public static GameManager Gm;

    // levels to move to on victory and lose
    public string levelAfterVictory;
    public string levelAfterGameOver;

    // game performance
    public int score;
    public int highscore;
    public int startLives = 3;
    public int lives = 3;

    // UI elements to control
    public Text uiScore;
    public Text uiHighScore;
    public Text uiLevel;
    public GameObject[] uiExtraLives;
    public GameObject uiGamePaused;

    // private variables
    private GameObject _player;
    private Scene _scene;
    private Vector3 _spawnLocation;

    // set things up here
    private void Awake()
    {
        // setup reference to game manager
        if (Gm == null)
            Gm = GetComponent<GameManager>();

        // setup all the variables, the UI, and provide errors if things not setup properly.
        SetupDefaults();
    }

    // game loop
    private void Update()
    {
        // if ESC pressed then pause the game
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (Time.timeScale > 0f)
        {
            uiGamePaused.SetActive(true); // this brings up the pause UI
            Time.timeScale = 0f; // this pauses the game action
        }
        else
        {
            Time.timeScale = 1f; // this unpauses the game action (ie. back to normal)
            uiGamePaused.SetActive(false); // remove the pause UI
        }
    }

    // setup all the variables, the UI, and provide errors if things not setup properly.
    private void SetupDefaults()
    {
        // setup reference to player
        if (_player == null)
            _player = GameObject.FindGameObjectWithTag("Player");

        if (_player == null)
            Debug.LogError("Player not found in Game Manager");

        // get current scene
        _scene = SceneManager.GetActiveScene();

        // get initial _spawnLocation based on initial position of player
        _spawnLocation = _player.transform.position;

        // if levels not specified, default to current level
        if (levelAfterVictory == "")
        {
            Debug.LogWarning("levelAfterVictory not specified, defaulted to current level");
            levelAfterVictory = _scene.name;
        }

        if (levelAfterGameOver == "")
        {
            Debug.LogWarning("levelAfterGameOver not specified, defaulted to current level");
            levelAfterGameOver = _scene.name;
        }

        // friendly error messages
        if (uiScore == null)
            Debug.LogError("Need to set UIScore on Game Manager.");

        if (uiHighScore == null)
            Debug.LogError("Need to set UIHighScore on Game Manager.");

        if (uiLevel == null)
            Debug.LogError("Need to set UILevel on Game Manager.");

        if (uiGamePaused == null)
            Debug.LogError("Need to set UIGamePaused on Game Manager.");

        // get stored player prefs
        RefreshPlayerState();

        // get the UI ready for the game
        RefreshGUI();
    }

    // get stored Player Prefs if they exist, otherwise go with defaults set on gameObject
    private void RefreshPlayerState()
    {
        lives = PlayerPrefManager.GetLives();

        // special case if lives <= 0 then must be testing in editor, so reset the player prefs
        if (lives <= 0)
        {
            PlayerPrefManager.ResetPlayerState(startLives, false);
            lives = PlayerPrefManager.GetLives();
        }

        score = PlayerPrefManager.GetScore();
        highscore = PlayerPrefManager.GetHighscore();

        // save that this level has been accessed so the MainMenu can enable it
        PlayerPrefManager.UnlockLevel();
    }

    // refresh all the GUI elements
    private void RefreshGUI()
    {
        // set the text elements of the UI
        uiScore.text = "Score: " + score;
        uiHighScore.text = "Highscore: " + highscore;
        uiLevel.text = _scene.name;

        // turn on the appropriate number of life indicators in the UI based on the number of lives left
        for (int i = 0; i < uiExtraLives.Length; i++)
            if (i < lives - 1)
                // show one less than the number of lives since you only typically show lifes after the current life in UI
                uiExtraLives[i].SetActive(true);
            else
                uiExtraLives[i].SetActive(false);
    }

    // public function to add points and update the gui and highscore player prefs accordingly
    public void AddPoints(int amount)
    {
        // increase score
        score += amount;

        // update UI
        uiScore.text = "Score: " + score;

        // if score>highscore then update the highscore UI too
        if (score > highscore)
        {
            highscore = score;
            uiHighScore.text = "Highscore: " + score;
        }
    }

    // public function to remove player life and reset game accordingly
    public void ResetGame()
    {
        // remove life and update GUI
        lives--;
        RefreshGUI();

        if (lives <= 0)
        {
            // no more lives
            // save the current player prefs before going to GameOver
            PlayerPrefManager.SavePlayerState(score, highscore, lives);

            // load the gameOver screen
            SceneManager.LoadScene(levelAfterGameOver);
        }
        else
        {
            // tell the player to respawn
            _player.GetComponent<CharacterController2D>().Respawn(_spawnLocation);
        }
    }

    // public function for level complete
    public void LevelCompete()
    {
        // save the current player prefs before moving to the next level
        PlayerPrefManager.SavePlayerState(score, highscore, lives);

        // use a coroutine to allow the player to get fanfare before moving to next level
        StartCoroutine(LoadNextLevel());
    }

    // load the nextLevel after delay
    private IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(3.5f);
        SceneManager.LoadScene(levelAfterVictory);
    }
}