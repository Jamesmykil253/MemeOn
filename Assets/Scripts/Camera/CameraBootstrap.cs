using UnityEngine;

namespace MemeArena.CameraSystem
{
    public class CameraBootstrap : MonoBehaviour
    {
        private UniteCameraController _cam;

        private void Awake()
        {
            _cam = GetComponent<UniteCameraController>();
            if (!_cam) _cam = gameObject.AddComponent<UniteCameraController>();
        }

        private void Start()
        {
            // Prefer the local player; otherwise, any player; fallback to any AI
            var player = FindFirstObjectByType<MemeArena.Players.PlayerController>();
            if (!player) player = FindAnyObjectByType<MemeArena.Players.PlayerController>();
            if (player) { _cam.target = player.transform; return; }

            var enemyAI = FindFirstObjectByType<MemeArena.AI.AIController>();
            if (!enemyAI) enemyAI = FindAnyObjectByType<MemeArena.AI.AIController>();
            if (enemyAI) _cam.target = enemyAI.transform;
        }
    }
}
