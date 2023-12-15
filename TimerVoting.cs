using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerVoting : MonoBehaviour
{
    [SerializeField] private TMP_Text TimerText;
    [SerializeField] private Image TimerFill;

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
        while (remainingDuration >= 0)
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
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
    }

    private void CountdownEnd()
    {

        Transform votingGroup = GameObject.Find("Voting_Layout_Group").transform;

        List<ButtonForVoting> availableButtons = new List<ButtonForVoting>();

        foreach (Transform button_for_voting in votingGroup)
        {
            if (button_for_voting.GetChild(1).GetComponent<Button>().interactable)
            {
                availableButtons.Add(button_for_voting.gameObject.GetComponent<ButtonForVoting>());
            }
        }


        System.Random random = new System.Random();
        int randomIndexFromAvailableButtons = random.Next(0, availableButtons.Count);

        ButtonForVoting chosenButtonForVoting = availableButtons[randomIndexFromAvailableButtons];

        chosenButtonForVoting.VoteForThisButtonEnteredAutomaticWithTimer();
        gameObject.SetActive(false);
    }

}
