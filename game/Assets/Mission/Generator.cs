using UnityEngine;

public class Generator : MonoBehaviour
{
    [Header("UI")]
    public GeneratorUI generatorUI;


    [Header("Jogador")]
    public PlayerPickup player;


    [Header("Tutorial")]
    [Tooltip("O tutorial aparece automaticamente quando o jogador entra nessa distância.")]
    public float tutorialDistance = 5f;

    private bool tutorialShown = false;


    [Header("Itens necessários")]
    public int gasolineRequired = 2;

    public bool fuseRequired = true;


    [Header("Itens colocados")]
    [SerializeField]
    private int gasolineInserted = 0;

    [SerializeField]
    private bool fuseInserted = false;


    [Header("Estado")]
    [SerializeField]
    private bool generatorReady = false;

    [SerializeField]
    private bool generatorOn = false;


    [Header("Interação")]
    public float interactionDistance = 3f;


    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        if (player == null)
        {
            player =
                FindFirstObjectByType<PlayerPickup>();
        }
    }


    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        CheckTutorialDistance();
    }


    // ============================================================
    // TUTORIAL AO SE APROXIMAR
    // ============================================================

    private void CheckTutorialDistance()
    {
        if (tutorialShown)
            return;

        if (player == null)
            return;

        if (generatorUI == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.transform.position
            );

        if (distance <= tutorialDistance)
        {
            tutorialShown = true;

            Debug.Log(
                "GERADOR: Jogador se aproximou. " +
                "Mostrando tutorial."
            );

            generatorUI.ShowGeneratorTutorial();
        }
    }


    // ============================================================
    // INTERAÇÃO
    // ============================================================

    public bool TryInteract(
        PlayerPickup player
    )
    {
        if (player == null)
            return false;


        float distance =
            Vector3.Distance(
                transform.position,
                player.transform.position
            );


        if (distance >
            interactionDistance)
        {
            return false;
        }


        /*
         * Se já estiver ligado,
         * não aceita mais itens.
         */
        if (generatorOn)
            return true;


        PickupObject heldObject =
            player.GetHeldObject();


        if (heldObject == null)
            return false;


        // ========================================================
        // GASOLINA
        // ========================================================

        if (heldObject.IsGasoline())
        {
            InsertGasoline(player);

            return true;
        }


        // ========================================================
        // FUSÍVEL
        // ========================================================

        if (heldObject.IsFuse())
        {
            InsertFuse(player);

            return true;
        }


        return false;
    }


    // ============================================================
    // COLOCAR GASOLINA
    // ============================================================

    private void InsertGasoline(
        PlayerPickup player
    )
    {
        if (gasolineInserted >=
            gasolineRequired)
        {
            return;
        }


        gasolineInserted++;


        player.ConsumeHeldObject();


        if (generatorUI != null)
        {
            /*
             * Verifica se ESTE foi o último item.
             */
            bool isLastItem =
                gasolineInserted >= gasolineRequired &&
                (!fuseRequired || fuseInserted);


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


    // ============================================================
    // COLOCAR FUSÍVEL
    // ============================================================

    private void InsertFuse(
        PlayerPickup player
    )
    {
        if (fuseInserted)
            return;


        fuseInserted = true;


        player.ConsumeHeldObject();


        if (generatorUI != null)
        {
            /*
             * Se o fusível foi o último item.
             */
            bool isLastItem =
                gasolineInserted >= gasolineRequired &&
                (!fuseRequired || fuseInserted);


            generatorUI.ShowFuseInserted(
                isLastItem
            );
        }


        Debug.Log(
            "Fusível colocado."
        );


        CheckGeneratorReady();
    }


    // ============================================================
    // VERIFICAR SE ESTÁ COMPLETO
    // ============================================================

    private void CheckGeneratorReady()
    {
        bool gasolineComplete =
            gasolineInserted >=
            gasolineRequired;


        bool fuseComplete =
            !fuseRequired ||
            fuseInserted;


        if (gasolineComplete &&
            fuseComplete)
        {
            generatorReady = true;


            Debug.Log(
                "TODOS OS ITENS FORAM COLOCADOS!"
            );

            string codigo = AudioCodeManager.Instance.codigo;
            Debug.Log("O código é: " + codigo);
            /*
             * IMPORTANTE:
             *
             * NÃO iniciamos o Generator ON aqui.
             *
             * O último vídeo precisa terminar primeiro.
             *
             * O GeneratorUI vai chamar
             * LastItemVideoFinished()
             * quando o vídeo terminar.
             */
        }
    }


    // ============================================================
    // FALTA EXATAMENTE 1 ITEM
    // ============================================================

    public bool IsExactlyOneItemMissing()
    {
        int totalRequired =
            gasolineRequired;


        if (fuseRequired)
        {
            totalRequired++;
        }


        int totalInserted =
            gasolineInserted;


        if (fuseInserted)
        {
            totalInserted++;
        }


        int itemsMissing =
            totalRequired -
            totalInserted;


        /*
         * TRUE somente quando falta
         * exatamente UM item.
         *
         * 2 gasolinas + 1 fusível:
         *
         * 0 itens = false
         * 1 item  = false
         * 2 itens = TRUE
         * 3 itens = false
         */
        return itemsMissing == 1;
    }


    // ============================================================
    // ÚLTIMO VÍDEO TERMINOU
    // ============================================================

    public void LastItemVideoFinished()
    {
        /*
         * Segurança:
         * só inicia se todos os itens realmente
         * tiverem sido colocados.
         */
        if (!generatorReady)
            return;


        if (generatorOn)
            return;


        Debug.Log(
            "GERADOR: Último vídeo terminou. " +
            "Iniciando sequência Generator ON."
        );


        if (generatorUI != null)
        {
            generatorUI.StartGeneratorOnSequence();
        }
    }


    // ============================================================
    // GERADOR LIGADO
    // ============================================================

    public void CompleteGeneratorOn()
    {
        if (!generatorReady)
            return;


        if (generatorOn)
            return;


        generatorOn = true;


        generatorReady = false;


        Debug.Log(
            "GERADOR LIGADO!"
        );
    }


    // ============================================================
    // GETTERS
    // ============================================================

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