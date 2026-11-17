"""Write card glyph SVGs and CardIconSvg.cs. Run: python tools/generate_card_icons.py"""

import math
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ICON_DIR = ROOT / "Assets" / "_Project" / "UI" / "CardIcons"
CS_PATH = ROOT / "Assets" / "_Project" / "UI" / "CardIconSvg.cs"
SHEET_PATH = ROOT / "docs" / "prototypes" / "card-tcg" / "contact-sheet.png"

INK = "#24180F"
PAPER = "#FFFCF6"
VIEW = 64


def star(cx, cy, outer, inner, points):
    pts = []
    for i in range(points * 2):
        radius = outer if i % 2 == 0 else inner
        angle = -math.pi / 2 + i * math.pi / points
        pts.append((cx + radius * math.cos(angle), cy + radius * math.sin(angle)))
    return pts


def poly(points, fill):
    return ("polygon", {"points": points, "fill": fill})


def rect(x, y, w, h, fill, rx=0):
    return ("rect", {"x": x, "y": y, "width": w, "height": h, "rx": rx, "fill": fill})


def circle(cx, cy, r, fill):
    return ("circle", {"cx": cx, "cy": cy, "r": r, "fill": fill})


ICONS = {
    "weight": [
        rect(8, 4, 48, 18, INK, 6),
        rect(26, 16, 12, 32, INK),
        rect(8, 42, 48, 18, INK, 6),
    ],
    "grip": [
        rect(4, 10, 18, 44, INK, 4),
        rect(42, 10, 18, 44, INK, 4),
        rect(20, 27, 24, 10, INK, 2),
    ],
    "bounce": [
        rect(6, 52, 52, 8, INK, 3),
        circle(32, 36, 12, INK),
        poly([(32, 4), (44, 18), (36, 18), (36, 26), (28, 26), (28, 18), (20, 18)], INK),
    ],
    "power": [poly(star(32, 32, 26, 10, 4), INK)],
    "speed": [
        rect(4, 12, 40, 8, INK, 4),
        rect(16, 28, 44, 8, INK, 4),
        rect(8, 44, 32, 8, INK, 4),
    ],
    "flick": [
        circle(48, 32, 12, INK),
        poly([(4, 20), (34, 32), (4, 44)], INK),
    ],
    "clock": [
        circle(32, 32, 24, INK),
        circle(32, 32, 17, PAPER),
        rect(29, 16, 6, 18, INK),
        rect(29, 29, 18, 6, INK),
        circle(32, 32, 3, INK),
    ],
    "sight": [
        poly([(2, 32), (14, 18), (32, 14), (50, 18), (62, 32), (50, 46), (32, 50), (14, 46)], INK),
        circle(32, 32, 10, PAPER),
        circle(32, 32, 5, INK),
    ],
    "edge": [
        rect(50, 4, 10, 56, INK, 2),
        rect(6, 22, 16, 20, INK, 3),
        poly([(22, 24), (46, 32), (22, 40)], INK),
    ],
    "center": [
        circle(32, 32, 18, INK),
        circle(32, 32, 11, PAPER),
        circle(32, 32, 4, INK),
        rect(29, 4, 6, 12, INK),
        rect(29, 48, 6, 12, INK),
        rect(4, 29, 12, 6, INK),
        rect(48, 29, 12, 6, INK),
    ],
    "extra": [
        rect(8, 18, 30, 36, INK, 4),
        rect(26, 8, 30, 36, INK, 4),
        rect(32, 14, 18, 24, PAPER),
    ],
    "swap": [
        rect(6, 12, 30, 12, INK, 3),
        poly([(36, 10), (58, 18), (36, 26)], INK),
        rect(28, 40, 30, 12, INK, 3),
        poly([(28, 38), (6, 46), (28, 54)], INK),
    ],
    "pin": [
        poly([(32, 60), (16, 30), (48, 30)], INK),
        circle(32, 24, 16, INK),
        circle(32, 24, 6, PAPER),
    ],
    "desk": [
        rect(22, 6, 20, 14, INK, 3),
        rect(6, 22, 52, 10, INK, 2),
        rect(12, 32, 8, 24, INK),
        rect(44, 32, 8, 24, INK),
    ],
    "shield": [
        poly([(32, 4), (56, 14), (54, 34), (32, 60), (10, 34), (8, 14)], INK),
        rect(28, 18, 8, 26, PAPER),
    ],
    "weapon": [
        poly([(32, 2), (46, 20), (18, 20)], INK),
        rect(20, 18, 24, 28, INK, 2),
        rect(26, 22, 12, 20, PAPER),
        rect(20, 44, 24, 6, INK),
        rect(22, 50, 20, 10, INK, 3),
    ],
    "heart": [
        circle(22, 24, 14, INK),
        circle(42, 24, 14, INK),
        poly([(8, 30), (56, 30), (32, 58)], INK),
        rect(29, 18, 6, 18, PAPER),
        rect(23, 24, 18, 6, PAPER),
    ],
    "star": [poly(star(32, 34, 26, 11, 5), INK)],
}

