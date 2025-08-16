using UnityEngine;

namespace MemeArena.Combat
{
    public interface IDamageable
    {
        void ApplyDamage(int amount, GameObject source, Vector3 hitPoint);
    }
}
