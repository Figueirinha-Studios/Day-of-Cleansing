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
    public float holdPositionSpeed = 15f;
    public float holdRotationSpeed = 15f;

    [Header("Arremesso")]
    public float throwForce = 8f;
    public float throwUpForce = 0.15f;

    private PickupObject currentObject;
    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        HidePickupPrompt();
    }

    private void Update()
    {
        if (!IsFirstPerson())
        {
            HidePickupPrompt();
            return;
        }

        if (currentObject == null)
        {
            CheckForPickup();
        }
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

    private void LateUpdate()
    {
        if (currentObject != null)
        {
            HoldObject();
        }
    }

    private bool IsFirstPerson()
    {
        CameraController cameraController =
            GetComponent<CameraController>();

        if (cameraController == null)
            return true;

        return cameraController.IsFirstPerson();
    }

    private void CheckForPickup()
    {
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        RaycastHit[] hits = Physics.SphereCastAll(
            ray,
            pickupAimRadius,
            pickupDistance
        );

        PickupObject closestPickup = null;
        float closestDistance = Mathf.Infinity;

        foreach (RaycastHit hit in hits)
        {
            PickupObject pickup =
                hit.collider.GetComponentInParent<PickupObject>();

            if (pickup == null)
                continue;

            float distance =
                Vector3.Distance(
                    playerCamera.transform.position,
                    hit.point
                );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPickup = pickup;
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

    private void ShowPickupPrompt()
    {
        if (showText && pickupText != null)
        {
            pickupText.gameObject.SetActive(true);
        }

        if (showImage && pickupImage != null)
        {
            pickupImage.gameObject.SetActive(true);
        }
    }

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

        // Reseta o sistema de ruído.
        NoiseSource noiseSource =
            currentObject.GetComponent<NoiseSource>();

        if (noiseSource != null)
        {
            noiseSource.ResetNoise();
        }

        // Reseta o sistema de quebra.
        BreakableObject breakable =
            currentObject.GetComponent<BreakableObject>();

        if (breakable != null)
        {
            breakable.DisableBreakOnThrow();
        }

        HidePickupPrompt();

        Rigidbody rb = currentObject.rb;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;
        rb.useGravity = false;

        if (characterController != null)
        {
            Collider objectCollider =
                currentObject.GetComponent<Collider>();

            if (objectCollider != null)
            {
                Physics.IgnoreCollision(
                    characterController,
                    objectCollider,
                    true
                );
            }
        }

        currentObject.transform.SetParent(holdPoint);

        currentObject.transform.localPosition =
            Vector3.zero;

        currentObject.transform.localRotation =
            Quaternion.identity;
    }

    private void HoldObject()
    {
        if (currentObject == null)
            return;

        Transform objectTransform =
            currentObject.transform;

        objectTransform.position = Vector3.Lerp(
            objectTransform.position,
            holdPoint.position,
            holdPositionSpeed * Time.deltaTime
        );

        objectTransform.rotation = Quaternion.Slerp(
            objectTransform.rotation,
            holdPoint.rotation,
            holdRotationSpeed * Time.deltaTime
        );
    }

    private void DropObject()
    {
        if (currentObject == null)
            return;

        PickupObject objectToDrop =
            currentObject;

        Rigidbody rb =
            objectToDrop.rb;

        // -----------------------------
        // SOM DE OBJETO SOLTO
        // -----------------------------

        NoiseSource noiseSource =
            objectToDrop.GetComponent<NoiseSource>();

        if (noiseSource != null)
        {
            noiseSource.EnableDropNoise();
        }

        // -----------------------------
        // NÃO QUEBRAR AO SOLTAR
        // -----------------------------

        BreakableObject breakable =
            objectToDrop.GetComponent<BreakableObject>();

        if (breakable != null)
        {
            breakable.DisableBreakOnThrow();
        }

        // -----------------------------
        // REMOVE DA MÃO
        // -----------------------------

        objectToDrop.transform.SetParent(null);

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (characterController != null)
        {
            Collider objectCollider =
                objectToDrop.GetComponent<Collider>();

            if (objectCollider != null)
            {
                Physics.IgnoreCollision(
                    characterController,
                    objectCollider,
                    false
                );
            }
        }

        currentObject = null;
    }

    private void ThrowObject()
    {
        if (currentObject == null)
            return;

        PickupObject objectToThrow =
            currentObject;

        Rigidbody rb =
            objectToThrow.rb;

        // -----------------------------
        // SOM DE IMPACTO
        // -----------------------------

        NoiseSource noiseSource =
            objectToThrow.GetComponent<NoiseSource>();

        if (noiseSource != null)
        {
            noiseSource.EnableNoise();
        }

        // -----------------------------
        // ATIVA QUEBRA
        // -----------------------------

        BreakableObject breakable =
            objectToThrow.GetComponent<BreakableObject>();

        if (breakable != null)
        {
            breakable.EnableBreakOnThrow();
        }

        // -----------------------------
        // REMOVE DA MÃO
        // -----------------------------

        objectToThrow.transform.SetParent(null);

        rb.isKinematic = false;
        rb.useGravity = true;

        if (characterController != null)
        {
            Collider objectCollider =
                objectToThrow.GetComponent<Collider>();

            if (objectCollider != null)
            {
                Physics.IgnoreCollision(
                    characterController,
                    objectCollider,
                    false
                );
            }
        }

        // -----------------------------
        // DIREÇÃO DO ARREMESSO
        // -----------------------------

        Vector3 throwDirection =
            playerCamera.transform.forward;

        throwDirection +=
            Vector3.up * throwUpForce;

        throwDirection.Normalize();

        float finalForce =
            throwForce *
            objectToThrow.throwMultiplier;

        rb.AddForce(
            throwDirection * finalForce,
            ForceMode.Impulse
        );

        currentObject = null;
    }
}