using System.Text.RegularExpressions;
using IbrahKit;
using UnityEngine;
using UnityEngine.UI;

public class UI_SteamUpdate : MonoBehaviour
{
    [SerializeField] private Text title;
    [SerializeField] private Text body;
    [SerializeField] private UI_Interactive interactive;

    public void Init(string title, string body)
    {
        string list = "(\\[list\\])([\\s\\S]*?)(\\[\\/list\\])";

        foreach (Match match in Regex.Matches(body, list))
        {
            string groupValue = match.Groups[2].Value;

            body = body.Replace(match.Groups[1].Value, "");
            body = body.Replace(match.Groups[3].Value, "");

            string listContentRegex = "([\\s]*\\[\\*\\].*[\\s]*)";

            foreach (Match listContentMatch in Regex.Matches(groupValue, listContentRegex))
            {
                string listContent = listContentMatch.Groups[1].Value;

                string newContent = listContent.Replace("[*]", "-");
                body = body.Replace(listContent, newContent);
            }
        }

        this.title.text = title;
        this.body.text = body;
        interactive.UpdateUI();
    }
}
