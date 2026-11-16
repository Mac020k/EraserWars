using System;
using EraserWars.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace EraserWars.Gameplay
{
    public sealed class HotseatFlickInput : MonoBehaviour
    {
        enum TrackpadPhase
        {
            Idle,
            Aiming,
            Released,
        }

        readonly struct TrackpadGesture
        {
            public TrackpadGesture(Vector2 pressScreen, Vector2 releaseScreen, float heldSeconds)
            {
                PressScreen = pressScreen;
                ReleaseScreen = releaseScreen;
                HeldSeconds = heldSeconds;
            }

            public Vector2 PressScreen { get; }
            public Vector2 ReleaseScreen { get; }
            public float HeldSeconds { get; }
        }

        readonly struct AimStroke
        {
            public AimStroke(LineRenderer line, float lift)
            {
                Line = line;
                Lift = lift;
            }

            public LineRenderer Line { get; }
            public float Lift { get; }
        }

        const float MinDrag01 = 0.05f;
        const int StrokeOutline = 0;
        const int StrokeCone = 1;
        const int StrokePower = 2;
        const int StrokeRelease = 3;
        const int StrokeCount = 4;

        [SerializeField] MatchDirector director;
        [SerializeField] Camera worldCamera;
        [SerializeField] float maxDragPixels = 180f;
        [SerializeField] float aimLineMeters = 0.8f;
        [SerializeField] float amplitude0 = FlickMath.DefaultSwayAmplitude;
        [SerializeField] float frequency0 = FlickMath.DefaultSwayFrequency;
        [SerializeField] float frequencySlope = FlickMath.DefaultSwayFrequencySlope;

        TrackpadPhase phase;
        EraserBody target;
        Vector2 pressScreen;
        float pressTime;
        TrackpadGesture pending;
        bool hasPending;
        AimStroke[] strokes;
        Material[] strokeMaterials;
        Color aimColor = Color.white;

        void Update()
        {
            if (!CanTrack())
            {
                ClearGesture();
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
            {
                ClearGesture();
                director.Pass();
                return;
            }

            var mouse = Mouse.current;
            if (mouse == null)
            {
                ClearGesture();
                return;
            }

            phase = Step(phase, mouse);
            if (phase == TrackpadPhase.Aiming && !mouse.leftButton.isPressed)
            {
                phase = Step(phase, mouse);
            }

            if (phase == TrackpadPhase.Released)
            {
                phase = Step(phase, mouse);
            }
        }

        void OnDestroy()
        {
            if (strokeMaterials == null)
            {
                return;
            }

            for (var i = 0; i < strokeMaterials.Length; i++)
            {
                if (strokeMaterials[i] == null)
                {
                    continue;
                }

                Destroy(strokeMaterials[i]);
                strokeMaterials[i] = null;
            }
        }

        bool CanTrack()
        {
            if (director == null || director.State == null || director.IsStartMenuOpen)
            {
                return false;
            }

            var matchPhase = director.State.Phase;
            return matchPhase != Phase.MatchEnd && matchPhase != Phase.Simulating;
        }

        TrackpadPhase Step(TrackpadPhase current, Mouse mouse)
        {
            switch (current)
            {
                case TrackpadPhase.Idle:
                    return TickIdle(mouse);
                case TrackpadPhase.Aiming:
                    return TickAiming(mouse);
                case TrackpadPhase.Released:
                    return TickReleased();
                default:
                    var unexpected = current;
                    throw new ArgumentOutOfRangeException(nameof(current), unexpected, null);
            }
        }

        TrackpadPhase TickIdle(Mouse mouse)
        {
            if (!mouse.leftButton.wasPressedThisFrame)
            {
                return TrackpadPhase.Idle;
            }

            var screen = mouse.position.ReadValue();
            if (director.IsOverDebugChrome(screen))
            {
                return TrackpadPhase.Idle;
            }

            if (!director.TryGetCurrentEraser(out var eraser))
            {
                return TrackpadPhase.Idle;
            }

            target = eraser;
            pressScreen = screen;
            pressTime = Time.time;
            aimColor = ReadTint(eraser);
            return TrackpadPhase.Aiming;
        }

        TrackpadPhase TickAiming(Mouse mouse)
        {
            var screen = mouse.position.ReadValue();
            if (mouse.leftButton.isPressed)
            {
                ShowAim(screen);
                return TrackpadPhase.Aiming;
            }

            pending = new TrackpadGesture(pressScreen, screen, Time.time - pressTime);
            hasPending = true;
            return TrackpadPhase.Released;
        }

        TrackpadPhase TickReleased()
        {
            var gesture = pending;
            var eraser = target;
            var armed = hasPending;
            ClearGesture();
            if (!armed || eraser == null || director.State == null)
            {
                return TrackpadPhase.Idle;
            }

            if (!director.TryGetCurrentEraser(out var current) || current != eraser)
            {
                return TrackpadPhase.Idle;
            }

            if (!TryMap(gesture, eraser, out var command))
            {
                return TrackpadPhase.Idle;
            }

            director.SubmitFlick(command);
            return TrackpadPhase.Idle;
        }

        void ClearGesture()
        {
            phase = TrackpadPhase.Idle;
            hasPending = false;
            target = null;
            HideAim();
        }

        bool TryMap(TrackpadGesture gesture, EraserBody eraser, out FlickCommand command)
        {
            command = default;
            var length = DragLength01(gesture.PressScreen, gesture.ReleaseScreen);
            if (length < MinDrag01)
            {
                return false;
            }

            var camera = ResolveCamera();
            if (camera == null)
            {
                return false;
            }

            if (!TryPull(camera, gesture.PressScreen, gesture.ReleaseScreen, eraser.transform.position.y, out var pull))
            {
                return false;
            }

            var direction = pull.normalized;
            command = new FlickCommand(
                director.State.CurrentPlayerId,
                direction.x,
                direction.z,
                length,
                0f,
                0f,
                gesture.HeldSeconds);
            return true;
        }

        float DragLength01(Vector2 press, Vector2 release)
        {
            var span = Mathf.Max(1f, maxDragPixels);
            return Mathf.Clamp01((release - press).magnitude / span);
        }

        static bool TryPull(Camera camera, Vector2 pressScreen, Vector2 releaseScreen, float planeY, out Vector3 pull)
        {
            var plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));
            if (TryHitPlane(camera, pressScreen, plane, out var pressWorld)
                && TryHitPlane(camera, releaseScreen, plane, out var releaseWorld))
            {
                pull = pressWorld - releaseWorld;
                pull.y = 0f;
                if (pull.sqrMagnitude > 1e-8f)
                {
                    return true;
                }
            }

            return TryCameraRelative(camera, pressScreen, releaseScreen, out pull);
        }

        static bool TryHitPlane(Camera camera, Vector2 screen, Plane plane, out Vector3 point)
        {
            var ray = camera.ScreenPointToRay(screen);
            if (!plane.Raycast(ray, out var enter))
            {
                point = default;
                return false;
            }

            point = ray.GetPoint(enter);
            return true;
        }

        static bool TryCameraRelative(Camera camera, Vector2 pressScreen, Vector2 releaseScreen, out Vector3 pull)
        {
            // Screen +Y is up. A downward pull aims along camera forward on XZ.
            var delta = pressScreen - releaseScreen;
            var right = camera.transform.right;
            right.y = 0f;
            var forward = camera.transform.forward;
            forward.y = 0f;
            if (right.sqrMagnitude < 1e-6f || forward.sqrMagnitude < 1e-6f)
            {
                pull = Vector3.zero;
                return false;
            }

            right.Normalize();
            forward.Normalize();
            pull = right * delta.x + forward * delta.y;
            pull.y = 0f;
            return pull.sqrMagnitude > 1e-8f;
        }

        void ShowAim(Vector2 currentScreen)
        {
            if (target == null)
            {
                HideAim();
                return;
            }

            var length = DragLength01(pressScreen, currentScreen);
            var camera = ResolveCamera();
            if (camera == null
                || length < 0.02f
                || !TryPull(camera, pressScreen, currentScreen, target.transform.position.y, out var pull))
            {
                HideAim();
                return;
            }

            EnsureAimStrokes();
            var origin = target.transform.position + Vector3.up * 0.03f;
            var heldSeconds = Time.time - pressTime;
            var sample = FlickMath.SampleAim(
                pull.x,
                pull.z,
                length,
                heldSeconds,
                target.SizeFactor,
                Mathf.Max(0.05f, aimLineMeters),
                amplitude0,
                frequency0,
                frequencySlope);
            DrawShaft(strokes[StrokeOutline], origin, sample);
            DrawFan(strokes[StrokeCone], origin, sample);
            DrawShaft(strokes[StrokePower], origin, sample);
            DrawRelease(strokes[StrokeRelease], origin, sample);
            ApplyStrokeColor(strokes[StrokePower].Line, strokeMaterials[StrokePower], aimColor);
        }

        void HideAim()
        {
            if (strokes == null)
            {
                return;
            }

            for (var i = 0; i < strokes.Length; i++)
            {
                if (strokes[i].Line != null)
                {
                    strokes[i].Line.enabled = false;
                }
            }
        }

        void DrawShaft(AimStroke stroke, Vector3 origin, FlickAimSample sample)
        {
            var line = stroke.Line;
            line.enabled = true;
            line.positionCount = 2;
            line.SetPosition(0, AimPoint(origin, stroke.Lift));
            line.SetPosition(1, AimPoint(origin, sample.CenterX, sample.CenterY, sample.LengthMeters, stroke.Lift));
        }

        void DrawFan(AimStroke stroke, Vector3 origin, FlickAimSample sample)
        {
            var line = stroke.Line;
            line.enabled = true;
            line.positionCount = 3;
            line.SetPosition(0, AimPoint(origin, sample.LeftX, sample.LeftY, sample.LengthMeters, stroke.Lift));
            line.SetPosition(1, AimPoint(origin, stroke.Lift));
            line.SetPosition(2, AimPoint(origin, sample.RightX, sample.RightY, sample.LengthMeters, stroke.Lift));
        }

        void DrawRelease(AimStroke stroke, Vector3 origin, FlickAimSample sample)
        {
            var length = sample.LengthMeters;
            var head = Mathf.Clamp(length * 0.16f, 0.012f, 0.05f);
            var cap = length * 0.45f;
            if (head > cap)
            {
                head = cap;
            }

            var dir = new Vector3(sample.ReleaseX, 0f, sample.ReleaseY);
            var perp = new Vector3(-dir.z, 0f, dir.x);
            var tip = origin + dir * length + Vector3.up * stroke.Lift;
            var notch = tip - dir * head;
            var wing = head * 0.62f;
            var line = stroke.Line;
            line.enabled = true;
            line.positionCount = 5;
            line.SetPosition(0, AimPoint(origin, stroke.Lift));
            line.SetPosition(1, tip);
            line.SetPosition(2, notch + perp * wing);
            line.SetPosition(3, tip);
            line.SetPosition(4, notch - perp * wing);
        }

        static Vector3 AimPoint(Vector3 origin, float lift)
        {
            return origin + Vector3.up * lift;
        }

        static Vector3 AimPoint(Vector3 origin, float x, float y, float distance, float lift)
        {
            return origin + new Vector3(x * distance, lift, y * distance);
        }

        void EnsureAimStrokes()
        {
            if (strokes != null)
            {
                return;
            }

            strokes = new AimStroke[StrokeCount];
            strokeMaterials = new Material[StrokeCount];
            strokes[StrokeOutline] = CreateStroke("AimOutline", 0.058f, new Color(0.08f, 0.07f, 0.06f, 1f), 0.002f, StrokeOutline);
            strokes[StrokeCone] = CreateStroke("AimCone", 0.012f, Color.white, 0.006f, StrokeCone);
            strokes[StrokePower] = CreateStroke("AimPower", 0.034f, aimColor, 0.010f, StrokePower);
            strokes[StrokeRelease] = CreateStroke("AimRelease", 0.016f, new Color(0.05f, 0.05f, 0.06f, 1f), 0.016f, StrokeRelease);
        }

        AimStroke CreateStroke(string name, float width, Color color, float lift, int index)
        {
            var host = new GameObject(name);
            host.transform.SetParent(transform, false);
            var line = host.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.alignment = LineAlignment.View;
            line.loop = false;
            line.positionCount = 2;
            line.widthMultiplier = width;
            line.numCapVertices = 4;
            line.numCornerVertices = 2;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.sortingOrder = index;
            line.enabled = false;
            var material = CreateLineMaterial();
            strokeMaterials[index] = material;
            if (material != null)
            {
                line.sharedMaterial = material;
            }

            ApplyStrokeColor(line, material, color);
            return new AimStroke(line, lift);
        }

        static Material CreateLineMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            if (shader == null)
            {
                return null;
            }

            return new Material(shader);
        }

        static void ApplyStrokeColor(LineRenderer line, Material material, Color color)
        {
            line.startColor = color;
            line.endColor = color;
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        Camera ResolveCamera()
        {
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            return worldCamera;
        }

        static Color ReadTint(EraserBody eraser)
        {
            var renderer = eraser.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                return Color.white;
            }

            var material = renderer.sharedMaterial;
            if (material == null)
            {
                return Color.white;
            }

            if (material.HasProperty("_BaseColor"))
            {
                var color = material.GetColor("_BaseColor");
                color.a = 1f;
                return color;
            }

            if (material.HasProperty("_Color"))
            {
                var color = material.GetColor("_Color");
                color.a = 1f;
                return color;
            }

            return Color.white;
        }
    }
}
