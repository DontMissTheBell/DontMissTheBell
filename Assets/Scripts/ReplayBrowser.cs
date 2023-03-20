using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ReplayBrowser : MonoBehaviour
{
    private delegate void RawEntries(string rawEntries, int page, int pageSize);
    private event RawEntries RawEntriesDownloaded;
    private readonly List<GameObject> leaderboardEntries = new();

    private void ProcessLeaderboardEntries(string serverResponse, int page, int pageSize)
    {
        // Deserialize LeaderboardSection object
        if (serverResponse == "null") { return; }
        var leaderboardSection = JsonUtility.FromJson<LeaderboardSection>(serverResponse);
        // Abort if json did not contain a valid LeaderboardSection object
        if (leaderboardSection.entries == null) return;
        
        // Create a panel for each LeaderboardEntry object in leaderboardSection.entries
        foreach (var leaderboardEntry in leaderboardSection.entries)
        {
            // Clone the entry template and position in list
            var currentEntry = Instantiate(leaderboardEntries[0], transform);
            leaderboardEntries.Add(currentEntry);
            currentEntry.SetActive(true);
            currentEntry.transform.Translate(Vector3.down * (1.5f * leaderboardEntries.Count - 2));

            // Set label contents
            var entryText = currentEntry.GetComponentInChildren<TextMeshProUGUI>();
            entryText.text = leaderboardEntry.ghostId;

            // Assign function call to Watch button
            var button = currentEntry.GetComponentInChildren<Button>();
            button.onClick.AddListener(delegate {StartReplay(leaderboardEntry.ghostId, leaderboardEntry.levelId); });
        }
    }

    public void Start()
    {
        RawEntriesDownloaded += ProcessLeaderboardEntries;
        StartCoroutine(DownloadEntries());
        leaderboardEntries.Add(transform.GetChild(0).gameObject);
    }

    private IEnumerator DownloadEntries(int page = 1, int pageSize = 10)
    {
        var www = UnityWebRequest.Get($"{Globals.Instance.APIEndpoint}/v1/leaderboard/{page}/{pageSize}");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log($"{www.error} - {www.downloadHandler.text}");
        }
        else
        {
            //Debug.Log($"Successfully downloaded leaderboard page {page}");
            RawEntriesDownloaded?.Invoke(www.downloadHandler.text, page, pageSize);
        }
    }

    private static void StartReplay(string ghostId, int levelId)
    {
        Globals.Instance.replayToStart = ghostId;
        
        Globals.Instance.StartCoroutine(Globals.Instance.TriggerLoadingScreen(sceneId: levelId));
    }
}

[Serializable]
public class LeaderboardEntry
{
    public int playerId;
    public int levelId;
    public float time;
    public string ghostId;
}

[Serializable]
public class LeaderboardSection
{
    public LeaderboardEntry[] entries;
    public int Size => entries.Length;
}
