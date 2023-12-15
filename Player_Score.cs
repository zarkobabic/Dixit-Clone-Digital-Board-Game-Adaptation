using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System;

public class Player_Score : NetworkBehaviour
{

    [SerializeField] private TMP_Text text_Player_Score_Name;
    [SerializeField] private GameObject image_Player_Score;
    [SerializeField] private TMP_Text Text_Player_Score_Points;

    [SerializeField] public Sprite[] spritesList = new Sprite[6];

    [SyncVar(hook = nameof(HandlePointsChanged))]
    [SerializeField] public int Points = 0;
    
    [SyncVar]
    [SerializeField]
    public string Player_name;
    
    [SyncVar]
    [SerializeField]
    public int imageNum;



    [Client]
    public override void OnStartClient()
    {
        GameObject parent = GameObject.Find("Score_Layout_Group");
        gameObject.transform.SetParent(parent.transform, false);

        Text_Player_Score_Points.text = Points.ToString();
        text_Player_Score_Name.text = Player_name;
        image_Player_Score.GetComponent<Image>().sprite = spritesList[imageNum];
        base.OnStartClient();
    }

    private void HandlePointsChanged(int oldValue, int newValue)
    {
        Text_Player_Score_Points.text = newValue.ToString();
    }


}
