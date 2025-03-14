using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class ScoreEntry
{
    public string username;
    public int score;
}

[System.Serializable]
public class LeaderboardResponse
{
    public List<ScoreEntry> scores;
}

public class ScoreManagerScript : MonoBehaviour
{
    public static ScoreManagerScript Instance { get; private set; }
    
    public static int Score { get; private set; }
    
    public string baseUrl = "https://sid-restapi.onrender.com";
    public TMP_Text leaderboardText;
    
    public Sprite[] numberSprites;
    public SpriteRenderer Units, Tens, Hundreds;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        Units.gameObject.SetActive(true);
        Tens.gameObject.SetActive(false);
        Hundreds.gameObject.SetActive(false);
        GetLeaderboard();
    }

    public static void AddScore(int value)
    {
    Score += value;
    Instance?.UpdateScoreUI(); 
    }

    private void UpdateScoreUI()
    {
        if (Score < 10)
        {
            Tens.gameObject.SetActive(false);
            Hundreds.gameObject.SetActive(false);
            Units.sprite = numberSprites[Score];
        }
        else if (Score < 100)
        {
            Tens.gameObject.SetActive(true);
            Hundreds.gameObject.SetActive(false);
            Tens.sprite = numberSprites[Score / 10];
            Units.sprite = numberSprites[Score % 10];
        }
        else
        {
            Tens.gameObject.SetActive(true);
            Hundreds.gameObject.SetActive(true);
            Hundreds.sprite = numberSprites[Score / 100];
            int rest = Score % 100;
            Tens.sprite = numberSprites[rest / 10];
            Units.sprite = numberSprites[rest % 10];
        }
    }

    public void SubmitScore()
    {
        string username = PlayerPrefs.GetString("username", "Guest");
        StartCoroutine(UpdateScoreCoroutine(username, Score));
    }

    private IEnumerator UpdateScoreCoroutine(string username, int score)
    {
        string path = "/leaderboard/update";
        string json = JsonUtility.ToJson(new ScoreEntry { username = username, score = score });

        using (UnityWebRequest www = new UnityWebRequest(baseUrl + path, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Puntaje actualizado correctamente.");
                GetLeaderboard();
            }
            else
            {
                Debug.LogError("Error al actualizar puntaje: " + www.downloadHandler.text);
            }
        }
    }

    public void GetLeaderboard()
    {
        StartCoroutine(GetLeaderboardCoroutine());
    }

    private IEnumerator GetLeaderboardCoroutine()
    {
        string path = "/leaderboard";
        using (UnityWebRequest www = UnityWebRequest.Get(baseUrl + path))
        {
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(www.downloadHandler.text);
                UpdateLeaderboardUI(response.scores);
            }
            else
            {
                Debug.LogError("Error al obtener leaderboard: " + www.downloadHandler.text);
            }
        }
    }

    private void UpdateLeaderboardUI(List<ScoreEntry> scores)
    {
        leaderboardText.text = "🏆 Leaderboard 🏆\n";
        foreach (var entry in scores)
        {
            leaderboardText.text += $"{entry.username}: {entry.score}\n";
        }
    }
}