SYMBOLS = [
    ("Weight", "weight"),
    ("Grip", "grip"),
    ("Bounce", "bounce"),
    ("Power", "power"),
    ("Speed", "speed"),
    ("Flick", "flick"),
    ("Clock", "clock"),
    ("Sight", "sight"),
    ("Edge", "edge"),
    ("Center", "center"),
    ("Extra", "extra"),
    ("Swap", "swap"),
    ("Pin", "pin"),
    ("Desk", "desk"),
    ("Shield", "shield"),
    ("Weapon", "weapon"),
    ("Heart", "heart"),
    ("Star", "star"),
]

REUSE = {
    "SwapPositions": "swap",
    "ExtraFlick": "extra",
    "RevealTrajectory:2": "sight",
    "Teleport:0.5:0.5": "pin",
    "RestoreStock:1": "heart",
    "ModifyRule:CenterBias:0.3:1": "center",
    "GrantShield:0.5:1": "shield",
}


def dumbbell(plate_w, plate_h):
    x = (VIEW - plate_w) / 2
    return [
        rect(x, 4, plate_w, plate_h, INK, 4),
        rect(28, plate_h, 8, VIEW - plate_h * 2, INK),
        rect(x, VIEW - 4 - plate_h, plate_w, plate_h, INK, 4),
    ]


