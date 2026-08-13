using UnityEngine;

public class QuitGame : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Application.Quit();
        }
    }
}