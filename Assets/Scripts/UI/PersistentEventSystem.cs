using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace MemeArena.UI
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10000)]
    public class PersistentEventSystem : MonoBehaviour
    {
        private static PersistentEventSystem s_instance;
        private EventSystem _eventSystem;

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            s_instance = this;
            DontDestroyOnLoad(gameObject);

            _eventSystem = GetComponent<EventSystem>();
            if (_eventSystem == null)
            {
                _eventSystem = gameObject.AddComponent<EventSystem>();
            }

            // Prefer Input System module if available; else fallback
            var inputModule = GetComponent("UnityEngine.InputSystem.UI.InputSystemUIInputModule") as Behaviour;
            if (inputModule == null)
            {
                var standalone = GetComponent<StandaloneInputModule>();
                if (standalone == null)
                {
                    // Try to add InputSystem module by reflection; fallback to Standalone
                    var t = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                    if (t != null)
                    {
                        gameObject.AddComponent(t);
                    }
                    else
                    {
                        gameObject.AddComponent<StandaloneInputModule>();
                    }
                }
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Remove any extra EventSystems introduced by the new scene
            var all = UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var es in all)
            {
                if (es == null) continue;
                if (es.gameObject == this.gameObject) continue;
                Destroy(es.gameObject);
            }
        }
    }
}
