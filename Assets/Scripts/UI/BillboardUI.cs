using UnityEngine;

namespace MemeArena.UI
{
    /// <summary>
    /// Rotates a world-space UI to always face the main camera.
    /// </summary>
    public class BillboardUI : MonoBehaviour
    {
        private Camera _cam;

        private void LateUpdate()
        {
            if (_cam == null)
            {
                _cam = Camera.main;
                if (_cam == null && Camera.allCamerasCount > 0)
                {
                    _cam = Camera.allCameras[0];
                }
            }
            if (_cam == null) return;
            var t = transform;
            t.forward = (_cam.transform.position - t.position).normalized;
            t.forward = -t.forward; // face camera
        }
    }
}