def build_effect_shapes():
    clock_slash = list(ICONS["clock"]) + [poly([(8, 54), (16, 60), (56, 12), (48, 6)], INK)]
    shield_cross = list(ICONS["shield"]) + [rect(16, 26, 32, 8, PAPER)]
    return {
        "ModifyStat:Mass:1.5:1": dumbbell(36, 12),
        "ModifyStat:Mass:1.8:1": dumbbell(48, 16),
        "ModifyStat:Mass:2.2:1": dumbbell(58, 20),
        "ModifyStat:Friction:0.5:1": [
            poly([(6, 8), (34, 22), (6, 36)], INK),
            poly([(30, 28), (58, 42), (30, 56)], INK),
        ],
        "ModifyStat:Friction:2:2": [
            rect(24, 6, 16, 10, INK, 2),
            rect(16, 16, 32, 40, INK, 8),
            rect(24, 28, 16, 16, PAPER),
        ],
        "ModifyStat:Friction:2.5:1": [
            circle(30, 32, 20, INK),
            circle(30, 32, 8, PAPER),
            rect(36, 26, 24, 12, INK, 2),
        ],
        "ModifyStat:Friction:3:1": [
            rect(8, 8, 48, 48, INK, 8),
            circle(22, 24, 5, PAPER),
            circle(42, 24, 5, PAPER),
            circle(32, 42, 5, PAPER),
        ],
        "ModifyStat:Bounciness:1.4:1": [
            rect(10, 52, 44, 6, INK, 2),
            circle(32, 36, 10, INK),
            poly([(32, 8), (42, 20), (22, 20)], INK),
        ],
        "ModifyStat:Bounciness:1.8:1": [
            rect(4, 54, 56, 6, INK, 2),
            circle(32, 32, 18, INK),
            poly([(32, 2), (48, 16), (16, 16)], INK),
        ],
        "ModifyStat:Power:1.15:1": [poly(star(32, 34, 16, 6, 4), INK)],
        "ModifyStat:Power:1.25:1": [poly(star(32, 32, 26, 10, 4), INK)],
        "ModifyStat:Power:1.35:1": [poly(star(32, 32, 26, 11, 6), INK)],
        "ModifyStat:Power:1.8:1": [poly(star(32, 32, 28, 12, 8), INK)],
        "ModifyStat:Speed:1.3:1": [
            poly([(6, 6), (48, 18), (6, 30)], INK),
            poly([(6, 34), (48, 46), (6, 58)], INK),
        ],
        "ModifyStat:Speed:1.5:1": [
            poly([(8, 4), (54, 12), (8, 20)], INK),
            poly([(8, 24), (54, 32), (8, 40)], INK),
            poly([(8, 44), (54, 52), (8, 60)], INK),
        ],
        "ModifyFlick:1:0:1": [
            poly([(4, 26), (40, 32), (4, 38)], INK),
            circle(50, 32, 8, INK),
        ],
        "ModifyFlick:1.4:1:2": [
            poly([(4, 16), (38, 32), (4, 48)], INK),
            circle(50, 32, 12, INK),
            poly(star(16, 12, 8, 3, 4), INK),
        ],
        "ModifyFlick:1:0.5:1": [
            poly([(4, 22), (46, 32), (4, 42)], INK),
            poly([(24, 40), (36, 52), (18, 56)], INK),
        ],
        "ModifyFlick:1:0:0.5": [
            poly([(8, 26), (28, 32), (8, 38)], INK),
            circle(40, 32, 6, INK),
        ],
        "ModifyRule:OpponentTurnTime:0.3:1": [
            circle(26, 32, 18, INK),
            circle(38, 26, 14, PAPER),
            rect(48, 12, 10, 4, INK),
            rect(54, 12, 4, 10, INK),
        ],
        "ModifyRule:OpponentTurnTime:0.5:1": [
            rect(10, 8, 44, 8, INK, 3),
            poly([(12, 40), (32, 22), (52, 40), (32, 52)], INK),
            circle(32, 38, 5, PAPER),
        ],
        "ModifyRule:OpponentTurnTime:0.6:1": [
            circle(36, 36, 16, INK),
            circle(14, 28, 8, INK),
            rect(18, 48, 10, 8, INK, 2),
            rect(40, 48, 10, 8, INK, 2),
            rect(24, 18, 10, 8, INK, 2),
        ],
        "ModifyRule:OpponentTurnTime:0.8:1": [
            poly([(10, 6), (40, 6), (54, 20), (54, 58), (10, 58)], INK),
            rect(18, 18, 26, 6, PAPER),
            rect(18, 30, 20, 6, PAPER),
            rect(18, 42, 24, 6, PAPER),
        ],
        "ModifyRule:SelfTurnTime:1.2:1": [
            poly([(32, 6), (50, 18), (46, 40), (18, 40), (14, 18)], INK),
            rect(28, 40, 8, 8, INK),
            rect(16, 50, 32, 6, INK, 2),
            circle(32, 26, 4, PAPER),
        ],
        "ModifyRule:SelfTurnTime:1.5:1": [
            poly([(22, 8), (40, 18), (36, 38), (14, 38), (10, 18)], INK),
            rect(44, 14, 8, 24, INK),
            rect(36, 22, 24, 8, INK),
            rect(12, 50, 28, 6, INK, 2),
        ],
        "ModifyRule:CooldownCut:0.5:1": clock_slash,
        "ModifyRule:HideTrajectory:1:1": [
            poly([(4, 32), (18, 18), (32, 16), (46, 18), (60, 32), (46, 46), (32, 48), (18, 46)], INK),
            poly([(14, 16), (24, 16), (50, 50), (40, 50)], INK),
            poly([(40, 16), (50, 16), (24, 50), (14, 50)], INK),
        ],
        "ModifyRule:PushEdge:0.4:1": [
            rect(50, 8, 8, 48, INK),
            poly([(8, 24), (30, 32), (8, 40)], INK),
        ],
        "ModifyRule:PushEdge:0.7:1": [
            rect(54, 4, 8, 56, INK),
            rect(6, 28, 22, 8, INK),
            poly([(20, 16), (48, 32), (20, 48)], INK),
        ],
        "PlaceDeskObject:RulerWall:2": [
            rect(4, 26, 56, 14, INK, 2),
            rect(12, 26, 3, 7, PAPER),
            rect(22, 26, 3, 7, PAPER),
            rect(32, 26, 3, 7, PAPER),
            rect(42, 26, 3, 7, PAPER),
            rect(52, 26, 3, 7, PAPER),
        ],
        "PlaceDeskObject:DustPatch:2": [
            circle(14, 16, 5, INK),
            circle(36, 12, 4, INK),
            circle(52, 24, 3, INK),
            circle(24, 34, 7, INK),
            circle(46, 40, 5, INK),
            circle(16, 50, 4, INK),
        ],
        "PlaceDeskObject:DustPile:2": [
            poly([(6, 54), (58, 54), (32, 14)], INK),
            circle(24, 42, 4, PAPER),
            circle(40, 36, 3, PAPER),
        ],
        "PlaceDeskObject:ClearDust:1": [
            rect(30, 4, 6, 34, INK),
            rect(10, 34, 44, 8, INK, 2),
            rect(14, 42, 4, 16, INK),
            rect(24, 42, 4, 16, INK),
            rect(34, 42, 4, 16, INK),
            rect(44, 42, 4, 16, INK),
        ],
        "PlaceDeskObject:Cone:2": [
            poly([(32, 4), (54, 56), (10, 56)], INK),
            rect(20, 34, 24, 8, PAPER),
        ],
        "GrantShield:1:2": shield_cross,
        "GrantShield:1:3": [
            poly([(32, 2), (60, 16), (56, 36), (32, 62), (8, 36), (4, 16)], INK),
            rect(16, 20, 32, 6, PAPER),
            rect(16, 32, 32, 6, PAPER),
            rect(16, 44, 32, 6, PAPER),
        ],
        "GrantWeapon:0.1:1": [
            poly([(32, 2), (40, 16), (36, 16), (36, 54), (28, 54), (28, 16), (24, 16)], INK),
            rect(29, 36, 6, 8, PAPER),
        ],
        "GrantWeapon:0.15:1": [
            poly([(32, 4), (48, 22), (42, 56), (22, 56), (16, 22)], INK),
            rect(26, 30, 12, 8, PAPER),
        ],
        "GrantWeapon:0.18:1": [
            circle(32, 34, 22, INK),
            rect(6, 34, 52, 26, PAPER),
            rect(6, 30, 52, 6, INK),
        ],
        "GrantWeapon:0.2:1": [
            rect(16, 28, 36, 16, INK, 3),
            rect(20, 12, 28, 18, INK, 3),
            rect(26, 16, 14, 8, PAPER),
            rect(20, 46, 8, 10, INK),
            rect(36, 46, 8, 10, INK),
        ],
        "GrantWeapon:0.25:1": [
            rect(14, 12, 10, 36, INK, 4),
            rect(40, 16, 10, 36, INK, 4),
            rect(14, 8, 36, 10, INK, 4),
            rect(18, 46, 28, 10, INK, 4),
        ],
        "GrantWeapon:0.28:1": [
            rect(26, 4, 12, 12, INK, 2),
            poly([(18, 16), (46, 16), (52, 56), (12, 56)], INK),
            circle(24, 30, 4, PAPER),
            circle(40, 38, 5, PAPER),
            circle(30, 48, 4, PAPER),
        ],
        "GrantWeapon:0.3:1": [
            rect(14, 30, 36, 14, INK, 2),
            rect(18, 14, 26, 18, INK, 2),
            poly([(6, 6), (16, 6), (58, 58), (48, 58)], INK),
            poly([(48, 6), (58, 6), (16, 58), (6, 58)], INK),
        ],
    }


