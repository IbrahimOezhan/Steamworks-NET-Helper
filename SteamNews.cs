using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SteamNews : MonoBehaviour
{
    [SerializeField] private UI_SteamUpdate uiPrefab;
    [SerializeField] private Transform list;

    IEnumerator Start()
    {
        string url = $"https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid={2892190}&count={10}";
        using UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to fetch Steam news: " + request.error);
            yield break;
        }

        string json = request.downloadHandler.text;
        SteamNewsResponse response = JsonUtility.FromJson<SteamNewsResponse>(json);

        foreach (var newsItem in response.appnews.newsitems)
        {
            UI_SteamUpdate ui = Instantiate(uiPrefab, list);
            ui.Init(newsItem.title, newsItem.contents);
        }
    }

    [System.Serializable]
    public class SteamNewsResponse
    {
        public AppNews appnews;
    }

    [System.Serializable]
    public class AppNews
    {
        public int appid;
        public NewsItem[] newsitems;
    }

    [System.Serializable]
    public class NewsItem
    {
        public string gid;
        public string title;
        public string url;
        public string contents;
    }
}
