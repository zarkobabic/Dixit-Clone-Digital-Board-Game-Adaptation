using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerNewRound : MonoBehaviour
{
    [SerializeField] private TMP_Text TimerText;
    [SerializeField] private Image TimerFill;
    [SerializeField] private GameObject managerForLocalPlayer = null;

    private int duration;
    private int remainingDuration;

    public void StartTimer(int second)
    {
        duration = second;
        remainingDuration = second;
        StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        while (remainingDuration >= 0)
        {
            TimerText.text = $"{remainingDuration / 60:00} : {remainingDuration % 60:00}";
            TimerFill.fillAmount = Mathf.InverseLerp(0, duration, remainingDuration);
            remainingDuration--;
            yield return new WaitForSeconds(1f);
        }
        CountdownEnd();
    }

    private void CountdownEnd()
    {
        managerForLocalPlayer.GetComponent<AssociationForRound>().SetFlagToResetAndReadyForNewRound();
        gameObject.SetActive(false);
    }

}
