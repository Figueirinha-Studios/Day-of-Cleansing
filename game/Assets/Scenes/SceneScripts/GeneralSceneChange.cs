using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralSceneChange : MonoBehaviour
{
    [Header("Nome da próxima cena:")]
    [SerializeField] private string SceneName;
    [SerializeField] private bool Button = true;

    private void Start()
    {
        if (Button == false)
        {
            SceneManager.LoadScene(SceneName);
        }
    }
    public void SceneChanger()
    {
        SceneManager.LoadScene(SceneName);
    }
}