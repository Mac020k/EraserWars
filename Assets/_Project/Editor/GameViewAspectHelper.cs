using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EraserWars.Editor
{
    /// <summary>
    /// Prefer Free Aspect so the Game panel at Scale 1x shows the full UITK frame
    /// regardless of fixed Full HD / 4K size entries (those crop when the panel is smaller).
    /// Built players still use ScaleWithScreenSize + Expand for responsive Full HD / 4K.
    /// </summary>
    [InitializeOnLoad]
    public static class GameViewAspectHelper
    {
        static GameViewAspectHelper()
        {
            EditorApplication.delayCall += () => TrySetFreeAspect();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.delayCall += () => TrySetFreeAspect();
            }
        }

        [MenuItem("EraserWars/Use Game View Free Aspect")]
        public static void ApplyFreeAspect()
        {
            if (!TrySetFreeAspect())
            {
                Debug.LogWarning("[EraserWars] Could not set Game view to Free Aspect. Pick Free Aspect in the Game view toolbar.");
                return;
            }

            Debug.Log("[EraserWars] Game view set to Free Aspect (Scale 1x shows the full panel frame).");
        }

        [MenuItem("EraserWars/Use Game View 16:9 Aspect")]
        public static void Apply16x9Aspect()
        {
            if (!TrySetAspectRatio(16f, 9f))
            {
                Debug.LogWarning("[EraserWars] Could not set Game view to 16:9 Aspect. Pick Aspect Ratio → 16:9 in the Game view toolbar.");
                return;
            }

            Debug.Log("[EraserWars] Game view set to 16:9 Aspect.");
        }

        public static bool TrySetFreeAspect()
        {
            return TrySetBySizeTypeName("FreeAspect") || TrySetAspectRatio(0f, 0f);
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
                var typeName = sizeTypeEnum?.ToString() ?? string.Empty;
                var w = Convert.ToSingle(sizeType.GetProperty("width")?.GetValue(size, null) ?? 0);
                var h = Convert.ToSingle(sizeType.GetProperty("height")?.GetValue(size, null) ?? 0);

                if (width <= 0f && height <= 0f)
                {
                    if (typeName == "FreeAspect" || typeName == "AspectRatio" && w <= 0f)
                    {
                        targetIndex = i;
                        break;
                    }

                    continue;
                }

                if (typeName == "AspectRatio" && Mathf.Abs(w - width) < 0.01f && Mathf.Abs(h - height) < 0.01f)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex < 0 && width > 0f && height > 0f)
            {
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

        static bool TrySetBySizeTypeName(string typeName)
        {
            var editorAsm = typeof(EditorWindow).Assembly;
            var gameViewType = editorAsm.GetType("UnityEditor.GameView");
            var sizesType = editorAsm.GetType("UnityEditor.GameViewSizes");
            var singletonType = editorAsm.GetType("UnityEditor.ScriptableSingleton`1")?.MakeGenericType(sizesType);
            var sizes = singletonType?.GetProperty("instance")?.GetValue(null, null);
            var group = sizesType?.GetProperty("currentGroup")?.GetValue(sizes, null);
            if (gameViewType == null || group == null)
            {
                return false;
            }

            var groupType = group.GetType();
            var total = (int)groupType.GetMethod("GetTotalCount").Invoke(group, null);
            var getGameViewSize = groupType.GetMethod("GetGameViewSize");
            for (var i = 0; i < total; i++)
            {
                var size = getGameViewSize.Invoke(group, new object[] { i });
                var enumVal = size?.GetType().GetProperty("sizeType")?.GetValue(size, null)?.ToString();
                if (enumVal != typeName)
                {
                    continue;
                }

                var gameView = EditorWindow.GetWindow(gameViewType, false, null, false);
                var sizeSelection = gameViewType.GetMethod(
                    "SizeSelectionCallback",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                sizeSelection?.Invoke(gameView, new object[] { i, null });
                gameView?.Repaint();
                return true;
            }

            return false;
        }
    }
}
