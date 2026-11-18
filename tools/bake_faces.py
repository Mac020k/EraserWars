"""Bake square face-bust crops from the full-body character stills.

Usage: python tools/bake_faces.py [--preview]

Each entry is (head_center_x, eye_y, head_width) measured on the 864x1152 still.
The crop is a square whose side makes the head ~68% of the width, with the eyes
about 45% from the top, matching the character card reference.
"""
import argparse
import os
import sys

from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
STILLS = os.path.join(ROOT, "Assets", "Art", "Characters", "Stills")
FACES = os.path.join(ROOT, "Assets", "Art", "Characters", "Faces")
PREVIEW = os.path.join(ROOT, "tmp_faces", "contact.png")

HEAD_RATIO = 0.68
EYE_RATIO = 0.45
OUT_SIZE = 512

FILES = [
    "char_still_00_akaboshi.jpg", "char_still_01_todoroki.jpg", "char_still_02_minazuki.jpg",
    "char_still_03_hanasaki.jpg", "char_still_04_alex.jpg", "char_still_05_ohori.jpg",
    "char_still_06_kuroe.jpg", "char_still_07_mei.jpg", "char_still_08_kageyama.jpg",
    "char_still_09_ozora.jpg", "char_still_10_hinata.jpg", "char_still_11_gou.jpg",
    "char_still_12_iroha.jpg", "char_still_13_atom.jpg", "char_still_14_meshiko.jpg",
    "char_still_15_mike.jpg", "char_still_16_pika.jpg", "char_still_17_kiyora.jpg",
    "char_still_18_sora.jpg", "char_still_19_nyan.jpg",
]

# (head_center_x, eye_y, head_width) in still pixels.
FACE = {
    0: (413, 194, 300),
    1: (427, 138, 268),
    2: (413, 166, 240),
    3: (425, 172, 290),
    4: (405, 148, 243),
    5: (429, 154, 240),
    6: (422, 200, 275),
    7: (390, 122, 235),
    8: (427, 249, 180),
    9: (418, 222, 243),
    10: (427, 186, 264),
    11: (397, 264, 222),
    12: (427, 190, 264),
    13: (409, 207, 243),
    14: (437, 207, 222),
    15: (446, 234, 317),
    16: (416, 232, 222),
    17: (429, 317, 222),
    18: (422, 241, 211),
    19: (393, 264, 211),
}


def crop_box(width, height, cx, eye_y, head_w):
    side = head_w / HEAD_RATIO
    side = min(side, width, height)
    left = cx - side / 2
    top = eye_y - EYE_RATIO * side
    left = max(0, min(left, width - side))
    top = max(0, min(top, height - side))
    return (left, top, left + side, top + side)


def circle_preview(face, size=220):
    face = face.resize((size, size), Image.LANCZOS)
    mask = Image.new("L", (size, size), 0)
    ImageDraw.Draw(mask).ellipse((4, 4, size - 4, size - 4), fill=255)
    out = Image.new("RGB", (size, size), (255, 250, 238))
    out.paste(face, (0, 0), mask)
    ImageDraw.Draw(out).ellipse((4, 4, size - 4, size - 4), outline=(0, 0, 0), width=4)
    return out


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--preview", action="store_true")
    args = parser.parse_args()

    os.makedirs(FACES, exist_ok=True)
    os.makedirs(os.path.dirname(PREVIEW), exist_ok=True)
    cells = []
    for index, name in enumerate(FILES):
        still = Image.open(os.path.join(STILLS, name)).convert("RGB")
        cx, eye_y, head_w = FACE[index]
        box = crop_box(still.width, still.height, cx, eye_y, head_w)
        face = still.crop(tuple(int(round(v)) for v in box)).resize((OUT_SIZE, OUT_SIZE), Image.LANCZOS)
        stem = name.replace("char_still_", "char_face_").replace(".jpg", ".png")
        face.save(os.path.join(FACES, stem))
        cells.append(circle_preview(face))

    if args.preview:
        cols = 5
        rows = (len(cells) + cols - 1) // cols
        sheet = Image.new("RGB", (cols * 224, rows * 224), (255, 250, 238))
        for i, cell in enumerate(cells):
            sheet.paste(cell, ((i % cols) * 224, (i // cols) * 224))
            ImageDraw.Draw(sheet).text(((i % cols) * 224 + 8, (i // cols) * 224 + 8), str(i), fill=(255, 0, 0))
        sheet.save(PREVIEW)
        print("preview", PREVIEW)
    print("baked", len(cells))
    return 0


if __name__ == "__main__":
    sys.exit(main())
