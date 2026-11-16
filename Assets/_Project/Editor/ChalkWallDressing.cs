using System.Collections.Generic;
using UnityEngine;

namespace EraserWars.Editor
{
    public enum ChalkWallPieceKind
    {
        Poster = 0,
        ChalkMark = 1,
        Ledge = 2,
    }

    public readonly struct ChalkWallPiece
    {
        public ChalkWallPiece(
            string name,
            Vector3 center,
            Vector3 size,
            Color color,
            float yawDegrees,
            float tiltDegrees,
            ChalkWallPieceKind kind)
        {
            Name = name;
            Center = center;
            Size = size;
            Color = color;
            YawDegrees = yawDegrees;
            TiltDegrees = tiltDegrees;
            Kind = kind;
        }

        public string Name { get; }
        public Vector3 Center { get; }
        public Vector3 Size { get; }
        public Color Color { get; }
        public float YawDegrees { get; }
        public float TiltDegrees { get; }
        public ChalkWallPieceKind Kind { get; }
    }

    /// <summary>
    /// Posters, chalk strokes, and tray clutter for the front wall.
    /// Positions are meters. frontZ is the chalkboard plaster face. floorTop is the floor surface.
    /// </summary>
    public static class ChalkWallDressing
    {
        public static ChalkWallPiece[] Build(float frontZ, float floorTop)
        {
            var list = new List<ChalkWallPiece>(32);
            AddPosters(list, frontZ);
            AddChalkMarks(list, frontZ);
            AddLedge(list, frontZ, floorTop);
            return list.ToArray();
        }

        public static string LayoutFailure(IReadOnlyList<ChalkWallPiece> pieces, float frontZ, float floorTop, float roomHalfX)
        {
            if (pieces == null || pieces.Count == 0)
            {
                return "Chalk wall dressing is empty.";
            }

            var seen = new HashSet<string>();
            for (var i = 0; i < pieces.Count; i++)
            {
                var piece = pieces[i];
                if (string.IsNullOrEmpty(piece.Name) || !seen.Add(piece.Name))
                {
                    return "Duplicate or empty chalk wall piece name.";
                }

                var reason = PieceFailure(piece, frontZ, floorTop, roomHalfX);
                if (reason != null)
                {
                    return reason;
                }

                for (var j = 0; j < i; j++)
                {
                    if (Overlaps(piece, pieces[j], 0.008f))
                    {
                        return piece.Name + " overlaps " + pieces[j].Name + ".";
                    }
                }
            }

            return null;
        }

        static void AddPosters(List<ChalkWallPiece> list, float frontZ)
        {
            var onBoard = frontZ - 0.065f;
            var onPlaster = frontZ - 0.02f;
            AddPoster(list, "Paper_L_White", -2.30f, 2.00f, onBoard, 0.34f, 0.38f, new Color(0.95f, 0.93f, 0.86f), -1.2f);
            AddPoster(list, "Paper_L_Green", -2.78f, 1.95f, onBoard, 0.36f, 0.30f, new Color(0.74f, 0.82f, 0.70f), 1.4f);
            AddPoster(list, "Paper_L_Yellow", -2.40f, 1.35f, onBoard, 0.40f, 0.36f, new Color(0.93f, 0.84f, 0.50f), 0.8f);
            AddPoster(list, "Paper_L_Blue", -2.82f, 1.32f, onBoard, 0.32f, 0.40f, new Color(0.70f, 0.80f, 0.84f), -1.6f);
            AddPoster(list, "Paper_R_Cream", 2.37f, 1.93f, onBoard, 0.32f, 0.40f, new Color(0.96f, 0.94f, 0.86f), 1.2f);
            AddPoster(list, "Paper_R_Manila", 2.73f, 1.43f, onBoard, 0.30f, 0.42f, new Color(0.93f, 0.86f, 0.55f), -0.8f);
            AddPoster(list, "Paper_R_Blue", 2.75f, 1.97f, onBoard, 0.32f, 0.28f, new Color(0.68f, 0.78f, 0.82f), 1.5f);
            AddPoster(list, "Paper_R_Pink", 2.35f, 1.37f, onBoard, 0.34f, 0.36f, new Color(0.90f, 0.74f, 0.70f), -1.1f);
            AddPoster(list, "Paper_Above_L", -0.95f, 2.47f, onPlaster, 1.05f, 0.28f, new Color(0.95f, 0.92f, 0.82f), -1.0f);
            AddPoster(list, "Paper_Above_R", 0.98f, 2.46f, onPlaster, 0.82f, 0.26f, new Color(0.90f, 0.78f, 0.48f), 0.8f);
        }

