using UnityEngine;

namespace MonsterSpace.AnimatorSpase
{
    [RequireComponent(typeof(MonsterGameModel), typeof(Animator))]
    public class MonsterAnimator : MonoBehaviour
    {
        private const string StartMove = "StartMove";
        private const string StopMove = "StopMove";
        private const string Speed = "Speed";

        private Animator _animator;
        private MonsterGameModel _monsterGameModel;

        private void Awake()
        {
            _monsterGameModel = GetComponent<MonsterGameModel>();
            _animator = GetComponent<Animator>();
        }

        private void SetSpeed()
        {
            _animator.SetFloat(Speed, _monsterGameModel.Monster.Speed);
        }

        public void OnStartedMove()
        {
            SetSpeed();
            _animator.SetTrigger(StartMove);
        }

        public void OnStopedMove()
        {
            SetSpeed();
            _animator.SetTrigger(StopMove);
        }
    }
}
