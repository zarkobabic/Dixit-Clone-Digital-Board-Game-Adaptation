using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] private TMP_Text TimerText;
    [SerializeField] private Image TimerFill;

    [SerializeField] private GameObject dropCardPanelForTimer = null;

    private int duration;
    private int remainingDuration;
    private Coroutine timerCoroutine;

    public void StartTimer(int second)
    {
        duration = second;
        remainingDuration = second;
        timerCoroutine = StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        while(remainingDuration >= 0)
        {
            TimerText.text = $"{remainingDuration / 60:00} : {remainingDuration % 60:00}";
            TimerFill.fillAmount = Mathf.InverseLerp(0, duration, remainingDuration);
            remainingDuration--;
            yield return new WaitForSeconds(1f);
        }
        CountdownEnd();
    }

    public void CancelTimer()
    {
        if(timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
    }

    private void CountdownEnd()
    {
        Transform handCards = GameObject.Find("Panel_Hand_Cards").transform;
        
        System.Random random = new System.Random();
        int randomIndexInHand = random.Next(0, handCards.childCount);
        
        HandCard chosenCardForDrop = handCards.GetChild(randomIndexInHand).gameObject.GetComponent<HandCard>();
        
        dropCardPanelForTimer.GetComponent<DropCardPanel>().OnDropAutomaticFromTimer(chosenCardForDrop);
        Destroy(chosenCardForDrop.gameObject);
    }

}