        static void AddChalkMarks(List<ChalkWallPiece> list, float frontZ)
        {
            var z = frontZ - 0.072f;
            var chalkWhite = new Color(0.94f, 0.93f, 0.88f);
            var chalkYellow = new Color(0.96f, 0.88f, 0.35f);
            var chalkPink = new Color(0.95f, 0.62f, 0.72f);
            AddMark(list, "ChalkMark_High", -0.85f, 1.82f, z, 0.48f, chalkWhite, 3f);
            AddMark(list, "ChalkMark_Mid", -0.10f, 1.58f, z, 0.64f, chalkYellow, -2f);
            AddMark(list, "ChalkMark_Right", 0.72f, 1.42f, z, 0.38f, chalkPink, 4f);
            AddMark(list, "ChalkMark_Low", 0.05f, 1.22f, z, 0.42f, chalkWhite, -3f);
            AddMark(list, "ChalkMark_Left", -0.55f, 1.10f, z, 0.30f, chalkYellow, 2f);
        }

        static void AddLedge(List<ChalkWallPiece> list, float frontZ, float floorTop)
        {
            AddStick(list, "Chalk_White", frontZ, floorTop, -0.80f, -0.135f, new Vector3(0.09f, 0.025f, 0.025f), Color.white, 8f, 4f);
            AddStick(list, "Chalk_Yellow", frontZ, floorTop, -0.58f, -0.125f, new Vector3(0.08f, 0.025f, 0.025f), new Color(0.98f, 0.88f, 0.30f), -10f, 2f);
            AddStick(list, "Chalk_Pink", frontZ, floorTop, -0.38f, -0.140f, new Vector3(0.08f, 0.025f, 0.025f), new Color(0.98f, 0.60f, 0.70f), 5f, -3f);
            AddStick(list, "Chalk_Blue", frontZ, floorTop, 0.12f, -0.130f, new Vector3(0.085f, 0.025f, 0.025f), new Color(0.55f, 0.78f, 0.92f), 16f, 6f);
            AddStick(list, "Chalk_Green", frontZ, floorTop, 0.58f, -0.145f, new Vector3(0.07f, 0.025f, 0.025f), new Color(0.45f, 0.75f, 0.48f), -8f, 3f);
            AddStick(list, "Chalk_Stub_L", frontZ, floorTop, -1.48f, -0.130f, new Vector3(0.04f, 0.02f, 0.02f), new Color(0.92f, 0.92f, 0.88f), 28f, 8f);
            AddStick(list, "Chalk_Stub_R", frontZ, floorTop, 1.48f, -0.125f, new Vector3(0.035f, 0.018f, 0.02f), new Color(0.95f, 0.85f, 0.40f), -22f, 5f);
            AddStick(list, "Chalk_White_L", frontZ, floorTop, -1.02f, -0.150f, new Vector3(0.09f, 0.025f, 0.025f), new Color(0.94f, 0.94f, 0.90f), -6f, 2f);
            AddStick(list, "ChalkBox", frontZ, floorTop, 0.34f, -0.130f, new Vector3(0.12f, 0.045f, 0.07f), new Color(0.72f, 0.58f, 0.40f), 5f, 0f);
            AddStick(list, "ChalkRag", frontZ, floorTop, -1.28f, -0.120f, new Vector3(0.16f, 0.012f, 0.07f), new Color(0.75f, 0.76f, 0.74f), 10f, 0f);
            AddStick(list, "ChalkEraser_A", frontZ, floorTop, 0.92f, -0.125f, new Vector3(0.14f, 0.035f, 0.055f), new Color(0.30f, 0.30f, 0.34f), -3f, 0f);
            AddStick(list, "ChalkEraser_B", frontZ, floorTop, 1.22f, -0.140f, new Vector3(0.13f, 0.035f, 0.055f), new Color(0.45f, 0.40f, 0.34f), 6f, 2f);
            AddStick(list, "Chalk_Pink_L", frontZ, floorTop, -0.16f, -0.125f, new Vector3(0.075f, 0.025f, 0.025f), new Color(0.95f, 0.70f, 0.78f), 20f, -4f);
        }

