using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RaceTimer : NetworkBehaviour
{
    PlayerInfo playerInfo;

    bool timerRunning = true;
    int index = 0;

    [SyncVar] float totalTime = 0f;
    float[] lapTimes;

    private void Start()
    {
        playerInfo = GetComponent<PlayerInfo>();
        lapTimes = new float[FindObjectOfType<RaceNetworkBehaviour>().GetMaxLaps()];
    }

    void Update()
    {
        if(timerRunning)
        {
            //Truncar a dos decimales
            float deltaTime = Mathf.Round(Time.deltaTime * 100f) * 0.01f;

            lapTimes[playerInfo.CurrentLap] += deltaTime;
            totalTime += deltaTime;
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

    public SyncListFloat GetLapTimes()
    {
        SyncListFloat lapTimesList = new SyncListFloat();

        foreach(float f in lapTimes)
        {
            lapTimesList.Add(f);
        }
        
        return lapTimesList;
    }
}
