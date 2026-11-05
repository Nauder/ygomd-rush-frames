"""Patch the Shine shader's fixed title bounds, preserving its other instructions.

DXBC uses MD5 with different final-block padding. Format reference:
https://github.com/wine-mirror/wine/blob/master/libs/vkd3d/libs/vkd3d-shader/checksum.c
This implementation checks every original checksum before making any change.
"""

import argparse
import ctypes
import hashlib
import math
import struct
from pathlib import Path

import UnityPy
from UnityPy.helpers import CompressionHelper


def dxbc_checksum(code):
    payload = code[20:]
    size = len(payload)
    whole = size // 64 * 64
    tail = payload[whole:] + b"\x80"
    if len(tail) > 56:
        blocks = payload[:whole] + tail.ljust(64, b"\0")
        final = bytes(56)
    else:
        blocks = payload[:whole]
        final = tail.ljust(56, b"\0")
    blocks += struct.pack("<I", size * 8) + final + struct.pack("<I", (size * 2) | 1)
    state = [0x67452301, 0xEFCDAB89, 0x98BADCFE, 0x10325476]
    rotations = [7, 12, 17, 22] * 4 + [5, 9, 14, 20] * 4 + [4, 11, 16, 23] * 4 + [6, 10, 15, 21] * 4
    constants = [int(abs(math.sin(i + 1)) * 2**32) for i in range(64)]
    for offset in range(0, len(blocks), 64):
        words = struct.unpack_from("<16I", blocks, offset)
        a, b, c, d = state
        for i in range(64):
            if i < 16:
                f, g = (b & c) | (~b & d), i
            elif i < 32:
                f, g = (d & b) | (~d & c), (5 * i + 1) % 16
            elif i < 48:
                f, g = b ^ c ^ d, (3 * i + 5) % 16
            else:
                f, g = c ^ (b | ~d), (7 * i) % 16
            value = (a + f + constants[i] + words[g]) & 0xFFFFFFFF
            value = ((value << rotations[i]) | (value >> (32 - rotations[i]))) & 0xFFFFFFFF
            a, d, c, b = d, c, b, (b + value) & 0xFFFFFFFF
        state = [(x + y) & 0xFFFFFFFF for x, y in zip(state, (a, b, c, d))]
    return struct.pack("<4I", *state)


def validate_shader(code):
    """Ask the Windows shader compiler to parse and validate the new container."""
    compiler = ctypes.WinDLL("d3dcompiler_47")
    strip = compiler.D3DStripShader
    strip.argtypes = [ctypes.c_void_p, ctypes.c_size_t, ctypes.c_uint, ctypes.POINTER(ctypes.c_void_p)]
    strip.restype = ctypes.c_long
    output = ctypes.c_void_p()
    buffer = ctypes.create_string_buffer(code)
    status = strip(buffer, len(code), 0, ctypes.byref(output))
    if status < 0:
        raise ValueError(f"Windows rejected shader container: 0x{status & 0xFFFFFFFF:08x}")
    table = ctypes.cast(output, ctypes.POINTER(ctypes.POINTER(ctypes.c_void_p))).contents
    ctypes.WINFUNCTYPE(ctypes.c_uint, ctypes.c_void_p)(table[2])(output)


def patch_programs(blob):
    changes = (
        # The finish was limited to UV 0.05..0.95, clipping the raised capitals.
        (struct.pack("<2f", -0.9, -0.9), struct.pack("<2f", -0.98, -0.98)),
        # Name colour mask: keep its bottom at .89, extend its top from .952 to .99.
        (struct.pack("<2f", 0.054, -0.421), struct.pack("<2f", 0.054, -0.44)),
    )
    result = bytearray(blob)
    position = count = 0
    while True:
        position = blob.find(b"DXBC", position)
        if position < 0:
            break
        size = struct.unpack_from("<I", blob, position + 24)[0]
        code = blob[position:position + size]
        if not size or len(code) != size:
            raise ValueError("Invalid shader program length")
        if changes[0][0] in code:
            if dxbc_checksum(code) != code[4:20]:
                raise ValueError("Original shader checksum mismatch")
            for old, new in changes:
                if code.count(old) != 1:
                    raise ValueError("Unexpected Shine shader instructions; refusing to patch")
                code = code.replace(old, new)
            # The 3D compiler places the Y bound in register Z instead of Y.
            height_patterns = (
                (struct.pack("<2f", -0.757, -0.062), struct.pack("<2f", -0.757, -0.1)),
                (struct.pack("<3f", -0.757, 0, -0.062), struct.pack("<3f", -0.757, 0, -0.1)),
            )
            matches = [(old, new) for old, new in height_patterns if old in code]
            if len(matches) != 1 or code.count(matches[0][0]) != 1:
                raise ValueError("Unexpected title height instruction")
            code = code.replace(*matches[0])
            code = code[:4] + dxbc_checksum(code) + code[20:]
            validate_shader(code)
            result[position:position + size] = code
            count += 1
        elif struct.pack("<2f", -0.98, -0.98) in code:
            # Reinstalling the assets must not fail or patch a second time.
            if dxbc_checksum(code) != code[4:20] or struct.pack("<2f", 0.054, -0.44) not in code:
                raise ValueError("Unexpected previously patched shader")
            validate_shader(code)
            count += 1
        position += size
    if not count:
        raise ValueError("No expected title clipping instructions found")
    return bytes(result), count


