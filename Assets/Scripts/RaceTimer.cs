using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RaceTimer : NetworkBehaviour
{
    PlayerInfo playerInfo;
    UIManager uIManager;

    bool timerRunning = true;
    int index = 0;

    [SyncVar] public float totalTime = 0f;
    float[] lapTimes;

    private void Start()
    {
        playerInfo = GetComponent<PlayerInfo>();
        //lapTimes = new float[FindObjectOfType<RaceNetworkBehaviour>().GetMaxLaps()];
        uIManager = FindObjectOfType<UIManager>();
    }

    void Update()
    {
        if(timerRunning && isLocalPlayer)
        {
            //Truncar a dos decimales
            float deltaTime = Mathf.Round(Time.deltaTime * 100f) * 0.01f;

            lapTimes[playerInfo.CurrentLap] += deltaTime;
            CmdAddTotalTime(deltaTime);
            uIManager.UpdateTime(lapTimes[playerInfo.CurrentLap], totalTime);
        }
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public float GetTotalTime()
    {
        return totalTime;
    }

    public float[] GetLapTimes()
    {
        return lapTimes;
    }

    [Command]
    public void CmdAddTotalTime(float time)
    {
        totalTime += time;
    }
}