def effect_shapes(effect):
    if effect in REUSE:
        return ICONS[REUSE[effect]]
    custom = build_effect_shapes()
    if effect not in custom:
        raise SystemExit(f"no icon for {effect}")
    return custom[effect]


def fx_slug(effect):
    return "fx-" + effect.replace(":", "-").replace(".", "_")


def catalog_effects():
    path = ROOT / "Assets" / "Data" / "Csv" / "cards.csv"
    seen = []
    known = set()
    for line in path.read_text(encoding="utf-8").splitlines()[1:]:
        if not line.strip():
            continue
        effect = line.split(",")[10]
        if effect not in known:
            known.add(effect)
            seen.append(effect)
    return seen


def fmt_num(value):
    if isinstance(value, float) and not value.is_integer():
        return f"{value:.1f}"
    return str(int(value) if isinstance(value, float) else value)


def fmt_points(points):
    return " ".join(f"{x:.1f},{y:.1f}" for x, y in points)


def shape_svg(kind, attrs):
    if kind == "polygon":
        return f'<polygon points="{fmt_points(attrs["points"])}" fill="{attrs["fill"]}"/>'
    if kind == "circle":
        return (
            f'<circle cx="{fmt_num(attrs["cx"])}" cy="{fmt_num(attrs["cy"])}" '
            f'r="{fmt_num(attrs["r"])}" fill="{attrs["fill"]}"/>'
        )
    rx = attrs.get("rx", 0)
    rx_attr = f' rx="{fmt_num(rx)}"' if rx else ""
    return (
        f'<rect x="{fmt_num(attrs["x"])}" y="{fmt_num(attrs["y"])}" '
        f'width="{fmt_num(attrs["width"])}" height="{fmt_num(attrs["height"])}"'
        f'{rx_attr} fill="{attrs["fill"]}"/>'
    )


