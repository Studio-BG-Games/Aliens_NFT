using System;
using System.Collections;
using CreatePath;
using UnityEngine;
using DG.Tweening;
using MapSpase.Hexagon;

namespace MonsterSpace.Mover
{
    [RequireComponent(typeof(MonsterGameModel))]
    public class MonsterMover : MonoBehaviour
    {
        private const float StandartRotateSpeed = 0.5f;
        private const float StandartMoveSpeed = 1f;

        private MonsterGameModel _monsterModel;

        public event Action StopedMove;

        private void Awake()
        {
            _monsterModel = GetComponent<MonsterGameModel>();
        }

        public void StartMove(Path path)
        {
            StartCoroutine(Move(path));
        }

        private IEnumerator Move(Path path)
        {
            if (path != null)
            {
                HexagonModel current = path.Hexagons[0];

                for (int i = 1; i < path.Hexagons.Count; i++)
                {
                    HexagonModel target = path.Hexagons[i];

                    if (!target.CanPlace)
                        break;

                    if (target.MonsterGameModel != null)
                    {
                        Destroy(target.MonsterGameModel.gameObject);
                        target.TryRemoveMonsterGameModel();
                    }

                    target.TrySetMonsterGameModel(_monsterModel);                    
                    path.Hexagons[i - 1].TryRemoveMonsterGameModel();
                    path.OnStep(path.Hexagons[i - 1]);

                    DefineTurnData(out float angle, out float speedChangeAngle, current, target);
                    Tween rotate = transform.DOLocalRotate(new Vector3(0, angle, 0), speedChangeAngle).SetEase(Ease.Linear);
                    yield return rotate.WaitForCompletion();

                    Tween move = transform.DOMove(target.transform.position + target.PlacementPosition, StandartMoveSpeed / _monsterModel.Monster.Speed).SetEase(Ease.Linear);
                    yield return move.WaitForCompletion();

                    transform.SetParent(target.transform);
                    current = target;
                }

                StopedMove?.Invoke();
            }
        }

        private void DefineTurnData(out float angle, out float speedChangeAngle, HexagonModel current, HexagonModel target)
        {
            if (transform.localEulerAngles.y > 360)
                transform.localEulerAngles -= Vector3.up * 360;
            if (transform.localEulerAngles.y < 0)
                transform.localEulerAngles += Vector3.up * 360;

            int curentAngleNumber = RotationAngleInformation.CurentAngleNumber(transform.localEulerAngles.y);
            int targetAngleNumber = RotationAngleInformation.TargetAngleNumber(current, target);
            angle = RotationAngleInformation.GetAngle(targetAngleNumber);
            speedChangeAngle = (StandartRotateSpeed / _monsterModel.Monster.Speed) * RotationAngleInformation.FindDistancesBetweenAngleNumber(curentAngleNumber, targetAngleNumber);
        }
    }
}

