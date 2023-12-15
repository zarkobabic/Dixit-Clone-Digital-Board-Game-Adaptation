using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System;


public class FieldCard : NetworkBehaviour
{
    [SyncVar]
    [SerializeField]
    public int idNum;
    
    [SyncVar]
    [SerializeField]
    public int owner = -1;
    
    [SyncVar(hook = nameof(HandlePictureChanged))]
    [SerializeField]
    public int imageIndex = -1;

    [SyncVar]
    [SerializeField]
    public string imageOwnerName;

    [SyncVar(hook = nameof(HandleNumTapsChanged))]
    [SerializeField]
    public int numTaps = 0;


    
    [SerializeField] private TMP_Text Text_Panel_Vote_Id;
    [SerializeField] private GameObject imagePlaceholder;
    [SerializeField] public GameObject nameResultPlaceholder;
    [SerializeField] public GameObject tapsResultPlaceholder;


    [Client]
    public override void OnStartClient()
    {
        GameObject parent = GameObject.Find("Panel_Card_Showcase");
        gameObject.transform.SetParent(parent.transform, false);
        Text_Panel_Vote_Id.text = idNum.ToString();
        base.OnStartClient();
    }

    public void HandlePictureChanged(int oldValue, int newValue)
    {
        if (newValue != (-1))
        {
            imagePlaceholder.GetComponent<Image>().sprite = NetworkManager.singleton.GetComponent<NetworkManagerLobby>().deck[newValue];
        }
    }

    
    public void HandleNumTapsChanged(int oldValue, int newValue)
    {
        tapsResultPlaceholder.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = newValue.ToString();
    }

    public void showResults()
    {
        nameResultPlaceholder.SetActive(true);
        nameResultPlaceholder.transform.GetChild(0).GetComponent<TMP_Text>().text = imageOwnerName;
        tapsResultPlaceholder.SetActive(true);
        if(tapsResultPlaceholder.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text == "")
        {
            tapsResultPlaceholder.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = "0";
        }
    }

    public void clearCard()
    {
        imagePlaceholder.GetComponent<Image>().sprite = null;
    }



}
