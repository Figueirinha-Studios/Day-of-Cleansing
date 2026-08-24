using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerPickup : MonoBehaviour
{
    [Header("Referências")]
    public Camera playerCamera;
    public Transform holdPoint;

    [Header("UI - Interação")]
    public TextMeshProUGUI pickupText;
    public Image pickupImage;

    [Tooltip("Mostrar o texto [E]")]
    public bool showText = true;

    [Tooltip("Mostrar a imagem")]
    public bool showImage = false;

    [Header("Pickup")]
    public float pickupDistance = 3f;

    [Tooltip("Margem de tolerância para mirar no objeto.")]
    public float pickupAimRadius = 0.25f;

    [Header("Objeto na mão")]
    public float holdPositionSpeed = 25f;
    public float holdRotationSpeed = 20f;

    [Header("Arremesso")]
    public float throwForce = 8f;
    public float throwUpForce = 0.15f;

    private PickupObject currentObject;
    private CharacterController characterController;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        characterController =
            GetComponent<CharacterController>();

        HidePickupPrompt();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!IsFirstPerson())
        {
            HidePickupPrompt();
            return;
        }

        // ==========================================
        // NENHUM OBJETO SENDO SEGURADO
        // ==========================================

        if (currentObject == null)
        {
            CheckForPickup();
        }

        // ==========================================
        // OBJETO SENDO SEGURADO
        // ==========================================

        else
        {
            HidePickupPrompt();

            // E = soltar
            if (Input.GetKeyDown(KeyCode.E))
            {
                DropObject();
                return;
            }

            // Botão esquerdo = arremessar
            if (Input.GetMouseButtonDown(0))
            {
                ThrowObject();
            }
        }
    }


    // =========================================================
    // FÍSICA - OBJETO NA MÃO
    // =========================================================

    private void FixedUpdate()
    {
        if (currentObject != null)
        {
            HoldObject();
        }
    }


    // =========================================================
    // PRIMEIRA PESSOA
    // =========================================================

    private bool IsFirstPerson()
    {
        CameraController cameraController =
            GetComponent<CameraController>();

        if (cameraController == null)
            return true;

        return cameraController.IsFirstPerson();
    }


    // =========================================================
    // PROCURAR OBJETO
    // =========================================================

    private void CheckForPickup()
    {
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        RaycastHit[] hits =
            Physics.SphereCastAll(
                ray,
                pickupAimRadius,
                pickupDistance
            );

        PickupObject closestPickup = null;

        float closestDistance =
            Mathf.Infinity;

        foreach (RaycastHit hit in hits)
        {
            PickupObject pickup =
                hit.collider
                    .GetComponentInParent<PickupObject>();

            if (pickup == null)
                continue;

            float distance =
                Vector3.Distance(
                    playerCamera.transform.position,
                    hit.point
                );

            if (distance < closestDistance)
            {
                closestDistance =
                    distance;

                closestPickup =
                    pickup;
            }
        }

        if (closestPickup != null)
        {
            ShowPickupPrompt();

            if (Input.GetKeyDown(KeyCode.E))
            {
                Pickup(closestPickup);
            }

            return;
        }

        HidePickupPrompt();
    }


    // =========================================================
    // MOSTRAR [E]
    // =========================================================

    private void ShowPickupPrompt()
    {
        if (showText &&
            pickupText != null)
        {
            pickupText.gameObject.SetActive(true);
        }

        if (showImage &&
            pickupImage != null)
        {
            pickupImage.gameObject.SetActive(true);
        }
    }


    // =========================================================
    // ESCONDER [E]
    // =========================================================

    private void HidePickupPrompt()
    {
        if (pickupText != null)
        {
            pickupText.gameObject.SetActive(false);
        }

        if (pickupImage != null)
        {
            pickupImage.gameObject.SetActive(false);
        }
    }


    // =========================================================
    // PEGAR OBJETO
    // =========================================================

    private void Pickup(PickupObject pickup)
    {
        if (pickup == null)
            return;

        if (pickup.rb == null)
        {
            Debug.LogWarning(
                "O objeto não possui Rigidbody.",
                pickup.gameObject
            );

            return;
        }

        currentObject = pickup;


        // =====================================================
        // NOISE
        // =====================================================

        NoiseSource noiseSource =
            currentObject
                .GetComponent<NoiseSource>();

        if (noiseSource != null)
        {
            noiseSource.ResetNoise();
        }


        // =====================================================
        // QUEBRA
        // =====================================================

        BreakableObject breakable =
            currentObject
                .GetComponent<BreakableObject>();

        if (breakable != null)
        {
            breakable.DisableBreakOnThrow();
        }


        HidePickupPrompt();

        Rigidbody rb =
            currentObject.rb;


        // =====================================================
        // PARA A FÍSICA
        // =====================================================

        rb.linearVelocity =
            Vector3.zero;

        rb.angularVelocity =
            Vector3.zero;

        rb.isKinematic =
            true;

        rb.useGravity =
            false;


        // =====================================================
        // DESATIVA COLISÃO ENQUANTO SEGURA
        // =====================================================

        Collider[] objectColliders =
            currentObject.GetComponentsInChildren<Collider>();

        foreach (Collider col in objectColliders)
        {
            col.enabled = false;
        }


        // =====================================================
        // NÃO COLOCA COMO FILHO DA CÂMERA
        // =====================================================

        currentObject
            .transform
            .SetParent(null);


        // =====================================================
        // POSIÇÃO INICIAL
        // =====================================================

        Vector3 startPosition =
            holdPoint.TransformPoint(
                currentObject.holdPosition
            );

        Quaternion startRotation =
            holdPoint.rotation *
            Quaternion.Euler(
                currentObject.holdRotation
            );

        rb.position =
            startPosition;

        rb.rotation =
            startRotation;
    }


    // =========================================================
    // SEGURAR OBJETO
    // =========================================================

    private void HoldObject()
    {
        if (currentObject == null)
            return;

        Rigidbody rb =
            currentObject.rb;

        if (rb == null)
            return;


        // =====================================================
        // POSIÇÃO DESEJADA
        // =====================================================

        Vector3 targetPosition =
            holdPoint.TransformPoint(
                currentObject.holdPosition
            );


        // =====================================================
        // ROTAÇÃO DESEJADA
        // =====================================================

        Quaternion targetRotation =
            holdPoint.rotation *
            Quaternion.Euler(
                currentObject.holdRotation
            );


        // =====================================================
        // MOVIMENTO SUAVE
        // =====================================================

        Vector3 newPosition =
            Vector3.Lerp(
                rb.position,
                targetPosition,
                holdPositionSpeed *
                Time.fixedDeltaTime
            );

        Quaternion newRotation =
            Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                holdRotationSpeed *
                Time.fixedDeltaTime
            );


        // =====================================================
        // MOVE RIGIDBODY
        // =====================================================

        rb.MovePosition(
            newPosition
        );

        rb.MoveRotation(
            newRotation
        );
    }


    // =========================================================
    // SOLTAR COM E
    // =========================================================

    private void DropObject()
    {
        if (currentObject == null)
            return;

        PickupObject objectToDrop =
            currentObject;

        Rigidbody rb =
            objectToDrop.rb;


        // =====================================================
        // SOM
        // =====================================================

        NoiseSource noiseSource =
            objectToDrop
                .GetComponent<NoiseSource>();

        if (noiseSource != null)
        {
            noiseSource.EnableDropNoise();
        }


        // =====================================================
        // NÃO QUEBRA AO SOLTAR
        // =====================================================

        BreakableObject breakable =
            objectToDrop
                .GetComponent<BreakableObject>();

        if (breakable != null)
        {
            breakable.DisableBreakOnThrow();
        }


        // =====================================================
        // REMOVE PARENT
        // =====================================================

        objectToDrop
            .transform
            .SetParent(null);


        // =====================================================
        // REATIVA COLISÃO
        // =====================================================

        Collider[] objectColliders =
            objectToDrop.GetComponentsInChildren<Collider>();

        foreach (Collider col in objectColliders)
        {
            col.enabled = true;
        }


        // =====================================================
        // REATIVA FÍSICA
        // =====================================================

        rb.isKinematic =
            false;

        rb.useGravity =
            true;

        rb.linearVelocity =
            Vector3.zero;

        rb.angularVelocity =
            Vector3.zero;


        currentObject = null;
    }


    // =========================================================
    // ARREMESSAR
    // =========================================================

    private void ThrowObject()
    {
        if (currentObject == null)
            return;

        PickupObject objectToThrow =
            currentObject;

        Rigidbody rb =
            objectToThrow.rb;


        // =====================================================
        // DIREÇÃO DO ARREMESSO
        // =====================================================

        Vector3 throwDirection =
            playerCamera.transform.forward;

        throwDirection +=
            Vector3.up *
            throwUpForce;

        throwDirection.Normalize();


        // =====================================================
        // SOM
        // =====================================================

        NoiseSource noiseSource =
            objectToThrow
                .GetComponent<NoiseSource>();

        if (noiseSource != null)
        {
            noiseSource.EnableNoise();
        }


        // =====================================================
        // ATIVA QUEBRA
        // =====================================================

        BreakableObject breakable =
            objectToThrow
                .GetComponent<BreakableObject>();

        if (breakable != null)
        {
            // Passa a direção do arremesso para a quebra.
            breakable.SetThrowDirection(
                throwDirection
            );

            breakable.EnableBreakOnThrow();
        }


        // =====================================================
        // REMOVE DA MÃO
        // =====================================================

        objectToThrow
            .transform
            .SetParent(null);


        // =====================================================
        // REATIVA COLISÃO
        // =====================================================

        Collider[] objectColliders =
            objectToThrow.GetComponentsInChildren<Collider>();

        foreach (Collider col in objectColliders)
        {
            col.enabled = true;
        }


        // =====================================================
        // REATIVA FÍSICA
        // =====================================================

        rb.isKinematic =
            false;

        rb.useGravity =
            true;


        // =====================================================
        // FORÇA FINAL
        // =====================================================

        float finalForce =
            throwForce *
            objectToThrow.throwMultiplier;


        // =====================================================
        // ARREMESSO
        // =====================================================

        rb.AddForce(
            throwDirection *
            finalForce,
            ForceMode.Impulse
        );

        currentObject = null;
    }
}