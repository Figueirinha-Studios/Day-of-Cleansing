using TMPro;
using UnityEngine;

public class TimerDisplay : MonoBehaviour
{
    [Header("Referências")]
    public GameTimer gameTimer;
    public TMP_Text timerText;

    [Header("Aparência")]
    public string prefix = "";
    public bool showMilliseconds = false;

    private void Update()
    {
        if (gameTimer == null || timerText == null)
            return;

        int totalSeconds = gameTimer.RemainingSeconds;
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        if (showMilliseconds && totalSeconds > 0)
        {
            timerText.text =
                $"{prefix}{minutes:00}:{seconds:00}";
        }
        else
        {
            timerText.text =
                $"{prefix}{minutes:00}:{seconds:00}";
        }
    }
}