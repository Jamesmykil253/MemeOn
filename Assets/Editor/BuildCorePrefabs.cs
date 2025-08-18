#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

namespace MemeArena.EditorTools
{
    public static class BuildCorePrefabs
    {
        [MenuItem("Tools/MemeArena/Build Core Network Prefabs...")]
        public static void BuildAll()
        {
            var outDir = "Assets/Resources/NetworkPrefabs"; // picked up by NetworkPrefabsRegistrar
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

            var player = BuildPlayerPrefab();
            var enemy = BuildEnemyPrefab();
            var projectile = BuildProjectilePrefab();
            var melee = BuildMeleeHitboxPrefab();

            SavePrefab(player, Path.Combine(outDir, "Player.prefab"));
            SavePrefab(enemy, Path.Combine(outDir, "Enemy.prefab"));
            SavePrefab(projectile, Path.Combine(outDir, "Projectile.prefab"));
            SavePrefab(melee, Path.Combine(outDir, "MeleeHitbox.prefab"));

            UnityEngine.Object.DestroyImmediate(player);
            UnityEngine.Object.DestroyImmediate(enemy);
            UnityEngine.Object.DestroyImmediate(projectile);
            UnityEngine.Object.DestroyImmediate(melee);

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Build Core Prefabs", "Created Player, Enemy, Projectile, and MeleeHitbox in Resources/NetworkPrefabs.", "OK");
        }

        private static GameObject BuildPlayerPrefab()
        {
            var go = new GameObject("Player");
            go.tag = "Player";

            // Visual
            CreatePrimitiveVisual(go.transform, PrimitiveType.Capsule, new Color(0.2f, 0.8f, 0.9f));

            // Required components
            go.AddComponent<NetworkObject>();
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.radius = 0.35f; cc.center = new Vector3(0, 0.9f, 0);

            AddIfTypeExists(go, "Unity.Netcode.Components.NetworkTransform");

            // Gameplay scripts (added via reflection by full name to avoid hard refs breaking compilation)
            AddIfTypeExists(go, "MemeArena.Players.PlayerMovement");
            AddIfTypeExists(go, "MemeArena.Players.PlayerCombatController");
            AddIfTypeExists(go, "MemeArena.Combat.BoostedAttackTracker");
            AddIfTypeExists(go, "MemeArena.Combat.NetworkHealth");
            // PlayerInventory lives under MemeArena.Players
            AddIfTypeExists(go, "MemeArena.Players.PlayerInventory");

            return go;
        }

        private static GameObject BuildEnemyPrefab()
        {
            var go = new GameObject("Enemy");
            go.tag = "Enemy";

            // Visual
            CreatePrimitiveVisual(go.transform, PrimitiveType.Capsule, new Color(0.9f, 0.2f, 0.2f));

            // Network + AI
            go.AddComponent<NetworkObject>();
            AddIfTypeExists(go, "Unity.Netcode.Components.NetworkTransform");
            AddIfTypeExists(go, "MemeArena.AI.AIController");
            AddIfTypeExists(go, "MemeArena.Combat.NetworkHealth");
            // Optional: NavMeshAgent if AI uses navigation
            var agent = go.AddComponent<NavMeshAgent>();
            agent.speed = 3.5f; agent.angularSpeed = 720f; agent.stoppingDistance = 1.5f;

            // Level provider if present in project
            AddIfTypeExists(go, "MemeArena.AI.EnemyLevel");

            // World-space UI (Health + Level)
            var canvas = new GameObject("WorldUI", typeof(RectTransform));
            canvas.transform.SetParent(go.transform, false);
            var c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            var scaler = canvas.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;
            canvas.AddComponent<GraphicRaycaster>();
            var rt = (RectTransform)canvas.transform;
            rt.sizeDelta = new Vector2(1.5f, 0.4f);
            rt.localPosition = new Vector3(0, 1.6f, 0);

            AddIfTypeExists(canvas, "MemeArena.UI.EnemyUnitUI");
            AddIfTypeExists(canvas, "MemeArena.UI.BillboardUI");

            var bar = CreateBar(canvas.transform, "HealthBar", new Color(0.9f, 0.1f, 0.1f));
            AddIfTypeExists(bar, "MemeArena.UI.HealthBarUI");

            var lvl = CreateText(canvas.transform, "LevelText", "Lv 1");
            AddIfTypeExists(lvl, "MemeArena.UI.LevelUI");

            return go;
        }

