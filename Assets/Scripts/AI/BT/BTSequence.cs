namespace MemeArena.AI.BT
{
    public class BTSequence : BTComposite
    {
        public override BTStatus Tick()
        {
            foreach (var c in children)
            {
                var s = c.Tick();
                if (s != BTStatus.Success) return s;
            }
            return BTStatus.Success;
        }
    }
}