def to_svg(shapes):
    body = "\n".join(f"  {shape_svg(kind, attrs)}" for kind, attrs in shapes)
    return (
        '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64">\n'
        f"{body}\n"
        "</svg>\n"
    )


def parse_color(text):
    text = text.strip()
    value = int(text[1:], 16)
    return ((value >> 16) & 255, (value >> 8) & 255, value & 255, 255)


def raster(svg_text, size, flip_y):
    scale = size / VIEW
    buf = [(0, 0, 0, 0)] * (size * size)

    def mx(x):
        return x * scale

    def my(y):
        return (VIEW - y) * scale if flip_y else y * scale

    def plot(px, py, color):
        if 0 <= px < size and 0 <= py < size:
            buf[py * size + px] = color

    def fill_circle(cx, cy, radius, color):
        r2 = radius * radius
        for py in range(int(cy - radius) - 1, int(cy + radius) + 2):
            for px in range(int(cx - radius) - 1, int(cx + radius) + 2):
                dx = px + 0.5 - cx
                dy = py + 0.5 - cy
                if dx * dx + dy * dy <= r2:
                    plot(px, py, color)

    def fill_round_rect(x, y, w, h, radius, color):
        if w <= 0 or h <= 0:
            return
        r = min(radius, w / 2, h / 2)
        cx = x + w / 2
        cy = y + h / 2
        hx = w / 2 - r
        hy = h / 2 - r
        for py in range(int(y) - 1, int(y + h) + 2):
            for px in range(int(x) - 1, int(x + w) + 2):
                dx = abs(px + 0.5 - cx) - hx
                dy = abs(py + 0.5 - cy) - hy
                ox = max(dx, 0)
                oy = max(dy, 0)
                if ox * ox + oy * oy <= r * r:
                    plot(px, py, color)

    def fill_polygon(pts, color):
        if len(pts) < 3:
            return
        min_y = min(p[1] for p in pts)
        max_y = max(p[1] for p in pts)
        for py in range(int(math.floor(min_y)), int(math.ceil(max_y)) + 1):
            scan = py + 0.5
            xs = []
            for i, (x1, y1) in enumerate(pts):
                x2, y2 = pts[(i + 1) % len(pts)]
                if (y1 <= scan < y2) or (y2 <= scan < y1):
                    xs.append(x1 + (scan - y1) * (x2 - x1) / (y2 - y1))
            xs.sort()
            pair = 0
            while pair + 1 < len(xs):
                start = int(math.ceil(xs[pair]))
                end = int(math.floor(xs[pair + 1]))
                for px in range(start, end + 1):
                    plot(px, py, color)
                pair += 2

    for raw in svg_text.split("<")[1:]:
        kind = raw.split(None, 1)[0].split(">", 1)[0].strip("/")
        if kind not in ("rect", "circle", "polygon"):
            continue
        attrs = {}
        for token in raw.split('"'):
            pass
        parts = raw.split('"')
        index = 0
        while index + 1 < len(parts):
            key = parts[index].split()[-1].strip("=").strip()
            attrs[key] = parts[index + 1]
            index += 2
        fill = parse_color(attrs["fill"])
        if kind == "circle":
            fill_circle(mx(float(attrs["cx"])), my(float(attrs["cy"])), float(attrs["r"]) * scale, fill)
        elif kind == "rect":
            x = mx(float(attrs["x"]))
            w = float(attrs["width"]) * scale
            h = float(attrs["height"]) * scale
            y_top = float(attrs["y"])
            y = my(y_top + float(attrs["height"])) if flip_y else my(y_top)
            rx = float(attrs["rx"]) * scale if "rx" in attrs else 0
            fill_round_rect(x, y, w, h, rx, fill)
        else:
            pts = []
            for pair in attrs["points"].split():
                sx, sy = pair.split(",")
                pts.append((mx(float(sx)), my(float(sy))))
            fill_polygon(pts, fill)
    return buf


