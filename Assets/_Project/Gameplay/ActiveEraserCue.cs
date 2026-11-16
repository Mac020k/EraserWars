using System.Collections.Generic;
using EraserWars.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace EraserWars.Gameplay
{
    public readonly struct ActiveEraserCue
    {
        public const float ActiveScale = 1.14f;

        const float PulseHz = 1.15f;
        const float PulseFloor = 0.42f;
        const float EmissionGain = 1.05f;
        const float OutlineAlpha = 0.7f;

        public bool Show { get; }
        public Color Emission { get; }
        public Color Outline { get; }

        public ActiveEraserCue(bool show, Color emission, Color outline)
        {
            Show = show;
            Emission = emission;
            Outline = outline;
        }

        public static bool PhaseShowsCue(Phase phase) => phase switch
        {
            Phase.TurnStart => true,
            Phase.CardSelect => true,
            Phase.Aim => true,
            Phase.Simulating => false,
            Phase.Resolve => false,
            Phase.RoundEnd => false,
            Phase.MatchEnd => false,
            _ => false,
        };

        public static ActiveEraserCue At(bool show, Color seatColor, float timeSeconds)
        {
            if (!show)
            {
                return new ActiveEraserCue(false, Color.black, new Color(seatColor.r, seatColor.g, seatColor.b, 0f));
            }

            var pulse = Pulse01(timeSeconds);
            var emission = seatColor * (EmissionGain * pulse);
            emission.a = 1f;
            var outline = seatColor;
            outline.a = OutlineAlpha * pulse;
            return new ActiveEraserCue(true, emission, outline);
        }

        static float Pulse01(float timeSeconds)
        {
            var wave = 0.5f + 0.5f * Mathf.Sin(timeSeconds * PulseHz * (Mathf.PI * 2f));
            return PulseFloor + (1f - PulseFloor) * wave;
        }
    }

    public sealed class ActiveEraserCueView
    {
        const float EaseRate = 12f;
        const int Segments = 32;
        const float RingRadius = 0.84f;
        const float RingHeight = 0.46f;
        const float LineWidth = 0.005f;
        const float VisibleEpsilon = 0.03f;

        readonly Dictionary<int, SeatCue> seats = new();
        static Material lineMaterial;

        public void Clear()
        {
            foreach (var seat in seats.Values)
            {
                if (seat.Line != null)
                {
                    Object.Destroy(seat.Line.gameObject);
                }
            }

            seats.Clear();
        }

        public void Apply(EraserBody eraser, ActiveEraserCue sample, float deltaSeconds)
        {
            if (eraser == null)
            {
                return;
            }

            var seat = Seat(eraser);
            var ease = Ease(deltaSeconds);
            var wantScale = sample.Show ? ActiveEraserCue.ActiveScale : 1f;
            seat.Scale = Mathf.Lerp(seat.Scale, wantScale, ease);
            eraser.transform.localScale = seat.BaseScale * seat.Scale;
            seat.Emission = Color.Lerp(seat.Emission, sample.Emission, ease);
            seat.Outline = Color.Lerp(seat.Outline, sample.Outline, ease);
            PaintBody(eraser, seat.Emission);
            PaintRing(eraser, seat);
        }

        SeatCue Seat(EraserBody eraser)
        {
            var id = eraser.PlayerIndex;
            if (!seats.TryGetValue(id, out var seat))
            {
                seat = new SeatCue { BaseScale = eraser.transform.localScale };
                seats[id] = seat;
            }

            return seat;
        }

        static float Ease(float deltaSeconds)
        {
            if (deltaSeconds <= 0f)
            {
                return 0f;
            }

            return 1f - Mathf.Exp(-EaseRate * deltaSeconds);
        }

        static void PaintBody(EraserBody eraser, Color emission)
        {
            var renderer = BodyRenderer(eraser);
            if (renderer == null)
            {
                return;
            }

            var material = renderer.material;
            if (!material.HasProperty("_EmissionColor"))
            {
                return;
            }

            material.SetColor("_EmissionColor", emission);
            if (emission.maxColorComponent > VisibleEpsilon)
            {
                material.EnableKeyword("_EMISSION");
                // EmissiveIsBlack keeps the pulse off the lightmapper. The shader still draws _EmissionColor.
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
            else
            {
                material.DisableKeyword("_EMISSION");
            }
        }

        void PaintRing(EraserBody eraser, SeatCue seat)
        {
            var visible = seat.Outline.a >= VisibleEpsilon;
            if (!visible)
            {
                if (seat.Line != null)
                {
                    seat.Line.enabled = false;
                }

                return;
            }

            var line = EnsureLine(eraser, seat);
            line.enabled = true;
            line.startColor = seat.Outline;
            line.endColor = seat.Outline;
            for (var i = 0; i < Segments; i++)
            {
                var angle = i * (Mathf.PI * 2f / Segments);
                var local = new Vector3(Mathf.Cos(angle) * RingRadius, RingHeight, Mathf.Sin(angle) * RingRadius);
                line.SetPosition(i, eraser.transform.TransformPoint(local));
            }
        }

        LineRenderer EnsureLine(EraserBody eraser, SeatCue seat)
        {
            if (seat.Line != null)
            {
                return seat.Line;
            }

            var host = new GameObject($"ActiveEraserOutline_{eraser.PlayerIndex}");
            var line = host.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = true;
            line.positionCount = Segments;
            line.widthMultiplier = LineWidth;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.numCornerVertices = 2;
            var material = LineMaterial();
            if (material != null)
            {
                line.sharedMaterial = material;
            }

            seat.Line = line;
            return line;
        }

        static Material LineMaterial()
        {
            if (lineMaterial != null)
            {
                return lineMaterial;
            }

            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Sprites/Default")
                         ?? Shader.Find("Unlit/Color");
            if (shader == null)
            {
                return null;
            }

            lineMaterial = new Material(shader) { name = "ActiveEraserOutline" };
            lineMaterial.color = Color.white;
            return lineMaterial;
        }

        static Renderer BodyRenderer(EraserBody eraser)
        {
            var renderers = eraser.GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer is TrailRenderer or LineRenderer)
                {
                    continue;
                }

                return renderer;
            }

            return null;
        }

        sealed class SeatCue
        {
            public Vector3 BaseScale;
            public float Scale = 1f;
            public Color Emission = Color.black;
            public Color Outline;
            public LineRenderer Line;
        }
    }
}
