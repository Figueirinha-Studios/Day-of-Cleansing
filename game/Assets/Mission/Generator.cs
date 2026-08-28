using UnityEngine;

public class Generator : MonoBehaviour
{
    [Header("UI")]
    public GeneratorUI generatorUI;


    [Header("Itens necessários")]
    public int gasolineRequired = 2;
    public bool fuseRequired = true;


    [Header("Itens colocados")]
    [SerializeField] private int gasolineInserted = 0;
    [SerializeField] private bool fuseInserted = false;


    [Header("Estado")]
    [SerializeField] private bool generatorReady = false;
    [SerializeField] private bool generatorOn = false;


    [Header("Interação")]
    public float interactionDistance = 3f;


    [Header("Objetivo - Primeira Aproximação")]
    public Transform player;

    public bool tutorialAlreadyShown = false;


    // =========================================================
    // CONTROLE
    // =========================================================

    private bool waitingForLastItemVideo = false;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        CheckPlayerApproach();
    }


    // =========================================================
    // APROXIMAÇÃO
    // =========================================================

    private void CheckPlayerApproach()
    {
        if (tutorialAlreadyShown)
            return;

        if (player == null)
            return;

        if (generatorUI == null)
            return;


        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );


        if (distance <= interactionDistance)
        {
            tutorialAlreadyShown = true;

            Debug.Log(
                "Player se aproximou do gerador pela primeira vez."
            );

            generatorUI.ShowGeneratorTutorial();
        }
    }


    // =========================================================
    // INTERAÇÃO
    // =========================================================

    public bool TryInteract(PlayerPickup player)
    {
        if (player == null)
            return false;


        float distance =
            Vector3.Distance(
                transform.position,
                player.transform.position
            );


        if (distance > interactionDistance)
            return false;


        // Gerador já ligado.

        if (generatorOn)
            return true;


        // Está esperando o último vídeo terminar.

        if (waitingForLastItemVideo)
            return true;


        PickupObject heldObject =
            player.GetHeldObject();


        if (heldObject == null)
            return false;


        // =====================================================
        // GASOLINA
        // =====================================================

        if (heldObject.IsGasoline())
        {
            InsertGasoline(player);

            return true;
        }


        // =====================================================
        // FUSÍVEL
        // =====================================================

        if (heldObject.IsFuse())
        {
            InsertFuse(player);

            return true;
        }


        return false;
    }


    // =========================================================
    // GASOLINA
    // =========================================================

    private void InsertGasoline(PlayerPickup player)
    {
        if (gasolineInserted >= gasolineRequired)
            return;


        gasolineInserted++;


        player.ConsumeHeldObject();


        bool isLastItem =
            gasolineInserted >= gasolineRequired &&
            (!fuseRequired || fuseInserted);


        if (generatorUI != null)
        {
            generatorUI.ShowGasolineInserted(
                gasolineInserted,
                isLastItem
            );
        }


        Debug.Log(
            "Gasolina colocada: " +
            gasolineInserted +
            "/" +
            gasolineRequired
        );


        CheckGeneratorReady();
    }


    // =========================================================
    // FUSÍVEL
    // =========================================================

    private void InsertFuse(PlayerPickup player)
    {
        if (fuseInserted)
            return;


        fuseInserted = true;


        player.ConsumeHeldObject();


        bool isLastItem =
            gasolineInserted >= gasolineRequired;


        if (generatorUI != null)
        {
            generatorUI.ShowFuseInserted(
                isLastItem
            );
        }


        Debug.Log(
            "Fusível colocado."
        );


        CheckGeneratorReady();
    }


    // =========================================================
    // VERIFICAR GERADOR
    // =========================================================

    private void CheckGeneratorReady()
    {
        bool gasolineComplete =
            gasolineInserted >= gasolineRequired;


        bool fuseComplete =
            !fuseRequired ||
            fuseInserted;


        if (gasolineComplete &&
            fuseComplete)
        {
            generatorReady = true;

            waitingForLastItemVideo = true;


            Debug.Log(
                "TODOS OS ITENS FORAM COLOCADOS!"
            );

            Debug.Log(
                "Esperando o vídeo do último item terminar..."
            );
        }
    }


    // =========================================================
    // ÚLTIMO VÍDEO TERMINOU
    // =========================================================

    public void LastItemVideoFinished()
    {
        if (!waitingForLastItemVideo)
            return;


        waitingForLastItemVideo = false;


        Debug.Log(
            "Vídeo do último item terminou!"
        );


        StartGeneratorSequence();
    }


    // =========================================================
    // COMEÇAR GENERATOR ON
    // =========================================================

    private void StartGeneratorSequence()
    {
        if (generatorOn)
            return;


        if (!generatorReady)
            return;


        if (generatorUI != null)
        {
            generatorUI.StartGeneratorOnSequence();
        }
        else
        {
            CompleteGeneratorOn();
        }
    }


    // =========================================================
    // GERADOR REALMENTE LIGADO
    // =========================================================

    public void CompleteGeneratorOn()
    {
        if (generatorOn)
            return;


        generatorOn = true;

        generatorReady = false;


        Debug.Log(
            "================================="
        );

        Debug.Log(
            "GERADOR ON!"
        );

        Debug.Log(
            "================================="
        );
    }


    // =========================================================
    // ESTADOS
    // =========================================================

    public bool IsGeneratorReady()
    {
        return generatorReady;
    }


    public bool IsGeneratorOn()
    {
        return generatorOn;
    }


    public int GetGasolineInserted()
    {
        return gasolineInserted;
    }


    public bool IsFuseInserted()
    {
        return fuseInserted;
    }
}