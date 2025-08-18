#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MemeArena.EditorTools
{
    public static class EnemyPrefabUIAuditor
    {
        [MenuItem("Tools/MemeArena/Audit/Enemy Prefab UI")] 
        public static void AuditEnemyUI()
        {
            var path = "Assets/Resources/NetworkPrefabs/Enemy.prefab";
            var enemy = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (enemy == null)
            {
                EditorUtility.DisplayDialog("Enemy UI Audit", "Enemy prefab not found. Build core prefabs first.", "OK");
                return;
            }

            var root = GameObject.Instantiate(enemy);
            try
            {
                bool changed = EnsureWorldUI(root);
                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    EditorUtility.DisplayDialog("Enemy UI Audit", "Enemy prefab UI was updated (WorldUI, HealthBar, LevelUI).", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Enemy UI Audit", "Enemy prefab UI looks complete.", "OK");
                }
            }
            finally
            {
                GameObject.DestroyImmediate(root);
            }
        }

        public static bool EnsureWorldUI(GameObject enemy)
        {
            bool changed = false;
            var canvas = enemy.transform.Find("WorldUI") as Transform;
            if (canvas == null)
            {
                canvas = new GameObject("WorldUI", typeof(RectTransform)).transform;
                canvas.SetParent(enemy.transform, false);
                var c = canvas.gameObject.AddComponent<Canvas>();
                c.renderMode = RenderMode.WorldSpace;
                var scaler = canvas.gameObject.AddComponent<CanvasScaler>();
                scaler.dynamicPixelsPerUnit = 10;
                canvas.gameObject.AddComponent<GraphicRaycaster>();
                var rt = (RectTransform)canvas.transform;
                rt.sizeDelta = new Vector2(1.5f, 0.4f);
                rt.localPosition = new Vector3(0, 1.6f, 0);
                changed = true;
            }

            // Ensure helper scripts
            if (canvas.GetComponent("MemeArena.UI.EnemyUnitUI") == null)
            {
                AddIfTypeExists(canvas.gameObject, "MemeArena.UI.EnemyUnitUI");
                changed = true;
            }
            if (canvas.GetComponent("MemeArena.UI.BillboardUI") == null)
            {
                AddIfTypeExists(canvas.gameObject, "MemeArena.UI.BillboardUI");
                changed = true;
            }

            // Health bar
            var bar = canvas.Find("HealthBar")?.gameObject;
            if (bar == null)
            {
                bar = new GameObject("HealthBar", typeof(RectTransform));
                bar.transform.SetParent(canvas, false);
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
                fillImg.color = new Color(0.9f, 0.1f, 0.1f);
                fillImg.type = Image.Type.Filled;
                fillImg.fillMethod = Image.FillMethod.Horizontal;
                fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
                fillImg.fillAmount = 1f;
                var fRt = (RectTransform)fill.transform;
                fRt.anchorMin = Vector2.zero; fRt.anchorMax = Vector2.one; fRt.offsetMin = Vector2.zero; fRt.offsetMax = Vector2.zero;
                AddIfTypeExists(bar, "MemeArena.UI.HealthBarUI");
                changed = true;
            }

            // Level text (TMP)
            var level = canvas.Find("LevelText")?.gameObject;
            if (level == null)
            {
                level = new GameObject("LevelText", typeof(RectTransform));
                level.transform.SetParent(canvas, false);
                var rt = (RectTransform)level.transform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = rt.anchorMin;
                rt.sizeDelta = new Vector2(0.6f, 0.25f);
                rt.anchoredPosition = new Vector2(0, 0.15f);
                var textGO = new GameObject("Text", typeof(RectTransform));
                textGO.transform.SetParent(level.transform, false);
                var trt = (RectTransform)textGO.transform;
                trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
                var tmp = textGO.AddComponent<TextMeshProUGUI>();
                tmp.text = "Lv 1";
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                tmp.fontSize = 18;
                AddIfTypeExists(level, "MemeArena.UI.LevelUI");
                changed = true;
            }

            return changed;
        }

        private static void AddIfTypeExists(GameObject go, string fullTypeName)
        {
            var t = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .FirstOrDefault(x => x.FullName == fullTypeName);
            if (t != null && typeof(Component).IsAssignableFrom(t))
            {
                if (go.GetComponent(t) == null)
                {
                    go.AddComponent(t);
                }
            }
        }
    }
}
#endif
