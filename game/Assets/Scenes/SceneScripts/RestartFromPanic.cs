using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartFromPanic : MonoBehaviour
{
    public void RestartFromPanicToMenu()
    {
        SceneManager.LoadScene("MainMenu");
        MQTTManager.Instance.Publish("game/controller", "PANICoff");
    }
}
