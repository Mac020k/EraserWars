using EraserWars.Core;
using NUnit.Framework;

namespace EraserWars.Tests.EditMode
{
    public sealed class StickFlickTests
    {
        static readonly PlayerId Seat = new PlayerId(0);

        [Test]
        public void RestingStickDoesNotFlick()
        {
            Assert.IsFalse(Aim(0f, 0f, out _));
            Assert.IsFalse(Aim(0.1f, 0f, out _));
        }

        [Test]
        public void FullUpStickFliesAlongCameraForward()
        {
            var aimed = Aim(0f, 1f, out var command);

            Assert.IsTrue(aimed);
            Assert.AreEqual(0f, command.DirectionX, 1e-5f);
            Assert.AreEqual(1f, command.DirectionY, 1e-5f);
            Assert.AreEqual(1f, command.DragLength01, 1e-5f);
            Assert.AreEqual(0f, command.ContactOffsetX, 1e-5f);
            Assert.AreEqual(0f, command.ContactOffsetY, 1e-5f);
        }

        [Test]
        public void FullRightStickFliesAlongCameraRight()
        {
            var aimed = Aim(1f, 0f, out var command);

            Assert.IsTrue(aimed);
            Assert.AreEqual(1f, command.DirectionX, 1e-5f);
            Assert.AreEqual(0f, command.DirectionY, 1e-5f);
            Assert.AreEqual(1f, command.DragLength01, 1e-5f);
        }

        [Test]
        public void StickPastFullStillAimsAtFullPower()
        {
            var aimed = StickFlick.TryCommand(Seat, 1.4f, 0f, 1f, 0f, 0f, 1f, 0.2f, out var command);

            Assert.IsTrue(aimed);
            Assert.AreEqual(1f, command.DirectionX, 1e-5f);
            Assert.AreEqual(1f, command.DragLength01, 1e-5f);
            Assert.AreEqual(0.2f, command.ReleaseTime, 1e-5f);
        }

        [Test]
        public void UpStickFollowsAYawedCamera()
        {
            var aimed = StickFlick.TryCommand(Seat, 0f, 1f, 0f, -1f, 1f, 0f, 0.4f, out var command);

            Assert.IsTrue(aimed, "Stick up follows camera forward when the camera looks along +X.");
            Assert.AreEqual(1f, command.DirectionX, 1e-5f);
            Assert.AreEqual(0f, command.DirectionY, 1e-5f);
            Assert.AreEqual(0.4f, command.ReleaseTime, 1e-5f);
            Assert.AreEqual(0f, command.ContactOffsetX);
            Assert.AreEqual(0f, command.ContactOffsetY);
        }

        [Test]
        public void HalfDeflectionIsHalfPowerOnCenterContact()
        {
            var aimed = StickFlick.TryCommand(Seat, 0.6f, 0f, 1f, 0f, 0f, 1f, 0.25f, out var command);

            Assert.IsTrue(aimed);
            Assert.AreEqual(0.5f, command.DragLength01, 1e-5f, "Power is the stick length after the 0.2 deadzone.");
            Assert.AreEqual(1f, command.DirectionX, 1e-5f);
            Assert.AreEqual(0f, command.DirectionY, 1e-5f);
            Assert.AreEqual(0f, command.ContactOffsetX);
            Assert.AreEqual(0f, command.ContactOffsetY);
            Assert.AreEqual(0.25f, command.ReleaseTime, 1e-5f);
        }

        [Test]
        public void DeflectionJustOutsideTheDeadzoneDoesNotFlick()
        {
            Assert.IsFalse(Aim(0.22f, 0f, out _));
        }

        [Test]
        public void ParallelCameraAxesDoNotFlick()
        {
            var aimed = StickFlick.TryCommand(Seat, 1f, -1f, 1f, 0f, 1f, 0f, 0.2f, out _);
            Assert.IsFalse(aimed, "Opposite stick axes on the same camera axis cancel.");
        }

        [Test]
        public void FlatCameraFallsBackToWorldAxes()
        {
            var aimed = StickFlick.TryCommand(Seat, 0f, 1f, 0f, 0f, 0f, 0f, 0.1f, out var command);

            Assert.IsTrue(aimed);
            Assert.AreEqual(0f, command.DirectionX, 1e-5f);
            Assert.AreEqual(1f, command.DirectionY, 1e-5f);
        }

        static bool Aim(float stickX, float stickY, out FlickCommand command)
        {
            return StickFlick.TryCommand(Seat, stickX, stickY, 1f, 0f, 0f, 1f, 0.3f, out command);
        }
    }
}
