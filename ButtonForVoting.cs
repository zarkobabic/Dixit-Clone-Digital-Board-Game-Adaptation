using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System;

public class ButtonForVoting : NetworkBehaviour
{
    [SyncVar]
    [SerializeField] public int idNum;

    [SerializeField] private TMP_Text Text_On_Button_For_Voting;
    [SerializeField] private GameObject Equvivalent_Fild_Card_Object;
    [SerializeField] private int buttonOwner;
    


    public void SetButtonOwner(int buttonOwnerToSet)
    {
        buttonOwner = buttonOwnerToSet;
    }

    [Client]
    public override void OnStartClient()
    {
        GameObject parent = GameObject.Find("Voting_Layout_Group");
        gameObject.transform.SetParent(parent.transform, false);

        Text_On_Button_For_Voting.text = idNum.ToString();

        Transform panelCardShowcase = GameObject.Find("Panel_Card_Showcase").transform;
        foreach (Transform child in panelCardShowcase)
        {
            if (child.gameObject.GetComponent<FieldCard>().idNum == idNum)
            {
                Equvivalent_Fild_Card_Object = child.gameObject;
                break;
            }
        }
    }

    public void turnOnIfNotHis(int chosenOwner, int WhichTurn)
    {
        if ((buttonOwner != chosenOwner) && (buttonOwner != WhichTurn))
        {
            transform.GetChild(1).gameObject.GetComponent<Button>().interactable = true;
        }
    }

    //----------------------------------------------------TRIGER ON BUTTON FOR VOTING FOR THIS CARD----------------------------------


    public void VoteForThisButtonEntered()
    {
        TimerVoting timerForVoting = GameObject.Find("Timer_Voting").GetComponent<TimerVoting>();
        timerForVoting.CancelTimer();
        timerForVoting.gameObject.SetActive(false);

        int playerToVoteFor = Equvivalent_Fild_Card_Object.GetComponent<FieldCard>().owner;

        CmdIncreaseNumTaps(Equvivalent_Fild_Card_Object.GetComponent<FieldCard>().idNum);

        CmdVoteForThisOne(playerToVoteFor, buttonOwner);

        Transform votingLayoutGroup = gameObject.transform.parent;
        foreach (Transform child in votingLayoutGroup)
        {
            child.GetChild(1).gameObject.GetComponent<Button>().interactable = false;
        }

    }

    public void VoteForThisButtonEnteredAutomaticWithTimer()
    {
        int playerToVoteFor = Equvivalent_Fild_Card_Object.GetComponent<FieldCard>().owner;
        CmdIncreaseNumTaps(Equvivalent_Fild_Card_Object.GetComponent<FieldCard>().idNum);

        CmdVoteForThisOne(playerToVoteFor, buttonOwner);

        Transform votingLayoutGroup = gameObject.transform.parent;
        foreach (Transform child in votingLayoutGroup)
        {
            child.GetChild(1).gameObject.GetComponent<Button>().interactable = false;
        }

    }



    [Command(requiresAuthority =false)]
    public void CmdIncreaseNumTaps(int idNum)
    {
        Transform panelCardShowcase = GameObject.Find("Panel_Card_Showcase").transform;
        foreach (Transform child in panelCardShowcase)
        {
            if(child.gameObject.GetComponent<FieldCard>().idNum == idNum)
            {
                child.gameObject.GetComponent<FieldCard>().numTaps += 1;
                break;
            }
        }
    }


    [Command(requiresAuthority = false)]
    public void CmdVoteForThisOne(int playerToVoteTo, int buttonOwner)
    {
        NetworkManagerLobby networkManagerObject = NetworkManager.singleton.GetComponent<NetworkManagerLobby>();

        for (int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
        {
            (networkManagerObject.GamePlayers[i]).playersPoints[playerToVoteTo]+=1;
        }


        if (playerToVoteTo == networkManagerObject.GamePlayers[0].WhichTurn)
        {
            for (int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
            {
                networkManagerObject.GamePlayers[i].successGuesses.Add(buttonOwner);
            }
        }


        for (int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
        {
            networkManagerObject.GamePlayers[i].numOfVotes += 1;
        } 
    }



}
