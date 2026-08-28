using UnityEngine;

public class Door : MonoBehaviour
{
    // =========================================================
    // SISTEMA
    // =========================================================

    [Header("Sistema")]
    public DoorNoise doorNoise;


    // =========================================================
    // PORTAS
    // =========================================================

    [Header("Folhas da Porta")]
    public Transform leftDoor;

    public Transform rightDoor;


    // =========================================================
    // CONFIGURAÇÃO
    // =========================================================

    [Header("Abertura")]
    public float openDistance = 2f;

    public float openSpeed = 0.5f;


    // =========================================================
    // POSIÇÕES
    // =========================================================

    private Vector3 leftOpenPosition;

    private Vector3 rightOpenPosition;


    // =========================================================
    // ESTADO
    // =========================================================

    private bool opening = false;

    private bool opened = false;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (leftDoor != null)
        {
            Vector3 startPosition =
                leftDoor.localPosition;


            // Porta esquerda vai para frente.

            leftOpenPosition =
                startPosition +
                Vector3.forward *
                openDistance;
        }


        if (rightDoor != null)
        {
            Vector3 startPosition =
                rightDoor.localPosition;


            // Porta direita vai para trás.

            rightOpenPosition =
                startPosition +
                Vector3.back *
                openDistance;
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (opened)
            return;


        // Espera a sirene terminar.

        if (!opening)
        {
            if (doorNoise != null &&
                doorNoise.CanDoorOpen())
            {
                opening = true;
            }
        }


        // Abre.

        if (opening)
        {
            OpenDoor();
        }
    }


    // =========================================================
    // ABRIR PORTA
    // =========================================================

    private void OpenDoor()
    {
        bool leftFinished = true;

        bool rightFinished = true;


        // =====================================================
        // PORTA ESQUERDA
        // =====================================================

        if (leftDoor != null)
        {
            leftDoor.localPosition =
                Vector3.MoveTowards(
                    leftDoor.localPosition,
                    leftOpenPosition,
                    openSpeed *
                    Time.deltaTime
                );


            leftFinished =
                Vector3.Distance(
                    leftDoor.localPosition,
                    leftOpenPosition
                ) < 0.01f;
        }


        // =====================================================
        // PORTA DIREITA
        // =====================================================

        if (rightDoor != null)
        {
            rightDoor.localPosition =
                Vector3.MoveTowards(
                    rightDoor.localPosition,
                    rightOpenPosition,
                    openSpeed *
                    Time.deltaTime
                );


            rightFinished =
                Vector3.Distance(
                    rightDoor.localPosition,
                    rightOpenPosition
                ) < 0.01f;
        }


        // =====================================================
        // TERMINOU
        // =====================================================

        if (leftFinished &&
            rightFinished)
        {
            opened = true;


            Debug.Log(
                "PORTA ABERTA!"
            );
        }
    }
}