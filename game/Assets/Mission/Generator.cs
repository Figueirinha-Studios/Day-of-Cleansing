using UnityEngine;

public class Generator : MonoBehaviour
{
    [Header("UI")]
    public GeneratorUI generatorUI;


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


        /*
         * GASOLINA
         */
        if (heldObject.IsGasoline())
        {
            InsertGasoline(player);

            return true;
        }


        /*
         * FUSÍVEL
         */
        if (heldObject.IsFuse())
        {
            InsertFuse(player);

            return true;
        }


        /*
         * Não é um item do gerador.
         */
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


        /*
         * Remove o objeto que está
         * na mão do player.
         */
        player.ConsumeHeldObject();


        if (generatorUI != null)
        {
            generatorUI.ShowGasolineInserted(
                gasolineInserted,
                gasolineInserted >= gasolineRequired &&
                (!fuseRequired || fuseInserted)
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


        /*
         * Remove o fusível da mão.
         */
        player.ConsumeHeldObject();


        if (generatorUI != null)
        {
            generatorUI.ShowFuseInserted(
                gasolineInserted >= gasolineRequired &&
                (!fuseRequired || fuseInserted)
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


            if (generatorUI != null)
            {
                generatorUI.StartGeneratorOnSequence();
            }
        }
    }


    // ============================================================
    // FALTA EXATAMENTE 1 ITEM
    // ============================================================

    public bool IsExactlyOneItemMissing()
    {
        /*
         * Quantidade total necessária.
         *
         * Exemplo:
         * 2 gasolinas + 1 fusível = 3.
         */
        int totalRequired =
            gasolineRequired;


        if (fuseRequired)
        {
            totalRequired++;
        }


        /*
         * Quantidade já colocada.
         */
        int totalInserted =
            gasolineInserted;


        if (fuseInserted)
        {
            totalInserted++;
        }


        /*
         * Quantos faltam.
         */
        int itemsMissing =
            totalRequired -
            totalInserted;


        /*
         * SOMENTE retorna true quando
         * falta exatamente UM.
         */
        return itemsMissing == 1;
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


    // ============================================================
    // COMPATIBILIDADE
    // ============================================================

    public void LastItemVideoFinished()
    {
        // Mantido para compatibilidade
        // com o GeneratorUI.
    }
}