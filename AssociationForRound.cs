using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;
using UnityEngine.SceneManagement;

public class AssociationForRound : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField inputEnterAssociation = null;
    [SerializeField] private Button confirmButton = null;
    [SerializeField] private TMP_Text textAssociationForThisRound = null;
    [SerializeField] private GameObject dropCardPanel = null;

    //---------------------------------------------------------BUTTON TRIGERED METHODS--------------------------------------
    public void SetFlagToPlayersToDealFirstHandIfFirstRound()
    {
        CmdSetFlagToPlayersToDealFirstHandIfFirstRound();
        GameObject panelFirstHand = GameObject.Find("Panel_First_Hand");
        panelFirstHand.SetActive(false);
    }

    [Command(requiresAuthority = false)]
    public void CmdSetFlagToPlayersToDealFirstHandIfFirstRound()
    {

        NetworkManagerLobby networkManagerObject = NetworkManager.singleton.GetComponent<NetworkManagerLobby>();

        for (int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
        {
            if (networkManagerObject.GamePlayers[i].firstdealing)
            {
                networkManagerObject.GamePlayers[i].firstdealing = false;
            }
        }

        RpcSetDropZonesToFalse();
    }

    [ClientRpc]
    public void RpcSetDropZonesToFalse()
    {
        dropCardPanel.SetActive(false);
    }

    //----------------------------------------------------------------BUTTON TRIGERED METHODS--------------------------------------

    public void OnChangeInputAssociationForThisRound(string inputAssociation)
    {
        confirmButton.interactable = !string.IsNullOrEmpty(inputAssociation);
    }

    public void EnterAssociationForThisRound()
    { 
        CmdChangeAssociationForThisRound(inputEnterAssociation.text);
    }

    [Command(requiresAuthority = false)]
    public void CmdChangeAssociationForThisRound(string associationText)
    {
        NetworkManagerLobby networkManagerObject = NetworkManager.singleton.GetComponent<NetworkManagerLobby>();
        RpcChangeDropZonesOfAll(networkManagerObject.GamePlayers[0].WhichTurn, networkManagerObject.GamePlayers.Count);
        RpcChangeAssociationForThisRound(associationText);
    }

    [ClientRpc]
    public void RpcChangeDropZonesOfAll(int whichTurnPlayer, int playerNumber)
    {  
        dropCardPanel.GetComponent<DropCardPanel>().TurnId = whichTurnPlayer;
        dropCardPanel.GetComponent<DropCardPanel>().playersNb = playerNumber;
    }


    [ClientRpc]
    public void RpcChangeAssociationForThisRound(string associationForThisRound)
    {
        textAssociationForThisRound.text = associationForThisRound;
        Transform rootTransform = GameObject.Find("Canvas_MainScene").transform;
        foreach (Transform child in rootTransform)
        {
            if (child.name == "Panel_Enter_Your_Association")
            {
                child.gameObject.SetActive(false);
            }
        }

        dropCardPanel.SetActive(true);
        dropCardPanel.GetComponent<DropCardPanel>().getDropZoneTimer().SetActive(true);
        dropCardPanel.GetComponent<DropCardPanel>().getDropZoneTimer().GetComponent<Timer>().StartTimer(60);
    }



    //----------------------------------------BUTTON ZA RESET--------------------------------------

    public void SetFlagToResetAndReadyForNewRound()
    {   
        CmdSetFlagToResetAndReadyForNewRound();
    }

    [Command(requiresAuthority = false)]
    public void CmdSetFlagToResetAndReadyForNewRound()
    {
        NetworkManagerLobby networkManagerObject = NetworkManager.singleton.GetComponent<NetworkManagerLobby>();
        for (int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
        {
            networkManagerObject.GamePlayers[i].resetRound++;
        }
    }

    //--------------------------------------BUTTON ZA POVRATAK NA MAIN MENU-------------------------------

    public void BackToMainMenu()
    {
        NetworkManagerLobby networkManagerObject = NetworkManager.singleton.GetComponent<NetworkManagerLobby>();
        if (isServer) networkManagerObject.StopHost();
        networkManagerObject.StopClient();
    }


}
