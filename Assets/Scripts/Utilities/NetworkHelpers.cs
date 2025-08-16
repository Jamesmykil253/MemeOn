using Unity.Netcode;
using UnityEngine;

namespace MemeArena.Utils
{
    public static class NetworkHelpers
    {
        public static bool IsEnemy(GameObject a, GameObject b)
        {
            // Simple tag-based team check; replace with proper team service
            if (!a || !b) return false;
            if (a == b) return false;
            return a.tag != b.tag;
        }
    }
}
