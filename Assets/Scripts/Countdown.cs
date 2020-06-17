using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Countdown : MonoBehaviour
{
    //public int currentTime;
    private Text countdownText;
    private PolePositionManager polePositionManager;

    void Start()
    {
        
    }

    IEnumerator DecreaseCountdown()
    {
        countdownText.text = "READY...";
        yield return new WaitForSecondsRealtime(polePositionManager.countdown/2f);

        countdownText.text = "SET...";
        yield return new WaitForSecondsRealtime(polePositionManager.countdown/2f);

        countdownText.text = "GO!";
        yield return new WaitForSecondsRealtime(0.5f);
        this.gameObject.SetActive(false);
    }

    public void StartCountdown()
    {
        countdownText = GetComponent<Text>();
        polePositionManager = FindObjectOfType<PolePositionManager>();
        //currentTime = FindObjectOfType<PolePositionManager>().countdown;
        //countdownText.text = currentTime.ToString();
        StartCoroutine(DecreaseCountdown());
    }
}
