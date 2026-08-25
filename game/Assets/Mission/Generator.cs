using UnityEngine;

public class Generator : MonoBehaviour
{
    [Header("Player")]
    public PlayerPickup playerPickup;

    [Header("Itens necessários")]
    public int gasolineRequired = 2;
    public bool fuseRequired = true;

    [Header("Itens colocados")]
    public int gasolineInserted = 0;
    public bool fuseInserted = false;

    [Header("Estado do gerador")]
    public bool generatorReady = false;
    public bool generatorOn = false;

    [Header("Interação")]
    public float interactionDistance = 3f;


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (playerPickup == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                playerPickup.transform.position
            );

        if (distance > interactionDistance)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }


    // =========================================================
    // INTERAÇÃO
    // =========================================================

    private void Interact()
    {
        if (generatorOn)
            return;

        if (generatorReady)
        {
            Debug.Log("Gerador pronto para ligar.");
            return;
        }

        PickupObject heldObject =
            playerPickup.GetHeldObject();

        if (heldObject == null)
        {
            Debug.Log(
                "Você precisa estar segurando gasolina ou um fusível."
            );

            return;
        }


        // =====================================================
        // GASOLINA
        // =====================================================

        if (heldObject.CompareTag("Gasolina"))
        {
            InsertGasoline();
            return;
        }


        // =====================================================
        // FUSÍVEL
        // =====================================================

        if (heldObject.CompareTag("Fusivel"))
        {
            InsertFuse();
            return;
        }


        // =====================================================
        // OBJETO ERRADO
        // =====================================================

        Debug.Log(
            "Esse objeto não serve para o gerador."
        );
    }


    // =========================================================
    // COLOCAR GASOLINA
    // =========================================================

    private void InsertGasoline()
    {
        if (gasolineInserted >= gasolineRequired)
        {
            Debug.Log(
                "O gerador já recebeu toda a gasolina necessária."
            );

            return;
        }

        gasolineInserted++;

        playerPickup.ConsumeHeldObject();

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

    private void InsertFuse()
    {
        if (fuseInserted)
        {
            Debug.Log(
                "O fusível já foi colocado."
            );

            return;
        }

        fuseInserted = true;

        playerPickup.ConsumeHeldObject();

        Debug.Log(
            "Fusível colocado."
        );

        CheckGeneratorReady();
    }


    // =========================================================
    // VERIFICAR SE ESTÁ PRONTO
    // =========================================================

    private void CheckGeneratorReady()
    {
        if (gasolineInserted >= gasolineRequired &&
            (!fuseRequired || fuseInserted))
        {
            generatorReady = true;

            Debug.Log(
                "GERADOR PRONTO! Agora pode ser ligado."
            );
        }
    }
}