using UnityEngine;
using UnityEngine.SceneManagement;

public class VoidDeath : MonoBehaviour
{
    public string nomeDaCena = "GameOver";
    public string tagMorte = "Void";

    private bool mudouCena = false;

    private void OnTriggerEnter(Collider other)
    {
        if (mudouCena) return;

        if (other.CompareTag(tagMorte))
        {
            mudouCena = true;
            SceneManager.LoadScene(nomeDaCena);
        }
    }
}