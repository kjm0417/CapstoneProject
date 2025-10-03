using System.Collections.Generic;
using UnityEngine;

public class MiniGameManager : MonoBehaviour 
    //EffectManager로 부터 받은 effectId(미니게임 ID)와 매핑하여 해당 미니게임 시작 
{
    public static string OnMiniGameStarted = "OnMiniGameStartEvent";

    private Dictionary<string, MiniGameBase> miniGameMap = new Dictionary<string, MiniGameBase>();

    private void Awake()
    {
        MiniGameBase[] allMiniGames = FindObjectsOfType<MiniGameBase>(true); //비활성화된 오브젝트도 찾음
        foreach (var game in allMiniGames)
        {
            if (!miniGameMap.ContainsKey(game.Id))
            {
                miniGameMap.Add(game.Id, game);
            }
            else
            {
                Debug.LogWarning($"중복된 MiniGame ID 발견: {game.Id}");
            }
        }

    }

    private void OnEnable()
    {
        EventBus.Subscribe<string>(OnMiniGameStarted, StartMiniGameById);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<string>(OnMiniGameStarted, StartMiniGameById);
    }



    private void StartMiniGameById(string id)
    {
        if (miniGameMap.TryGetValue(id, out var miniGame))
        {
            Debug.Log($"미니게임 시작: {miniGame.Id}");
            miniGame.Init();
            miniGame.StartGame();
        }
        else
        {
            Debug.LogWarning($"MiniGame ID {id} 에 해당하는 미니게임 없음");
        }
    }
}
