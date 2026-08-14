using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{    public void RestartMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
