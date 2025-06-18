using UnityEngine;

public class Steam_Instance : MonoBehaviour
{
    public static Steam_Instance Instance;

    private void Awake()
    {
        Instance = this;
    }
}
