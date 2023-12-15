using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class NetworkGamePlayerLobby : NetworkBehaviour
{

    //-----------------------------------------------------FIELDS--------------------------------------------------

    [SerializeField] private GameObject handCardPrefab = null;

    [SyncVar]
    [SerializeField] public string DisplayName;


    [SyncVar]
    [SerializeField] public int localPlayerID;

    [SyncVar(hook = nameof(TurnChanged))]
    [SerializeField] public int WhichTurn = -1;

    [SyncVar(hook = nameof(DrawFirstHand))]
    [SerializeField] public bool firstdealing = true;

    [SyncVar]
    [SerializeField] public List<int> cards = new List<int>();

    [SyncVar]
    [SerializeField] public List<int> ChosenCardsForRound = new List<int>();

    [SyncVar]
    [SerializeField] public List<int> ChosenOwnersForRound = new List<int>();

    [SyncVar(hook = nameof(GenerateCardsInField))]
    [SerializeField] public int numReadyCardsForField = 0;

    [SyncVar]
    public List<int> playersPoints = new List<int>();

    [SyncVar]
    public List<int> successGuesses = new List<int>();


    [SyncVar(hook = nameof(CheckHowManyVotes))]
    public int numOfVotes = 0;

    [SyncVar(hook = nameof(ResetRoundAndReadyForNewOne))]
    public int resetRound = 0;

    [SyncVar(hook = nameof(EnableLeaderboardPage))]
    public bool EndOfGame = false;

    

    //-----------------------------------------------------METHODS-------------------------------------------------

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);
    }


    public override void OnStartServer()
    {
        NetworkManagerLobby networkManagerObject = NetworkManager.singleton.GetComponent<NetworkManagerLobby>();
        for (int i = 0; i < networkManagerObject.deck.Count; i++)
        {
            cards.Add(i);
        }

        for (int i = 0; i < 6; i++)
        {
            playersPoints.Add(0);
        }
    }

    public override void OnStopClient()
    {
        NetworkManagerLobby networkManagerObject = NetworkManager.singleton.GetComponent<NetworkManagerLobby>();
        networkManagerObject.GamePlayers.Remove(this);
    }


    [Server]
    public void SetDisplayName(string displayName)
    {
        this.DisplayName = displayName;
    }


    public void TurnChanged(int oldValue, int newValue)
    {

        if ((isLocalPlayer) && (newValue == localPlayerID))
        {
            Transform rootTransform = GameObject.Find("Canvas_MainScene").transform;
            foreach (Transform child in rootTransform)
            {
                if (child.name == "Panel_Enter_Your_Association")
                {
                    child.gameObject.SetActive(true);

                    if (firstdealing)
                    {
                        child.GetChild(3).gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    public void DrawFirstHand(bool oldValue, bool newValue)
    {
        if(newValue == false)
        {
            if (isLocalPlayer)
            {
                CmdDealFirstHand();
                CmdSetEachPlayerButtonsOwner(localPlayerID);
            }
        }
    }

    [Command(requiresAuthority =false)]
    public void CmdDealFirstHand()
    {
        NetworkManagerLobby networkManagerObject = NetworkManager.singleton.GetComponent<NetworkManagerLobby>();

        int numCardsToDraw = 6;
        if (networkManagerObject.GamePlayers.Count == 3)
        {
            numCardsToDraw += 1;
        }

        for (int i = 0; i < numCardsToDraw; i++)
        {
            System.Random random = new System.Random();
            int randomIndex = random.Next(0, cards.Count);
            int chosenCard = cards[randomIndex];

            for (int j = 0; j < networkManagerObject.GamePlayers.Count; j++)
            {
                networkManagerObject.GamePlayers[j].cards.RemoveAt(randomIndex);
            }

            TargetInstantiateDrawnCard(chosenCard);
        }
    }


    [TargetRpc]
    public void TargetInstantiateDrawnCard(int indexForCard)
    {
        GameObject newHandCard = Instantiate(handCardPrefab);
        newHandCard.GetComponent<HandCard>().setIndexInDeck(indexForCard);
        newHandCard.GetComponent<HandCard>().setCardSprite(NetworkManager.singleton.GetComponent<NetworkManagerLobby>().deck[indexForCard]);
        newHandCard.GetComponent<HandCard>().setOwner(localPlayerID);
    }

   
    [Command(requiresAuthority = false)]
    public void CmdSetEachPlayerButtonsOwner(int owner)
    {
        TargetSetEachPlayerButtonsOwner(owner);
    }


    [TargetRpc]
    public void TargetSetEachPlayerButtonsOwner(int owner)
    {
        Transform votingLayoutGroup = GameObject.Find("Voting_Layout_Group").transform;
        foreach (Transform child in votingLayoutGroup)
        {
            child.gameObject.GetComponent<ButtonForVoting>().SetButtonOwner(owner);
        }
    }



    public void GenerateCardsInField(int oldValue, int newValue)
    {
        CmdCheckIfReadyToShowCards(newValue, WhichTurn, localPlayerID, isLocalPlayer);
    }


    [Command(requiresAuthority =false)]
    public void CmdCheckIfReadyToShowCards(int newValue, int WhichTurn, int localPlayerID, bool isLocalPlayer)
    {
        NetworkManagerLobby networkManagerObject = NetworkManager.singleton.GetComponent<NetworkManagerLobby>();
        
        int numOfCardRequired = networkManagerObject.GamePlayers.Count;
        if (networkManagerObject.GamePlayers.Count == 3)
        {
            numOfCardRequired += 2;
        }


        if((newValue == numOfCardRequired) && (WhichTurn == localPlayerID) && isLocalPlayer)
        {
            for(int i = 0; i < numOfCardRequired; i++)
            {
                System.Random random = new System.Random();
                int randomIndex = random.Next(0, ChosenCardsForRound.Count);
                int chosen = ChosenCardsForRound[randomIndex];
                int chosenOwner = ChosenOwnersForRound[randomIndex];

                for(int j = 0; j < networkManagerObject.GamePlayers.Count; j++)
                {
                    networkManagerObject.GamePlayers[j].ChosenOwnersForRound.RemoveAt(randomIndex);
                    networkManagerObject.GamePlayers[j].ChosenCardsForRound.RemoveAt(randomIndex);
                }

                Transform panelCardShowcase = GameObject.Find("Panel_Card_Showcase").transform;
                panelCardShowcase.GetChild(i).gameObject.GetComponent<FieldCard>().owner = chosenOwner;
                panelCardShowcase.GetChild(i).gameObject.GetComponent<FieldCard>().imageOwnerName = networkManagerObject.GamePlayers[chosenOwner].DisplayName;
                panelCardShowcase.GetChild(i).gameObject.GetComponent<FieldCard>().imageIndex = chosen;


                RpcTurnOnButtonForThisCard(chosenOwner, i, WhichTurn);
            }

            for(int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
            {
                if(networkManagerObject.GamePlayers[i].localPlayerID != WhichTurn)
                {
                    networkManagerObject.GamePlayers[i].RpcTurnOnTimersForEveryone();
                }
            }
            
        }
    }


    [ClientRpc]
    public void RpcTurnOnTimersForEveryone()
    {
        if (isLocalPlayer)
        {
            Transform rootTransform = GameObject.Find("Right_Panel").transform;
            foreach (Transform child in rootTransform)
            {
                if (child.name == "Timer_Voting")
                {
                    child.gameObject.SetActive(true);
                    child.gameObject.GetComponent<TimerVoting>().StartTimer(90);
                    break;
                }
            }
        }
        
    }


    [ClientRpc]
    public void RpcTurnOnButtonForThisCard(int chosenOwner, int childIndex, int WhichTurn)
    {
            Transform votingLayoutGroup = GameObject.Find("Voting_Layout_Group").transform;
            votingLayoutGroup.GetChild(childIndex).gameObject.GetComponent<ButtonForVoting>().turnOnIfNotHis(chosenOwner, WhichTurn);
    }



    public void CheckHowManyVotes(int oldValue, int newValue)
    {
        CmdCheckHowManyVotes(newValue, WhichTurn, localPlayerID, isLocalPlayer);
    }


    [Command(requiresAuthority =false)]
    public void CmdCheckHowManyVotes(int newValue, int WhichTurn, int localPlayerID, bool isLocalPlayer)
    {
        NetworkManagerLobby networkManagerObject = NetworkManager.singleton.GetComponent<NetworkManagerLobby>();

        if ((newValue == (networkManagerObject.GamePlayers.Count - 1)) && (WhichTurn == localPlayerID) && isLocalPlayer)
        {

            if ((playersPoints[localPlayerID] == 0))
            {
                for (int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
                {
                    if (i != localPlayerID)
                    {
                        CmdChangeScoreForPlayerProfile(i, playersPoints[i] + 2);
                    }
                }

            }
            else if ((playersPoints[localPlayerID] == (networkManagerObject.GamePlayers.Count - 1)))
            {
                for (int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
                {
                    if (i != localPlayerID)
                    {
                        CmdChangeScoreForPlayerProfile(i, 2);
                    }
                }
            }
            else
            {
                CmdChangeScoreForPlayerProfile(localPlayerID, 3);

                for (int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
                {
                    if (i != localPlayerID)
                    {
                        if (successGuesses.Contains(i))
                        {
                            CmdChangeScoreForPlayerProfile(i, playersPoints[i] + 3);
                        }
                        else
                        {
                            CmdChangeScoreForPlayerProfile(i, playersPoints[i]);
                        }
                    }
                }
            }

            RpcShowFieldCardsOwnerAndNumTapped();

            for (int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
            {
                for(int j = 0; j < networkManagerObject.GamePlayers.Count; j++)
                {
                    networkManagerObject.GamePlayers[i].playersPoints[j] = 0;
                }

                networkManagerObject.GamePlayers[i].successGuesses.Clear();
            }
        }

    }


    [Server]
    public void CmdChangeScoreForPlayerProfile(int childIndex, int amount)
    {
        NetworkManagerLobby networkManagerObject = NetworkManager.singleton.GetComponent<NetworkManagerLobby>();

        Transform scoreLayoutGroup = GameObject.Find("Score_Layout_Group").transform;
        Player_Score playerToGive = scoreLayoutGroup.GetChild(networkManagerObject.GamePlayers.Count - 1 - childIndex).gameObject.GetComponent<Player_Score>();
        playerToGive.Points += amount;
    }


    [ClientRpc]
    public void RpcShowFieldCardsOwnerAndNumTapped()
    {
        Transform panelCardShowcase = GameObject.Find("Panel_Card_Showcase").transform;
        foreach (Transform child in panelCardShowcase)
        {
            child.gameObject.GetComponent<FieldCard>().showResults();
        }

        Transform pickOneCardPanel = GameObject.Find("Panel_Pick_One_Card").transform;
        pickOneCardPanel.GetChild(1).gameObject.SetActive(true);
        pickOneCardPanel.GetChild(1).gameObject.GetComponent<TimerNewRound>().StartTimer(15);
    }


    public void ResetRoundAndReadyForNewOne(int oldValue, int newValue)
    {
        CmdResetEverythingForNextRound(newValue, WhichTurn, localPlayerID, isLocalPlayer, cards.Count);
    }


    [Command(requiresAuthority = false)]
    public void CmdResetEverythingForNextRound(int newValue, int WhichTurn, int localPlayerID, bool isLocalPlayer, int deckCardCount)
    {
        NetworkManagerLobby networkManagerObject = NetworkManager.singleton.GetComponent<NetworkManagerLobby>();

       

        if ((newValue == networkManagerObject.GamePlayers.Count) && (WhichTurn == localPlayerID) && isLocalPlayer)
        {

            if (ServerCheckIfEndOfGame(deckCardCount))
            {
                return;
            }

            for (int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
            {
                networkManagerObject.GamePlayers[i].numReadyCardsForField = 0;
                networkManagerObject.GamePlayers[i].numOfVotes = 0;
            }

            for (int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
            {
                networkManagerObject.GamePlayers[i].RpcRequestToDealCards();
            }

            Transform panelCardShowcase = GameObject.Find("Panel_Card_Showcase").transform;
            foreach (Transform child in panelCardShowcase)
            {
                child.gameObject.GetComponent<FieldCard>().numTaps = 0;
            }
            RpcDisableResultInfoAndClearFieldCard();

            int newWhichTurn;
           
            if(WhichTurn == networkManagerObject.GamePlayers.Count - 1)
            {
                newWhichTurn = 0;
            }
            else
            {
                newWhichTurn = WhichTurn + 1;
            }

            GameObject gameManager = GameObject.Find("GameManager(Clone)");
            gameManager.GetComponent<GameManager>().ChangeWhoGivesAssociation(newWhichTurn);


            for (int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
            {
                networkManagerObject.GamePlayers[i].resetRound = 0;
            }

        }

    }


    [Server]
    public bool ServerCheckIfEndOfGame(int deckCardCount)
    {
        NetworkManagerLobby networkManagerObject = NetworkManager.singleton.GetComponent<NetworkManagerLobby>();

        int cardsNeedForNextRound = networkManagerObject.GamePlayers.Count;
        if (networkManagerObject.GamePlayers.Count == 3)
        {
            cardsNeedForNextRound += 2;
        }

        Transform scoreLayoutGroup = GameObject.Find("Score_Layout_Group").transform;
        foreach (Transform child in scoreLayoutGroup)
        {
            if ((child.gameObject.GetComponent<Player_Score>().Points >= 30) || (deckCardCount < cardsNeedForNextRound))
            {
                for (int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
                {
                    networkManagerObject.GamePlayers[i].EndOfGame = true;
                }
                return true;
            }

        }
        return false;
    }



    [ClientRpc]
    public void RpcDisableResultInfoAndClearFieldCard()
    {
        Transform panelCardShowcase = GameObject.Find("Panel_Card_Showcase").transform;
        foreach (Transform fieldCardTransform in panelCardShowcase)
        {
            fieldCardTransform.gameObject.GetComponent<FieldCard>().clearCard();
            fieldCardTransform.gameObject.GetComponent<FieldCard>().nameResultPlaceholder.SetActive(false);
            fieldCardTransform.gameObject.GetComponent<FieldCard>().tapsResultPlaceholder.SetActive(false);
        }
    }


    [ClientRpc]
    public void RpcRequestToDealCards()
    {
        if (isLocalPlayer)
        {
            Transform panelHandCards = GameObject.Find("Panel_Hand_Cards").transform;
            CmdDealCardsNotFirstHand(panelHandCards.childCount);
        }
        
    }


    [Command(requiresAuthority =false)]
    public void CmdDealCardsNotFirstHand(int numOfCardsInHand)
    {
        NetworkManagerLobby networkManagerObject = NetworkManager.singleton.GetComponent<NetworkManagerLobby>();

        int numCardsToDraw = 6 - numOfCardsInHand;
        if (networkManagerObject.GamePlayers.Count == 3)
        {
            numCardsToDraw += 1;
        }

        for (int i = 0; i < numCardsToDraw; i++)
        {
            System.Random random = new System.Random();
            int randomIndex = random.Next(0, cards.Count);
            int chosenCard = cards[randomIndex];

            for (int j = 0; j < networkManagerObject.GamePlayers.Count; j++)
            {
                networkManagerObject.GamePlayers[j].cards.RemoveAt(randomIndex);
            }

            TargetInstantiateDrawnCard(chosenCard);
        }
    }



    public void EnableLeaderboardPage(bool oldValue, bool newValue)
    {
        if (isLocalPlayer && newValue)
        {
            Transform rootTransform = GameObject.Find("Canvas_MainScene").transform;
            foreach (Transform child in rootTransform)
            {
                if (child.name == "Leaderboard_Panel")
                {
                    child.gameObject.SetActive(true);
                }
            }

            List<Player_Score> playerFinalResults = new List<Player_Score>();

            Transform scoreLayoutGroup = GameObject.Find("Score_Layout_Group").transform;
            foreach (Transform child in scoreLayoutGroup)
            {
                playerFinalResults.Add(child.gameObject.GetComponent<Player_Score>());
            }

            for (int j = 0; j < 3; j++)
            {
                int maxIndex = -1;
                int maxPoints = -1;

                for(int i = 0; i < playerFinalResults.Count; i++)
                {
                    if(playerFinalResults[i].Points > maxPoints)
                    {
                        maxIndex = i;
                        maxPoints = playerFinalResults[i].Points;
                    }
                }

                Transform podium = GameObject.Find("Podium_Player_" + (j+1)).transform;
                podium.GetChild(0).gameObject.GetComponent<TMP_Text>().text = playerFinalResults[maxIndex].Player_name;
                podium.GetChild(1).gameObject.GetComponent<Image>().sprite = playerFinalResults[maxIndex].spritesList[playerFinalResults[maxIndex].imageNum];
                podium.GetChild(2).gameObject.GetComponent<TMP_Text>().text = maxPoints.ToString();

                playerFinalResults.RemoveAt(maxIndex);
            }

        }
    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 