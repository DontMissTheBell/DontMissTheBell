using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReplayBrowser : MonoBehaviour
{
    private readonly List<GameObject> leaderboardEntries = new();
    private int currentPage, totalPages;

    public void Start()
    {
        RawEntriesDownloaded += ProcessLeaderboardEntries;
        StartCoroutine(DownloadEntries());
        leaderboardEntries.Add(transform.GetChild(2).gameObject);
    }

    private event RawEntries RawEntriesDownloaded;

    private void ProcessLeaderboardEntries(string serverResponse, int page, int pageSize)
    {
        // Deserialize LeaderboardSection object
        if (serverResponse == "null") return;
        var leaderboardSection = JsonUtility.FromJson<LeaderboardSection>(serverResponse);
        currentPage = page;
        totalPages = leaderboardSection.totalPages;
        // Abort if json did not contain a valid LeaderboardSection object
        if (leaderboardSection.entries == null) return;

        var usernameQueue = new Queue<string>(leaderboardSection.usernames);

        // Clean up old entries from UI
        foreach (var leaderboardEntry in leaderboardEntries.Skip(1))
        {
            Destroy(leaderboardEntry);
        }
        leaderboardEntries.RemoveRange(1, leaderboardEntries.Count - 1);

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
            entryText.text = $"{usernameQueue.Dequeue()} - {leaderboardEntry.time}s on {Globals.GetSceneNameFromId(leaderboardEntry.levelId)}";

            // Assign function call to Watch button
            var button = currentEntry.GetComponentInChildren<Button>();
            button.onClick.AddListener(delegate { StartReplay(leaderboardEntry.ghostId, leaderboardEntry.levelId); });
        }

        var footer = transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        footer.text = $"Page {page} / {leaderboardSection.totalPages}";
    }

    private IEnumerator DownloadEntries(int page = 1, int pageSize = 6)
    {
        var www = UnityWebRequest.Get($"{Globals.Instance.APIEndpoint}/v1/leaderboard/{page}/{pageSize}");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log($"{www.error} - {www.downloadHandler.text}");
        else
            //Debug.Log($"Successfully downloaded leaderboard page {page}");
            RawEntriesDownloaded?.Invoke(www.downloadHandler.text, page, pageSize);
    }

    public void ChangePage(bool next)
    {
        var nextPage = currentPage + (Convert.ToInt32(next) * 2 - 1);
        if (nextPage >= 1 && nextPage <= totalPages)
        {
            StartCoroutine(DownloadEntries(currentPage + (Convert.ToInt32(next) * 2 - 1)));
        }
    }

    private static void StartReplay(string ghostId, int levelId)
    {
        Globals.Instance.replayToStart = ghostId;

        Globals.Instance.StartCoroutine(Globals.Instance.TriggerLoadingScreen(sceneId: levelId));
    }

    private delegate void RawEntries(string rawEntries, int page, int pageSize);
}

[Serializable]
public class LeaderboardEntry
{
    public int levelId;
    public float time;
    public string ghostId;
}

[Serializable]
public class LeaderboardSection
{
    public LeaderboardEntry[] entries;
    public int pageSize;
    public int totalPages;
    public int Size => entries.Length;

    public string[] usernames;
}