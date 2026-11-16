using EraserWars.Editor;
using NUnit.Framework;
using UnityEngine;

namespace EraserWars.Tests.EditMode
{
    public sealed class ChalkWallDressingTests
    {
        [Test]
        public void ChalkWallKeepsPostersAndTrayClearOfDesksAndOpenings()
        {
            var failure = ClassroomSceneBuilder.ChalkWallLayoutFailure();
            Assert.IsNull(failure, failure);

            var pieces = ClassroomSceneBuilder.ChalkWallPieces();
            var posters = 0;
            var marks = 0;
            var ledge = 0;
            var ledgeMinX = 99f;
            var ledgeMaxX = -99f;
            var leftPoster = false;
            var rightPoster = false;
            for (var i = 0; i < pieces.Length; i++)
            {
                var piece = pieces[i];
                if (piece.Kind == ChalkWallPieceKind.Poster && piece.Size.y >= 0.2f)
                {
                    posters++;
                    if (piece.Center.x <= -2f)
                    {
                        leftPoster = true;
                    }

                    if (piece.Center.x >= 2f)
                    {
                        rightPoster = true;
                    }
                }

                if (piece.Kind == ChalkWallPieceKind.ChalkMark)
                {
                    marks++;
                }

                if (piece.Kind == ChalkWallPieceKind.Ledge)
                {
                    ledge++;
                    if (piece.Center.x < ledgeMinX)
                    {
                        ledgeMinX = piece.Center.x;
                    }

                    if (piece.Center.x > ledgeMaxX)
                    {
                        ledgeMaxX = piece.Center.x;
                    }
                }
            }

            Assert.GreaterOrEqual(posters, 10, "Side boards and the strip above the chalkboard need a full set of posters.");
            Assert.GreaterOrEqual(marks, 5, "The green board needs leftover chalk strokes.");
            Assert.GreaterOrEqual(ledge, 12, "The chalk tray needs clutter along its length.");
            Assert.Less(ledgeMinX, -1.3f, "Clutter reaches the left end of the tray.");
            Assert.Greater(ledgeMaxX, 1.3f, "Clutter reaches the right end of the tray.");
            Assert.IsTrue(leftPoster, "The left side board has a poster.");
            Assert.IsTrue(rightPoster, "The right side board has a poster.");
        }

        [Test]
        public void PlacedChalkWallFacesTheRoomWithoutColliders()
        {
            var root = new GameObject("ChalkWallDressingProbe");
            try
            {
                ClassroomSceneBuilder.PlaceChalkWallDressing(root.transform);
                var pieces = ClassroomSceneBuilder.ChalkWallPieces();
                Assert.AreEqual(pieces.Length, root.transform.childCount, "Every dressing piece is placed.");

                var above = root.transform.Find("Paper_Above_L");
                Assert.IsNotNull(above, "The poster above the board is placed.");
                Assert.Less(above.forward.z, -0.9f, "Posters face into the room.");
                Assert.Greater(above.position.z, 4.2f, "The poster stays on the chalkboard wall.");

                for (var i = 0; i < root.transform.childCount; i++)
                {
                    var child = root.transform.GetChild(i);
                    Assert.IsNull(child.GetComponent<Collider>(), child.name + " must not block play.");
                }
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void DressingOnAPlayDeskOrPastAWindowIsRejected()
        {
            var onDesk = new ChalkWallPiece(
                "OnDesk",
                new Vector3(0f, 0.75f, 0f),
                new Vector3(0.3f, 0.1f, 0.3f),
                Color.white,
                0f,
                0f,
                ChalkWallPieceKind.Ledge);
            var pastWindow = new ChalkWallPiece(
                "PastWindow",
                new Vector3(-3.4f, 1.6f, 4.45f),
                new Vector3(0.3f, 0.3f, 0.02f),
                Color.white,
                180f,
                0f,
                ChalkWallPieceKind.Poster);
            var deskReason = ChalkWallDressing.LayoutFailure(new[] { onDesk }, 4.5f, 0.04f, 3.5f);
            var windowReason = ChalkWallDressing.LayoutFailure(new[] { pastWindow }, 4.5f, 0.04f, 3.5f);
            Assert.That(deskReason, Does.Contain("play desk"));
            Assert.That(windowReason, Does.Contain("window or door"));
        }
    }
}