        static void AddPoster(
            List<ChalkWallPiece> list,
            string name,
            float x,
            float y,
            float z,
            float width,
            float height,
            Color color,
            float rollDegrees)
        {
            list.Add(new ChalkWallPiece(
                name,
                new Vector3(x, y, z),
                new Vector3(width, height, 0.01f),
                color,
                180f,
                rollDegrees,
                ChalkWallPieceKind.Poster));
        }

        static void AddMark(List<ChalkWallPiece> list, string name, float x, float y, float z, float width, Color color, float rollDegrees)
        {
            list.Add(new ChalkWallPiece(
                name,
                new Vector3(x, y, z),
                new Vector3(width, 0.028f, 0.01f),
                color,
                180f,
                rollDegrees,
                ChalkWallPieceKind.ChalkMark));
        }

        static void AddStick(
            List<ChalkWallPiece> list,
            string name,
            float frontZ,
            float floorTop,
            float x,
            float zOffset,
            Vector3 size,
            Color color,
            float yawDegrees,
            float pitchDegrees)
        {
            var y = floorTop + 0.885f + size.y * 0.5f;
            list.Add(new ChalkWallPiece(
                name,
                new Vector3(x, y, frontZ + zOffset),
                size,
                color,
                yawDegrees,
                pitchDegrees,
                ChalkWallPieceKind.Ledge));
        }

        static string PieceFailure(ChalkWallPiece piece, float frontZ, float floorTop, float roomHalfX)
        {
            Aabb(piece, out var min, out var max);
            var playMin = new Vector3(-0.62f, 0.69f, -0.42f);
            var playMax = new Vector3(0.62f, 0.81f, 0.42f);
            if (Overlaps(min, max, playMin, playMax, 0f))
            {
                return piece.Name + " overlaps a play desk.";
            }

            if (min.x < -roomHalfX + 0.25f || max.x > roomHalfX - 0.25f)
            {
                return piece.Name + " crosses a window or door plane.";
            }

            if (min.z < frontZ - 0.45f || max.z > frontZ + 0.02f)
            {
                return piece.Name + " leaves the chalkboard wall.";
            }

            var boardBottom = floorTop + 0.9f;
            var boardTop = boardBottom + 1.3f;
            if (piece.Kind == ChalkWallPieceKind.ChalkMark)
            {
                if (min.x < -1.7f || max.x > 1.7f || min.y < boardBottom + 0.1f || max.y > boardTop - 0.1f)
                {
                    return piece.Name + " leaves the chalkboard.";
                }
            }

            if (piece.Kind == ChalkWallPieceKind.Poster && piece.Center.y < boardTop)
            {
                var cx = piece.Center.x >= 0f ? 2.55f : -2.55f;
                if (min.x < cx - 0.51f || max.x > cx + 0.51f || min.y < 0.99f || max.y > 2.31f)
                {
                    return piece.Name + " leaves the side board.";
                }
            }

            if (piece.Kind == ChalkWallPieceKind.Poster && piece.Center.y >= boardTop)
            {
                if (min.y < boardTop + 0.04f || max.y > 2.7f || min.x < -1.7f || max.x > 1.7f)
                {
                    return piece.Name + " leaves the strip above the chalkboard.";
                }
            }

            if (piece.Kind == ChalkWallPieceKind.Ledge)
            {
                if (min.x < -1.82f || max.x > 1.82f || min.z < frontZ - 0.19f || max.z > frontZ - 0.03f || min.y < floorTop + 0.8f || max.y > floorTop + 1.15f)
                {
                    return piece.Name + " leaves the chalk tray.";
                }
            }

            // Clock rim is (2.2, 2.65). Speaker is (0, 2.78). Both sit on this wall.
            var clockMin = new Vector3(2.02f, 2.47f, frontZ - 0.08f);
            var clockMax = new Vector3(2.38f, 2.83f, frontZ + 0.01f);
            if (Overlaps(min, max, clockMin, clockMax, 0f))
            {
                return piece.Name + " covers the clock.";
            }

            var speakerMin = new Vector3(-0.26f, 2.63f, frontZ - 0.18f);
            var speakerMax = new Vector3(0.26f, 2.93f, frontZ - 0.02f);
            if (Overlaps(min, max, speakerMin, speakerMax, 0f))
            {
                return piece.Name + " covers the speaker.";
            }

            return null;
        }

