namespace Emulation
{
    using UnityEngine;
    public static class ColorMap
    {
        // NES 2C02 Color Palette Lookup Table (NES colors mapped to 24-bit RGB colors)
        public static Color32[] LUT = new Color32[]
        {
            new Color32(98, 98, 98, 255),    // 0x00
            new Color32(13, 34, 107, 255),   // 0x01
            new Color32(36, 20, 118, 255),   // 0x02
            new Color32(59, 10, 107, 255),   // 0x03
            new Color32(76, 7, 77, 255),     // 0x04
            new Color32(82, 12, 36, 255),    // 0x05
            new Color32(76, 23, 0, 255),     // 0x06
            new Color32(59, 38, 0, 255),     // 0x07
            new Color32(36, 52, 0, 255),     // 0x08
            new Color32(13, 61, 0, 255),     // 0x09
            new Color32(0, 64, 0, 255),      // 0x0A
            new Color32(0, 59, 36, 255),     // 0x0B
            new Color32(0, 48, 77, 255),     // 0x0C
            new Color32(0, 0, 0, 255),       // 0x0D
            new Color32(0, 0, 0, 255),       // 0x0E
            new Color32(0, 0, 0, 255),       // 0x0F
            new Color32(171, 171, 171, 255), // 0x10
            new Color32(49, 86, 177, 255),   // 0x11
            new Color32(80, 67, 197, 255),   // 0x12
            new Color32(112, 52, 187, 255),  // 0x13
            new Color32(137, 47, 149, 255),  // 0x14
            new Color32(148, 52, 95, 255),   // 0x15
            new Color32(142, 66, 38, 255),   // 0x16
            new Color32(121, 85, 0, 255),    // 0x17
            new Color32(91, 104, 0, 255),    // 0x18
            new Color32(59, 119, 0, 255),    // 0x19
            new Color32(34, 124, 21, 255),   // 0x1A
            new Color32(23, 119, 76, 255),   // 0x1B
            new Color32(29, 105, 133, 255),  // 0x1C
            new Color32(0, 0, 0, 255),       // 0x1D
            new Color32(0, 0, 0, 255),       // 0x1E
            new Color32(0, 0, 0, 255),       // 0x1F
            new Color32(255, 255, 255, 255), // 0x20
            new Color32(124, 170, 255, 255), // 0x21
            new Color32(155, 150, 255, 255), // 0x22
            new Color32(189, 134, 255, 255), // 0x23
            new Color32(216, 126, 241, 255), // 0x24
            new Color32(230, 130, 186, 255), // 0x25
            new Color32(227, 143, 127, 255), // 0x26
            new Color32(208, 162, 78, 255),  // 0x27
            new Color32(178, 183, 52, 255),  // 0x28
            new Color32(144, 199, 57, 255),  // 0x29
            new Color32(116, 206, 92, 255),  // 0x2A
            new Color32(102, 203, 146, 255), // 0x2B
            new Color32(105, 190, 206, 255), // 0x2C
            new Color32(78, 78, 78, 255),    // 0x2D
            new Color32(0, 0, 0, 255),       // 0x2E
            new Color32(0, 0, 0, 255),       // 0x2F
            new Color32(255, 255, 255, 255), // 0x30
            new Color32(201, 222, 252, 255), // 0x31
            new Color32(213, 214, 255, 255), // 0x32
            new Color32(226, 207, 255, 255), // 0x33
            new Color32(238, 204, 252, 255), // 0x34
            new Color32(245, 204, 231, 255), // 0x35
            new Color32(245, 209, 207, 255), // 0x36
            new Color32(238, 216, 187, 255), // 0x37
            new Color32(226, 225, 174, 255), // 0x38
            new Color32(213, 232, 174, 255), // 0x39
            new Color32(201, 235, 187, 255), // 0x3A
            new Color32(194, 235, 207, 255), // 0x3B
            new Color32(194, 230, 231, 255), // 0x3C
            new Color32(184, 184, 184, 255), // 0x3D
            new Color32(0, 0, 0, 255),       // 0x3E
            new Color32(0, 0, 0, 255)        // 0x3F
        };
    }
}
