using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{



    //--------------------------------------------------------METHODS-----------------------------------------------

    [Server]
    public override void OnStartServer()
    {
        NetworkManagerLobby networkManagerObject = NetworkManager.singleton.GetComponent<NetworkManagerLobby>();
        

        for (int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
        {
            networkManagerObject.GamePlayers[i].localPlayerID = i;
        }


        System.Random random = new System.Random();
        int turnID = random.Next(0, networkManagerObject.GamePlayers.Count);
        for (int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
        {
            networkManagerObject.GamePlayers[i].WhichTurn = turnID;
        }
    }

    [Server]
    public void ChangeWhoGivesAssociation(int WhichTurn)
    {
        NetworkManagerLobby networkManagerObject = NetworkManager.singleton.GetComponent<NetworkManagerLobby>();
        for (int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
        {
            networkManagerObject.GamePlayers[i].WhichTurn = WhichTurn;
        }
    }

}
