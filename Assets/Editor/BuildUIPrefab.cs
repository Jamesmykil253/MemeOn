#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MemeArena.EditorTools
{
    public static class BuildUIPrefab
    {
        [MenuItem("Tools/MemeArena/Build UI Prefab...")]
        public static void BuildUIPrefabMenu()
        {
            var root = CreateHUDRoot(out var canvas);
            try
            {
                // Add canonical binder
                AddIfTypeExists(root, "MemeArena.HUD.PlayerHUDBinder");

                // HUD panel container
                var panel = new GameObject("Panel", typeof(RectTransform));
                panel.transform.SetParent(root.transform, false);
                var rt = (RectTransform)panel.transform;
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

                // Health bar
                var healthGO = CreateBar(panel.transform, "HealthBar", new Color(0.9f, 0.1f, 0.1f), new Vector2(16, 16));
                AddIfTypeExists(healthGO, "MemeArena.UI.HealthBarUI");

                // XP bar
                var xpGO = CreateBar(panel.transform, "XPBar", new Color(0.1f, 0.5f, 0.9f), new Vector2(16, 40));
                AddIfTypeExists(xpGO, "MemeArena.UI.XPBarUI");

                // Level text
                var levelGO = CreateText(panel.transform, "LevelText", "Lv 1", TextAnchor.UpperLeft, new Vector2(16, 64));
                AddIfTypeExists(levelGO, "MemeArena.UI.LevelUI");

                // Coin counter
                var coinGO = CreateText(panel.transform, "Coins", "0", TextAnchor.UpperRight, new Vector2(-16, 16));
                AddIfTypeExists(coinGO, "MemeArena.UI.CoinCounterUI");

                // Boosted status
                var boostGO = CreateText(panel.transform, "Boost", "", TextAnchor.UpperRight, new Vector2(-16, 40));
                AddIfTypeExists(boostGO, "MemeArena.UI.BoostedAttackUI");

                // Save under Assets/Prefabs/UI/UI.prefab
                var dir = "Assets/Prefabs/UI";
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, "UI.prefab");
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
                Selection.activeObject = prefab;
                EditorUtility.DisplayDialog("Build UI Prefab", $"Created {path}", "OK");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateHUDRoot(out Canvas canvas)
        {
            var go = new GameObject("UI", typeof(RectTransform));
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static GameObject CreateBar(Transform parent, string name, Color fillColor, Vector2 anchoredPos)
        {
            var bar = new GameObject(name, typeof(RectTransform));
            bar.transform.SetParent(parent, false);
            var rt = (RectTransform)bar.transform;
            rt.sizeDelta = new Vector2(220, 16);
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.anchoredPosition = anchoredPos;

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

        private static GameObject CreateText(Transform parent, string name, string text, TextAnchor anchor, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(200, 24);
            rt.anchorMin = anchor == TextAnchor.UpperRight ? new Vector2(1, 1) : new Vector2(0, 1);
            rt.anchorMax = rt.anchorMin;
            rt.pivot = new Vector2(rt.anchorMin.x, rt.anchorMin.y);
            rt.anchoredPosition = anchoredPos;

            var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(go.transform, false);
            var txt = textGO.GetComponent<Text>();
            txt.text = text;
            txt.alignment = anchor;
            txt.color = Color.white;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var trt = (RectTransform)textGO.transform;
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
            return go;
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
