namespace MemeArena.AI.FSM.States
{
    public class AttackState : IState
    {
        private readonly AIController _ctrl;
        private readonly MemeArena.AI.Blackboard _bb;

        public AttackState(AIController ctrl, MemeArena.AI.Blackboard bb) { _ctrl = ctrl; _bb = bb; }

        public void Enter()
        {
            _ctrl.Fire();
            _ctrl.StartCooldown();
            _ctrl.SendMessage("FSM_ChangeToCooldown", UnityEngine.SendMessageOptions.DontRequireReceiver);
        }

        public void Tick() { }
        public void Exit() { }
    }
}
