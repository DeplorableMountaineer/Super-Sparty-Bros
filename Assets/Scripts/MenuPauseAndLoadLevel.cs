using UnityEngine;
using UnityEngine.SceneManagement;

// include so we can load new scenes

public class MenuPauseAndLoadLevel : MonoBehaviour
{
    public string levelToLoad;
    public float delay = 2f;

    // use invoke to wait for a delay then call LoadLevel
    private void Update()
    {
        Invoke("LoadLevel", delay);
    }

    // load the specified level
    private void LoadLevel()
    {
        SceneManager.LoadScene(levelToLoad);
    }
}