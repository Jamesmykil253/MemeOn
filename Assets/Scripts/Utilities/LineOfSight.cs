using UnityEngine;
using MemeArena.Network;

namespace MemeArena.Utilities
{
    /// <summary>
    /// Provides helper methods for line of sight checks.  Uses Physics
    /// raycasts to determine whether two points are visible to each other,
    /// ignoring specified layers.  All line of sight queries should be
    /// performed on the server to ensure deterministic results.
    /// </summary>
    public static class LineOfSight
    {
        /// <summary>
        /// Returns true if there is an unobstructed path between origin and
        /// target.  Ignores the AI and the target layers themselves.  If the
        /// distance is zero the method returns true.
        /// </summary>
        public static bool HasLineOfSight(Vector3 origin, Vector3 target)
        {
            Vector3 dir = target - origin;
            float dist = dir.magnitude;
            if (dist <= 0.01f) return true;
            dir /= dist;
            int mask = 1 << ProjectConstants.Layers.Environment;
            return !Physics.Raycast(origin, dir, dist, mask);
        }
    }
}