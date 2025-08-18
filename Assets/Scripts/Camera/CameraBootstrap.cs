using UnityEngine;

namespace MemeArena.CameraSystem
{
    public class CameraBootstrap : MonoBehaviour
    {
        private UniteCameraController _cam;
    [Header("Debug")] public bool debugLogs = false;

        private void Awake()
        {
            _cam = GetComponent<UniteCameraController>();
            if (!_cam) _cam = gameObject.AddComponent<UniteCameraController>();
        }

        private void Start()
        {
            // Prefer the local player
            var allMovers = FindObjectsByType<MemeArena.Players.PlayerMovement>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var pm in allMovers)
            {
                var no = pm.GetComponent<Unity.Netcode.NetworkObject>();
                if (no != null && no.IsLocalPlayer)
                {
            _cam.SetTarget(pm.transform);
            if (debugLogs) Debug.Log($"CameraBootstrap: Targeting local player {pm.name} (ClientId={no.OwnerClientId}).");
            return;
                }
            }
            // Fallback: any PlayerMovement
        if (allMovers.Length > 0) { _cam.SetTarget(allMovers[0].transform); if (debugLogs) Debug.Log($"CameraBootstrap: Targeting first player {allMovers[0].name} (no local found)."); return; }

            var enemyAI = FindFirstObjectByType<MemeArena.AI.AIController>();
            if (!enemyAI) enemyAI = FindAnyObjectByType<MemeArena.AI.AIController>();
            if (enemyAI) { _cam.SetTarget(enemyAI.transform); if (debugLogs) Debug.Log($"CameraBootstrap: Targeting fallback AI {enemyAI.name}."); }
        }

        public void Retarget(Transform t)
        {
            if (!_cam) _cam = GetComponent<UniteCameraController>();
            _cam?.SetTarget(t);
            if (debugLogs && t != null) Debug.Log($"CameraBootstrap: Retarget called → {t.name}");
        }
    }
}
