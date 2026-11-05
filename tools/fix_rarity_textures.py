"""Prepare exact mask/normal data corrections for the Rush frame installer.

Run against an unmodified res/mask directory. Output goes to a separate directory
so running this repeatedly never resamples already corrected normal maps.
"""

import argparse
from pathlib import Path

from PIL import Image


def correct_texture(path):
    image = Image.open(path).convert("RGBA")
    if path.name in ("CardMask_N.png", "CardMask_P.png"):
        if image.size != (512, 512):
            raise ValueError(f"Unexpected mask dimensions: {path}")
        # The name was lifted 20 canvas units. Its capitals now overlap the old
        # red border at rows 8..13. Continue the black name region through it.
        image.paste((0, 0, 0, 255), (20, 8, 414, 14))
    elif path.name.startswith("CardNormal_"):
        if image.size != (1024, 1024):
            raise ValueError(f"Unexpected normal dimensions: {path}")
        source = (842, 47, 948, 120)
        # CardPicture is 723.093 x 1054. The Ruby sprite is 76 x 76, scaled by 1.3,
        # with its center at (281, 441.4) relative to the canvas center.
        half_icon = 76 * 1.3 / 2
        destination = (
            round((0.5 + (281 - half_icon) / 723.093) * 1024),
            round((0.5 - (441.4 + half_icon) / 1054) * 1024),
            round((0.5 + (281 + half_icon) / 723.093) * 1024),
            round((0.5 - (441.4 - half_icon) / 1054) * 1024),
        )
        size = (destination[2] - destination[0], destination[3] - destination[1])
        # Alpha encodes normal X in the packed RNA/Link textures. Resize each
        # channel as data; RGBA resizing would premultiply by that normal value.
        patch = Image.merge("RGBA", tuple(
            channel.resize(size, Image.Resampling.BILINEAR)
            for channel in image.crop(source).split()
        ))
        image.paste(image.getpixel((500, 40)), source)
        image.paste(patch, destination)
    else:
        raise ValueError(f"Unsupported texture: {path}")
    return image


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    if args.source.resolve() == args.output.resolve():
        parser.error("Source and output must be different directories")
    args.output.mkdir(parents=True, exist_ok=True)
    for name in ("CardMask_N", "CardMask_P", "CardNormal_RNA", "CardNormal_RNA_Sp",
                 "CardNormal_Pen", "CardNormal_Link"):
        path = args.source / f"{name}.png"
        corrected = correct_texture(path)
        corrected.save(args.output / path.name)
        print(f"Prepared {path.name} ({corrected.width} x {corrected.height})")


if __name__ == "__main__":
    main()
