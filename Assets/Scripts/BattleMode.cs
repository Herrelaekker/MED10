using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BattleMode : MonoBehaviour
{
    WaveSpawner waveSpawner;

    [System.Serializable]
    public class OnBattleStart : UnityEvent { }
    public OnBattleStart onBattleStart;

    [System.Serializable]
    public class OnBattleEnd : UnityEvent { }
    public OnBattleEnd onBattleEnd;

    private void Start()
    {
        waveSpawner = FindObjectOfType<WaveSpawner>();
    }

    public void StartBattle()
    {
        onBattleStart.Invoke();
        waveSpawner.StartSpawning();
    }

    public void EndBattle()
    {
        onBattleEnd.Invoke();
        waveSpawner.StopSpawning();
    }
}
