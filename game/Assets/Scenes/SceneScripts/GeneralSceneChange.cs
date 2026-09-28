using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralSceneChange : MonoBehaviour
{
    [Header("Nome da próxima cena:")]
    [SerializeField] private string SceneName;


    public void SceneChanger()
    {
        SceneManager.LoadScene(SceneName);
    }
}