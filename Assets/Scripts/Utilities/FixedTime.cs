using UnityEngine;

namespace MemeArena.Utils
{
    /// <summary>
    /// Deterministic-esque fixed tick coordinator (server uses it to step logic at a constant rate).
    /// </summary>
    public static class FixedTime
    {
        public static float time;
        private static float _accum;
        private static float _step = 1f / 60f;

        public static void EnsureStep(int tickRate)
        {
            var step = 1f / Mathf.Max(15, tickRate);
            if (Mathf.Abs(step - _step) > 0.00001f) _step = step;
            _accum += Time.fixedDeltaTime;
            while (_accum >= _step)
            {
                time += _step;
                _accum -= _step;
            }
        }
    }
}
