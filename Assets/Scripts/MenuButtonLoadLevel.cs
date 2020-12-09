using UnityEngine;
using UnityEngine.SceneManagement;

// include so we can load new scenes

public class MenuButtonLoadLevel : MonoBehaviour
{
    public void LoadLevel(string levelToLoad)
    {
        SceneManager.LoadScene(levelToLoad);
    }
}