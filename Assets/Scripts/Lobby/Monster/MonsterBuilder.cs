using MapSpase.Hexagon;
using System;
using UnityEngine;

namespace MonsterSpace.Build
{   
    [RequireComponent(typeof(MovePostRequest))]
    public class MonsterBuilder : MonoBehaviour
    {
        private MovePostRequest _moveRequest;

        private void Awake()
        {
            _moveRequest = GetComponent<MovePostRequest>();
        }

        public void TryBuildCheckedOnServer(Monster monster, HexagonModel hexagon, Action<bool> actionBoll = null) => _moveRequest.TryPost(hexagon, monster, TryBuild, actionBoll);

        public void TryBuild(Monster monster, HexagonModel hexagon)
        {
            if (CreateMonster(monster, hexagon) != null)
                monster.PlaceOnMap();
        }

        private MonsterGameModel CreateMonster(Monster monster, HexagonModel hexagon)
        {
            if (hexagon.MonsterGameModel != null)
                throw new InvalidOperationException($"{nameof(HexagonModel)}.{nameof(MonsterGameModel)} not null");

            GameObject prefab = Resources.Load(MonsterResourcePaths.Model.GetPaths(monster.RaceId, monster.ModelId)) as GameObject;

            if (prefab == null)
                throw new ArgumentNullException("Prefab is null");

            MonsterGameModel monsterGameModel = Instantiate(prefab, hexagon.transform).GetComponent<MonsterGameModel>();

            monsterGameModel.Initialization(monster, hexagon);
            hexagon.TrySetMonsterGameModel(monsterGameModel);

            monsterGameModel.transform.localPosition = hexagon.PlacementPosition;

            return monsterGameModel;
        }
    }
}
