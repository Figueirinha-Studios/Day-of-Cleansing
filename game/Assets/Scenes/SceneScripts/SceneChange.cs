using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public Transform Enemy;
    public Transform Player;

    public float distanciaMinima = 2f;
    private bool mudouCena = false;
    void Update()
    {
        if (mudouCena) return;

        float distancia = Vector3.Distance(Enemy.position, Player.position);

        if (distancia <= distanciaMinima)
        {
            mudouCena = true;
            SceneManager.LoadScene("GameOver");
            
        }
    }
}