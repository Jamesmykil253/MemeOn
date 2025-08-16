using UnityEngine;

namespace MemeArena.CameraSystem
{
    public class UniteCameraController : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 6, -6);
        public float smooth = 10f;
        public float zoomSpeed = 5f;
        public float minZoom = 4f;
        public float maxZoom = 12f;

        private float _zoom = 8f;

        private void Update()
        {
            // NOTE: Use Input System for gameplay; here we only use legacy mouse wheel for editor convenience if available.
            try
            {
                _zoom = Mathf.Clamp(_zoom - UnityEngine.Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime, minZoom, maxZoom);
            }
            catch { /* Ignore if legacy input disabled */ }
        }

        private void LateUpdate()
        {
            if (!target) return;
            var desired = target.position + (Quaternion.Euler(0, 0, 0) * offset.normalized) * _zoom;
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-smooth * Time.deltaTime));
            transform.LookAt(target);
        }
    }
}
