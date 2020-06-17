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

        StartCoroutine(UpdateUI());
    }

    void FixedUpdate()
    {
        if(timerRunning && isLocalPlayer)
        {
            playerInfo.totalTime += Time.fixedDeltaTime;//deltaTime;
            playerInfo.currentLapTime += Time.fixedDeltaTime;//deltaTime;

            //uIManager.UpdateTime(playerInfo.currentLapTime, playerInfo.totalTime);
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

    IEnumerator UpdateUI()
    {
        yield return new WaitForSecondsRealtime(0.1f);

        uIManager.UpdateTime(playerInfo.currentLapTime, playerInfo.totalTime);

        StartCoroutine(UpdateUI());
    }

    public void GetFinalTimes()
    {
        CmdChangeTotalTime(playerInfo.totalTime);
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