        static void Aabb(ChalkWallPiece piece, out Vector3 min, out Vector3 max)
        {
            var half = HalfExtents(piece);
            min = piece.Center - half;
            max = piece.Center + half;
        }

        static Vector3 HalfExtents(ChalkWallPiece piece)
        {
            var hx = piece.Size.x * 0.5f;
            var hy = piece.Size.y * 0.5f;
            var hz = piece.Size.z * 0.5f;
            if (piece.Kind == ChalkWallPieceKind.Ledge)
            {
                var pitch = piece.TiltDegrees * Mathf.Deg2Rad;
                var cp = Mathf.Abs(Mathf.Cos(pitch));
                var sp = Mathf.Abs(Mathf.Sin(pitch));
                var y1 = cp * hy + sp * hz;
                var z1 = sp * hy + cp * hz;
                var yaw = piece.YawDegrees * Mathf.Deg2Rad;
                var cy = Mathf.Abs(Mathf.Cos(yaw));
                var sy = Mathf.Abs(Mathf.Sin(yaw));
                return new Vector3(cy * hx + sy * z1, y1, sy * hx + cy * z1);
            }

            var roll = piece.TiltDegrees * Mathf.Deg2Rad;
            var cr = Mathf.Abs(Mathf.Cos(roll));
            var sr = Mathf.Abs(Mathf.Sin(roll));
            var x1 = cr * hx + sr * hy;
            var y1p = sr * hx + cr * hy;
            var yawP = piece.YawDegrees * Mathf.Deg2Rad;
            var cyP = Mathf.Abs(Mathf.Cos(yawP));
            var syP = Mathf.Abs(Mathf.Sin(yawP));
            return new Vector3(cyP * x1 + syP * hz, y1p, syP * x1 + cyP * hz);
        }

        static bool Overlaps(ChalkWallPiece a, ChalkWallPiece b, float slack)
        {
            Aabb(a, out var aMin, out var aMax);
            Aabb(b, out var bMin, out var bMax);
            return Overlaps(aMin, aMax, bMin, bMax, slack);
        }

        static bool Overlaps(Vector3 aMin, Vector3 aMax, Vector3 bMin, Vector3 bMax, float slack)
        {
            return aMax.x > bMin.x + slack
                && aMin.x < bMax.x - slack
                && aMax.y > bMin.y + slack
                && aMin.y < bMax.y - slack
                && aMax.z > bMin.z + slack
                && aMin.z < bMax.z - slack;
        }
    }
}
