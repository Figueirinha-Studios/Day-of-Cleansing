
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

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

    [Header("Gerador")]
    public float generatorInteractionDistance = 3f;

    [Header("Proteção ao soltar/arremessar")]
    [Tooltip("Tempo em que o objeto ignora colisões com o jogador após ser solto/arremessado.")]
    public float playerCollisionIgnoreTime = 0.15f;

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

        // PRIORIDADE 1: CHUTAR O INIMIGO.
        // Se o chute estiver disponível, não solta o item.
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (SecondEnemyKick.TryKickForPlayer(
                playerCamera,
                transform))
            {
                HidePickupPrompt();
                return;
            }
        }

        if (currentObject == null)
        {
            CheckForPickup();
        }
        else
        {
            CheckForGeneratorInteraction();

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (TryInteractWithGenerator())
                {
                    return;
                }

                HidePickupPrompt();
                DropObject();
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (currentObject.IsGeneratorItem())
                    return;

                ThrowObject();
            }
        }
    }

    private void FixedUpdate()
    {
        if (currentObject != null)
            HoldObject();
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
        if (playerCamera == null)
        {
            HidePickupPrompt();
            return;
        }

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

            float distance = Vector3.Distance(
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
            // PRIORIDADE DO HUD:
            // Se houver um inimigo disponível para chute,
            // esconde o HUD do item para evitar sobreposição.
            if (SecondEnemyKick.TryKickForPlayer(
                playerCamera,
                transform,
                false))
            {
                HidePickupPrompt();
                return;
            }

            ShowPickupPrompt();

            if (Input.GetKeyDown(KeyCode.E))
                Pickup(closestPickup);

            return;
        }

        HidePickupPrompt();
    }

    private void ShowPickupPrompt()
    {
        if (showText && pickupText != null)
            pickupText.gameObject.SetActive(true);

        if (showImage && pickupImage != null)
            pickupImage.gameObject.SetActive(true);
    }

    private void HidePickupPrompt()
    {
        if (pickupText != null)
            pickupText.gameObject.SetActive(false);

        if (pickupImage != null)
            pickupImage.gameObject.SetActive(false);
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

        NoiseSource noiseSource =
            currentObject.GetComponent<NoiseSource>();

        if (noiseSource != null)
            noiseSource.ResetNoise();

        BreakableObject breakable =
            currentObject.GetComponent<BreakableObject>();

        if (breakable != null)
            breakable.DisableBreakOnThrow();

        HidePickupPrompt();

        Rigidbody rb = currentObject.rb;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        Collider[] objectColliders =
            currentObject.GetComponentsInChildren<Collider>();

        foreach (Collider col in objectColliders)
            col.enabled = false;

        currentObject.transform.SetParent(null);

        Vector3 startPosition = holdPoint.TransformPoint(
            currentObject.holdPosition
        );

        Quaternion startRotation = holdPoint.rotation *
            Quaternion.Euler(currentObject.holdRotation);

        rb.position = startPosition;
        rb.rotation = startRotation;
    }

    private void HoldObject()
    {
        if (currentObject == null)
            return;

        Rigidbody rb = currentObject.rb;

        if (rb == null)
            return;

        Vector3 targetPosition = holdPoint.TransformPoint(
            currentObject.holdPosition
        );

        Quaternion targetRotation = holdPoint.rotation *
            Quaternion.Euler(currentObject.holdRotation);

        Vector3 newPosition = Vector3.Lerp(
            rb.position,
            targetPosition,
            holdPositionSpeed * Time.fixedDeltaTime
        );

        Quaternion newRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            holdRotationSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);
        rb.MoveRotation(newRotation);
    }

    private void CheckForGeneratorInteraction()
    {
        if (currentObject == null)
        {
            HidePickupPrompt();
            return;
        }

        if (!currentObject.IsGeneratorItem())
        {
            HidePickupPrompt();
            return;
        }

        if (playerCamera == null)
        {
            HidePickupPrompt();
            return;
        }

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        RaycastHit hit;

        if (Physics.Raycast(
            ray,
            out hit,
            generatorInteractionDistance))
        {
            Generator generator =
                hit.collider.GetComponentInParent<Generator>();

            if (generator != null)
            {
                if (generator.IsGeneratorOn())
                {
                    HidePickupPrompt();
                    return;
                }

                ShowPickupPrompt();
                return;
            }
        }

        HidePickupPrompt();
    }

    private bool TryInteractWithGenerator()
    {
        if (currentObject == null || playerCamera == null)
            return false;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        RaycastHit hit;

        if (!Physics.Raycast(
            ray,
            out hit,
            generatorInteractionDistance))
        {
            return false;
        }

        Generator generator =
            hit.collider.GetComponentInParent<Generator>();

        if (generator == null)
            return false;

        return generator.TryInteract(this);
    }

    public PickupObject GetHeldObject()
    {
        return currentObject;
    }

    public PickupObject ConsumeHeldObject()
    {
        if (currentObject == null)
            return null;

        PickupObject objectToConsume = currentObject;

        Collider[] objectColliders =
            objectToConsume.GetComponentsInChildren<Collider>();

        foreach (Collider col in objectColliders)
            col.enabled = false;

        currentObject = null;

        Destroy(objectToConsume.gameObject);

        return objectToConsume;
    }

    private void IgnorePlayerCollisionTemporarily(
        PickupObject pickupObject)
    {
        if (pickupObject == null)
            return;

        Collider[] objectColliders =
            pickupObject.GetComponentsInChildren<Collider>();

        Collider[] playerColliders =
            GetComponentsInChildren<Collider>();

        foreach (Collider objectCollider in objectColliders)
        {
            if (objectCollider == null)
                continue;

            foreach (Collider playerCollider in playerColliders)
            {
                if (playerCollider == null ||
                    objectCollider == playerCollider)
                {
                    continue;
                }

                Physics.IgnoreCollision(
                    objectCollider,
                    playerCollider,
                    true
                );
            }
        }

        StartCoroutine(
            RestorePlayerCollision(
                pickupObject,
                objectColliders,
                playerColliders
            )
        );
    }

    private IEnumerator RestorePlayerCollision(
        PickupObject pickupObject,
        Collider[] objectColliders,
        Collider[] playerColliders)
    {
        yield return new WaitForSeconds(
            playerCollisionIgnoreTime
        );

        if (pickupObject == null)
            yield break;

        foreach (Collider objectCollider in objectColliders)
        {
            if (objectCollider == null)
                continue;

            foreach (Collider playerCollider in playerColliders)
            {
                if (playerCollider == null ||
                    objectCollider == playerCollider)
                {
                    continue;
                }

                Physics.IgnoreCollision(
                    objectCollider,
                    playerCollider,
                    false
                );
            }
        }
    }

    private void DropObject()
    {
        if (currentObject == null)
            return;

        PickupObject objectToDrop = currentObject;
        Rigidbody rb = objectToDrop.rb;

        if (rb == null)
        {
            currentObject = null;
            return;
        }

        NoiseSource noiseSource =
            objectToDrop.GetComponent<NoiseSource>();

        if (noiseSource != null)
            noiseSource.EnableDropNoise();

        BreakableObject breakable =
            objectToDrop.GetComponent<BreakableObject>();

        if (breakable != null)
            breakable.DisableBreakOnThrow();

        objectToDrop.transform.SetParent(null);

        Collider[] objectColliders =
            objectToDrop.GetComponentsInChildren<Collider>();

        foreach (Collider col in objectColliders)
            col.enabled = true;

        IgnorePlayerCollisionTemporarily(objectToDrop);

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        currentObject = null;
    }

    private void ThrowObject()
    {
        if (currentObject == null)
            return;

        PickupObject objectToThrow = currentObject;
        Rigidbody rb = objectToThrow.rb;

        if (rb == null)
        {
            currentObject = null;
            return;
        }

        Vector3 throwDirection =
            playerCamera.transform.forward;

        throwDirection += Vector3.up * throwUpForce;
        throwDirection.Normalize();

        NoiseSource noiseSource =
            objectToThrow.GetComponent<NoiseSource>();

        if (noiseSource != null)
            noiseSource.EnableNoise();

        BreakableObject breakable =
            objectToThrow.GetComponent<BreakableObject>();

        if (breakable != null)
        {
            breakable.SetThrowDirection(throwDirection);
            breakable.EnableBreakOnThrow();
        }

        objectToThrow.transform.SetParent(null);

        Collider[] objectColliders =
            objectToThrow.GetComponentsInChildren<Collider>();

        foreach (Collider col in objectColliders)
            col.enabled = true;

        IgnorePlayerCollisionTemporarily(objectToThrow);

        rb.isKinematic = false;
        rb.useGravity = true;

        float finalForce =
            throwForce * objectToThrow.throwMultiplier;

        rb.AddForce(
            throwDirection * finalForce,
            ForceMode.Impulse
        );

        currentObject = null;
    }
}