using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerManager : MonoBehaviour
{
    PhotonView PV;
    GameObject controller;
    int deaths;

    private void Awake()
    {
        deaths = 0;
        PV = GetComponent<PhotonView>();
    }
    // Start is called before the first frame update
    void Start()
    {
        if (PV.IsMine)
        {
            CreateController();
            Hashtable hash = new Hashtable
            {
                { "kills", 0 },
                { "deaths", deaths }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    void CreateController()
    {
        Transform spawnpoint = SpawnManager.Instance.GetSpawnpoint();
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnpoint.position, spawnpoint.rotation, 0, new object[] { PV.ViewID });
    }

    public void Die()
    {
        deaths++;
        Hashtable hash = new Hashtable();
        hash.Add("deaths", deaths);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        PhotonNetwork.Destroy(controller);
        CreateController();
    }

    public void kill(int id)
    {
        int kills = (int)PhotonNetwork.PlayerList[0].Get(id).CustomProperties["kills"];
        Hashtable hash = new Hashtable();
        hash.Add("kills", kills+1);
        PhotonNetwork.PlayerList[0].Get(id).SetCustomProperties(hash);
    }
}
