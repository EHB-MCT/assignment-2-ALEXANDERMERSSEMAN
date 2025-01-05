using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public string PlayerId { get; private set; }
    public int HighestLevelReached { get; private set; }
    public float TimeForHighestLevel { get; private set; } // Tijd voor de laatst voltooide level

    public string Username { get; private set; }

    private string updateLevelUrl = "http://localhost:5033/api/playerstats/update-highest-level";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Blijf bestaan tussen scènes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetPlayerData(string playerId, string username, int highestLevelReached)
    {
        this.PlayerId = playerId;
        this.Username = username; // Stel de gebruikersnaam in
        this.HighestLevelReached = highestLevelReached;

        Debug.Log($"PlayerManager initialized: PlayerId={playerId}, Username={username}, HighestLevelReached={highestLevelReached}");
    }

    public void UpdateHighestLevel(int completedLevel, float timeElapsed)
    {
        // Controleer of het voltooide level een nieuwe hoogste score is
        if (completedLevel > HighestLevelReached)
        {
            HighestLevelReached = completedLevel; // Update hoogste voltooide level
            TimeForHighestLevel = timeElapsed; // Tijd van dat level
            Debug.Log($"New highest level reached: {HighestLevelReached} at {TimeForHighestLevel} seconds.");
        }
        else
        {
            Debug.Log($"Level {completedLevel} completed, but it's not a new highest level.");
        }
    }

    public IEnumerator UpdateHighestLevelAndTimeInDatabase(float timeElapsed, int achievedLevel)
    {
        int minutes = Mathf.FloorToInt(timeElapsed / 60);
        int seconds = Mathf.FloorToInt(timeElapsed % 60);
        int milliseconds = Mathf.FloorToInt((timeElapsed * 1000) % 1000);

        var json = JsonUtility.ToJson(new UpdateTimeRequest
        {
            PlayerId = PlayerId,
            NewLevel = achievedLevel, // Gebruik het doorgegeven voltooide niveau
            Minutes = minutes,
            Seconds = seconds,
            Milliseconds = milliseconds
        });

        using (UnityWebRequest request = new UnityWebRequest(updateLevelUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Highest level and completion time updated successfully in the database.");
            }
            else
            {
                Debug.LogError($"Failed to update highest level and time: {request.error}");
            }
        }
    }

    public IEnumerator GetPlayerStatsFromDatabase(System.Action<PlayerStats> onStatsRetrieved)
    {
        string url = $"http://localhost:5033/api/playerstats/{PlayerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var playerStats = JsonUtility.FromJson<PlayerStats>(request.downloadHandler.text);
                onStatsRetrieved?.Invoke(playerStats);
            }
            else
            {
                Debug.LogError($"Failed to retrieve player stats: {request.error}");
                onStatsRetrieved?.Invoke(null); // Stuur null als de aanvraag mislukt
            }
        }
    }

    [System.Serializable]
    public class UpdateTimeRequest
    {
        public string PlayerId;
        public int NewLevel; // Verander van LevelCompleted naar HighestLevelReached
        public int Minutes;
        public int Seconds;
        public int Milliseconds;
    }

    [System.Serializable]
    public class PlayerStats
    {
        public string PlayerId;
        public int HighestLevelReached;
        public int Minutes;
        public int Seconds;
        public int Milliseconds;
    }
}