def write_png(path, width, height, pixels):
    import struct
    import zlib

    def chunk(tag, data):
        return struct.pack(">I", len(data)) + tag + data + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF)

    raw = bytearray()
    for y in range(height):
        raw.append(0)
        for x in range(width):
            raw.extend(pixels[y * width + x])
    png = b"\x89PNG\r\n\x1a\n"
    png += chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0))
    png += chunk(b"IDAT", zlib.compress(bytes(raw), 9))
    png += chunk(b"IEND", b"")
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(png)


def top_first(buf, size, flip_y):
    if not flip_y:
        return buf
    rows = []
    for y in range(size - 1, -1, -1):
        rows.extend(buf[y * size:(y + 1) * size])
    return rows


def emit_cs(markups, card_markups):
    cases = []
    fields = []
    names = []
    for enum_name, key in SYMBOLS:
        cases.append(
            f"                case CardSymbol.{enum_name}:\n                    return {enum_name};"
        )
        names.append(
            f"                case CardSymbol.{enum_name}:\n                    return \"{key}.svg\";"
        )
        escaped = markups[key].replace('"', '""')
        fields.append(f"        const string {enum_name} =\n@\"{escaped}\";")
    card_cases = []
    for index, (effect, markup) in enumerate(card_markups.items()):
        const = f"Fx{index}"
        escaped = markup.replace('"', '""')
        fields.append(f"        const string {const} =\n@\"{escaped}\";")
        card_cases.append(
            f'                case "{effect}":\n                    markup = {const};\n                    return true;'
        )
    return (
        "using System;\n"
        "using EraserWars.Cards;\n"
        "\n"
        "namespace EraserWars.UI\n"
        "{\n"
        "    public static class CardIconSvg\n"
        "    {\n"
        "        public static string Markup(CardSymbol symbol)\n"
        "        {\n"
        "            switch (symbol)\n"
        "            {\n"
        + "\n".join(cases)
        + "\n                default:\n"
        + '                    throw new InvalidOperationException($"Unhandled CardSymbol {symbol}");\n'
        + "            }\n"
        + "        }\n"
        + "\n"
        + "        public static bool TryMarkup(string iconKey, out string markup)\n"
        + "        {\n"
        + "            switch (iconKey)\n"
        + "            {\n"
        + "\n".join(card_cases)
        + "\n                default:\n"
        + "                    markup = null;\n"
        + "                    return false;\n"
        + "            }\n"
        + "        }\n"
        + "\n"
        + "        public static string FileName(CardSymbol symbol)\n"
        + "        {\n"
        + "            switch (symbol)\n"
        + "            {\n"
        + "\n".join(names)
        + "\n                default:\n"
        + '                    throw new InvalidOperationException($"Unhandled CardSymbol {symbol}");\n'
        + "            }\n"
        + "        }\n"
        + "\n"
        + "        public static string EffectFileName(string iconKey)\n"
        + "        {\n"
        + '            return "fx-" + iconKey.Replace(":", "-").Replace(".", "_") + ".svg";\n'
        + "        }\n"
        + "\n"
        + "\n\n".join(fields)
        + "\n    }\n"
        + "}\n"
    )


