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


    // =========================================================
    // INTERAÇÃO
    // =========================================================

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


        if (distance > interactionDistance)
        {
            return false;
        }


        // Gerador já ligado.

        if (generatorOn)
        {
            return true;
        }


        // Descobre o que o jogador está segurando.

        PickupObject heldObject =
            player.GetHeldObject();


        if (heldObject == null)
        {
            return false;
        }


        // =====================================================
        // GASOLINA
        // =====================================================

        if (heldObject.IsGasoline())
        {
            InsertGasoline(
                player
            );

            return true;
        }


        // =====================================================
        // FUSÍVEL
        // =====================================================

        if (heldObject.IsFuse())
        {
            InsertFuse(
                player
            );

            return true;
        }


        return false;
    }


    // =========================================================
    // COLOCAR GASOLINA
    // =========================================================

    private void InsertGasoline(
        PlayerPickup player
    )
    {
        if (gasolineInserted >= gasolineRequired)
        {
            return;
        }


        gasolineInserted++;


        // Remove gasolina da mão.

        player.ConsumeHeldObject();


        // Mostra vídeo 1/2 ou 2/2.

        if (generatorUI != null)
        {
            generatorUI.ShowGasolineInserted(
                gasolineInserted
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
    // COLOCAR FUSÍVEL
    // =========================================================

    private void InsertFuse(
        PlayerPickup player
    )
    {
        if (fuseInserted)
        {
            return;
        }


        fuseInserted = true;


        // Remove da mão.

        player.ConsumeHeldObject();


        // Mostra vídeo 1/1.

        if (generatorUI != null)
        {
            generatorUI.ShowFuseInserted();
        }


        Debug.Log(
            "Fusível colocado."
        );


        CheckGeneratorReady();
    }


    // =========================================================
    // VERIFICAR SE ESTÁ COMPLETO
    // =========================================================

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
            generatorReady =
                true;


            Debug.Log(
                "TODOS OS ITENS FORAM COLOCADOS!"
            );


            // Liga automaticamente.
            // O GeneratorUI vai esperar o vídeo
            // do último item terminar.

            TurnOnGenerator();
        }
    }


    // =========================================================
    // LIGAR GERADOR
    // =========================================================

    private void TurnOnGenerator()
    {
        if (generatorOn)
            return;


        if (!generatorReady)
            return;


        generatorOn =
            true;


        generatorReady =
            false;


        Debug.Log(
            "GERADOR LIGADO!"
        );


        if (generatorUI != null)
        {
            generatorUI.StartGeneratorOnSequence();
        }
    }


    // =========================================================
    // ESTADOS PÚBLICOS
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