        private static GameObject BuildProjectilePrefab()
        {
            var go = new GameObject("Projectile");
            go.layer = LayerMask.NameToLayer("Default");

            // Visual
            CreatePrimitiveVisual(go.transform, PrimitiveType.Sphere, new Color(1f, 0.8f, 0.1f), 0.2f);

            // Physics
            var rb = go.AddComponent<Rigidbody>();
            rb.useGravity = false; rb.isKinematic = false; rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var col = go.AddComponent<SphereCollider>();
            col.radius = 0.1f; col.isTrigger = false;

            // Network + Logic
            go.AddComponent<NetworkObject>();
            AddIfTypeExists(go, "Unity.Netcode.Components.NetworkTransform");
            AddIfTypeExists(go, "MemeArena.Combat.ProjectileServer");

            return go;
        }

        private static GameObject BuildMeleeHitboxPrefab()
        {
            var go = new GameObject("MeleeHitbox");
            go.AddComponent<NetworkObject>();
            AddIfTypeExists(go, "MemeArena.Combat.MeleeWeaponServer");

            var hit = new GameObject("Hitbox");
            hit.transform.SetParent(go.transform, false);
            var cc = hit.AddComponent<CapsuleCollider>();
            cc.isTrigger = true; cc.center = new Vector3(0, 1f, 0.6f); cc.height = 1.2f; cc.radius = 0.35f; cc.direction = 2; // z axis

            return go;
        }

        private static void SavePrefab(GameObject temp, string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            PrefabUtility.SaveAsPrefabAsset(temp, path);
        }

        private static void CreatePrimitiveVisual(Transform parent, PrimitiveType type, Color color, float scale = 1f)
        {
            var temp = GameObject.CreatePrimitive(type);
            var mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            UnityEngine.Object.DestroyImmediate(temp.GetComponent<Collider>());

            var vis = new GameObject("Visual", typeof(MeshFilter), typeof(MeshRenderer));
            vis.transform.SetParent(parent, false);
            vis.GetComponent<MeshFilter>().sharedMesh = mesh;
            vis.GetComponent<MeshRenderer>().sharedMaterial = mat;
            vis.transform.localScale = Vector3.one * scale;
            UnityEngine.Object.DestroyImmediate(temp);
        }

        private static void AddIfTypeExists(GameObject go, string fullTypeName)
        {
            var t = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .FirstOrDefault(x => x.FullName == fullTypeName);
            if (t != null && typeof(Component).IsAssignableFrom(t))
            {
                go.AddComponent(t);
            }
        }

        private static GameObject CreateBar(Transform parent, string name, Color fillColor)
        {
            var bar = new GameObject(name, typeof(RectTransform));
            bar.transform.SetParent(parent, false);
            var rt = (RectTransform)bar.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = rt.anchorMin;
            rt.sizeDelta = new Vector2(1.2f, 0.15f);
            rt.anchoredPosition = new Vector2(0, -0.05f);

            var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(bar.transform, false);
            var bgImg = bg.GetComponent<Image>();
            bgImg.color = new Color(0,0,0,0.5f);
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(bg.transform, false);
            var fillImg = fill.GetComponent<Image>();
            fillImg.color = fillColor;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 1f;
            var fRt = (RectTransform)fill.transform;
            fRt.anchorMin = Vector2.zero; fRt.anchorMax = Vector2.one; fRt.offsetMin = Vector2.zero; fRt.offsetMax = Vector2.zero;

            return bar;
        }

        private static GameObject CreateText(Transform parent, string name, string text)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = rt.anchorMin;
            rt.sizeDelta = new Vector2(0.6f, 0.25f);
            rt.anchoredPosition = new Vector2(0, 0.15f);

            // Prefer TextMeshProUGUI if available
            var textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(go.transform, false);
            var trt = (RectTransform)textGO.transform;
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontSize = 18;

            return go;
        }
    }
}
#endif
