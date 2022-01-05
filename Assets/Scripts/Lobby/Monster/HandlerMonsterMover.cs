using System;
using System.Collections.Generic;
using CreatePath;
using UnityEngine;

namespace MonsterSpace.Mover
{
    [RequireComponent(typeof(MonsterMover))]
    [RequireComponent(typeof(MovePostRequest))]
    public class HandlerMonsterMover : MonoBehaviour
    {
        private MonsterMover _mover;
        private MovePostRequest _moveRequest;
        private MonsterGameModel _monster;

        public event Action StartedMove;
        public event Action StopedMove;

        private bool _waitingServerResponse = false;
        private bool _isMove = false;

        private Path _path;

        private void Awake()
        {
            _monster = GetComponent<MonsterGameModel>();
            _moveRequest = GetComponent<MovePostRequest>();
            _mover = GetComponent<MonsterMover>();
            _mover.StopedMove += OnStopedMove;
        }

        public void OnSelected()
        {
            if(_path != null)
                _path.OnPathHighlighting();
        }

        public void OnSelectionCanceled()
        {
            if (_path != null)
                _path.OffPathHighlighting();
        }

        public bool TrySetPath(Path path)
        {
            if (_waitingServerResponse)
                return false;
            if (_isMove)
                return false;
            if (path == null)
                return false;

            if (_path != null)
            {
                if (WayСhecker.DuplicatePathCheck(_path, path))
                    ResetPath();
                else
                    return false;
            }

            SetPath(path);
            _waitingServerResponse = true;
            _moveRequest.TryPost(path.Hexagons[path.Hexagons.Count - 1], _monster.Monster, SetPathFromServer);
            return true;
        }

        public void SetPathFromServer(bool result)
        {      
            if (!result)
            {
                ResetPath();
                return;
            }
            /*
            if (path != null)
                SetPath(path); Временно пока нет логики на сервере*/

            StartMove();
            _waitingServerResponse = false;
        }

        private void StartMove()
        {
            if (_path != null)
            {
                _isMove = true;
                StartedMove?.Invoke();
                _mover.StartMove(_path);
            }
        }

        private void OnStopedMove()
        {
            _isMove = false;
            StopedMove?.Invoke();
            _path.OffPathHighlighting();
            _path = null;
        }

        private void ResetPath()
        {
            _path.OffPathHighlighting();
            _path = null;
        }

        private void SetPath(Path path)
        {
            _path = path;
            _path.OnPathHighlighting();
        }

        private void OnDestroy()
        {
            _mover.StopedMove -= OnStopedMove;
        }
    }
}
