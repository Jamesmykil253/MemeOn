using UnityEngine;

namespace MemeArena.AI.FSM.States
{
    public class CooldownState : IState
    {
        private readonly AIController _ctrl;
        private readonly MemeArena.AI.Blackboard _bb;
        private float _cooldownTimer;

        public CooldownState(AIController ctrl, MemeArena.AI.Blackboard bb) { _ctrl = ctrl; _bb = bb; }

        public void Enter() { _cooldownTimer = 0f; }

        public void Tick()
        {
            _cooldownTimer += Time.fixedDeltaTime;
            if (_ctrl.OffCooldown())
            {
                _ctrl.SendMessage("FSM_ChangeToAim", SendMessageOptions.DontRequireReceiver);
            }
        }

        public void Exit() { }
    }
}
