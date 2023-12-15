using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

public class DropCardPanel : NetworkBehaviour, IDropHandler
{

    [SerializeField] private GameObject dropingTimer = null;

    public int TurnId = -1;
    public int playersNb = -1;
    public bool alreadyGiven = false;

    //--------------------------------------------KADA SE KARTA DROPUJE U DROP ZONU-----------------------------------
    public void OnDrop(PointerEventData eventData)
    {

        if(eventData.pointerDrag != null){
            eventData.pointerDrag.GetComponent<HandCard>().setParentReturnTo(gameObject.GetComponent<Transform>());
            CmdAddChosenCardForThisRound(eventData.pointerDrag.GetComponent<HandCard>().getIndexInDeck(), eventData.pointerDrag.GetComponent<HandCard>().getOwner());

            if (eventData.pointerDrag.GetComponent<HandCard>().getOwner() != TurnId && playersNb == 3 && !alreadyGiven)
            {
                alreadyGiven = true;
                dropingTimer.GetComponent<Timer>().CancelTimer();
                dropingTimer.GetComponent<Timer>().StartTimer(60);
            }
            else
            {
                alreadyGiven = false;
                dropingTimer.GetComponent<Timer>().CancelTimer();
                dropingTimer.SetActive(false);
                gameObject.SetActive(false);
            }
        }
    }

    public void OnDropAutomaticFromTimer(HandCard chosenHandCard)
    {
        CmdAddChosenCardForThisRound(chosenHandCard.getIndexInDeck(), chosenHandCard.getOwner());

        if (chosenHandCard.getOwner() != TurnId && playersNb == 3 && !alreadyGiven)
        {
            alreadyGiven = true;
            dropingTimer.GetComponent<Timer>().CancelTimer();
            dropingTimer.GetComponent<Timer>().StartTimer(60);
        }
        else
        {
            alreadyGiven = false;
            dropingTimer.GetComponent<Timer>().CancelTimer();
            dropingTimer.SetActive(false);
            gameObject.SetActive(false);
        }
    }





    [Command(requiresAuthority = false)]
    public void CmdAddChosenCardForThisRound(int indexInDeck, int owner)
    {
        NetworkManagerLobby networkManagerObject = NetworkManager.singleton.GetComponent<NetworkManagerLobby>();

        for (int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
        {
            networkManagerObject.GamePlayers[i].ChosenCardsForRound.Add(indexInDeck);
            networkManagerObject.GamePlayers[i].ChosenOwnersForRound.Add(owner);
        }
        
        for (int i = 0; i < networkManagerObject.GamePlayers.Count; i++)
        {
            networkManagerObject.GamePlayers[i].numReadyCardsForField++;
        }

    }

    public GameObject getDropZoneTimer()
    {
        return dropingTimer;
    }

}

