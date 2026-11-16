using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    public static class SecretRoomPanel
    {
        public const string PanelName = "secret-room-panel";
        public const string OpenName = "open-secret-room";
        public const string CloseName = "close-secret-room";
        public const string FieldName = "join-code";
        public const string GenerateName = "secret-generate";
        public const string CopyName = "secret-copy";
        public const string StatusName = "secret-status";

        const string BoundClass = "secret-room-bound";
        const string StatusMade = "secret-status-made";
        const string StatusCopied = "secret-status-copied";
        const string StatusBad = "secret-status-bad";

        public static void Bind(VisualElement root)
        {
            var panel = root?.Q(PanelName);
            if (panel == null || panel.ClassListContains(BoundClass))
            {
                return;
            }

            panel.AddToClassList(BoundClass);
            var field = panel.Q<TextField>(FieldName);
            if (field != null)
            {
                field.maxLength = JoinCode.Length;
                field.multiline = false;
                field.labelElement.style.display = DisplayStyle.None;
                field.RegisterValueChangedCallback(evt =>
                {
                    var upper = evt.newValue == null ? string.Empty : evt.newValue.ToUpperInvariant();
                    if (upper != evt.newValue)
                    {
                        field.SetValueWithoutNotify(upper);
                    }
                });
            }

            panel.Q<Button>(GenerateName)?.RegisterCallback<ClickEvent>(_ => Generate(root, null));
            panel.Q<Button>(CopyName)?.RegisterCallback<ClickEvent>(_ => Copy(root));
        }

        static string Generate(VisualElement root, System.Random random)
        {
            var code = JoinCode.Generate(random);
            root?.Q<TextField>(FieldName)?.SetValueWithoutNotify(code);
            ShowStatus(root?.Q<Label>(StatusName), StatusMade, UiKey.SecretRoomMade);
            return code;
        }

        static bool Copy(VisualElement root)
        {
            var field = root?.Q<TextField>(FieldName);
            var code = JoinCode.Normalize(field?.value);
            if (code == null)
            {
                ShowStatus(root?.Q<Label>(StatusName), StatusBad, UiKey.SecretRoomBadCode);
                return false;
            }

            field.SetValueWithoutNotify(code);
            GUIUtility.systemCopyBuffer = code;
            ShowStatus(root?.Q<Label>(StatusName), StatusCopied, UiKey.SecretRoomCopied);
            return true;
        }

        public static void RefreshStatus(VisualElement root)
        {
            var status = root?.Q<Label>(StatusName);
            if (status == null)
            {
                return;
            }

            if (status.ClassListContains(StatusMade))
            {
                status.text = UiText.Get(UiKey.SecretRoomMade);
            }
            else if (status.ClassListContains(StatusCopied))
            {
                status.text = UiText.Get(UiKey.SecretRoomCopied);
            }
            else if (status.ClassListContains(StatusBad))
            {
                status.text = UiText.Get(UiKey.SecretRoomBadCode);
            }
        }

        static void ShowStatus(Label status, string marker, UiKey key)
        {
            if (status == null)
            {
                return;
            }

            status.EnableInClassList(StatusMade, marker == StatusMade);
            status.EnableInClassList(StatusCopied, marker == StatusCopied);
            status.EnableInClassList(StatusBad, marker == StatusBad);
            status.text = UiText.Get(key);
        }
    }
}
