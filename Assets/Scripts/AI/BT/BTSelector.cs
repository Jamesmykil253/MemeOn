namespace MemeArena.AI.BT
{
    public class BTSelector : BTComposite
    {
        public override BTStatus Tick()
        {
            foreach (var c in children)
            {
                var s = c.Tick();
                if (s != BTStatus.Failure) return s;
            }
            return BTStatus.Failure;
        }
    }
}
