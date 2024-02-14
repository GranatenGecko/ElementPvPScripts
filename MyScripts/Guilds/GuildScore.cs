using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuildScore : MonoBehaviour
{
    public int guildPoints;
    public int guildKills;
    public int guildDeaths;

    void Start()
    {
        InitializeGuildStats(0, 0, 0);
    }

    private void InitializeGuildStats(int _guildPoints, int _guildKills, int _guildDeaths)
    {
        guildPoints = _guildPoints;
        guildKills = _guildKills;
        guildDeaths = _guildDeaths;
    }
}
