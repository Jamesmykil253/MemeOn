using System.Collections.Generic;

namespace MemeArena.AI.BT
{
    public abstract class BTComposite : BTNode
    {
        protected readonly List<BTNode> children = new();
        public BTComposite Add(BTNode node) { children.Add(node); return this; }
    }
}
