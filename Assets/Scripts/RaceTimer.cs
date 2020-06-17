using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class RaceTimer : NetworkBehaviour
{
    PlayerInfo playerInfo;

    bool timerRunning = true;

    public static event Action<PlayerInfo, float> OnTotalTime;
    public static event Action<PlayerInfo, float> OnLapTime;

    UIManager uIManager;

    private void Start()
    {
        playerInfo = GetComponent<PlayerInfo>();
        uIManager = FindObjectOfType<UIManager>();

        StartCoroutine(NotifyTimeChange());
    }

    void Update()
    {
        if(timerRunning && isLocalPlayer)
        {
            //Truncar a dos decimales
            float deltaTime = Mathf.Round(Time.deltaTime * 100f) * 0.01f;

            playerInfo.totalTime += deltaTime;
            playerInfo.currentLapTime += deltaTime;

            uIManager.UpdateTime(playerInfo.currentLapTime, playerInfo.totalTime);
        }
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public void ResetLapTime()
    {
        playerInfo.currentLapTime = 0f;
    }

    IEnumerator NotifyTimeChange()
    {
        yield return new WaitForSecondsRealtime(2f);

        CmdChangeLapTime(playerInfo.currentLapTime);
        CmdChangeTotalTime(playerInfo.totalTime);

        StartCoroutine(NotifyTimeChange());
    }

    //Command y Rpc para cambiar totalTime
    [Command]
    public void CmdChangeTotalTime(float totalTime)
    {
        RpcChangeTotalTime(totalTime);
    }

    [ClientRpc]
    public void RpcChangeTotalTime(float totalTime)
    {
        OnTotalTime?.Invoke(playerInfo, totalTime);
    }

    //Command y Rpc para cambiar LapTime
    [Command]
    public void CmdChangeLapTime(float lapTime)
    {
        RpcChangeLapTime(lapTime);
    }

    [ClientRpc]
    public void RpcChangeLapTime(float lapTime)
    {
        OnLapTime?.Invoke(playerInfo, lapTime);
    }
}
