using UnityEngine;

public class StopGame : MonoBehaviour
{
    [SerializeField] private GameOverPopup gameOverPopup; // Reference to the GameOverPopup

    // Stop het spel door de tijd te bevriezen
    public void StopGameProcess(LineDrawer lineDrawer, float timeElapsed)
    {
        Time.timeScale = 0f;

        // Haal de laatst voltooide levelgegevens op
        int lastCompletedLevel = PlayerManager.Instance.LastCompletedLevel;
        float timeForLastCompletedLevel = PlayerManager.Instance.TimeForLastCompletedLevel;

        Debug.Log($"Game over! Last completed level: {lastCompletedLevel}, Time: {timeForLastCompletedLevel} seconds.");

        // Update de database met de correcte tijd en level
        PlayerManager.Instance.StartCoroutine(
            PlayerManager.Instance.UpdateHighestLevelAndTimeInDatabase(timeForLastCompletedLevel, lastCompletedLevel)
        );

        // Toon de popup met de correcte gegevens
        PlayerManager.Instance.StartCoroutine(
            PlayerManager.Instance.GetPlayerStatsFromDatabase((playerStats) =>
            {
                if (gameOverPopup != null)
                {
                    int bestLevel = playerStats?.HighestLevelReached ?? 0;
                    float bestTime = playerStats != null
                        ? (playerStats.Minutes * 60 + playerStats.Seconds + playerStats.Milliseconds / 1000f)
                        : 0;

                    gameOverPopup.ShowGameOverPopup(lastCompletedLevel, timeForLastCompletedLevel, bestLevel, bestTime);
                }
            })
        );

        // Stop de tekenactiviteit
        StopDrawing stopDrawing = lineDrawer.GetComponent<StopDrawing>();
        stopDrawing?.StopDrawingProcess();
    }


}
