using MonsterSpace;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AskingLobbyMembers : MonoBehaviour
{
    public event Action<Player> PlayerReceived;

    public void Asking(string playerId)
    {
        StartCoroutine(LoadLobbyMembers(playerId));
    }

    private IEnumerator LoadLobbyMembers(string url)
    {
        Player player;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
                HttpAddresses.ErrorHandling(webRequest, url);

            PlayerInfoData playerInfoData = JsonUtility.FromJson<PlayerInfoData>(webRequest.downloadHandler.text);
            player = CreatePlayer(playerInfoData);
        }

        PlayerReceived?.Invoke(player);
    }

    private Player CreatePlayer(PlayerInfoData playerInfoData)
    {
        List<Monster> monsters = new List<Monster>();

        foreach (var monsterInfo in playerInfoData.data.monsters)
            monsters.Add(new Monster(monsterInfo));

        return new Player(playerInfoData.data.player, monsters);
    }
}
