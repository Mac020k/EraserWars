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


def emit_cs(markups):
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

    ICON_DIR.mkdir(parents=True, exist_ok=True)
    for key, markup in markups.items():
        (ICON_DIR / f"{key}.svg").write_text(markup, encoding="utf-8", newline="\n")
    CS_PATH.write_text(emit_cs(markups), encoding="utf-8", newline="\n")
    paint_sheet(markups)
    print(f"wrote {len(markups)} icons and {SHEET_PATH}")


if __name__ == "__main__":
    main()
