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

        // Broadcast-style debug chrome: top banner stays readable on stream, side panel keeps the desk clear.
        static readonly Rect SidePanelRect = new(12f, 96f, 288f, 318f);

        GUIStyle bannerTitleStyle;
        GUIStyle bannerHintStyle;
        GUIStyle bannerStatStyle;
        GUIStyle seatStyle;
        GUIStyle stockStyle;
        GUIStyle panelHeaderStyle;
        bool stylesReady;

        public MatchState State => state;
        public IReadOnlyList<EraserBody> Erasers => erasers;

        public void SetDebugChromeVisible(bool visible)
        {
            showDebug = visible;
        }

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
            return TopBarRect().Contains(guiPosition) || SidePanelRect.Contains(guiPosition);
        }

        static Rect TopBarRect()
        {
            return new Rect(12f, 12f, Mathf.Max(320f, Screen.width - 24f), 72f);
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

        void Update()
        {
            TickTurnTimer(Time.unscaledDeltaTime);
            RefreshActiveEraserCue();
        }

        void RefreshActiveEraserCue()
        {
            var cueSeat = -1;
            if (state != null && state.Phase is Phase.Aim or Phase.CardSelect or Phase.TurnStart)
            {
                cueSeat = state.CurrentPlayerId.Value;
            }

            for (var i = 0; i < erasers.Count; i++)
            {
                var eraser = erasers[i];
                if (eraser == null)
                {
                    continue;
                }

                var want = eraser.PlayerIndex == cueSeat ? 1.14f : 1f;
                var scale = eraser.transform.localScale;
                var next = Mathf.Lerp(scale.x, want, 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime));
                eraser.transform.localScale = new Vector3(next, next, next);
            }
        }

        void TickTurnTimer(float deltaSeconds)
        {
            if (state == null || deltaSeconds <= 0f)
            {
                return;
            }

            if (state.Rules.TurnTimeSeconds <= 0f)
            {
                return;
            }

            // Only burn the clock while the seat can still flick or pass.
            if (state.Phase is not (Phase.Aim or Phase.CardSelect or Phase.TurnStart))
            {
                return;
            }

            var next = state.TurnTimeLeft - deltaSeconds;
            if (next > 0f)
            {
                state = state with { TurnTimeLeft = next };
                return;
            }

            state = state with { TurnTimeLeft = 0f };
            state = MatchReduce.Reduce(state, new TimerExpired(state.CurrentPlayerId));
            SyncActiveSeat();
            RespawnIfNeeded();
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
            EnsureOrbitCamera()?.FrameDeskOverview();
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
            // Keep the player's orbit framing from the previous turn; only drop chase focus.
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

            EnsureGuiStyles();
            DrawTopBanner();
            DrawSidePanel();
        }

        void EnsureGuiStyles()
        {
            if (stylesReady)
            {
                return;
            }

            bannerTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white },
            };
            bannerHintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.78f, 0.80f, 0.74f) },
            };
            bannerStatStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(1f, 0.93f, 0.70f) },
            };
            seatStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white },
            };
            stockStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white },
            };
            panelHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.82f, 0.84f, 0.78f) },
            };
            stylesReady = true;
        }

        void DrawTopBanner()
        {
            var bar = TopBarRect();
            DrawPanel(bar, new Color(0.05f, 0.07f, 0.10f, 0.88f), new Color(0.95f, 0.78f, 0.28f, 0.95f));
            GUILayout.BeginArea(new Rect(bar.x + 16f, bar.y + 8f, bar.width - 32f, bar.height - 14f));
            GUILayout.BeginHorizontal();
            GUILayout.Label("ERASER WARS", bannerTitleStyle, GUILayout.Width(210f));
            GUILayout.Label($"ROUND {state.Round}", bannerStatStyle, GUILayout.Width(110f));
            var seatColor = PlayerColors[activeSeat % PlayerColors.Length];
            var prev = GUI.color;
            GUI.color = seatColor;
            GUILayout.Label(
                $"SEAT {activeSeat}  {PlayerIndexColorName(activeSeat).ToUpperInvariant()}",
                seatStyle,
                GUILayout.Width(210f));
            GUI.color = prev;
            GUILayout.Label($"TIME {state.TurnTimeLeft:0.0}", bannerStatStyle, GUILayout.Width(110f));
            GUILayout.Label($"{state.Phase}", bannerStatStyle, GUILayout.Width(150f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label("LMB flick  ·  RMB orbit  ·  Tab top-down  ·  Space / Pass skip", bannerHintStyle);
            GUILayout.EndArea();
        }

        void DrawSidePanel()
        {
            DrawPanel(SidePanelRect, new Color(0.04f, 0.05f, 0.08f, 0.86f), new Color(1f, 1f, 1f, 0.16f));
            GUILayout.BeginArea(new Rect(SidePanelRect.x + 14f, SidePanelRect.y + 12f, SidePanelRect.width - 28f, SidePanelRect.height - 24f));
            GUILayout.Label("STOCKS", panelHeaderStyle);
            GUILayout.Space(4f);
            foreach (var p in state.Players)
            {
                var idx = p.Id.Value;
                var color = p.IsAlive ? PlayerColors[idx % PlayerColors.Length] : new Color(0.38f, 0.38f, 0.38f);
                DrawStockRow(idx, p.Stocks, p.Points, color, idx == activeSeat);
            }

            GUILayout.Space(12f);
            GUILayout.Label("MATCH", panelHeaderStyle);
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Hotseat 2P", GUILayout.Height(34f)))
            {
                StartHotseat(2);
            }

            if (GUILayout.Button("Hotseat 4P", GUILayout.Height(34f)))
            {
                StartHotseat(4);
            }

            GUILayout.EndHorizontal();

            if (state.Phase == Phase.MatchEnd && GUILayout.Button("Rematch", GUILayout.Height(36f)))
            {
                Rematch();
            }

            if (GUILayout.Button("PASS", GUILayout.Height(42f)))
            {
                Pass();
            }

            GUILayout.EndArea();
        }

        void DrawStockRow(int index, int stocks, int points, Color color, bool active)
        {
            var row = GUILayoutUtility.GetRect(1f, 28f, GUILayout.ExpandWidth(true));
            var prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, active ? 0.12f : 0.05f);
            GUI.DrawTexture(row, Texture2D.whiteTexture);
            GUI.color = color;
            GUI.DrawTexture(new Rect(row.x, row.y + 4f, 5f, row.height - 8f), Texture2D.whiteTexture);
            GUI.color = prev;
            var mark = active ? "▶" : " ";
            GUI.Label(
                new Rect(row.x + 12f, row.y, row.width - 12f, row.height),
                $"{mark} P{index}   stocks {stocks}   pts {points}",
                stockStyle);
        }

        static void DrawPanel(Rect rect, Color fill, Color accent)
        {
            var prev = GUI.color;
            GUI.color = fill;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = accent;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 3f), Texture2D.whiteTexture);
            GUI.color = new Color(1f, 1f, 1f, 0.08f);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), Texture2D.whiteTexture);
            GUI.color = prev;
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

