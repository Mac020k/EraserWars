using System;

namespace EraserWars.Core
{
    public static class StickFlick
    {
        public const float Deadzone = 0.2f;
        public const float MinPower = 0.05f;

        public static bool TryCommand(
            PlayerId player,
            float stickX,
            float stickY,
            float cameraRightX,
            float cameraRightZ,
            float cameraForwardX,
            float cameraForwardZ,
            float heldSeconds,
            out FlickCommand command)
        {
            command = default;
            var magnitude = MathF.Sqrt(stickX * stickX + stickY * stickY);
            if (magnitude < Deadzone)
            {
                return false;
            }

            var power = (magnitude - Deadzone) / (1f - Deadzone);
            if (power > 1f)
            {
                power = 1f;
            }

            if (power < MinPower)
            {
                return false;
            }

            if (!TryBasis(cameraRightX, cameraRightZ, out var rightX, out var rightZ)
                || !TryBasis(cameraForwardX, cameraForwardZ, out var forwardX, out var forwardZ))
            {
                rightX = 1f;
                rightZ = 0f;
                forwardX = 0f;
                forwardZ = 1f;
            }

            // Stick up flies along camera forward. A mouse drag is a pull, so it points the other way.
            var directionX = rightX * stickX + forwardX * stickY;
            var directionZ = rightZ * stickX + forwardZ * stickY;
            if (!TryBasis(directionX, directionZ, out directionX, out directionZ))
            {
                return false;
            }

            command = new FlickCommand(player, directionX, directionZ, power, 0f, 0f, heldSeconds);
            return true;
        }

        static bool TryBasis(float x, float z, out float unitX, out float unitZ)
        {
            var magnitude = MathF.Sqrt(x * x + z * z);
            if (magnitude < 1e-6f)
            {
                unitX = 0f;
                unitZ = 0f;
                return false;
            }

            unitX = x / magnitude;
            unitZ = z / magnitude;
            return true;
        }
    }
}
