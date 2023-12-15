using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class HandCard : NetworkBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject imageContainerPanel = null;
    [SerializeField] private Sprite cardSprite = null;
    [SerializeField] private int owner = -1;
    [SerializeField] private int indexInDeck = -1;
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;

    [SerializeField] private Transform parentReturnTo = null;
    [SerializeField] private Vector3 lastPositionVector3;
    [SerializeField] private Quaternion lastPositionQuaternion;

    [SerializeField] private GameObject emptyCardPrefab = null;
    [SerializeField] private int indexInHand = -1;
    [SerializeField] private GameObject temp;
    

    private void Start()
    {
        GameObject parent = GameObject.Find("Panel_Hand_Cards");
        gameObject.transform.SetParent(parent.transform, false);

        imageContainerPanel.GetComponent<Image>().sprite = cardSprite;

        canvas = GameObject.Find("Canvas_MainScene").GetComponent<Canvas>();
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        rectTransform = gameObject.GetComponent<RectTransform>();
    }

    public int getIndexInDeck()
    {
        return indexInDeck;
    }

    public void setIndexInDeck(int indexInDeckToSet)
    {
        indexInDeck = indexInDeckToSet;
    }

    public void setParentReturnTo(Transform parent)
    {
        parentReturnTo = parent;
    }

    public Sprite getCardSprite()
    {
        return cardSprite;
    }
    public void setCardSprite(Sprite sprite)
    {
        cardSprite = sprite;
    }

    public void setOwner(int clientIDOwner)
    {
        owner = clientIDOwner;
    }

    public int getOwner()
    {
        return owner;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
       
        parentReturnTo = this.transform.parent;
        rectTransform.GetPositionAndRotation(out lastPositionVector3, out lastPositionQuaternion);
       
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
       
        if (parentReturnTo != this.transform.parent)
        {
            Destroy(gameObject);
        }
        transform.SetParent(parentReturnTo, false);
        gameObject.GetComponent<RectTransform>().SetPositionAndRotation(lastPositionVector3, lastPositionQuaternion);
        canvasGroup.blocksRaycasts = true;
       
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }
}
