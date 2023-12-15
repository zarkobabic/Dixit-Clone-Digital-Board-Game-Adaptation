using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class NetworkManagerLobby : NetworkManager

{



    [SerializeField] private int minPlayers = 3;
    [Header("Room")]
    [SerializeField] private NetworkRoomPlayerLobby roomPlayerPrefab = null;

    [Header("Game")]
    [SerializeField] private NetworkGamePlayerLobby gamePlayerPrefab = null;


    [Header("Prefabs")]
    [SerializeField] private GameObject playerScorePrefab = null;
    [SerializeField] private GameObject buttonForVotingPrefab = null;
    [SerializeField] private GameObject fieldCardPrefab = null;
    [SerializeField] private GameObject gameManagerPrefab = null;


    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

  
    [SerializeField] public List<Sprite> deck { get; private set; }

    public List<NetworkRoomPlayerLobby> RoomPlayers { get; } = new List<NetworkRoomPlayerLobby>(); 
    public List<NetworkGamePlayerLobby> GamePlayers { get; } = new List<NetworkGamePlayerLobby>(); 

    public override void Start()
    {
        base.Start();
        deck = Resources.LoadAll<Sprite>("CardSprites").ToList();
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        OnClientDisconnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {

        if (numPlayers >= maxConnections)
         {
             conn.Disconnect();
             return;
         }

        NotifyPlayersOfReadyState();

        if (SceneManager.GetActiveScene().name != "Scene_Lobby")
        {
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if(conn.identity != null)
        {
            var player = conn.identity.GetComponent<NetworkRoomPlayerLobby>();

            RoomPlayers.Remove(player);

            NotifyPlayersOfReadyState();

        }

        base.OnServerDisconnect(conn);
    }


    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {

        if (SceneManager.GetActiveScene().name == "Scene_Lobby")
        {
            base.OnServerAddPlayer(conn);

            NetworkRoomPlayerLobby addedPlayer = conn.identity.gameObject.GetComponent<NetworkRoomPlayerLobby>();
            bool isLeader = RoomPlayers.Count == 0;
            addedPlayer.IsLeader = isLeader;
        }
    }

    public override void OnStopServer()
    {
        RoomPlayers.Clear();
        base.OnStopServer();
    }

    public void NotifyPlayersOfReadyState()
    {
        foreach (var player in RoomPlayers)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    public bool IsReadyToStart()
    {
        if (numPlayers < minPlayers) { return false; }

        foreach (var player in RoomPlayers)
        {
            if (!player.IsReady) { return false; }
        }

        return true;
    }

    public void StartGame()
    {
        if(SceneManager.GetActiveScene().name == "Scene_Lobby")
        {
            if (!IsReadyToStart()) { return; }

            ServerChangeScene("Main_Scene");

        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        if (SceneManager.GetActiveScene().name == "Scene_Lobby" && newSceneName == "Main_Scene")
        {

            for (int i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                var conn = RoomPlayers[i].connectionToClient;
                var gameplayerInstance = Instantiate(gamePlayerPrefab);
                gameplayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);
                NetworkServer.RemovePlayerForConnection(conn, true);
                NetworkServer.AddPlayerForConnection(conn, gameplayerInstance.gameObject);
                GamePlayers.Add(gameplayerInstance);

            }
        }

        base.ServerChangeScene(newSceneName);
    }

    [Server]
    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);


        if(sceneName == "Main_Scene")
        {

            List<int> sortedNums = new List<int>();
            List<int> randomNums = new List<int>();
            System.Random random = new System.Random();

            for (int i = 0; i < 6; i++)
            {
                sortedNums.Add(i);
            }

            int randomIndexSorted;
            for (int i = 0; i < 6; i++)
            {
                randomIndexSorted = random.Next(0, sortedNums.Count);
                randomNums.Add(sortedNums[randomIndexSorted]);
                sortedNums.RemoveAt(randomIndexSorted);
            }

            Debug.Log("Gameplayers");
            for (int i = GamePlayers.Count - 1; i >= 0; i--)
            {


                GameObject pScorePrefab = Instantiate(playerScorePrefab);
                pScorePrefab.GetComponent<Player_Score>().Player_name = GamePlayers[i].DisplayName;

                
                int randomIndex = random.Next(0, randomNums.Count);
                int randomNum = randomNums[randomIndex];
                randomNums.RemoveAt(randomIndex);

                pScorePrefab.GetComponent<Player_Score>().imageNum = randomNum;
                NetworkServer.Spawn(pScorePrefab, GamePlayers[i].GetComponent<NetworkIdentity>().connectionToClient);

               

                GameObject field_card_prefab = Instantiate(fieldCardPrefab);
                field_card_prefab.GetComponent<FieldCard>().idNum = GamePlayers.Count - i;
                NetworkServer.Spawn(field_card_prefab);

                GameObject button_voting_prefab = Instantiate(buttonForVotingPrefab);
                button_voting_prefab.GetComponent<ButtonForVoting>().idNum = GamePlayers.Count - i;

                
                NetworkServer.Spawn(button_voting_prefab);


            }
            if (numPlayers == 3)
            {

               
                GameObject field_card_prefab4 = Instantiate(fieldCardPrefab);
                field_card_prefab4.GetComponent<FieldCard>().idNum = 4;
                NetworkServer.Spawn(field_card_prefab4);

                GameObject field_card_prefab5 = Instantiate(fieldCardPrefab);
                field_card_prefab5.GetComponent<FieldCard>().idNum = 5;
                NetworkServer.Spawn(field_card_prefab5);

                GameObject button_voting_prefab4 = Instantiate(buttonForVotingPrefab);
                button_voting_prefab4.GetComponent<ButtonForVoting>().idNum = 4;
                
                NetworkServer.Spawn(button_voting_prefab4);

                GameObject button_voting_prefab5 = Instantiate(buttonForVotingPrefab);
                button_voting_prefab5.GetComponent<ButtonForVoting>().idNum = 5;
               
                NetworkServer.Spawn(button_voting_prefab5);
            }


            GameObject gameManager = Instantiate(gameManagerPrefab);
            NetworkServer.Spawn(gameManager);
           
        }


        

    }
   
}
