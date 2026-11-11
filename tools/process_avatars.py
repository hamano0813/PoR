"""Remove the white anti-alias matte from the original portrait GIFs.

The original game portraits are GIFs with a hard transparency mask. Their
outer anti-aliased ring is stored as opaque grayscale pixels blended against
white, which shows up as a bright fringe on dark backgrounds. This script
converts grayscale edge pixels back into black with an estimated alpha value
and writes transparent PNGs that look correct in both light and dark themes.

Usage:
    python tools/process_avatars.py
    python tools/process_avatars.py --source dotnet/src/PoR.App/Assets/PID
"""

from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image

GRAY_TOLERANCE = 8
NEIGHBORS = tuple(
    (dx, dy)
    for dy in (-1, 0, 1)
    for dx in (-1, 0, 1)
    if (dx, dy) != (0, 0)
)


def defringe(image: Image.Image) -> tuple[Image.Image, int]:
    source = image.convert("RGBA")
    width, height = source.size
    source_pixels = source.load()
    result = source.copy()
    result_pixels = result.load()
    changed = 0

    for y in range(height):
        for x in range(width):
            red, green, blue, alpha = source_pixels[x, y]
            if alpha == 0:
                continue

            touches_transparency = any(
                0 <= x + dx < width
                and 0 <= y + dy < height
                and source_pixels[x + dx, y + dy][3] == 0
                for dx, dy in NEIGHBORS
            )
            if not touches_transparency:
                continue

            # Only the neutral matte ring is rewritable. Colored edge pixels
            # are part of the artwork and stay opaque.
            if max(red, green, blue) - min(red, green, blue) > GRAY_TOLERANCE:
                continue

            # The pixel was composited over white: value = alpha * color
            # + (1 - alpha) * 255. The outline color is black, so its
            # coverage is 1 - value / 255.
            result_pixels[x, y] = (0, 0, 0, 255 - min(red, green, blue))
            changed += 1

    return result, changed


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--source",
        type=Path,
        default=Path("dotnet/src/PoR.App/Assets/PID"),
        help="Directory containing the original PID_*.gif portraits.",
    )
    args = parser.parse_args()

    source_dir: Path = args.source
    gifs = sorted(source_dir.glob("*.gif"))
    if not gifs:
        raise SystemExit(f"No GIFs found in {source_dir}")

    total_pixels = 0
    for index, gif in enumerate(gifs, start=1):
        with Image.open(gif) as image:
            processed, changed = defringe(image)
        output = gif.with_suffix(".png")
        processed.save(output, "PNG", optimize=True)
        total_pixels += changed
        if index % 50 == 0 or index == len(gifs):
            print(f"{index}/{len(gifs)} portraits processed")

    print(f"Wrote {len(gifs)} PNGs to {source_dir} ({total_pixels} edge pixels rewritten)")


if __name__ == "__main__":
    main()
