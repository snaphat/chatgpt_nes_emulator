namespace Emulation
{
    using UnityEngine;
    public static class ColorMap
    {
        // NES Color Palette Lookup Table (NES colors mapped to 24-bit RGB colors)
        public static Color32[] LUT = new Color32[]
        {
            new Color32 ( 124, 124, 124 , 255),     // 0x00
            new Color32 ( 0, 0, 252 , 255),         // 0x01
            new Color32 ( 0, 0, 188 , 255),         // 0x02
            new Color32 ( 68, 40, 188 , 255),       // 0x03
            new Color32 ( 148, 0, 132 , 255),       // 0x04
            new Color32 ( 168, 0, 32 , 255),        // 0x05
            new Color32 ( 168, 16, 0 , 255),        // 0x06
            new Color32 ( 136, 20, 0 , 255),        // 0x07
            new Color32 ( 80, 48, 0 , 255),         // 0x08
            new Color32 ( 0, 120, 0 , 255),         // 0x09
            new Color32 ( 0, 104, 0 , 255),         // 0x0A
            new Color32 ( 0, 88, 0 , 255),          // 0x0B
            new Color32 ( 0, 64, 88 , 255),         // 0x0C
            new Color32 ( 0, 0, 0 , 255),           // 0x0D
            new Color32 ( 0, 0, 0 , 255),           // 0x0E
            new Color32 ( 0, 0, 0 , 255),           // 0x0F
            new Color32 ( 188, 188, 188 , 255),     // 0x10
            new Color32 ( 0, 120, 248 , 255),       // 0x11
            new Color32 ( 0, 88, 248 , 255),        // 0x12
            new Color32 ( 104, 68, 252 , 255),      // 0x13
            new Color32 ( 216, 0, 204 , 255),       // 0x14
            new Color32 ( 228, 0, 88 , 255),        // 0x15
            new Color32 ( 248, 56, 0 , 255),        // 0x16
            new Color32 ( 228, 92, 16 , 255),       // 0x17
            new Color32 ( 172, 124, 0 , 255),       // 0x18
            new Color32 ( 0, 184, 0 , 255),         // 0x19
            new Color32 ( 0, 168, 0 , 255),         // 0x1A
            new Color32 ( 0, 168, 68 , 255),        // 0x1B
            new Color32 ( 0, 136, 136 , 255),       // 0x1C
            new Color32 ( 0, 0, 0 , 255),           // 0x1D
            new Color32 ( 0, 0, 0 , 255),           // 0x1E
            new Color32 ( 0, 0, 0 , 255),           // 0x1F
            new Color32 ( 248, 248, 248 , 255),     // 0x20
            new Color32 ( 60, 188, 252 , 255),      // 0x21
            new Color32 ( 104, 136, 252 , 255),     // 0x22
            new Color32 ( 152, 120, 248 , 255),     // 0x23
            new Color32 ( 248, 120, 248 , 255),     // 0x24
            new Color32 ( 248, 88, 152 , 255),      // 0x25
            new Color32 ( 248, 120, 88 , 255),      // 0x26
            new Color32 ( 252, 160, 68 , 255),      // 0x27
            new Color32 ( 248, 184, 0 , 255),       // 0x28
            new Color32 ( 184, 248, 24 , 255),      // 0x29
            new Color32 ( 88, 216, 84 , 255),       // 0x2A
            new Color32 ( 88, 248, 152 , 255),      // 0x2B
            new Color32 ( 0, 232, 216 , 255),       // 0x2C
            new Color32 ( 120, 120, 120 , 255),     // 0x2D
            new Color32 ( 0, 0, 0 , 255),           // 0x2E
            new Color32 ( 0, 0, 0 , 255),           // 0x2F
            new Color32 ( 252, 252, 252 , 255),     // 0x30
            new Color32 ( 164, 228, 252 , 255),     // 0x31
            new Color32 ( 184, 184, 248 , 255),     // 0x32
            new Color32 ( 216, 184, 248 , 255),     // 0x33
            new Color32 ( 248, 184, 248 , 255),     // 0x34
            new Color32 ( 248, 164, 192 , 255),     // 0x35
            new Color32 ( 240, 208, 176 , 255),     // 0x36
            new Color32 ( 252, 224, 168 , 255),     // 0x37
            new Color32 ( 248, 216, 120 , 255),     // 0x38
            new Color32 ( 216, 248, 120 , 255),     // 0x39
            new Color32 ( 184, 248, 184 , 255),     // 0x3A
            new Color32 ( 184, 248, 216 , 255),     // 0x3B
            new Color32 ( 0, 252, 252 , 255),       // 0x3C
            new Color32 ( 216, 216, 216 , 255),     // 0x3D
            new Color32 ( 0, 0, 0 , 255),           // 0x3E
            new Color32 ( 0, 0, 0 , 255)            // 0x3F
        };
    }
}