FAMILIES = [
    ("kiai", (214, 24, 40), (255, 206, 196), "1", "weight", False),
    ("homework", (12, 92, 214), (210, 230, 255), "1", "clock", False),
    ("seat", (8, 150, 64), (196, 238, 204), "2", "swap", False),
    ("stationery", (230, 128, 8), (255, 224, 150), "2", "desk", False),
    ("cover", (214, 32, 104), (255, 196, 214), "2", "shield", False),
    ("signature", (232, 184, 72), (22, 26, 42), "4", "extra", True),
]


def blend(bg, fg):
    sr, sg, sb, sa = fg
    if sa == 0:
        return bg
    a = sa / 255
    return (
        int(fg[0] * a + bg[0] * (1 - a)),
        int(fg[1] * a + bg[1] * (1 - a)),
        int(fg[2] * a + bg[2] * (1 - a)),
        255,
    )


def pattern_pixel(family, x, y):
    ink = (232, 184, 72, 190) if family == "signature" else (40, 22, 16, 175)
    if family == "kiai" and (x + y) % 7 < 2:
        return ink
    if family == "homework" and (x - y) % 7 < 2:
        return ink
    if family == "seat" and (x + y) % 6 < 2:
        return ink
    if family == "stationery" and (x - y) % 6 < 2:
        return ink
    if family == "cover" and (x + y) % 8 < 2:
        return ink
    if family == "signature" and ((x + y) % 10 < 2 or (x - y) % 10 < 2):
        return ink
    if family not in ("kiai", "homework", "seat", "stationery", "cover", "signature") and (x + y) % 8 < 2:
        return ink
    return (0, 0, 0, 0)


def rounded_card(px, py, w, h, radius):
    r = radius
    cx = w / 2
    cy = h / 2
    dx = abs(px + 0.5 - cx) - (w / 2 - r)
    dy = abs(py + 0.5 - cy) - (h / 2 - r)
    ox = max(dx, 0)
    oy = max(dy, 0)
    return ox * ox + oy * oy <= r * r


