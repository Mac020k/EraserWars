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

        const float MinDrag01 = 0.05f;

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
        LineRenderer aimLine;
        Material lineMaterial;
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
            if (lineMaterial != null)
            {
                Destroy(lineMaterial);
                lineMaterial = null;
            }
        }

        bool CanTrack()
        {
            if (director == null || director.State == null || director.IsStartMenuOpen || director.IsCardCutInShowing)
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

            EnsureAimLine();
            var origin = target.transform.position + Vector3.up * 0.03f;
            var heldSeconds = Time.time - pressTime;
            var (dx, dz) = FlickMath.SwayedDirection(
                pull.x,
                pull.z,
                length,
                heldSeconds,
                target.SizeFactor,
                amplitude0,
                frequency0,
                frequencySlope);
            var end = origin + new Vector3(dx, 0f, dz) * (length * Mathf.Max(0.05f, aimLineMeters));
            aimLine.enabled = true;
            aimLine.positionCount = 2;
            aimLine.SetPosition(0, origin);
            aimLine.SetPosition(1, end);
            ApplyLineColor(aimColor);
        }

        void HideAim()
        {
            if (aimLine != null)
            {
                aimLine.enabled = false;
            }
        }

        void EnsureAimLine()
        {
            if (aimLine != null)
            {
                return;
            }

            var host = new GameObject("AimLine");
            host.transform.SetParent(transform, false);
            aimLine = host.AddComponent<LineRenderer>();
            aimLine.useWorldSpace = true;
            aimLine.alignment = LineAlignment.View;
            aimLine.loop = false;
            aimLine.positionCount = 2;
            aimLine.widthMultiplier = 0.02f;
            aimLine.numCapVertices = 2;
            aimLine.shadowCastingMode = ShadowCastingMode.Off;
            aimLine.receiveShadows = false;
            aimLine.enabled = false;
            lineMaterial = CreateLineMaterial();
            if (lineMaterial != null)
            {
                aimLine.sharedMaterial = lineMaterial;
            }
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

        void ApplyLineColor(Color color)
        {
            aimLine.startColor = color;
            aimLine.endColor = color;
            if (lineMaterial == null)
            {
                return;
            }

            if (lineMaterial.HasProperty("_BaseColor"))
            {
                lineMaterial.SetColor("_BaseColor", color);
            }

            if (lineMaterial.HasProperty("_Color"))
            {
                lineMaterial.SetColor("_Color", color);
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
