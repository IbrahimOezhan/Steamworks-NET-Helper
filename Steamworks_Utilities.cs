using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Steamworks_Utilities
{
    public static (CSteamID,int)[] GetLobbyAvatarHandles(CSteamID lobbyID)
    {
        int lobbySize = SteamMatchmaking.GetNumLobbyMembers(lobbyID);

        (CSteamID,int)[] handles = new (CSteamID, int)[lobbySize];

        Debug.Log("Getting all avatar handles for the lobby: " + lobbyID + " with the size of " + lobbySize);

        for (int i = 0; i < lobbySize; i++)
        {
            CSteamID steamID = SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i);

            //Returns AvaterImageLoaded_t Callback
            int handle = SteamFriends.GetLargeFriendAvatar(steamID);

            handles[i] = (steamID,handle);

            if (handle != -1)
            {
                Debug.LogWarning("Avatar handle for steam user " + steamID + " could not be found");
            }
            else
            {
                Debug.Log("Found handle for steam user  " + steamID + ". Handle: " + handle);
            }
        }

        return handles;
    }

    public static CSteamID[] GetLobbyMemberIDs(CSteamID lobbyID)
    {
        int amount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);

        CSteamID[] steamIDs = new CSteamID[amount];

        for (int i = 0; i < steamIDs.Length; i++)
        {
            steamIDs[i] = SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i);
        }

        return steamIDs;
    }
}
