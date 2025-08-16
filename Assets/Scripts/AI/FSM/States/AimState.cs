using UnityEngine;

namespace MemeArena.AI.FSM.States
{
    public class AimState : IState
    {
        private readonly AIController _ctrl;
        private readonly MemeArena.AI.Blackboard _bb;

        public AimState(AIController ctrl, MemeArena.AI.Blackboard bb) { _ctrl = ctrl; _bb = bb; }

        public void Enter() { }

        public void Tick()
        {
            var t = _ctrl.TargetTransform();
            if (!t) return;

            _ctrl.FaceToward(t.position, Time.fixedDeltaTime);

            if (_ctrl.InAttackRange(t) && _ctrl.OffCooldown())
            {
                _ctrl.SendMessage("FSM_ChangeToAttack", SendMessageOptions.DontRequireReceiver);
            }
        }

        public void Exit() { }
    }
}
