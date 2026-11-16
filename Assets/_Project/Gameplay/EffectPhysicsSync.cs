using System.Collections.Generic;
using System.Collections.Immutable;
using EraserWars.Core;
using UnityEngine;

namespace EraserWars.Gameplay
{
    /// <summary>
    /// Converges eraser physics and spawned desk objects onto the active effects in <see cref="MatchState"/>.
    /// </summary>
    public sealed class EffectPhysicsSync : MonoBehaviour
    {
        const string DeskObjectPrefix = "object:";

        readonly List<((CardId card, PlayerId owner, string kind) key, GameObject spawned)> deskObjects = new();
        ImmutableArray<ActiveEffect> syncedEffects;
        bool hasSynced;
        Transform deskObjectRoot;

        public int DeskObjectCount => deskObjects.Count;

        public void Sync(MatchState state, IReadOnlyList<EraserBody> erasers)
        {
            if (state == null || (hasSynced && state.Effects == syncedEffects))
            {
                return;
            }

            hasSynced = true;
            syncedEffects = state.Effects;
            ActiveEffectTuning.ApplyTo(state, erasers);
            ReconcileDeskObjects(state, erasers);
        }

        void ReconcileDeskObjects(MatchState state, IReadOnlyList<EraserBody> erasers)
        {
            var wanted = new Dictionary<(CardId card, PlayerId owner, string kind), int>();
            foreach (var effect in state.Effects)
            {
                if (effect.SpecKind.StartsWith(DeskObjectPrefix))
                {
                    var key = (effect.SourceCard, effect.Owner, effect.SpecKind);
                    wanted[key] = wanted.TryGetValue(key, out var n) ? n + 1 : 1;
                }
            }

            for (var i = deskObjects.Count - 1; i >= 0; i--)
            {
                var (key, spawned) = deskObjects[i];
                if (spawned != null && wanted.TryGetValue(key, out var left) && left > 0)
                {
                    wanted[key] = left - 1;
                    continue;
                }

                if (spawned != null)
                {
                    Destroy(spawned);
                }

                deskObjects.RemoveAt(i);
            }

            foreach (var (key, count) in wanted)
            {
                var prefabKey = key.kind.Substring(DeskObjectPrefix.Length);
                if (!DeskObjectCatalog.TryGet(prefabKey, out var spec) || !TryFindEraser(erasers, key.owner, out var owner))
                {
                    continue;
                }

                for (var i = 0; i < count; i++)
                {
                    var spawned = DeskObjectCatalog.Spawn(prefabKey, spec, owner, erasers, DeskObjectRoot());
                    if (spawned == null)
                    {
                        continue;
                    }

                    deskObjects.Add((key, spawned));
                    var p = spawned.transform.position;
                    Debug.Log($"[EraserWars] DeskObject: spawned {prefabKey} for P{key.owner.Value + 1} at ({p.x:0.000},{p.y:0.000},{p.z:0.000})");
                }
            }
        }

        Transform DeskObjectRoot()
        {
            if (deskObjectRoot == null)
            {
                deskObjectRoot = new GameObject("DeskObjects").transform;
            }

            return deskObjectRoot;
        }

        static bool TryFindEraser(IReadOnlyList<EraserBody> erasers, PlayerId owner, out EraserBody eraser)
        {
            foreach (var candidate in erasers)
            {
                if (candidate != null && candidate.PlayerIndex == owner.Value && candidate.gameObject.activeInHierarchy)
                {
                    eraser = candidate;
                    return true;
                }
            }

            eraser = null;
            return false;
        }
    }
}
