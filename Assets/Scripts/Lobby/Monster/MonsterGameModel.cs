using UnityEngine;
using MapSpase.Hexagon;
using MonsterSpace.Mover;

namespace MonsterSpace
{
    public class MonsterGameModel : MonoBehaviour
    {
        private Monster _monster;

        public HandlerMonsterMover Mover { get; private set; }

        public IMonster Monster => _monster ?? throw new System.ArgumentNullException("Monster is null");

        private void Awake()
        {
            Mover = GetComponent<HandlerMonsterMover>();
        }

        public void Initialization(Monster monster, HexagonModel currentHexagon)
        {
            _monster = monster;
        }
    }
}
