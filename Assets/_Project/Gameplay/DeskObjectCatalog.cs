using System;
using System.Collections.Generic;
using UnityEngine;

namespace EraserWars.Gameplay
{
    public enum DeskObjectShape
    {
        Wall,
        Cone,
        DragZone,
    }

    public enum DeskObjectPlacement
    {
        EdgeGuard,
        BetweenRival,
    }

    /// <summary>Size is (along the edge, height, depth toward the edge) in meters.</summary>
    public readonly struct DeskObjectSpec
    {
        public readonly DeskObjectShape Shape;
        public readonly DeskObjectPlacement Placement;
        public readonly Vector3 Size;
        public readonly float Gap;
        public readonly float Drag;
        public readonly Color Color;

        public DeskObjectSpec(DeskObjectShape shape, DeskObjectPlacement placement, Vector3 size, float gap, float drag, Color color)
        {
            Shape = shape;
            Placement = placement;
            Size = size;
            Gap = gap;
            Drag = drag;
            Color = color;
        }
    }

    public static class DeskObjectCatalog
    {
        static readonly Dictionary<string, DeskObjectSpec> Specs = new()
        {
            ["RulerWall"] = new DeskObjectSpec(
                DeskObjectShape.Wall, DeskObjectPlacement.EdgeGuard,
                new Vector3(0.15f, 0.015f, 0.004f), 0.045f, 0f, new Color(0.96f, 0.86f, 0.38f)),
            ["Cone"] = new DeskObjectSpec(
                DeskObjectShape.Cone, DeskObjectPlacement.BetweenRival,
                new Vector3(0.025f, 0.03f, 0.025f), 0.06f, 0f, new Color(1f, 0.45f, 0.12f)),
            ["DustPatch"] = new DeskObjectSpec(
                DeskObjectShape.DragZone, DeskObjectPlacement.EdgeGuard,
                new Vector3(0.10f, 0.004f, 0.06f), 0.02f, 25f, new Color(0.78f, 0.76f, 0.72f)),
            ["DustPile"] = new DeskObjectSpec(
                DeskObjectShape.DragZone, DeskObjectPlacement.EdgeGuard,
                new Vector3(0.14f, 0.008f, 0.08f), 0.02f, 40f, new Color(0.70f, 0.68f, 0.64f)),
        };

        static PhysicsMaterial obstacleSurface;

        static PhysicsMaterial ObstacleSurface
        {
            get
            {
                if (obstacleSurface == null)
                {
                    obstacleSurface = new PhysicsMaterial("DeskObstacle")
                    {
                        dynamicFriction = 0.3f,
                        staticFriction = 0.3f,
                        bounciness = 0.4f,
                        bounceCombine = PhysicsMaterialCombine.Maximum,
                    };
                }

                return obstacleSurface;
            }
        }

        public static bool TryGet(string prefabKey, out DeskObjectSpec spec) => Specs.TryGetValue(prefabKey, out spec);

        public static GameObject Spawn(
            string prefabKey,
            DeskObjectSpec spec,
            EraserBody owner,
            IReadOnlyList<EraserBody> erasers,
            Transform parent)
        {
            if (!TryFindDeskTop(owner.Body.position, parent, out var desk))
            {
                return null;
            }

            var pose = spec.Placement switch
            {
                DeskObjectPlacement.EdgeGuard => EdgeGuardPose(owner.Body.position, desk, spec),
                DeskObjectPlacement.BetweenRival => TryFindRival(owner, erasers, out var rival)
                    ? BetweenPose(owner.Body.position, rival.Body.position, desk, spec)
                    : EdgeGuardPose(owner.Body.position, desk, spec),
                _ => throw new InvalidOperationException($"Unhandled placement {spec.Placement}"),
            };

            var go = Build(spec);
            go.name = $"DeskObject_{prefabKey}_P{owner.PlayerIndex}";
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(pose.position, pose.rotation);
            return go;
        }

        static GameObject Build(DeskObjectSpec spec)
        {
            GameObject go;
            switch (spec.Shape)
            {
                case DeskObjectShape.Wall:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.localScale = spec.Size;
                    go.GetComponent<Collider>().sharedMaterial = ObstacleSurface;
                    break;
                case DeskObjectShape.Cone:
                    // Unity's cylinder primitive is 2 units tall.
                    go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    go.transform.localScale = new Vector3(spec.Size.x, spec.Size.y * 0.5f, spec.Size.z);
                    go.GetComponent<Collider>().sharedMaterial = ObstacleSurface;
                    break;
                case DeskObjectShape.DragZone:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.localScale = spec.Size;
                    go.GetComponent<Collider>().isTrigger = true;
                    go.AddComponent<DeskDragZone>().DragPerSecond = spec.Drag;
                    break;
                default:
                    throw new InvalidOperationException($"Unhandled shape {spec.Shape}");
            }

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = spec.Color;
            }

            return go;
        }

        static bool TryFindDeskTop(Vector3 at, Transform deskObjects, out Bounds desk)
        {
            desk = default;
            var best = float.PositiveInfinity;
            var hits = Physics.RaycastAll(at + Vector3.up * 0.3f, Vector3.down, 1f, ~0, QueryTriggerInteraction.Ignore);
            foreach (var hit in hits)
            {
                if (hit.distance >= best
                    || hit.collider.GetComponentInParent<EraserBody>() != null
                    || hit.transform.IsChildOf(deskObjects))
                {
                    continue;
                }

                best = hit.distance;
                desk = hit.collider.bounds;
            }

            return !float.IsPositiveInfinity(best);
        }

        static Pose EdgeGuardPose(Vector3 from, Bounds desk, DeskObjectSpec spec)
        {
            var toEdge = new[]
            {
                (from.x - desk.min.x, Vector3.left),
                (desk.max.x - from.x, Vector3.right),
                (from.z - desk.min.z, Vector3.back),
                (desk.max.z - from.z, Vector3.forward),
            };
            var (distance, normal) = toEdge[0];
            foreach (var candidate in toEdge)
            {
                if (candidate.Item1 < distance)
                {
                    (distance, normal) = candidate;
                }
            }

            var halfDepth = spec.Size.z * 0.5f;
            var reach = Mathf.Min(spec.Gap + halfDepth, distance - halfDepth);
            var center = from + normal * reach;
            center.y = desk.max.y + spec.Size.y * 0.5f;
            return new Pose(center, Quaternion.LookRotation(normal));
        }

        static Pose BetweenPose(Vector3 from, Vector3 rival, Bounds desk, DeskObjectSpec spec)
        {
            var center = (from + rival) * 0.5f;
            center.y = desk.max.y + spec.Size.y * 0.5f;
            return new Pose(center, Quaternion.identity);
        }

        static bool TryFindRival(EraserBody owner, IReadOnlyList<EraserBody> erasers, out EraserBody rival)
        {
            rival = null;
            var best = float.PositiveInfinity;
            foreach (var other in erasers)
            {
                if (other == null || other == owner || !other.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var d = (other.Body.position - owner.Body.position).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    rival = other;
                }
            }

            return rival != null;
        }
    }
}
