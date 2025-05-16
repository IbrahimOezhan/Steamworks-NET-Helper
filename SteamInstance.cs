using UnityEngine;

public class SteamInstance : MonoBehaviour
{
    public static SteamInstance Instance;

    private void Awake()
    {
        Instance = this;
    }
}
