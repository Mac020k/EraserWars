using FishNet.Object;
using EraserWars.Core;
using EraserWars.Gameplay;
using UnityEngine;

namespace EraserWars.Net
{
    /// <summary>
    /// Host-authoritative canned flick for the Fish-Net spike.
    /// Validates through <see cref="HostCommandGate"/>, applies PhysX on the server only,
    /// then broadcasts the settle pose for client comparison.
    /// </summary>
    public sealed class FishNetSpikeSession : NetworkBehaviour
    {
        [SerializeField] EraserBody eraser;
        [SerializeField] FlickApplier flickApplier;
        [SerializeField] SettleWatcher settleWatcher;
        [SerializeField] Transform spawnPoint;
        [SerializeField] Vector3 cannedDirection = new(0f, 0f, 1f);
        [SerializeField] float cannedDrag = 0.7f;
        [SerializeField] float cannedReleaseTime = 0.25f;

        MatchState state;
        bool firing;
        string lastStatus = "idle";
        Vector3 lastHostPose;
        Quaternion lastHostRot = Quaternion.identity;
        Vector3 lastReportedPose;
        Quaternion lastReportedRot = Quaternion.identity;
        bool hasReportedPose;

        public string LastStatus => lastStatus;
        public bool HasReportedPose => hasReportedPose;
        public Vector3 LastReportedPose => lastReportedPose;
        public Quaternion LastReportedRot => lastReportedRot;
        public Vector3 LocalEraserPose => eraser != null ? eraser.transform.position : Vector3.zero;
        public float PoseDeltaMeters =>
            hasReportedPose && eraser != null
                ? Vector3.Distance(eraser.transform.position, lastReportedPose)
                : -1f;

        public override void OnStartServer()
        {
            base.OnStartServer();
            ResetMatchState();
            ConfigureAuthorityBodies();
            if (settleWatcher != null)
            {
                settleWatcher.Settled += OnSettledServer;
            }
        }

        public override void OnStopServer()
        {
            if (settleWatcher != null)
            {
                settleWatcher.Settled -= OnSettledServer;
            }

            base.OnStopServer();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            ConfigureAuthorityBodies();
        }

        public void UiRequestCannedFlick()
        {
            if (!IsServerStarted)
            {
                ServerRequestCannedFlick();
                return;
            }

            FireCannedFlick();
        }

        [ServerRpc(RequireOwnership = false)]
        void ServerRequestCannedFlick()
        {
            FireCannedFlick();
        }

        void FireCannedFlick()
        {
            if (!IsServerStarted || firing || eraser == null || flickApplier == null)
            {
                lastStatus = "cannot-fire";
                return;
            }

            ResetMatchState();
            ResetEraserPose();

            var dir = cannedDirection;
            if (dir.sqrMagnitude < 1e-6f)
            {
                dir = Vector3.forward;
            }

            dir.Normalize();
            var command = new FlickCommand(
                state.CurrentPlayerId,
                dir.x,
                dir.z,
                Mathf.Clamp01(cannedDrag),
                0f,
                0f,
                cannedReleaseTime);

            if (!HostCommandGate.ValidateFlick(state, command, out var reason))
            {
                lastStatus = $"gate-reject:{reason}";
                return;
            }

            flickApplier.Apply(eraser, command);
            state = MatchReduce.Reduce(state, new Flicked(command));
            firing = true;
            lastStatus = "simulating";
            settleWatcher.Begin(new[] { eraser });
        }

        void OnSettledServer()
        {
            if (!IsServerStarted || !firing || eraser == null)
            {
                return;
            }

            firing = false;
            lastHostPose = eraser.transform.position;
            lastHostRot = eraser.transform.rotation;
            lastStatus = $"settled host=({lastHostPose.x:F3},{lastHostPose.y:F3},{lastHostPose.z:F3})";
            ObserversReportSettle(lastHostPose, lastHostRot, lastStatus);
        }

        [ObserversRpc(BufferLast = true)]
        void ObserversReportSettle(Vector3 pose, Quaternion rot, string status)
        {
            lastReportedPose = pose;
            lastReportedRot = rot;
            hasReportedPose = true;
            lastStatus = status;
        }

        void ResetMatchState()
        {
            state = MatchFactory.CreateHotseat(2, RuleSet.CasualStock);
            state = MatchReduce.Reduce(state, new TurnStarted(state.CurrentPlayerId));
        }

        void ResetEraserPose()
        {
            var spawn = spawnPoint != null ? spawnPoint : transform;
            var body = eraser.Body;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = spawn.position;
            body.rotation = spawn.rotation;
            eraser.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
        }

        void ConfigureAuthorityBodies()
        {
            if (eraser == null || eraser.Body == null)
            {
                return;
            }

            // Host simulates. Clients follow NetworkTransform as kinematic.
            eraser.Body.isKinematic = !IsServerStarted;
        }
    }
}
