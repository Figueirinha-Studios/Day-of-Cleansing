using System.Collections;
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
        {
            return false;
        }


        // =====================================================
        // GERADOR JÁ LIGADO
        // =====================================================

        if (generatorOn)
        {
            return true;
        }


        // =====================================================
        // DESCOBRE O QUE O PLAYER ESTÁ SEGURANDO
        // =====================================================

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
    // COLOCAR GASOLINA
    // =========================================================

    private void InsertGasoline(PlayerPickup player)
    {
        if (gasolineInserted >= gasolineRequired)
        {
            return;
        }


        // =====================================================
        // AUMENTA CONTADOR
        // =====================================================

        gasolineInserted++;


        // =====================================================
        // REMOVE GASOLINA DA MÃO
        // =====================================================

        player.ConsumeHeldObject();


        // =====================================================
        // MOSTRA VÍDEO
        // =====================================================

        if (generatorUI != null)
        {
            generatorUI.ShowGasolineInserted(
                gasolineInserted
            );
        }
        else
        {
            Debug.LogWarning(
                "Generator: GeneratorUI não está configurado!"
            );
        }


        Debug.Log(
            "Gasolina colocada: " +
            gasolineInserted +
            "/" +
            gasolineRequired
        );


        // =====================================================
        // VERIFICA SE COMPLETOU
        // =====================================================

        CheckGeneratorReady();
    }


    // =========================================================
    // COLOCAR FUSÍVEL
    // =========================================================

    private void InsertFuse(PlayerPickup player)
    {
        if (fuseInserted)
        {
            return;
        }


        // =====================================================
        // MARCA FUSÍVEL
        // =====================================================

        fuseInserted = true;


        // =====================================================
        // REMOVE FUSÍVEL DA MÃO
        // =====================================================

        player.ConsumeHeldObject();


        // =====================================================
        // MOSTRA VÍDEO
        // =====================================================

        if (generatorUI != null)
        {
            generatorUI.ShowFuseInserted();
        }
        else
        {
            Debug.LogWarning(
                "Generator: GeneratorUI não está configurado!"
            );
        }


        Debug.Log(
            "Fusível colocado."
        );


        // =====================================================
        // VERIFICA SE COMPLETOU
        // =====================================================

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


        if (!gasolineComplete)
        {
            return;
        }


        if (!fuseComplete)
        {
            return;
        }


        // =====================================================
        // TUDO COMPLETO
        // =====================================================

        if (!generatorReady)
        {
            generatorReady =
                true;


            Debug.Log(
                "========================================"
            );

            Debug.Log(
                "TODOS OS ITENS FORAM COLOCADOS!"
            );

            Debug.Log(
                "ESPERANDO O ÚLTIMO VÍDEO TERMINAR..."
            );

            Debug.Log(
                "========================================"
            );


            // =================================================
            // ESPERA O VÍDEO TERMINAR
            // =================================================

            StartCoroutine(
                WaitForLastItemVideo()
            );
        }
    }


    // =========================================================
    // ESPERAR ÚLTIMO VÍDEO
    // =========================================================

    private IEnumerator WaitForLastItemVideo()
    {
        // =====================================================
        // ESPERA O VÍDEO DO ÚLTIMO ITEM TERMINAR
        // =====================================================

        if (generatorUI != null)
        {
            while (!generatorUI.IsItemVideoFinished())
            {
                yield return null;
            }
        }


        // =====================================================
        // AGORA PODE LIGAR O GERADOR
        // =====================================================

        TurnOnGenerator();
    }


    // =========================================================
    // LIGAR GERADOR
    // =========================================================

    private void TurnOnGenerator()
    {
        if (generatorOn)
        {
            return;
        }


        if (!generatorReady)
        {
            return;
        }


        // =====================================================
        // MARCA COMO LIGADO
        // =====================================================

        generatorOn =
            true;


        generatorReady =
            false;


        Debug.Log(
            "========================================"
        );

        Debug.Log(
            "GERADOR LIGADO!"
        );

        Debug.Log(
            "========================================"
        );


        // =====================================================
        // INICIA SEQUÊNCIA FINAL
        // =====================================================

        if (generatorUI != null)
        {
            generatorUI.StartGeneratorOnSequence();
        }
        else
        {
            Debug.LogWarning(
                "Generator: GeneratorUI não está configurado!"
            );
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