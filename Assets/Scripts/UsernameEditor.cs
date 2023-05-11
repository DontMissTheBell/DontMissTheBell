using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class UsernameEditor : MonoBehaviour
{
    public TextMeshProUGUI usernameBox;
    public MenuTransitioner menuManager;

    public void StartUsernameUpload()
    {
        // Do nothing if name blank
        if (string.IsNullOrWhiteSpace(usernameBox.text)) return;
        Globals.Username = usernameBox.text.Replace(" ", string.Empty);
        StartCoroutine(UploadUsername());
    }

    private IEnumerator UploadUsername()
    {
        var www = UnityWebRequest.Put($"{Globals.Instance.APIEndpoint}/v1/update-username", Globals.Username);
        www.SetRequestHeader("X-Player-Id", Globals.Instance.playerID.ToString());
        www.SetRequestHeader("X-Player-Secret", Globals.Instance.playerSecret.ToString());
        yield return www.SendWebRequest();

        Debug.Log(www.result != UnityWebRequest.Result.Success
            ? www.error
            : $"Upload complete! {www.downloadHandler.text}");
        menuManager.ToggleUsernamePanel(false);
    }
}