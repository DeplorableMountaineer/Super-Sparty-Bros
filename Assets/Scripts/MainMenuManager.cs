using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // include UI namespace since references UI Buttons directly
// include EventSystems namespace so can set initial input for controller support

// include so we can load new scenes

public class MainMenuManager : MonoBehaviour
{
    public int startLives = 3; // how many lives to start the game with on New Game

    // references to Submenus
    public GameObject mainMenu;
    public GameObject levelsMenu;
    public GameObject aboutMenu;

    // references to Button GameObjects
    public GameObject menuDefaultButton;
    public GameObject aboutDefaultButton;
    public GameObject levelSelectDefaultButton;
    public GameObject quitButton;

    // list the level names
    public string[] levelNames;

    // reference to the LevelsPanel gameObject where the buttons should be childed
    public GameObject levelsPanel;

    // reference to the default Level Button template
    public GameObject levelButtonPrefab;

    // reference the titleText so we can change it dynamically
    public Text titleText;

    // store the initial title so we can set it back
    private string _mainTitle;

    // init the menu
    private void Awake()
    {
        // store the initial title so we can set it back
        _mainTitle = titleText.text;

        // disable/enable Level buttons based on player progress
        SetLevelSelect();

        // determine if Quit button should be shown
        DisplayQuitWhenAppropriate();

        // Show the proper menu
        ShowMenu("MainMenu");
    }

    // loop through all the LevelButtons and set them to interactable 
    // based on if PlayerPref key is set for the level.
    private void SetLevelSelect()
    {
        // turn on levels menu while setting it up so no null refs
        levelsMenu.SetActive(true);

        // loop through each levelName defined in the editor
        for (int i = 0; i < levelNames.Length; i++)
        {
            // get the level name
            string levelname = levelNames[i];

            // dynamically create a button from the template
            GameObject levelButton = Instantiate(levelButtonPrefab, Vector3.zero, Quaternion.identity);

            // name the game object
            levelButton.name = levelname + " Button";

            // set the parent of the button as the LevelsPanel so it will be dynamically arrange based on the defined layout
            levelButton.transform.SetParent(levelsPanel.transform, false);

            // get the Button script attached to the button
            Button levelButtonScript = levelButton.GetComponent<Button>();

            // setup the listener to loadlevel when clicked
            levelButtonScript.onClick.RemoveAllListeners();
            levelButtonScript.onClick.AddListener(() => LoadLevel(levelname));

            // set the label of the button
            Text levelButtonLabel = levelButton.GetComponentInChildren<Text>();
            levelButtonLabel.text = levelname;

            // determine if the button should be interactable based on if the level is unlocked
            levelButtonScript.interactable = PlayerPrefManager.LevelIsUnlocked(levelname);
        }
    }

    // determine if the QUIT button should be present based on what platform the game is running on
    private void DisplayQuitWhenAppropriate()
    {
        switch (Application.platform)
        {
            // platforms that should have quit button
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.LinuxPlayer:
                quitButton.SetActive(true);
                break;

            // platforms that should not have quit button
            // note: included just for demonstration purposed since
            // default will cover all of these. 
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXEditor:
                quitButton.SetActive(true);
                break;

            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.WebGLPlayer:
                quitButton.SetActive(false);
                break;

            // all other platforms default to no quit button
            default:
                quitButton.SetActive(false);
                break;
        }
    }

    // Public functions below that are available via the UI Event Triggers, such as on Buttons.

    // Show the proper menu
    public void ShowMenu(string menuName)
    {
        // turn all menus off
        mainMenu.SetActive(false);
        aboutMenu.SetActive(false);
        levelsMenu.SetActive(false);

        // turn on desired menu and set default selected button for controller input
        switch (menuName)
        {
            case "MainMenu":
                mainMenu.SetActive(true);
                EventSystem.current.SetSelectedGameObject(menuDefaultButton);
                titleText.text = _mainTitle;
                break;
            case "LevelSelect":
                levelsMenu.SetActive(true);
                EventSystem.current.SetSelectedGameObject(levelSelectDefaultButton);
                titleText.text = "Level Select";
                break;
            case "About":
                aboutMenu.SetActive(true);
                EventSystem.current.SetSelectedGameObject(aboutDefaultButton);
                titleText.text = "About";
                break;
        }
    }

    // load the specified Unity level
    public void LoadLevel(string levelToLoad)
    {
        // start new game so initialize player state
        PlayerPrefManager.ResetPlayerState(startLives, false);

        // load the specified level
        SceneManager.LoadScene(levelToLoad);
    }

    // quit the game
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}