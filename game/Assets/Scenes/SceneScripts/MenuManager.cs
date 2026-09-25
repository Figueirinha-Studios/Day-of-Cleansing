using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        MQTTManager.Instance.Publish("dayofcleansing/controller", "RELEAoff");
        MQTTManager.Instance.Publish("dayofcleansing/controller", "RELEBoff");

        SceneManager.LoadScene("CutScene");
    }
}