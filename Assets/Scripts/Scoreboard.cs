using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Scoreboard : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform container;
    [SerializeField] GameObject scoreboardItemPrefab;

    Dictionary<Player, ScoreboardItem> scoreboardItems = new Dictionary<Player, ScoreboardItem>();
    bool tab;

    void Start()
    {
        scoreboardItemPrefab.SetActive(true);
        tab = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (tab)
            {
                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    RemoveScoreboardItem(player);
                }
            }
            else
            {
                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    AddScoreboardItem(player);
                }
            }
            tab = !tab;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (tab)
        {
            AddScoreboardItem(newPlayer);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (tab)
        {
            RemoveScoreboardItem(otherPlayer);
        }
    }

    void AddScoreboardItem(Player player)
    {
        ScoreboardItem item = Instantiate(scoreboardItemPrefab, container).GetComponent<ScoreboardItem>();
        item.Initialize(player);
        scoreboardItems[player] = item;
    }

    void RemoveScoreboardItem(Player player)
    {
        Destroy(scoreboardItems[player].gameObject);
        scoreboardItems.Remove(player);
    }
}
