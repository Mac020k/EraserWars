using System.Collections.Generic;
using System.Collections.Immutable;
using EraserWars.Audio;
using EraserWars.CameraRig;
using EraserWars.Cards;
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
        [SerializeField] bool showDebug = true;

        readonly List<EraserBody> erasers = new();
        readonly Dictionary<int, EraserBody> byPlayer = new();
        readonly Dictionary<int, Vector3> eraserBaseScale = new();
        readonly Dictionary<int, float> eraserCueScale = new();
        HitstopController hitstop;
        EdgeSlowmoWatcher edgeSlowmo;
        EffectPhysicsSync effectPhysics;
        MatchState state;
        int activeSeat;
        const float ActiveEraserCueScale = 1.14f;

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
        public bool IsStartMenuOpen { get; private set; } = true;

        public void SetStartMenuOpen(bool open)
        {
            IsStartMenuOpen = open;
        }

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
            effectPhysics = GetComponent<EffectPhysicsSync>();
            if (effectPhysics == null)
            {
                effectPhysics = gameObject.AddComponent<EffectPhysicsSync>();
            }

            if (settleWatcher != null)
            {
                settleWatcher.Settled += OnSettled;
            }
        }

        void Update()
        {
            TickTurnTimer(Time.unscaledDeltaTime);
            RefreshActiveEraserCue();
            effectPhysics.Sync(state, erasers);
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

                var seat = eraser.PlayerIndex;
                if (!eraserBaseScale.TryGetValue(seat, out var baseScale))
                {
                    baseScale = eraser.transform.localScale;
                    eraserBaseScale[seat] = baseScale;
                    eraserCueScale[seat] = 1f;
                }

                var want = seat == cueSeat ? ActiveEraserCueScale : 1f;
                var current = eraserCueScale.TryGetValue(seat, out var cue) ? cue : 1f;
                var next = Mathf.Lerp(current, want, 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime));
                eraserCueScale[seat] = next;
                eraser.transform.localScale = baseScale * next;
            }
        }

        void TickTurnTimer(float deltaSeconds)
        {
            if (state == null || deltaSeconds <= 0f || IsStartMenuOpen)
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
            state = MatchEffectRules.OnTurnHandover(state);
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

        CharacterDefinition[] seatRoster = new CharacterDefinition[4];
        readonly List<CardDefinition>[] seatCommons =
        {
            new(), new(), new(), new(),
        };
        readonly Dictionary<int, CardDefinition> cardById = new();

        public void SetSeatRoster(IReadOnlyList<CharacterDefinition> roster)
        {
            for (var i = 0; i < seatRoster.Length; i++)
            {
                seatRoster[i] = roster != null && i < roster.Count ? roster[i] : null;
            }
        }

        public void SetSeatCommons(int seat, IReadOnlyList<CardDefinition> commons)
        {
            if (seat < 0 || seat >= seatCommons.Length)
            {
                return;
            }

            seatCommons[seat].Clear();
            if (commons == null)
            {
                return;
            }

            foreach (var card in commons)
            {
                if (card != null)
                {
                    seatCommons[seat].Add(card);
                    cardById[card.IdValue] = card;
                }
            }
        }

        public void StartHotseat(int playerCount)
        {
            EnsureEdgeSlowmo();
            edgeSlowmo.BeginMatch();
            ClearErasers();
            BuildCardIndex(playerCount);
            var hands = new ImmutableArray<CardId>[playerCount];
            var decks = new ImmutableArray<CardId>[playerCount];
            for (var i = 0; i < playerCount; i++)
            {
                BuildSeatCards(i, out hands[i], out decks[i]);
            }

            var rules = RuleSet.CasualStock with
            {
                TurnTimeSeconds = Mathf.Clamp(PlayerPrefs.GetInt("eraserwars.turnSeconds", 20), 10, 60),
            };
            state = MatchFactory.CreateHotseat(playerCount, rules, hands, decks);
            for (var i = 0; i < playerCount; i++)
            {
                var spawn = spawnPoints[i % spawnPoints.Length];
                var eraser = Instantiate(eraserPrefab, spawn.position, spawn.rotation);
                eraser.gameObject.SetActive(true);
                eraser.PlayerIndex = i;
                var character = i < seatRoster.Length ? seatRoster[i] : null;
                var label = character != null ? character.DisplayName : $"P{i}";
                eraser.name = $"Eraser_P{i}_{label}";
                eraser.ApplyDefinition(character?.Eraser);
                ApplyPlayerLook(eraser, i);
                var fallout = eraser.GetComponent<FalloutDetector>();
                if (fallout != null)
                {
                    fallout.Fell += OnEraserFell;
                }

                erasers.Add(eraser);
                byPlayer[i] = eraser;
                eraserBaseScale[i] = eraser.transform.localScale;
                eraserCueScale[i] = 1f;
                BindEraserHitstop(eraser);
            }

            activeSeat = 0;
            state = MatchReduce.Reduce(state, new TurnStarted(state.CurrentPlayerId));
            EnsureOrbitCamera()?.FrameDeskOverview();
        }

        void BuildCardIndex(int playerCount)
        {
            cardById.Clear();
            for (var i = 0; i < playerCount; i++)
            {
                var character = i < seatRoster.Length ? seatRoster[i] : null;
                RememberCard(character?.SignatureA);
                RememberCard(character?.SignatureB);
                if (i < seatCommons.Length)
                {
                    foreach (var card in seatCommons[i])
                    {
                        RememberCard(card);
                    }
                }
            }
        }

        void RememberCard(CardDefinition card)
        {
            if (card != null)
            {
                cardById[card.IdValue] = card;
            }
        }

        void BuildSeatCards(int seat, out ImmutableArray<CardId> hand, out ImmutableArray<CardId> deck)
        {
            var pool = new List<CardDefinition>();
            var character = seat < seatRoster.Length ? seatRoster[seat] : null;
            if (character?.SignatureA != null)
            {
                pool.Add(character.SignatureA);
            }

            if (character?.SignatureB != null)
            {
                pool.Add(character.SignatureB);
            }

            if (seat < seatCommons.Length)
            {
                pool.AddRange(seatCommons[seat]);
            }

            var handLimit = character?.Eraser?.HandLimit ?? 3;
            var handBuilder = ImmutableArray.CreateBuilder<CardId>();
            var deckBuilder = ImmutableArray.CreateBuilder<CardId>();
            for (var i = 0; i < pool.Count; i++)
            {
                var id = pool[i].Id;
                if (handBuilder.Count < handLimit)
                {
                    handBuilder.Add(id);
                }
                else
                {
                    deckBuilder.Add(id);
                }
            }

            hand = handBuilder.ToImmutable();
            deck = deckBuilder.ToImmutable();
        }

        public bool TryGetCard(int cardId, out CardDefinition card) =>
            cardById.TryGetValue(cardId, out card);

        public void Rematch()
        {
            if (state == null)
            {
                return;
            }

            StartHotseat(state.Players.Length);
        }

        public void Pass()
        {
            if (state.Phase is Phase.MatchEnd or Phase.Simulating)
            {
                return;
            }

            state = MatchReduce.Reduce(state, new Passed(state.CurrentPlayerId));
            state = MatchEffectRules.OnTurnHandover(state);
            SyncActiveSeat();
        }

        public void PlayCard(int cardId)
        {
            if (state == null || IsStartMenuOpen)
            {
                return;
            }

            var player = state.CurrentPlayerId;
            var beforeHand = state.Players[player.Value].Hand.Length;
            var card = new CardId(cardId);
            state = MatchReduce.Reduce(state, new CardPlayed(player, card));
            if (state.Players[player.Value].Hand.Length >= beforeHand)
            {
                return;
            }

            if (!cardById.TryGetValue(cardId, out var definition))
            {
                Debug.Log($"[EraserWars] PlayCard id={cardId} phase={state.Phase} (no definition)");
                return;
            }

            var effects = EffectSpecParser.ParseAll(definition.EffectKinds);
            if (effects.Count > 0)
            {
                state = CardResolver.Apply(state, player, card, effects, target: null);
                ApplyCardSideEffects(player, effects);
            }

            Debug.Log($"[EraserWars] PlayCard id={cardId} '{definition.DisplayName}' phase={state.Phase} effects={effects.Count} turnLeft={state.TurnTimeLeft:0.0} stocks={state.GetPlayer(player).Stocks}");
        }

        void ApplyCardSideEffects(PlayerId owner, IReadOnlyList<EffectSpec> effects)
        {
            foreach (var effect in effects)
            {
                if (effect is SwapPositions)
                {
                    SwapWithNextOpponent(owner);
                }
            }
        }

        void SwapWithNextOpponent(PlayerId owner)
        {
            var partner = MatchEffectRules.SwapPartner(state, owner);
            if (partner is not PlayerId other
                || !byPlayer.TryGetValue(owner.Value, out var mine)
                || !byPlayer.TryGetValue(other.Value, out var theirs)
                || mine == null
                || theirs == null
                || !mine.gameObject.activeSelf
                || !theirs.gameObject.activeSelf)
            {
                return;
            }

            var minePose = (mine.transform.position, mine.transform.rotation);
            var theirsPose = (theirs.transform.position, theirs.transform.rotation);
            PlaceAt(mine, theirsPose.position, theirsPose.rotation);
            PlaceAt(theirs, minePose.position, minePose.rotation);
            Debug.Log($"[EraserWars] SeatSwap: P{owner.Value + 1} <-> P{other.Value + 1}");
        }

        static void PlaceAt(EraserBody eraser, Vector3 position, Quaternion rotation)
        {
            eraser.transform.SetPositionAndRotation(position, rotation);
            eraser.Body.position = position;
            eraser.Body.rotation = rotation;
            eraser.Body.linearVelocity = Vector3.zero;
            eraser.Body.angularVelocity = Vector3.zero;
        }

        bool pendingExtraFlick;

        public void SubmitFlick(FlickCommand command)
        {
            if (!byPlayer.TryGetValue(command.Player.Value, out var eraser))
            {
                return;
            }

            pendingExtraFlick = ActiveEffectTuning.HasExtraFlick(state, command.Player);
            // Mass and surface must be live before the impulse lands, not on the next Update.
            effectPhysics.Sync(state, erasers);
            var modifiers = ActiveEffectTuning.For(state, command.Player);
            Debug.Log($"[EraserWars] Flick P{command.Player.Value + 1} {modifiers} bodyMass={eraser.Body.mass:0.0000} damping={eraser.Body.linearDamping:0.00}");

            state = MatchReduce.Reduce(state, new Flicked(command));
            if (PlayerPrefs.GetInt("eraserwars.cameraShake", 1) != 0)
            {
                hitstop?.BeginShot();
            }

            EnsureOrbitCamera()?.SetChaseTarget(eraser.transform);
            var flickScale = Mathf.Clamp(PlayerPrefs.GetFloat("eraserwars.flickScale", 1f), 0.5f, 1.5f);
            flickApplier.Apply(eraser, command, modifiers.Power * flickScale);
            AudioBus.Ensure().Play(AudioCue.Flick);
            settleWatcher.Begin(erasers);
        }

        void OnEraserFell(EraserBody eraser)
        {
            var victim = new PlayerId(eraser.PlayerIndex);
            if (MatchEffectRules.TryConsumeShield(state, victim, out var shielded))
            {
                state = shielded;
                var pose = FindClearSpawn(victim.Value);
                PlaceAt(eraser, pose.position, pose.rotation);
                eraser.GetComponent<FalloutDetector>()?.ResetReport();
                Debug.Log($"[EraserWars] Shield saved P{victim.Value + 1} from the edge; effects={state.Effects.Length}");
                return;
            }

            state = MatchReduce.Reduce(state, new EraserFell(victim, state.CurrentPlayerId));
            eraser.gameObject.SetActive(false);
        }

        public void DebugKnockOut(int playerIndex)
        {
            if (!byPlayer.TryGetValue(playerIndex, out var eraser) || eraser == null)
            {
                return;
            }

            OnEraserFell(eraser);
        }

        public void DebugSettle()
        {
            if (state == null || state.Phase != Phase.Simulating)
            {
                return;
            }

            OnSettled();
        }

        void OnSettled()
        {
            if (pendingExtraFlick)
            {
                pendingExtraFlick = false;
                state = CardResolver.TickEffects(ConsumeExtraFlick(state));
                state = state with { Phase = Phase.Aim };
                EnsureOrbitCamera()?.ClearChaseTarget();
                SyncActiveSeat();
                RespawnIfNeeded();
                Debug.Log($"[EraserWars] ExtraFlick: P{state.CurrentPlayerId.Value + 1} gets another aim");
                return;
            }

            state = MatchReduce.Reduce(state, new SimulationSettled());
            state = MatchEffectRules.OnTurnHandover(state);
            state = CardResolver.TickEffects(state);
            // Keep the player's orbit framing from the previous turn; only drop chase focus.
            EnsureOrbitCamera()?.ClearChaseTarget();
            SyncActiveSeat();
            RespawnIfNeeded();
        }

        static MatchState ConsumeExtraFlick(MatchState current)
        {
            var builder = current.Effects.ToBuilder();
            for (var i = builder.Count - 1; i >= 0; i--)
            {
                var effect = builder[i];
                if (effect.Owner.Equals(current.CurrentPlayerId) && effect.SpecKind == "extra-flick")
                {
                    builder.RemoveAt(i);
                    break;
                }
            }

            return current with { Effects = builder.ToImmutable() };
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
            if ((state.Phase is Phase.Aim or Phase.CardSelect or Phase.TurnStart)
                && TryGetCurrentEraser(out var eraser)
                && eraser != null)
            {
                // Soft focus for the next seat without resetting overview framing.
                EnsureOrbitCamera()?.SetChaseTarget(eraser.transform);
            }
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

            var pose = FindClearSpawn(player.Id.Value);
            eraser.transform.SetPositionAndRotation(pose.position, pose.rotation);
            eraser.Body.linearVelocity = Vector3.zero;
            eraser.Body.angularVelocity = Vector3.zero;
            eraser.GetComponent<FalloutDetector>()?.ResetReport();
            eraser.gameObject.SetActive(true);
        }

        (Vector3 position, Quaternion rotation) FindClearSpawn(int seat)
        {
            var preferred = spawnPoints[seat % spawnPoints.Length];
            var bestPos = preferred.position;
            var bestRot = preferred.rotation;
            var bestClearance = -1f;

            for (var i = 0; i < spawnPoints.Length; i++)
            {
                var candidate = spawnPoints[(seat + i) % spawnPoints.Length];
                var clearance = MinDistanceToActiveErasers(candidate.position, seat);
                if (clearance > bestClearance)
                {
                    bestClearance = clearance;
                    bestPos = candidate.position;
                    bestRot = candidate.rotation;
                }

                if (clearance >= 0.12f)
                {
                    break;
                }
            }

            if (bestClearance < 0.08f)
            {
                bestPos += preferred.right * 0.08f;
            }

            return (bestPos, bestRot);
        }

        float MinDistanceToActiveErasers(Vector3 position, int ignoreSeat)
        {
            var min = float.PositiveInfinity;
            for (var i = 0; i < erasers.Count; i++)
            {
                var other = erasers[i];
                if (other == null || !other.gameObject.activeSelf || other.PlayerIndex == ignoreSeat)
                {
                    continue;
                }

                var d = Vector3.Distance(position, other.transform.position);
                if (d < min)
                {
                    min = d;
                }
            }

            return float.IsPositiveInfinity(min) ? float.PositiveInfinity : min;
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
            settleWatcher?.Cancel();
            pendingExtraFlick = false;
            foreach (var eraser in erasers)
            {
                if (eraser != null)
                {
                    Destroy(eraser.gameObject);
                }
            }

            erasers.Clear();
            byPlayer.Clear();
            eraserBaseScale.Clear();
            eraserCueScale.Clear();
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

