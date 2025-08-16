namespace MemeArena.AI.BT.Nodes
{
    public class TaskFire : BTAction
    {
        private readonly AIController _ctrl;
        public TaskFire(AIController ctrl) { _ctrl = ctrl; }

        public override BTStatus Tick()
        {
            if (!_ctrl.OffCooldown()) return BTStatus.Running;
            _ctrl.Fire();
            _ctrl.StartCooldown();
            return BTStatus.Success;
        }
    }
}
