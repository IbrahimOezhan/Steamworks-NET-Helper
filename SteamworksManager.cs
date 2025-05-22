using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using TemplateTools;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = TemplateTools.Debug;

public class SteamworksManager : MonoBehaviour
{
    private Callback<LobbyCreated_t> lobbyCreated;
    private Callback<LobbyEnter_t> lobbyEntered;
    private Callback<LobbyChatUpdate_t> lobbyUpdate;
    private Callback<GameLobbyJoinRequested_t> joinRequest;
    private Callback<LobbyDataUpdate_t> lobbyDataUpdate;
    private Callback<AvatarImageLoaded_t> avatarImageLoaded;

    private const string KEY_LOBBY_JOIN_CODE = "LobbyJoinCode";

    private SteamLobbyData lobbyData;

    public static SteamworksManager Instance;


    private void Awake()
    {
        Instance = this;
        avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvaterImageLoaded);
        joinRequest = Callback<GameLobbyJoinRequested_t>.Create(LobbyJoinRquest);
    }

    private void Start()
    {
        const int bufferSize = 256;

        string launchParams;

        int bytesCopied = SteamApps.GetLaunchCommandLine(out launchParams, bufferSize);

        if(bytesCopied > 0 && !String_Utilities.IsEmpty(launchParams))
        {
            Debug.Log(launchParams);
            string s = launchParams.Replace("+connect_lobby", "");
            Debug.Log(s);
            CSteamID lobbyID = new();
            if(ulong.TryParse(s, out ulong res))
            {
                lobbyID.m_SteamID = res; 
                Debug.Log(lobbyID.m_SteamID.ToString());
                LobbyJoinRequest(lobbyID);
            }
            else
            {
                Debug.Log("Error when parsing");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }

        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    private void OnDestroy()
    {
        avatarImageLoaded.Unregister();
        avatarImageLoaded.Dispose();
        joinRequest.Unregister();
        joinRequest.Dispose();
    }

    public void CreateLobby()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("Steam is not initialized");
            return;
        }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 2);
    }

    public void OnLobbyCreated(LobbyCreated_t result)
    {
        if (result.m_eResult == EResult.k_EResultOK)
        {
            //Debug.Log("Steam Lobby sucessfully created. ID: " + result.m_ulSteamIDLobby);

            //lobbyData = new()
            //{
            //    lobbyID = new(result.m_ulSteamIDLobby)
            //};

            //SteamMatchmaking.SetLobbyData(lobbyData.lobbyID, KEY_LOBBY_JOIN_CODE, Multiplayer_SessionManager.Instance.session.lobbyCode);

            //Multiplayer_SessionManager.Instance.session.UpdateSteamLobbbyID(lobbyData.lobbyID);

            //lobbyUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyUpdate);
        }
        else
        {
            Debug.LogError(result.m_eResult.ToString());
        }

        lobbyCreated.Unregister();
        lobbyCreated.Dispose();
    }

    public void ExitLobby()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("Steam is not initialized");
            return;
        }

        if (lobbyData == null)
        {
            Debug.LogWarning("You are not part of a lobby");
            return;
        }

        lobbyData.ExitLobby();
        lobbyData = null;
    }

    public void JoinLobby(CSteamID id)
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("Steam is not initialized");
            return;
        }

        if (lobbyData != null)
        {
            Debug.LogWarning("You are part of a lobby already");
            return;
        }

        lobbyEntered = Callback<LobbyEnter_t>.Create(LobbyEntered);

        SteamMatchmaking.JoinLobby(id);

        lobbyData = new()
        {
            lobbyID = id
        };
    }

    public void LobbyEntered(LobbyEnter_t t)
    {
        if (t.m_EChatRoomEnterResponse == 1)
        {
            Debug.Log("Steam Lobby entered successfully. ID: " + t.m_ulSteamIDLobby);

            GetAllMemberAvaters();

            lobbyEntered.Unregister();
            lobbyEntered.Dispose();
        }
        else
        {
            Debug.Log("Error when joining lobby");
        }
    }

    public void LobbyJoinRquest(GameLobbyJoinRequested_t t)
    {
        LobbyJoinRequest(t.m_steamIDLobby);
    }

    public void LobbyJoinRequest(CSteamID lobbyID)
    {
        //SceneManager.LoadScene("SteamLobbyJoin");

        //FindFirstObjectByType<Mode_Menu_Manager_Multiplayer>().OnSelect();

        //lobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(LobbyDataUpdate);

        //SteamMatchmaking.RequestLobbyData(lobbyID);

    }

    public void LobbyDataUpdate(LobbyDataUpdate_t t)
    {
        try
        {
            //CSteamID steamID = new()
            //{
            //    m_SteamID = t.m_ulSteamIDLobby
            //};

            //string lobbyCode = SteamMatchmaking.GetLobbyData(steamID, KEY_LOBBY_JOIN_CODE);

            //FindFirstObjectByType<Mode_Menu_Manager_Multiplayer>().Join(lobbyCode, JoinMethod.CODE, steamID);

            //lobbyDataUpdate.Unregister();
            //lobbyDataUpdate.Dispose();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public bool GetLobbyID(out CSteamID id)
    {
        id = default;

        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("Steam is not initialized");
            return false;
        }

        if (lobbyData == null)
        {
            Debug.LogWarning("You are not part of a lobby");
            return false;
        }

        id = lobbyData.lobbyID;

        return true;
    }

    public void OnLobbyUpdate(LobbyChatUpdate_t t)
    {
        if (t.m_rgfChatMemberStateChange == 1)
        {
            Debug.Log("Steam user: " + t.m_ulSteamIDUserChanged + " has joined the lobby");

            GetAllMemberAvaters();

            lobbyUpdate.Unregister();
            lobbyUpdate.Dispose();
        }
        else
        {
            Debug.LogWarning("OnlobbyUpdate: " + t.m_rgfChatMemberStateChange + " ID: " + t.m_ulSteamIDUserChanged);
        }
    }

    public void GetAllMemberAvaters()
    {
        int lobbySize = SteamMatchmaking.GetNumLobbyMembers(lobbyData.lobbyID);

        Debug.Log("Getting all avatar handles for the lobby: " + lobbyData.lobbyID + " with the size of " + lobbySize);

        for (int i = 0; i < lobbySize; i++)
        {
            CSteamID steamID = SteamMatchmaking.GetLobbyMemberByIndex(lobbyData.lobbyID, i);

            //Returns AvaterImageLoaded_t Callback
            int handle = SteamFriends.GetLargeFriendAvatar(steamID);

            Debug.Log("Get Handle for steam ID: " + steamID + " Handle: " + handle);

            if (handle != -1) TryAddHandle(steamID, handle);
        }
    }

    public void OnAvaterImageLoaded(AvatarImageLoaded_t t)
    {
        TryAddHandle(t.m_steamID, t.m_iImage);
    }

    public void TryAddHandle(CSteamID steamID, int handle)
    {
        if (!lobbyData.avatarHandles.TryAdd(steamID, handle))
        {
            if (lobbyData.avatarHandles.TryGetValue(steamID, out int _handle))
            {
                Debug.LogWarning("Failed to add avatar handle to dictionary because it already has been added");
            }
            else Debug.LogWarning("Failed to add avater handle to dictionary because fuck you");
        }
        else
        {
            Debug.Log("Successfully added avater handle: " + handle + " for user: " + steamID);
        }
    }

    public bool GetLobbyMemberAvatarHandle(CSteamID steamID, out int handle)
    {
        if (lobbyData.avatarHandles.TryGetValue(steamID, out handle))
        {
            Debug.Log("Sucessfully returned the handle for steam user: " + steamID);

            return true;
        }
        else
        {
            Debug.LogWarning("The avatar handle for steam user: " + steamID + " does not exist");

            return false;
        }
    }

    public List<CSteamID> GetLobbyMemberIDs()
    {
        int amount = SteamMatchmaking.GetNumLobbyMembers(lobbyData.lobbyID);

        CSteamID[] steamIDs = new CSteamID[amount];

        for (int i = 0; i < steamIDs.Length; i++)
        {
            steamIDs[i] = SteamMatchmaking.GetLobbyMemberByIndex(lobbyData.lobbyID, i);
        }

        return steamIDs.ToList();
    }
}

public class SteamLobbyData
{
    public CSteamID lobbyID;

    public Dictionary<CSteamID, int> avatarHandles = new();

    public void ExitLobby()
    {
        try
        {
            SteamMatchmaking.LeaveLobby(lobbyID);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}
