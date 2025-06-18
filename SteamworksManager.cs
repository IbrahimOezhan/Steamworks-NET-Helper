using Discord;
using IbrahKit;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = IbrahKit.Debug;

public class SteamworksManager : MonoBehaviour
{
    private const string KEY_LOBBY_JOIN_CODE = "LobbyJoinCode";

    private Callback<LobbyCreated_t> lobbyCreated;
    private Callback<LobbyEnter_t> lobbyEntered;
    private Callback<GameLobbyJoinRequested_t> joinRequest;
    private Callback<LobbyDataUpdate_t> lobbyDataUpdate;

    public Action OnLobbyJoinRequest;

    private SteamLobbyInstance lobbyData;

    public static SteamworksManager Instance;

    private void Awake()
    {
        Instance = this;

        joinRequest = Callback<GameLobbyJoinRequested_t>.Create(LobbyJoinRquest);
    }

    private void Start()
    {
        const int bufferSize = 256;

        string launchParams;

        int bytesCopied = SteamApps.GetLaunchCommandLine(out launchParams, bufferSize);

        if(bytesCopied > 0 && !String_Utilities.IsEmpty(launchParams))
        {
            string s = launchParams.Replace("+connect_lobby", "");

            Debug.Log(launchParams);

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
        joinRequest.Unregister();
        joinRequest.Dispose();
    }

    public void LobbyJoinRquest(GameLobbyJoinRequested_t t)
    {
        LobbyJoinRequest(t.m_steamIDLobby);
    }

    public void LobbyJoinRequest(CSteamID lobbyID)
    {
        JoinLobby(lobbyID);
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
    }

    private void LobbyEntered(LobbyEnter_t t)
    {
        if (t.m_EChatRoomEnterResponse == 1)
        {
            Debug.Log("Steam Lobby entered successfully. ID: " + t.m_ulSteamIDLobby);

            lobbyData = new(new(t.m_ulSteamIDLobby));

            lobbyEntered.Unregister();

            lobbyEntered.Dispose();
        }
        else
        {
            Debug.Log("Error when joining lobby");
        }
    }

    public void CreateLobby()
    {
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 2);
    }

    private void OnLobbyCreated(LobbyCreated_t result)
    {
        if (result.m_eResult == EResult.k_EResultOK)
        {
            Debug.Log("Steam Lobby sucessfully created. ID: " + result.m_ulSteamIDLobby);

            CSteamID lobbyID = new(result.m_ulSteamIDLobby);

            SteamLobbyInstance lobbyInstance = new(lobbyID);
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

        lobbyData.Exit();
        lobbyData = null;
    }

    private class SteamLobbyInstance
    {
        private Callback<LobbyChatUpdate_t> lobbyUpdate;

        private Callback<AvatarImageLoaded_t> avatarImageLoaded;

        private Dictionary<CSteamID, int> avatarHandles = new();

        private CSteamID lobbyID;

        public SteamLobbyInstance(CSteamID id)
        {
            lobbyID = id;

            lobbyUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyUpdate);

            avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvaterImageLoaded);

            AddAvatarHandles();
        }

        public void Exit()
        {
            SteamMatchmaking.LeaveLobby(lobbyID);

            lobbyUpdate.Unregister();
            lobbyUpdate.Dispose();

            avatarImageLoaded.Unregister();
            avatarImageLoaded.Dispose();
        }

        private void OnLobbyUpdate(LobbyChatUpdate_t t)
        {
            if (t.m_rgfChatMemberStateChange == 1)
            {
                Debug.Log("Steam user: " + t.m_ulSteamIDUserChanged + " has joined the lobby");

                AddAvatarHandles();

                lobbyUpdate.Unregister();

                lobbyUpdate.Dispose();
            }
            else
            {
                Debug.LogWarning("OnlobbyUpdate: " + t.m_rgfChatMemberStateChange + " ID: " + t.m_ulSteamIDUserChanged);
            }
        }

        private void AddAvatarHandles()
        {
            (CSteamID, int)[] handles = Steamworks_Utilities.GetLobbyAvatarHandles(lobbyID);

            for (int i = 0; i < handles.Length; i++)
            {
                if (handles[i].Item2 != -1) TryAddHandle(handles[i].Item1, handles[i].Item2);
            }
        }

        private void TryAddHandle(CSteamID steamID, int handle)
        {
            if (!avatarHandles.TryAdd(steamID, handle))
            {
                if (avatarHandles.TryGetValue(steamID, out int _handle))
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

        private void OnAvaterImageLoaded(AvatarImageLoaded_t t)
        {
            TryAddHandle(t.m_steamID, t.m_iImage);
        }

        public bool GetLobbyMemberAvatarHandle(CSteamID steamID, out int handle)
        {
            if (avatarHandles.TryGetValue(steamID, out handle))
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
    }
}