def paint_sheet(markups):
    card_w, card_h = 140, 196
    gap = 16
    margin = 24
    cols = len(FAMILIES)
    width = margin * 2 + cols * card_w + (cols - 1) * gap
    icon_rows = 2
    height = margin * 2 + card_h + 16 + icon_rows * 48
    pixels = [(18, 32, 28, 255)] * (width * height)
    icons = {}
    for key, _shapes in ICONS.items():
        yup = raster(markups[key], 128, True)
        icons[key] = top_first(yup, 128, True)

    keyline = (28, 16, 14, 255)
    for index, (family, frame, paper, cost, icon, signature) in enumerate(FAMILIES):
        left = margin + index * (card_w + gap)
        top = margin
        plate = (255, 232, 170, 255) if signature else (255, 255, 255, 255)
        stroke = (255, 220, 140, 255) if signature else (255, 255, 255, 255)
        for py in range(card_h):
            for px in range(card_w):
                if not rounded_card(px, py, card_w, card_h, 16):
                    continue
                color = (*frame, 255)
                edge = px < 3 or py < 3 or px >= card_w - 3 or py >= card_h - 3
                in_plate = 8 <= px < card_w - 8 and card_h - 44 <= py < card_h - 8
                in_art = 8 <= px < card_w - 8 and 8 <= py < card_h - 52
                on_stroke = in_art and (
                    px < 11 or py < 11 or px >= card_w - 11 or py >= card_h - 55
                )
                if edge:
                    color = keyline
                elif in_plate:
                    color = plate
                elif on_stroke:
                    color = stroke
                elif in_art:
                    color = blend((*paper, 255), pattern_pixel(family, px, py))
                pixels[(top + py) * width + left + px] = color
        glyph = icons[icon]
        art_top = 8
        art_h = card_h - 52 - 8
        art_w = card_w - 16
        icon_px = 78
        gx = left + 8 + (art_w - icon_px) // 2
        gy = top + art_top + (art_h - icon_px) // 2
        for py in range(icon_px):
            for px in range(icon_px):
                src = glyph[(py * 128 // icon_px) * 128 + (px * 128 // icon_px)]
                dest = gy + py
                if dest >= top + card_h:
                    continue
                base = pixels[dest * width + gx + px]
                pixels[dest * width + gx + px] = blend(base, src)
        gem = (232, 184, 72) if signature else (12, 10, 10)
        gem_ink = (28, 22, 12) if signature else (255, 255, 255)
        for py in range(30):
            for px in range(30):
                dist = (px - 14.5) ** 2 + (py - 14.5) ** 2
                if dist <= 14.5 ** 2:
                    color = (255, 255, 255, 255) if dist >= 11.5 ** 2 else (*gem, 255)
                    pixels[(top + py) * width + left + px] = color
        draw_digit(pixels, width, left + 12, top + 8, cost, gem_ink)
    icon_top = margin + card_h + 16
    for index, key in enumerate(ICONS):
        glyph = icons[key]
        col = index % 9
        row = index // 9
        gx = margin + col * 56
        gy = icon_top + row * 48
        for py in range(40):
            for px in range(40):
                src = glyph[(py * 128 // 40) * 128 + (px * 128 // 40)]
                pixels[(gy + py) * width + gx + px] = blend((255, 252, 246, 255), src)
    write_png(SHEET_PATH, width, height, pixels)


def draw_digit(pixels, width, x, y, digit, color):
    font = {
        "1": ["010", "110", "010", "010", "010", "010", "111"],
        "2": ["111", "001", "001", "111", "100", "100", "111"],
        "4": ["100", "100", "101", "101", "111", "001", "001"],
    }
    glyph = font[digit]
    for row, bits in enumerate(glyph):
        for col, bit in enumerate(bits):
            if bit == "1":
                for py in range(2):
                    for px in range(2):
                        pixels[(y + row * 2 + py) * width + x + col * 2 + px] = (*color, 255)


def write_svg_meta(path):
    meta_path = Path(str(path) + ".meta")
    if meta_path.exists():
        return
    template = (ICON_DIR / "weight.svg.meta").read_text(encoding="utf-8")
    import uuid

    guid = uuid.uuid4().hex
    lines = template.splitlines()
    replaced = False
    out = []
    for line in lines:
        if not replaced and line.startswith("guid: "):
            out.append("guid: " + guid)
            replaced = True
        else:
            out.append(line)
    meta_path.write_text("\n".join(out) + "\n", encoding="utf-8", newline="\n")


def assert_distinct(card_markups):
    rasters = {}
    for effect, markup in card_markups.items():
        up = raster(markup, 96, True)
        down = raster(markup, 96, False)
        if top_first(up, 96, True) != down:
            raise SystemExit(f"flip check failed for {effect}")
        rasters[effect] = up
    effects = list(rasters)
    for i, left in enumerate(effects):
        for right in effects[i + 1 :]:
            differ = sum(1 for a, b in zip(rasters[left], rasters[right]) if a != b)
            if differ < 120:
                raise SystemExit(f"icons too similar ({differ} px): {left} vs {right}")


def main():
    markups = {key: to_svg(shapes) for key, shapes in ICONS.items()}
    yup = raster(markups["shield"], 64, True)
    ydown = raster(markups["shield"], 64, False)
    if top_first(yup, 64, True) != ydown:
        raise SystemExit("flip check failed for shield")
    for key, markup in markups.items():
        up = raster(markup, 96, True)
        down = raster(markup, 96, False)
        if top_first(up, 96, True) != down:
            raise SystemExit(f"flip check failed for {key}")

    card_markups = {effect: to_svg(effect_shapes(effect)) for effect in catalog_effects()}
    assert_distinct(card_markups)

    ICON_DIR.mkdir(parents=True, exist_ok=True)
    for key, markup in markups.items():
        (ICON_DIR / f"{key}.svg").write_text(markup, encoding="utf-8", newline="\n")
    for effect, markup in card_markups.items():
        path = ICON_DIR / f"{fx_slug(effect)}.svg"
        path.write_text(markup, encoding="utf-8", newline="\n")
        write_svg_meta(path)
    CS_PATH.write_text(emit_cs(markups, card_markups), encoding="utf-8", newline="\n")
    paint_sheet(markups)
    print(f"wrote {len(markups)} family icons, {len(card_markups)} effect icons, and {SHEET_PATH}")


if __name__ == "__main__":
    main()
