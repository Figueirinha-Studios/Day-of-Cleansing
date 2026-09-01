using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    [Header("Referências")]
    public PlayerMovement playerMovement;
    public Image staminaFill;

    [Header("Fade")]
    public float fadeInSpeed = 3f;
    public float fadeOutSpeed = 8f;

    private CanvasGroup canvasGroup;
    private float targetAlpha = 0f;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if (playerMovement == null || staminaFill == null)
            return;

        // Atualiza a quantidade de estamina
        staminaFill.fillAmount =
            playerMovement.GetStaminaPercentage();

        // Define se deve aparecer ou desaparecer
        if (playerMovement.isRunning)
        {
            targetAlpha = 1f;

            // Fade-in mais lento
            canvasGroup.alpha = Mathf.MoveTowards(
                canvasGroup.alpha,
                targetAlpha,
                fadeInSpeed * Time.deltaTime
            );
        }
        else
        {
            targetAlpha = 0f;

            // Fade-out na velocidade que você já gostou
            canvasGroup.alpha = Mathf.MoveTowards(
                canvasGroup.alpha,
                targetAlpha,
                fadeOutSpeed * Time.deltaTime
            );
        }
    }
}