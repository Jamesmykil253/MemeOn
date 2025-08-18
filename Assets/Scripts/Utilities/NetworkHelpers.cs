using Unity.Netcode;
using UnityEngine;
using MemeArena.Network;

namespace MemeArena.Utils
{
    public static class NetworkHelpers
    {
        public static bool IsEnemy(GameObject a, GameObject b)
        {
            if (!a || !b) return false;
            if (a == b) return false;
            var ta = a.GetComponentInParent<TeamId>();
            var tb = b.GetComponentInParent<TeamId>();
            if (ta == null || tb == null) return false;
            // Treat team 0 as neutral; enemies must be non-zero and different
            if (ta.team == 0 || tb.team == 0) return false;
            return ta.team != tb.team;
        }
    }
}
