using System.IO;
using EraserWars.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.Editor
{
    /// <summary>
    /// Drop Temp/menu-shot.flag during Play with body
    /// "&lt;home|collection|options|deck&gt; &lt;output png&gt; [characterId] [16x9]".
    /// Shows that menu screen, saves the Game view, logs the smallest button on it,
    /// then hands a running match back.
    /// </summary>
    [InitializeOnLoad]
    public static class MenuShotFlag
    {
        const string Flag = "Temp/menu-shot.flag";
        const int SettleFrames = 6;
        const int SuperSize = 2;
        const int SaveTimeoutTicks = 600;

        enum Step
        {
            Idle,
            Settling,
            Saving,
        }

        static Step step;
        static StartMenuController menu;
        static string screen;
        static string output;
        static bool tookOverMatch;
        static bool forcedAspect;
        static int shotFrame;
        static int ticksLeft;

        static MenuShotFlag()
        {
            EditorApplication.update += Poll;
        }

        static void Poll()
        {
            switch (step)
            {
                case Step.Idle:
                    TryBegin();
                    break;
                case Step.Settling:
                    Settle();
                    break;
                case Step.Saving:
                    FinishWhenSaved();
                    break;
            }
        }

        static void TryBegin()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var flagPath = Path.Combine(projectRoot, Flag);
            if (!File.Exists(flagPath) || !EditorApplication.isPlaying || EditorApplication.isCompiling)
            {
                return;
            }

            var args = File.ReadAllText(flagPath).Trim().Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            File.Delete(flagPath);
            if (args.Length < 2)
            {
                Debug.LogWarning("[EraserWars] menu-shot: body must be '<screen> <output png> [characterId] [16x9]'");
                return;
            }

            menu = Object.FindAnyObjectByType<StartMenuController>(FindObjectsInactive.Include);
            if (menu == null)
            {
                Debug.LogWarning("[EraserWars] menu-shot: StartMenuController missing");
                return;
            }

            screen = args[0];
            output = Path.GetFullPath(Path.Combine(projectRoot, args[1]));
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            File.Delete(output);

            var characterId = -1;
            forcedAspect = false;
            for (var i = 2; i < args.Length; i++)
            {
                if (args[i] == "16x9")
                {
                    forcedAspect = GameViewAspectHelper.TrySetAspectRatio(16f, 9f);
                }
                else
                {
                    int.TryParse(args[i], out characterId);
                }
            }

            tookOverMatch = !menu.IsMenuVisible;
            if (tookOverMatch)
            {
                menu.ShowMenu(suspendMatch: true);
            }

            switch (screen)
            {
                case "home":
                    menu.GoHome();
                    break;
                case "collection":
                    menu.OpenCollection();
                    if (characterId >= 0)
                    {
                        menu.SelectCollectionCharacter(characterId);
                    }

                    break;
                case "options":
                    menu.OpenOptions();
                    break;
                case "deck":
                    menu.OpenDeck();
                    break;
                default:
                    Debug.LogWarning($"[EraserWars] menu-shot: unknown screen '{screen}'");
                    Restore();
                    return;
            }

            EditorApplication.isPaused = false;
            shotFrame = Time.frameCount + SettleFrames;
            step = Step.Settling;
        }

        static void Settle()
        {
            if (!EditorApplication.isPlaying)
            {
                step = Step.Idle;
                return;
            }

            EditorApplication.QueuePlayerLoopUpdate();
            if (Time.frameCount < shotFrame)
            {
                return;
            }

            LogLayout();
            ScreenCapture.CaptureScreenshot(output, SuperSize);
            ticksLeft = SaveTimeoutTicks;
            step = Step.Saving;
        }

        static void FinishWhenSaved()
        {
            EditorApplication.QueuePlayerLoopUpdate();
            ticksLeft--;
            if (!File.Exists(output) && ticksLeft > 0 && EditorApplication.isPlaying)
            {
                return;
            }

            Debug.Log($"[EraserWars] menu-shot: {screen} saved={File.Exists(output)} path={output}");
            Restore();
        }

        static void Restore()
        {
            if (forcedAspect)
            {
                GameViewAspectHelper.TrySetFreeAspect();
            }

            if (menu != null && EditorApplication.isPlaying)
            {
                if (tookOverMatch)
                {
                    menu.ResumeMatch();
                }
                else
                {
                    menu.GoHome();
                }
            }

            step = Step.Idle;
        }

        static void LogLayout()
        {
            var root = menu.GetComponent<UIDocument>()?.rootVisualElement;
            if (root == null)
            {
                Debug.LogWarning("[EraserWars] menu-shot: no root element");
                return;
            }

            var host = root.Q("menu-content");
            var page = screen == "home" ? root.Q("home-stack") : root.Q($"{screen}-panel");
            Button smallest = null;
            var buttons = 0;
            page?.Query<Button>().ForEach(button =>
            {
                var box = button.worldBound;
                if (!button.visible || box.width <= 0f || box.height <= 0f)
                {
                    return;
                }

                buttons++;
                if (smallest == null
                    || Mathf.Min(box.width, box.height) < Mathf.Min(smallest.worldBound.width, smallest.worldBound.height))
                {
                    smallest = button;
                }
            });

            var smallestBits = smallest == null
                ? "(none)"
                : $"'{(string.IsNullOrEmpty(smallest.name) ? smallest.text : smallest.name)}' {smallest.worldBound.width:0}x{smallest.worldBound.height:0}";
            Debug.Log(
                $"[EraserWars] menu-shot: {screen} screen={Screen.width}x{Screen.height} " +
                $"panel={host?.resolvedStyle.width:0}x{host?.resolvedStyle.height:0} " +
                $"page={page?.resolvedStyle.width:0}x{page?.resolvedStyle.height:0} " +
                $"buttons={buttons} smallestButton={smallestBits}");
        }
    }
}
