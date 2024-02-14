using UnityEngine;

public class GuildPlayerManager : MonoBehaviour
{
    public GuildInfo parentGuildInfo;
    public GuildPlayers parentGuildPlayers;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (parentGuildPlayers.actualAmount != parentGuildPlayers.maxAmount)
            {
                PlayerInfo player = other.gameObject.transform.parent.GetComponent<PlayerInfo>();
                player.playerElement = parentGuildInfo.guildElement;
                parentGuildPlayers.PlayerJoinGuild(other.gameObject);
            }
        }
    }
}
