using MonsterSpace.Mover;
using UnityEngine;

namespace MonsterSpace.AnimatorSpase
{
    [RequireComponent(typeof(HandlerMonsterMover), typeof(MonsterAnimator))]
    public class ControllerMonsterAnimator : MonoBehaviour
    {
        private HandlerMonsterMover _handlerMonsterMover;
        private MonsterAnimator _monsterAnimator;

        private void Awake()
        {
            _monsterAnimator = GetComponent<MonsterAnimator>();
            _handlerMonsterMover = GetComponent<HandlerMonsterMover>();

            _handlerMonsterMover.StartedMove += _monsterAnimator.OnStartedMove;
            _handlerMonsterMover.StopedMove += _monsterAnimator.OnStopedMove;
        }

        private void OnDestroy()
        {
            _handlerMonsterMover.StartedMove -= _monsterAnimator.OnStartedMove;
            _handlerMonsterMover.StopedMove -= _monsterAnimator.OnStopedMove;
        }
    }
}
