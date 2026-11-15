using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EraserWars.Editor
{
    /// <summary>
    /// Fixed Full HD Game view + Scale 1x crops UITK. Prefer free 16:9 so Scale 1x matches the panel.
    /// </summary>
    [InitializeOnLoad]
    public static class GameViewAspectHelper
    {
        static GameViewAspectHelper()
        {
            EditorApplication.delayCall += () => TrySetAspectRatio(16f, 9f);
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.delayCall += () => TrySetAspectRatio(16f, 9f);
            }
        }

        [MenuItem("EraserWars/Use Game View 16:9 Aspect")]
        public static void Apply16x9Aspect()
        {
            if (!TrySetAspectRatio(16f, 9f))
            {
                Debug.LogWarning("[EraserWars] Could not set Game view to 16:9 Aspect. Pick Aspect Ratio → 16:9 in the Game view toolbar.");
                return;
            }

            Debug.Log("[EraserWars] Game view set to 16:9 Aspect (Scale 1x shows the full frame).");
        }

        public static bool TrySetAspectRatio(float width, float height)
        {
            var editorAsm = typeof(EditorWindow).Assembly;
            var gameViewType = editorAsm.GetType("UnityEditor.GameView");
            if (gameViewType == null)
            {
                return false;
            }

            var gameView = EditorWindow.GetWindow(gameViewType, false, null, false);
            if (gameView == null)
            {
                return false;
            }

            var sizesType = editorAsm.GetType("UnityEditor.GameViewSizes");
            var singletonType = editorAsm.GetType("UnityEditor.ScriptableSingleton`1")?.MakeGenericType(sizesType);
            var instanceProp = singletonType?.GetProperty("instance");
            var sizes = instanceProp?.GetValue(null, null);
            if (sizes == null)
            {
                return false;
            }

            var currentGroupProp = sizesType.GetProperty("currentGroup");
            var group = currentGroupProp?.GetValue(sizes, null);
            if (group == null)
            {
                return false;
            }

            var groupType = group.GetType();
            var getTotalCount = groupType.GetMethod("GetTotalCount");
            var getGameViewSize = groupType.GetMethod("GetGameViewSize");
            if (getTotalCount == null || getGameViewSize == null)
            {
                return false;
            }

            var total = (int)getTotalCount.Invoke(group, null);
            var targetIndex = -1;
            for (var i = 0; i < total; i++)
            {
                var size = getGameViewSize.Invoke(group, new object[] { i });
                if (size == null)
                {
                    continue;
                }

                var sizeType = size.GetType();
                var sizeTypeEnum = sizeType.GetProperty("sizeType")?.GetValue(size, null);
                var w = Convert.ToInt32(sizeType.GetProperty("width")?.GetValue(size, null) ?? 0);
                var h = Convert.ToInt32(sizeType.GetProperty("height")?.GetValue(size, null) ?? 0);
                // AspectRatio enum value is typically 1.
                if (sizeTypeEnum != null && sizeTypeEnum.ToString() == "AspectRatio" && w == (int)width && h == (int)height)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex < 0)
            {
                // Fallback: first AspectRatio entry that is near 16:9.
                for (var i = 0; i < total; i++)
                {
                    var size = getGameViewSize.Invoke(group, new object[] { i });
                    if (size == null)
                    {
                        continue;
                    }

                    var sizeType = size.GetType();
                    var sizeTypeEnum = sizeType.GetProperty("sizeType")?.GetValue(size, null);
                    var w = Convert.ToSingle(sizeType.GetProperty("width")?.GetValue(size, null) ?? 0);
                    var h = Convert.ToSingle(sizeType.GetProperty("height")?.GetValue(size, null) ?? 0);
                    if (sizeTypeEnum != null && sizeTypeEnum.ToString() == "AspectRatio" && h > 0f && Mathf.Abs(w / h - width / height) < 0.02f)
                    {
                        targetIndex = i;
                        break;
                    }
                }
            }

            if (targetIndex < 0)
            {
                return false;
            }

            var sizeSelection = gameViewType.GetMethod(
                "SizeSelectionCallback",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (sizeSelection == null)
            {
                return false;
            }

            sizeSelection.Invoke(gameView, new object[] { targetIndex, null });
            gameView.Repaint();
            return true;
        }
    }
}
