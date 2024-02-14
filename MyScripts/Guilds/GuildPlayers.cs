using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuildPlayers : MonoBehaviour
{
    public int maxAmount, actualAmount;
    [Space]
    public GameObject[] guildPlayers;

    void Start()
    {
        InitializePlayerSlots();
    }

    private void InitializePlayerSlots()
    {
        guildPlayers = new GameObject[4];
        maxAmount = guildPlayers.Length;
        actualAmount = 0;
    }

    public void PlayerJoinGuild(GameObject player)
    {
        CheckIfPlayerIsInGuild(player);
    }

    // Checks if player is in guild
    private bool CheckIfPlayerIsInGuild(GameObject player)
    {
        for (int playerSlot = 0; playerSlot < guildPlayers.Length; playerSlot++)
        {
            if (guildPlayers[playerSlot] == player)
            {
                return true;
            }
        }
        AsignPlayerToSlotInGuild(player);
        return false;
    }

    // Update guild player slots
    private void AsignPlayerToSlotInGuild(GameObject player)
    {
        // loop through player Slots
        for (int playerSlot = 0; playerSlot < guildPlayers.Length; playerSlot++)
        {
            // search for a free slot
            if (guildPlayers[playerSlot] == null)
            {
                RemovePlayerFromOtherGuild(player);
                UpdatePlayerGuildStatus(player, playerSlot);

                guildPlayers[playerSlot] = player;

                return;
            }
        }
    }


    // Reset slot of players previous guild
    private void RemovePlayerFromOtherGuild(GameObject player)
    {
      //  if (player.GetComponent<PlayerInfo>().playerActiveGuild != null)
      //  {
      //      //GameObject playerPreviousGuild = player.GetComponent<PlayerInfo>().playerActiveGuild;
     //       int playerPreviousGuildSlot = player.GetComponent<PlayerInfo>().playerActiveGuildSlot;
     //
     //       //playerPreviousGuild.GetComponent<GuildPlayers>().guildPlayers[playerPreviousGuildSlot] = null;
     //   }
    }

    private void UpdatePlayerGuildStatus(GameObject player, int playerSlot)
    {
        //player.GetComponent<PlayerInfo>().playerActiveGuild = gameObject;
       // player.GetComponent<PlayerInfo>().playerActiveGuildSlot = playerSlot;
    }
}
