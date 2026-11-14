using Keshipin.Core;

namespace Keshipin.Net
{
    public static class HostCommandGate
    {
        public static bool ValidateFlick(MatchState state, FlickCommand command, out string reason)
        {
            if (state.Phase is not (Phase.Aim or Phase.CardSelect))
            {
                reason = "not-aiming";
                return false;
            }

            if (!state.CurrentPlayerId.Equals(command.Player))
            {
                reason = "not-your-turn";
                return false;
            }

            if (command.DragLength01 < 0f || command.DragLength01 > 1f)
            {
                reason = "drag-out-of-range";
                return false;
            }

            reason = null;
            return true;
        }

        public static bool ValidatePass(MatchState state, PlayerId player, out string reason)
        {
            if (!state.CurrentPlayerId.Equals(player))
            {
                reason = "not-your-turn";
                return false;
            }

            if (state.Phase is Phase.Simulating or Phase.MatchEnd)
            {
                reason = "cannot-pass-now";
                return false;
            }

            reason = null;
            return true;
        }
    }
}
