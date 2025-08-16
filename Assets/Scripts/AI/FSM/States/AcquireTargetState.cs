using Unity.Netcode;
using UnityEngine;

namespace MemeArena.AI.FSM.States
{
    using ID = AIController.AIStateId;

    public class AcquireTargetState : IState
    {
        private readonly AIController _ctrl;
        private readonly MemeArena.AI.Blackboard _bb;
        private float _timer;

        public AcquireTargetState(AIController ctrl, MemeArena.AI.Blackboard bb) { _ctrl = ctrl; _bb = bb; }

        public void Enter() { _timer = 0f; }

        public void Tick()
        {
            _timer += Time.fixedDeltaTime;
            if (_timer < 0.15f) return;
            _timer = 0f;

            // naive: find closest opposing team NetworkObject with "Player" tag
            GameObject best = null;
            float bestSqr = float.MaxValue;
            foreach (var no in Object.FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
            {
                if (no == null || !no.IsSpawned) continue;
                if (no.gameObject == _ctrl.gameObject) continue;
                if (!no.CompareTag(AITags.PlayerTag)) continue;

                var d2 = (no.transform.position - _ctrl.transform.position).sqrMagnitude;
                if (d2 < bestSqr && d2 <= (_bb.config.aggroRadius * _bb.config.aggroRadius))
                {
                    best = no.gameObject;
                    bestSqr = d2;
                }
            }

            if (best)
            {
                _bb.currentTarget = best.GetComponent<NetworkObject>();
                _bb.lastKnownTargetPosition = _bb.currentTarget.transform.position;
                _ctrl.GetComponent<AIController>().SendMessage("FSM_ChangeToAim", SendMessageOptions.DontRequireReceiver);
            }
        }

        public void Exit() { }
    }
}