def calibrate_attribute(original):
    """Fit the 96x74 red glyph to the 119x92 green target, including its offset."""
    x, y, z, w = original
    scale_x, scale_y = x * 96 / 119, y * 74 / 92
    pixels_per_u, pixels_per_v = 96 * x / (179 / 256), 74 * y / (142 / 256)
    center_x = 0.5 - z / x + 5.5 / pixels_per_u - (0.5 / 256) * (1 / scale_x - 1 / x)
    center_y = 0.5 - w / y - 4 / pixels_per_v - (5 / 256) * (1 / scale_y - 1 / y)
    return (scale_x, scale_y, (0.5 - center_x) * scale_x, (0.5 - center_y) * scale_y)


def attribute_mapping(current, original):
    corrected = calibrate_attribute(original)
    def matches(a, b):
        return all(abs(x - y) < 0.00005 for x, y in zip(a, b))
    if not matches(current, original) and not matches(current, corrected):
        raise ValueError(f"Unexpected attribute mapping: {current}")
    return corrected


def patch_shader_assets(env, required=False):
    patched = []
    title_shaders = []
    attribute_assets = []
    for obj in env.objects:
        if obj.type.name == "Material":
            material = obj.read()
            if material.m_Name not in ("ShineStyle", "RoyalStyle"):
                continue
            for key, value in material.m_SavedProperties.m_Colors:
                if key != "_AttributeSize_Pos":
                    continue
                current = (value.r, value.g, value.b, value.a)
                value.r, value.g, value.b, value.a = attribute_mapping(current, (10.64, 14.87, -4.048, -6.2))
                material.save()
                patched.append(material.m_Name)
                attribute_assets.append(material.m_Name)
                print(f"{material.m_Name}: saved attribute mapping {(value.r, value.g, value.b, value.a)}")
            continue
        if obj.type.name != "Shader":
            continue
        shader = obj.read()
        name = shader.m_ParsedForm.m_Name
        if name in ("Shader Graphs/ShineStyleUI", "Shader Graphs/ShineStyle3D",
                    "Shader Graphs/RoyalStyleUI", "Shader Graphs/RoyalStyle3D"):
            for prop in shader.m_ParsedForm.m_PropInfo.m_Props:
                if prop.m_Name != "_AttributeSize_Pos":
                    continue
                fields = [f"m_DefValue_{i}_" for i in range(4)]
                current = tuple(getattr(prop, field) for field in fields)
                corrected = attribute_mapping(current, (9.85, 13.9, -3.7, -5.81))
                for field, value in zip(fields, corrected):
                    setattr(prop, field, value)
                shader.save()
                patched.append(name)
                attribute_assets.append(name)
                print(f"{name}: saved default attribute mapping {corrected}")
        if name not in ("Shader Graphs/ShineStyleUI", "Shader Graphs/ShineStyle3D"):
            continue
        if shader.platforms != [4] or len(shader.offsets[0]) != 1:
            raise ValueError(f"Unsupported shader platforms: {name}")
        start = shader.offsets[0][0]
        length = shader.compressedLengths[0][0]
        raw = CompressionHelper.decompress_lz4(bytes(shader.compressedBlob)[start:start + length],
                                             shader.decompressedLengths[0][0])
        corrected, count = patch_programs(raw)
        compressed = CompressionHelper.compress_lz4(corrected)
        shader.compressedBlob = compressed
        shader.offsets = [[0]]
        shader.compressedLengths = [[len(compressed)]]
        shader.decompressedLengths = [[len(corrected)]]
        shader.save()
        title_shaders.append(name)
        print(f"{name}: verified title correction in {count} pixel shader variants")
    if len(title_shaders) not in (0, 2) or (required and len(title_shaders) != 2):
        raise ValueError(f"Expected two Shine shaders, found {title_shaders}")
    if len(attribute_assets) not in (0, 6) or (required and len(attribute_assets) != 6):
        raise ValueError(f"Expected six attribute mapping assets, found {attribute_assets}")
    return patched


def patch_bundle(source, output):
    def fingerprint():
        with source.open("rb") as stream:
            return hashlib.file_digest(stream, "sha256").hexdigest()
    original_hash = fingerprint()
    env = UnityPy.load(str(source))
    patch_shader_assets(env, required=True)
    if fingerprint() != original_hash:
        raise ValueError("Source bundle changed while preparing the patch; run again against the latest file")
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_bytes(env.file.save())
    output.with_suffix(output.suffix + ".source.sha256").write_text(original_hash, encoding="ascii")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    if args.source.resolve() == args.output.resolve():
        parser.error("Source and output must differ")
    patch_bundle(args.source, args.output)
