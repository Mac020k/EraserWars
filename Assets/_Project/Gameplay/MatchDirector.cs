using System.Collections.Generic;
using EraserWars.CameraRig;
using EraserWars.Core;
using UnityEngine;

namespace EraserWars.Gameplay
{
    public sealed class MatchDirector : MonoBehaviour
    {
        [SerializeField] FlickApplier flickApplier;
        [SerializeField] SettleWatcher settleWatcher;
        [SerializeField] Transform[] spawnPoints = new Transform[4];
        [SerializeField] EraserBody eraserPrefab;
        [SerializeField] OrbitCamera orbitCamera;
        [SerializeField] bool showDebug;

        readonly List<EraserBody> erasers = new();
        readonly Dictionary<int, EraserBody> byPlayer = new();
        HitstopController hitstop;
        EdgeSlowmoWatcher edgeSlowmo;
        MatchState state;
        int activeSeat;

        static readonly Rect DebugChromeRect = new(12f, 12f, 460f, 280f);

        public MatchState State => state;
        public IReadOnlyList<EraserBody> Erasers => erasers;

        public bool TryGetCurrentEraser(out EraserBody eraser)
        {
            eraser = null;
            if (state == null)
            {
                return false;
            }

            if (!byPlayer.TryGetValue(state.CurrentPlayerId.Value, out eraser) || eraser == null)
            {
                eraser = null;
                return false;
            }

            if (!eraser.gameObject.activeInHierarchy)
            {
                eraser = null;
                return false;
            }

            return true;
        }

        public bool IsOverDebugChrome(Vector2 screenPosition)
        {
            if (!showDebug || state == null)
            {
                return false;
            }

            // Input System origin is bottom-left. IMGUI origin is top-left.
            var guiPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
            return DebugChromeRect.Contains(guiPosition);
        }

        void Awake()
        {
            hitstop = GetComponent<HitstopController>();
            EnsureEdgeSlowmo();
            if (settleWatcher != null)
            {
                settleWatcher.Settled += OnSettled;
            }
        }

        void OnDestroy()
        {
            if (settleWatcher != null)
            {
                settleWatcher.Settled -= OnSettled;
            }
        }

        public void StartHotseat(int playerCount)
        {
            EnsureEdgeSlowmo();
            edgeSlowmo.BeginMatch();
            ClearErasers();
            state = MatchFactory.CreateHotseat(playerCount, RuleSet.CasualStock);
            for (var i = 0; i < playerCount; i++)
            {
                var spawn = spawnPoints[i % spawnPoints.Length];
                var eraser = Instantiate(eraserPrefab, spawn.position, spawn.rotation);
                eraser.gameObject.SetActive(true);
                eraser.PlayerIndex = i;
                eraser.name = $"Eraser_P{i}";
                ApplyPlayerLook(eraser, i);
                var fallout = eraser.GetComponent<FalloutDetector>();
                if (fallout != null)
                {
                    fallout.Fell += OnEraserFell;
                }

                erasers.Add(eraser);
                byPlayer[i] = eraser;
                BindEraserHitstop(eraser);
            }

            activeSeat = 0;
            state = MatchReduce.Reduce(state, new TurnStarted(state.CurrentPlayerId));
        }

        public void Rematch()
        {
            StartHotseat(state.Players.Length);
        }

        public void Pass()
        {
            if (state.Phase is Phase.MatchEnd or Phase.Simulating)
            {
                return;
            }

            state = MatchReduce.Reduce(state, new Passed(state.CurrentPlayerId));
            SyncActiveSeat();
        }

        public void SubmitFlick(FlickCommand command)
        {
            if (!byPlayer.TryGetValue(command.Player.Value, out var eraser))
            {
                return;
            }

            state = MatchReduce.Reduce(state, new Flicked(command));
            hitstop?.BeginShot();
            EnsureOrbitCamera()?.SetChaseTarget(eraser.transform);
            flickApplier.Apply(eraser, command);
            settleWatcher.Begin(erasers);
        }

        void OnEraserFell(EraserBody eraser)
        {
            var victim = new PlayerId(eraser.PlayerIndex);
            state = MatchReduce.Reduce(state, new EraserFell(victim, state.CurrentPlayerId));
            eraser.gameObject.SetActive(false);
        }

