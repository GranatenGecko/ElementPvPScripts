using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public string playerName;
    public int playerID;
    [Space]
    public Element playerElement;
    public int playerActiveGuildSlot;

    private void Start()
    {
        InitializePlayerStats();
    }

    private void InitializePlayerStats()
    {
        playerName = gameObject.name;
        playerID = 0;
    }
}
