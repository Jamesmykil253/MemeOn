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
    private static bool s_cleaning;
    private static bool s_applicationQuitting = false;

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            s_instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureLocalEventSystem();
            EnforceSingleEventSystem();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            // Early sweeps in case other systems spawn EventSystems on startup
            StartCoroutine(StartupSweep());
        }

        private void OnApplicationQuit()
        {
            s_applicationQuitting = true;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private System.Collections.IEnumerator StartupSweep()
        {
            // Run a few frames to catch late creators
            for (int i = 0; i < 10; i++)
            {
                EnforceSingleEventSystem();
                yield return null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnforceSingleEventSystem();
        }

        private void EnsureLocalEventSystem()
        {
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

        private void EnforceSingleEventSystem()
        {
            if (s_cleaning) return;
            if (s_applicationQuitting) return;
            s_cleaning = true;
            try
            {
                EventSystem[] all = null;
                try
                {
                    all = UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                }
                catch
                {
                    // Fallback for older Unity versions
                    all = UnityEngine.Object.FindObjectsOfType<EventSystem>(true);
                }
                foreach (var es in all ?? new EventSystem[0])
                {
                    if (es == null) continue;
                    var go = es.gameObject;
                    if (go == this.gameObject) continue;

                    try
                    {
                        // Determine if this is a stand-alone EventSystem container (safe to destroy whole GO)
                        var components = go.GetComponents<Component>();
                        bool onlyEventStuff = true;
                        foreach (var c in components)
                        {
                            if (c == null) continue;
                            if (c is Transform) continue;
                            if (c is EventSystem) continue;
                            if (c is StandaloneInputModule) continue;

                            var tInput = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                            if (tInput != null && tInput.IsInstanceOfType(c)) continue;

                            // Found something else; not safe to destroy whole GO
                            onlyEventStuff = false;
                            break;
                        }

                        if (onlyEventStuff)
                        {
                            Destroy(go);
                            continue;
                        }

                        // Otherwise, strip EventSystem and input modules only
                        var ev = go.GetComponent<EventSystem>();
                        if (ev) Destroy(ev);

                        var sim = go.GetComponents<StandaloneInputModule>();
                        if (sim != null)
                        {
                            foreach (var m in sim) if (m) Destroy(m);
                        }

                        var t = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                        if (t != null)
                        {
                            var comps = go.GetComponents(t);
                            foreach (var c in comps)
                            {
                                if (c is Component comp) Destroy(comp);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"PersistentEventSystem: Failed cleaning duplicate EventSystem: {ex.Message}");
                    }
                }
            }
            finally
            {
                s_cleaning = false;
            }
        }
    }
}
