namespace Emulation
{
    public static class ColorMap
    {
        // NES Color Palette Lookup Table (NES colors mapped to 24-bit RGB colors)
        public static byte[][] LUT = new byte[][]
        {
            new byte[] { 124, 124, 124 },     // 0x00
            new byte[] { 0, 0, 252 },         // 0x01
            new byte[] { 0, 0, 188 },         // 0x02
            new byte[] { 68, 40, 188 },       // 0x03
            new byte[] { 148, 0, 132 },       // 0x04
            new byte[] { 168, 0, 32 },        // 0x05
            new byte[] { 168, 16, 0 },        // 0x06
            new byte[] { 136, 20, 0 },        // 0x07
            new byte[] { 80, 48, 0 },         // 0x08
            new byte[] { 0, 120, 0 },         // 0x09
            new byte[] { 0, 104, 0 },         // 0x0A
            new byte[] { 0, 88, 0 },          // 0x0B
            new byte[] { 0, 64, 88 },         // 0x0C
            new byte[] { 0, 0, 0 },           // 0x0D
            new byte[] { 0, 0, 0 },           // 0x0E
            new byte[] { 0, 0, 0 },           // 0x0F
            new byte[] { 188, 188, 188 },     // 0x10
            new byte[] { 0, 120, 248 },       // 0x11
            new byte[] { 0, 88, 248 },        // 0x12
            new byte[] { 104, 68, 252 },      // 0x13
            new byte[] { 216, 0, 204 },       // 0x14
            new byte[] { 228, 0, 88 },        // 0x15
            new byte[] { 248, 56, 0 },        // 0x16
            new byte[] { 228, 92, 16 },       // 0x17
            new byte[] { 172, 124, 0 },       // 0x18
            new byte[] { 0, 184, 0 },         // 0x19
            new byte[] { 0, 168, 0 },         // 0x1A
            new byte[] { 0, 168, 68 },        // 0x1B
            new byte[] { 0, 136, 136 },       // 0x1C
            new byte[] { 0, 0, 0 },           // 0x1D
            new byte[] { 0, 0, 0 },           // 0x1E
            new byte[] { 0, 0, 0 },           // 0x1F
            new byte[] { 248, 248, 248 },     // 0x20
            new byte[] { 60, 188, 252 },      // 0x21
            new byte[] { 104, 136, 252 },     // 0x22
            new byte[] { 152, 120, 248 },     // 0x23
            new byte[] { 248, 120, 248 },     // 0x24
            new byte[] { 248, 88, 152 },      // 0x25
            new byte[] { 248, 120, 88 },      // 0x26
            new byte[] { 252, 160, 68 },      // 0x27
            new byte[] { 248, 184, 0 },       // 0x28
            new byte[] { 184, 248, 24 },      // 0x29
            new byte[] { 88, 216, 84 },       // 0x2A
            new byte[] { 88, 248, 152 },      // 0x2B
            new byte[] { 0, 232, 216 },       // 0x2C
            new byte[] { 120, 120, 120 },     // 0x2D
            new byte[] { 0, 0, 0 },           // 0x2E
            new byte[] { 0, 0, 0 },           // 0x2F
            new byte[] { 252, 252, 252 },     // 0x30
            new byte[] { 164, 228, 252 },     // 0x31
            new byte[] { 184, 184, 248 },     // 0x32
            new byte[] { 216, 184, 248 },     // 0x33
            new byte[] { 248, 184, 248 },     // 0x34
            new byte[] { 248, 164, 192 },     // 0x35
            new byte[] { 240, 208, 176 },     // 0x36
            new byte[] { 252, 224, 168 },     // 0x37
            new byte[] { 248, 216, 120 },     // 0x38
            new byte[] { 216, 248, 120 },     // 0x39
            new byte[] { 184, 248, 184 },     // 0x3A
            new byte[] { 184, 248, 216 },     // 0x3B
            new byte[] { 0, 252, 252 },       // 0x3C
            new byte[] { 216, 216, 216 },     // 0x3D
            new byte[] { 0, 0, 0 },           // 0x3E
            new byte[] { 0, 0, 0 }            // 0x3F
        };
    }
}
