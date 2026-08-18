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
        // Só permite pegar objetos em primeira pessoa.
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
            // Enquanto segura um objeto,
            // não mostra a indicação de pegar.
            HidePickupPrompt();

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

        // Esconde a indicação depois de pegar.
        HidePickupPrompt();

        Rigidbody rb = currentObject.rb;

        // Para completamente o objeto.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Desativa a física.
        rb.isKinematic = true;
        rb.useGravity = false;

        // Evita que o objeto bata no Player enquanto está sendo segurado.
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

        // Coloca o objeto na "mão".
        currentObject.transform.SetParent(holdPoint);

        // Posição e rotação relativas ao HoldPoint.
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

        // Posição suavizada.
        objectTransform.position = Vector3.Lerp(
            objectTransform.position,
            holdPoint.position,
            holdPositionSpeed * Time.deltaTime
        );

        // Rotação suavizada.
        objectTransform.rotation = Quaternion.Slerp(
            objectTransform.rotation,
            holdPoint.rotation,
            holdRotationSpeed * Time.deltaTime
        );
    }

    private void ThrowObject()
    {
        if (currentObject == null)
            return;

        PickupObject objectToThrow =
            currentObject;

        Rigidbody rb =
            objectToThrow.rb;

        // Permite que o objeto produza ruído
        // quando bater em alguma superfície.
        NoiseSource noiseSource =
            objectToThrow.GetComponent<NoiseSource>();

        if (noiseSource != null)
        {
            noiseSource.EnableNoise();
        }

        // Remove da câmera.
        objectToThrow.transform.SetParent(null);

        // Reativa física.
        rb.isKinematic = false;
        rb.useGravity = true;

        // Reativa colisão com o Player.
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

        // Direção do arremesso.
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