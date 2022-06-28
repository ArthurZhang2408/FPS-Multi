using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ScoreboardItem : MonoBehaviourPunCallbacks
{
    public TMP_Text usernameText, killsText, deathsText;
    public Player myPlayer;

    public void Initialize(Player player)
    {
        usernameText.text = player.NickName;
        myPlayer = player;
        Hashtable hash = player.CustomProperties;
        if (hash.ContainsKey("kills")) killsText.text = ((int)hash["kills"]).ToString();
        if(hash.ContainsKey("deaths")) deathsText.text = ((int)hash["deaths"]).ToString();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer == myPlayer)
        {
            if (changedProps.ContainsKey("kills"))
            {
                killsText.text = ((int)changedProps["kills"]).ToString();
            }
            if (changedProps.ContainsKey("deaths"))
            {
                deathsText.text = ((int)changedProps["deaths"]).ToString();
            }
        }
    }
}
