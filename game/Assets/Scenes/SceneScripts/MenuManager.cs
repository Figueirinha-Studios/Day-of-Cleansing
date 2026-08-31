using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene("SampleScene");
        MQTTManager.Instance.Publish("game/controller", "RELEAoff");
        MQTTManager.Instance.Publish("game/controller", "RELEBoff");
    }
}