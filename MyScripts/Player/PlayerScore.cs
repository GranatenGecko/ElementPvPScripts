using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    public int playerKills;
    public int playerDeaths;

    private void Start()
    {
        InitializePlayerStats();
    }

    private void InitializePlayerStats()
    {
        playerKills = 0;
        playerDeaths = 0;
    }
}
