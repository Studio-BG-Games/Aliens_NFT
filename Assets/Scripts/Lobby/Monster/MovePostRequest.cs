using System;
using System.Collections;
using System.Text;
using MapSpase.Hexagon;
using MonsterSpace;
using UnityEngine;
using UnityEngine.Networking;

public partial class MovePostRequest : MonoBehaviour
{
    public void TryPost(HexagonModel hexagon, IMonster monster, Action<bool> actionBoll) => StartCoroutine(TrySendRequestToServer(hexagon, monster, new BollLogic(actionBoll)));
    public void TryPost(HexagonModel hexagon, Monster monster, Action<Monster, HexagonModel> actionMonster = null, Action<bool> actionBoll = null) 
    {
        ILogic logic = null; 
        ILogic secondLogic = null;

        if (actionMonster != null)
            logic = new MonsterLogic(actionMonster, monster, hexagon);
        if (actionBoll != null)
            secondLogic = new BollLogic(actionBoll);

        StartCoroutine(TrySendRequestToServer(hexagon, monster, logic, secondLogic)); 
    }

    private IEnumerator TrySendRequestToServer(HexagonModel hexagon, IMonster monster, ILogic logic, ILogic secondLogic = null)
    {
        string url = HttpAddresses.Move + hexagon.Cell.Id;

        WWWForm formData = new WWWForm();

        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, formData))
        {
            PostMoveStruct post = new PostMoveStruct()
            {
                monster_id = monster.Id,
                player_id = monster.PlayerId
            };

            string json = JsonUtility.ToJson(post);

            byte[] psotBytes = Encoding.UTF8.GetBytes(json);

            UploadHandler uploadHandler = new UploadHandlerRaw(psotBytes);

            webRequest.uploadHandler = uploadHandler;
            webRequest.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
                HttpAddresses.ErrorHandling(webRequest, url);

            PostMoveData postAnswer = JsonUtility.FromJson<PostMoveData>(webRequest.downloadHandler.text);

            if (logic != null)
                logic.Go(post.monster_id == postAnswer.data.monster_id);
            if (secondLogic != null)
                secondLogic.Go(post.monster_id == postAnswer.data.monster_id);
        }
    }

    private struct BollLogic : ILogic
    {
        private Action<bool> _action;

        public BollLogic(Action<bool> action)
        {
            _action = action;
        }

        public void Go(bool result)
        {
            _action?.Invoke(result);
        }
    }

    private struct MonsterLogic : ILogic
    {
        private Action<Monster, HexagonModel> _action;
        private Monster _monster;
        private HexagonModel _hexagon;

        public MonsterLogic(Action<Monster, HexagonModel> action, Monster monster, HexagonModel hexagon)
        {
            _action = action;
            _monster = monster;
            _hexagon = hexagon;
        }

        public void Go(bool result)
        {
            if(result)
                _action?.Invoke(_monster, _hexagon);
        }
    }

    private interface ILogic 
    {
        public void Go(bool result);
    }
}
