using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Countdown : MonoBehaviour
{
    private Text countdownText;
    private PolePositionManager polePositionManager;

    public void StartCountdown()
    {
        countdownText = GetComponent<Text>();
        polePositionManager = FindObjectOfType<PolePositionManager>();

        StartCoroutine(DecreaseCountdown());
    }

    //Muestra ready... set.. Go! En pantalla esperando el tiempo indicado
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
}