        void OnSettled()
        {
            state = MatchReduce.Reduce(state, new SimulationSettled());
            EnsureOrbitCamera()?.ClearChaseTarget();
            SyncActiveSeat();
            RespawnIfNeeded();
        }

        OrbitCamera EnsureOrbitCamera()
        {
            if (orbitCamera == null)
            {
                orbitCamera = FindAnyObjectByType<OrbitCamera>();
            }

            return orbitCamera;
        }

        void SyncActiveSeat()
        {
            activeSeat = state.CurrentPlayerId.Value;
        }

        void RespawnIfNeeded()
        {
            if (state.Phase == Phase.MatchEnd)
            {
                return;
            }

            var player = state.CurrentPlayer;
            if (!player.IsAlive)
            {
                return;
            }

            if (!byPlayer.TryGetValue(player.Id.Value, out var eraser))
            {
                return;
            }

            if (eraser.gameObject.activeSelf)
            {
                return;
            }

            var spawn = spawnPoints[player.Id.Value % spawnPoints.Length];
            eraser.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
            eraser.Body.linearVelocity = Vector3.zero;
            eraser.Body.angularVelocity = Vector3.zero;
            eraser.GetComponent<FalloutDetector>()?.ResetReport();
            eraser.gameObject.SetActive(true);
        }

        void BindEraserHitstop(EraserBody eraser)
        {
            if (hitstop == null || eraser == null)
            {
                return;
            }

            var relay = eraser.GetComponent<EraserCollisionRelay>();
            if (relay == null)
            {
                relay = eraser.gameObject.AddComponent<EraserCollisionRelay>();
            }

            relay.Bind(hitstop, IsSimulating);
        }

        public bool IsSimulating()
        {
            return state != null && state.Phase == Phase.Simulating;
        }

        void EnsureEdgeSlowmo()
        {
            if (edgeSlowmo == null)
            {
                edgeSlowmo = GetComponent<EdgeSlowmoWatcher>();
            }

            if (edgeSlowmo == null)
            {
                edgeSlowmo = gameObject.AddComponent<EdgeSlowmoWatcher>();
            }
        }

        void ClearErasers()
        {
            foreach (var eraser in erasers)
            {
                if (eraser != null)
                {
                    Destroy(eraser.gameObject);
                }
            }

            erasers.Clear();
            byPlayer.Clear();
        }

        static readonly Color[] PlayerColors =
        {
            new(0.92f, 0.28f, 0.35f),
            new(0.25f, 0.45f, 0.95f),
            new(0.25f, 0.75f, 0.40f),
            new(0.95f, 0.78f, 0.20f),
        };

        static void ApplyPlayerLook(EraserBody eraser, int playerIndex)
        {
            var renderer = eraser.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                return;
            }

            var color = PlayerColors[playerIndex % PlayerColors.Length];
            var mat = renderer.material;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }

            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
            }
        }

        void OnGUI()
        {
            if (!showDebug || state == null)
            {
                return;
            }

            GUILayout.BeginArea(DebugChromeRect);
            GUILayout.Label($"Phase={state.Phase} Round={state.Round} Seat={activeSeat}");
            GUILayout.Label($"TurnTime={state.TurnTimeLeft:0.0}");
            GUILayout.Label("Controls: LMB drag anywhere (pull back to aim) / Space or Pass");
            foreach (var p in state.Players)
            {
                var colorName = PlayerIndexColorName(p.Id.Value);
                GUILayout.Label($"{p.Id} ({colorName}) stocks={p.Stocks} pts={p.Points} alive={p.IsAlive}");
            }

            if (state.Phase == Phase.MatchEnd && GUILayout.Button("Rematch"))
            {
                Rematch();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Hotseat 2P"))
            {
                StartHotseat(2);
            }

            if (GUILayout.Button("Hotseat 4P"))
            {
                StartHotseat(4);
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Pass"))
            {
                Pass();
            }

            GUILayout.EndArea();
        }

        static string PlayerIndexColorName(int index)
        {
            return index switch
            {
                0 => "red",
                1 => "blue",
                2 => "green",
                3 => "yellow",
                _ => $"p{index}",
            };
        }
    }
}

