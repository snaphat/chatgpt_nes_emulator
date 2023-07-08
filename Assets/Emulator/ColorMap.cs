namespace Emulation
{
    using UnityEngine;
    public static class ColorMap
    {
        // NES Color Palette Lookup Tables (NES colors mapped to 24-bit RGB colors)
        // Emphasis bits in BGR order (same as PPUMask)
        // Generated using https://github.com/Gumball2415/palgen-persune
        //    `palgen-persune.py -r -rfc "ITU-R BT.709" -phs -5.0 -blp 0.0 -sat 1.0 -e --html-hex`

        // NES palette (emphasis = 000) - No Emphasis
        public static Color32[] LUT_000 = new Color32[]
        {
            new Color32(98, 98, 98, 255),    // 0x00
            new Color32(0, 31, 178, 255),    // 0x01
            new Color32(36, 4, 200, 255),    // 0x02
            new Color32(82, 0, 178, 255),    // 0x03
            new Color32(115, 0, 118, 255),   // 0x04
            new Color32(128, 0, 36, 255),    // 0x05
            new Color32(115, 11, 0, 255),    // 0x06
            new Color32(82, 40, 0, 255),     // 0x07
            new Color32(36, 67, 0, 255),     // 0x08
            new Color32(0, 87, 0, 255),      // 0x09
            new Color32(0, 92, 0, 255),      // 0x0A
            new Color32(0, 83, 36, 255),     // 0x0B
            new Color32(0, 60, 118, 255),    // 0x0C
            new Color32(0, 0, 0, 255),       // 0x0D
            new Color32(0, 0, 0, 255),       // 0x0E
            new Color32(0, 0, 0, 255),       // 0x0F

            new Color32(171, 171, 171, 255), // 0x10
            new Color32(13, 87, 255, 255),   // 0x11
            new Color32(74, 48, 255, 255),   // 0x12
            new Color32(138, 19, 255, 255),  // 0x13
            new Color32(188, 8, 214, 255),   // 0x14
            new Color32(210, 18, 105, 255),  // 0x15
            new Color32(199, 46, 0, 255),    // 0x16
            new Color32(157, 84, 0, 255),    // 0x17
            new Color32(96, 123, 0, 255),    // 0x18
            new Color32(32, 152, 0, 255),    // 0x19
            new Color32(0, 163, 0, 255),     // 0x1A
            new Color32(0, 153, 66, 255),    // 0x1B
            new Color32(0, 125, 180, 255),   // 0x1C
            new Color32(0, 0, 0, 255),       // 0x1D
            new Color32(0, 0, 0, 255),       // 0x1E
            new Color32(0, 0, 0, 255),       // 0x1F

            new Color32(255, 255, 255, 255), // 0x20
            new Color32(83, 174, 255, 255),  // 0x21
            new Color32(144, 134, 255, 255), // 0x22
            new Color32(211, 101, 255, 255), // 0x23
            new Color32(255, 87, 255, 255),  // 0x24
            new Color32(255, 93, 207, 255),  // 0x25
            new Color32(255, 119, 87, 255),  // 0x26
            new Color32(250, 158, 0, 255),   // 0x27
            new Color32(189, 199, 0, 255),   // 0x28
            new Color32(122, 231, 0, 255),   // 0x29
            new Color32(67, 246, 17, 255),   // 0x2A
            new Color32(38, 239, 126, 255),  // 0x2B
            new Color32(44, 213, 246, 255),  // 0x2C
            new Color32(78, 78, 78, 255),    // 0x2D
            new Color32(0, 0, 0, 255),       // 0x2E
            new Color32(0, 0, 0, 255),       // 0x2F

            new Color32(255, 255, 255, 255), // 0x30
            new Color32(182, 225, 255, 255), // 0x31
            new Color32(206, 209, 255, 255), // 0x32
            new Color32(233, 195, 255, 255), // 0x33
            new Color32(255, 188, 255, 255), // 0x34
            new Color32(255, 189, 243, 255), // 0x35
            new Color32(255, 198, 195, 255), // 0x36
            new Color32(255, 213, 154, 255), // 0x37
            new Color32(233, 230, 130, 255), // 0x38
            new Color32(206, 244, 130, 255), // 0x39
            new Color32(182, 251, 154, 255), // 0x3A
            new Color32(169, 250, 195, 255), // 0x3B
            new Color32(169, 240, 243, 255), // 0x3C
            new Color32(184, 184, 184, 255), // 0x3D
            new Color32(0, 0, 0, 255),       // 0x3E
            new Color32(0, 0, 0, 255)        // 0x3F
        };

        // NES palette (emphasis = 001) - Red Emphasis
        public static Color32[] LUT_001 = new Color32[]
        {
            new Color32(103, 72, 55, 255),   // 0x00
            new Color32(0, 10, 130, 255),    // 0x01
            new Color32(37, 0, 152, 255),    // 0x02
            new Color32(80, 0, 135, 255),    // 0x03
            new Color32(113, 0, 83, 255),    // 0x04
            new Color32(126, 0, 12, 255),    // 0x05
            new Color32(117, 3, 0, 255),     // 0x06
            new Color32(85, 26, 0, 255),     // 0x07
            new Color32(41, 49, 0, 255),     // 0x08
            new Color32(0, 64, 0, 255),      // 0x09
            new Color32(0, 67, 0, 255),      // 0x0A
            new Color32(0, 56, 3, 255),      // 0x0B
            new Color32(0, 34, 74, 255),     // 0x0C
            new Color32(0, 0, 0, 255),       // 0x0D
            new Color32(0, 0, 0, 255),       // 0x0E
            new Color32(0, 0, 0, 255),       // 0x0F

            new Color32(180, 132, 115, 255), // 0x10
            new Color32(21, 55, 206, 255),   // 0x11
            new Color32(79, 23, 245, 255),   // 0x12
            new Color32(139, 0, 230, 255),   // 0x13
            new Color32(186, 0, 166, 255),   // 0x14
            new Color32(209, 5, 71, 255),    // 0x15
            new Color32(202, 33, 0, 255),    // 0x16
            new Color32(162, 64, 0, 255),    // 0x17
            new Color32(104, 96, 0, 255),    // 0x18
            new Color32(44, 119, 0, 255),    // 0x19
            new Color32(0, 126, 0, 255),     // 0x1A
            new Color32(0, 114, 25, 255),    // 0x1B
            new Color32(0, 86, 124, 255),    // 0x1C
            new Color32(3, 0, 0, 255),       // 0x1D
            new Color32(0, 0, 0, 255),       // 0x1E
            new Color32(0, 0, 0, 255),       // 0x1F

            new Color32(255, 205, 192, 255), // 0x20
            new Color32(94, 130, 255, 255),  // 0x21
            new Color32(153, 96, 255, 255),  // 0x22
            new Color32(216, 69, 255, 255),  // 0x23
            new Color32(255, 58, 255, 255),  // 0x24
            new Color32(255, 67, 161, 255),  // 0x25
            new Color32(255, 94, 55, 255),   // 0x26
            new Color32(255, 126, 0, 255),   // 0x27
            new Color32(199, 161, 0, 255),   // 0x28
            new Color32(135, 187, 0, 255),   // 0x29
            new Color32(82, 198, 0, 255),    // 0x2A
            new Color32(54, 189, 77, 255),   // 0x2B
            new Color32(57, 163, 183, 255),  // 0x2C
            new Color32(84, 52, 46, 255),    // 0x2D
            new Color32(0, 0, 0, 255),       // 0x2E
            new Color32(0, 0, 0, 255),       // 0x2F

            new Color32(255, 202, 200, 255), // 0x30
            new Color32(196, 175, 227, 255), // 0x31
            new Color32(219, 161, 251, 255), // 0x32
            new Color32(244, 149, 251, 255), // 0x33
            new Color32(255, 143, 230, 255), // 0x34
            new Color32(255, 146, 193, 255), // 0x35
            new Color32(255, 155, 150, 255), // 0x36
            new Color32(255, 168, 110, 255), // 0x37
            new Color32(245, 182, 87, 255),  // 0x38
            new Color32(220, 194, 86, 255),  // 0x39
            new Color32(197, 200, 108, 255), // 0x3A
            new Color32(184, 197, 145, 255), // 0x3B
            new Color32(183, 188, 188, 255), // 0x3C
            new Color32(195, 141, 138, 255), // 0x3D
            new Color32(0, 0, 0, 255),       // 0x3E
            new Color32(0, 0, 0, 255)        // 0x3F
        };

        // NES palette (emphasis = 010) - Green Emphasis
        public static Color32[] LUT_010 = new Color32[]
        {
            new Color32(56, 97, 55, 255),    // 0x00
            new Color32(0, 31, 135, 255),    // 0x01
            new Color32(6, 4, 152, 255),     // 0x02
            new Color32(45, 0, 130, 255),    // 0x03
            new Color32(73, 0, 74, 255),     // 0x04
            new Color32(86, 0, 3, 255),      // 0x05
            new Color32(77, 10, 0, 255),     // 0x06
            new Color32(49, 38, 0, 255),     // 0x07
            new Color32(10, 65, 0, 255),     // 0x08
            new Color32(0, 85, 0, 255),      // 0x09
            new Color32(0, 92, 0, 255),      // 0x0A
            new Color32(0, 81, 12, 255),     // 0x0B
            new Color32(0, 59, 83, 255),     // 0x0C
            new Color32(0, 0, 0, 255),       // 0x0D
            new Color32(0, 0, 0, 255),       // 0x0E
            new Color32(0, 0, 0, 255),       // 0x0F

            new Color32(112, 168, 105, 255), // 0x10
            new Color32(0, 85, 205, 255),    // 0x11
            new Color32(34, 47, 238, 255),   // 0x12
            new Color32(88, 18, 217, 255),   // 0x13
            new Color32(130, 6, 147, 255),   // 0x14
            new Color32(153, 17, 53, 255),   // 0x15
            new Color32(145, 45, 0, 255),    // 0x16
            new Color32(111, 83, 0, 255),    // 0x17
            new Color32(59, 120, 0, 255),    // 0x18
            new Color32(4, 149, 0, 255),     // 0x19
            new Color32(0, 162, 0, 255),     // 0x1A
            new Color32(0, 150, 29, 255),    // 0x1B
            new Color32(0, 122, 129, 255),   // 0x1C
            new Color32(0, 0, 0, 255),       // 0x1D
            new Color32(0, 0, 0, 255),       // 0x1E
            new Color32(0, 0, 0, 255),       // 0x1F

            new Color32(185, 252, 167, 255), // 0x20
            new Color32(37, 171, 255, 255),  // 0x21
            new Color32(90, 131, 255, 255),  // 0x22
            new Color32(148, 99, 255, 255),  // 0x23
            new Color32(196, 83, 227, 255),  // 0x24
            new Color32(225, 92, 131, 255),  // 0x25
            new Color32(222, 118, 25, 255),  // 0x26
            new Color32(189, 156, 0, 255),   // 0x27
            new Color32(137, 196, 0, 255),   // 0x28
            new Color32(78, 228, 0, 255),    // 0x29
            new Color32(31, 244, 0, 255),    // 0x2A
            new Color32(2, 236, 68, 255),    // 0x2B
            new Color32(5, 209, 174, 255),   // 0x2C
            new Color32(42, 76, 33, 255),    // 0x2D
            new Color32(0, 0, 0, 255),       // 0x2E
            new Color32(0, 0, 0, 255),       // 0x2F

            new Color32(188, 251, 161, 255), // 0x30
            new Color32(124, 222, 194, 255), // 0x31
            new Color32(145, 205, 215, 255), // 0x32
            new Color32(169, 192, 214, 255), // 0x33
            new Color32(189, 184, 191, 255), // 0x34
            new Color32(202, 186, 154, 255), // 0x35
            new Color32(203, 196, 111, 255), // 0x36
            new Color32(192, 210, 73, 255),  // 0x37
            new Color32(171, 227, 52, 255),  // 0x38
            new Color32(148, 240, 53, 255),  // 0x39
            new Color32(127, 248, 76, 255),  // 0x3A
            new Color32(114, 246, 113, 255), // 0x3B
            new Color32(113, 236, 157, 255), // 0x3C
            new Color32(128, 181, 106, 255), // 0x3D
            new Color32(0, 0, 0, 255),       // 0x3E
            new Color32(0, 0, 0, 255)        // 0x3F
        };

        // NES palette (emphasis = 011) - Green Red Emphasis
        public static Color32[] LUT_011 = new Color32[]
        {
            new Color32(67, 72, 43, 255),    // 0x00
            new Color32(0, 11, 118, 255),    // 0x01
            new Color32(13, 0, 136, 255),    // 0x02
            new Color32(50, 0, 118, 255),    // 0x03
            new Color32(76, 0, 70, 255),     // 0x04
            new Color32(90, 0, 0, 255),      // 0x05
            new Color32(81, 2, 0, 255),      // 0x06
            new Color32(54, 25, 0, 255),     // 0x07
            new Color32(17, 47, 0, 255),     // 0x08
            new Color32(0, 63, 0, 255),      // 0x09
            new Color32(0, 67, 0, 255),      // 0x0A
            new Color32(0, 56, 0, 255),      // 0x0B
            new Color32(0, 34, 70, 255),     // 0x0C
            new Color32(0, 0, 0, 255),       // 0x0D
            new Color32(0, 0, 0, 255),       // 0x0E
            new Color32(0, 0, 0, 255),       // 0x0F

            new Color32(128, 132, 92, 255),  // 0x10
            new Color32(0, 56, 187, 255),    // 0x11
            new Color32(45, 25, 218, 255),   // 0x12
            new Color32(96, 2, 202, 255),    // 0x13
            new Color32(135, 0, 143, 255),   // 0x14
            new Color32(158, 5, 48, 255),    // 0x15
            new Color32(151, 33, 0, 255),    // 0x16
            new Color32(118, 64, 0, 255),    // 0x17
            new Color32(69, 94, 0, 255),     // 0x18
            new Color32(18, 117, 0, 255),    // 0x19
            new Color32(0, 126, 0, 255),     // 0x1A
            new Color32(0, 114, 17, 255),    // 0x1B
            new Color32(0, 86, 116, 255),    // 0x1C
            new Color32(0, 0, 0, 255),       // 0x1D
            new Color32(0, 0, 0, 255),       // 0x1E
            new Color32(0, 0, 0, 255),       // 0x1F

            new Color32(204, 206, 157, 255), // 0x20
            new Color32(56, 132, 246, 255),  // 0x21
            new Color32(106, 98, 255, 255),  // 0x22
            new Color32(161, 72, 255, 255),  // 0x23
            new Color32(206, 60, 222, 255),  // 0x24
            new Color32(234, 69, 126, 255),  // 0x25
            new Color32(232, 95, 20, 255),   // 0x26
            new Color32(200, 127, 0, 255),   // 0x27
            new Color32(150, 160, 0, 255),   // 0x28
            new Color32(95, 187, 0, 255),    // 0x29
            new Color32(50, 199, 0, 255),    // 0x2A
            new Color32(22, 190, 59, 255),   // 0x2B
            new Color32(24, 164, 165, 255),  // 0x2C
            new Color32(52, 53, 28, 255),    // 0x2D
            new Color32(0, 0, 0, 255),       // 0x2E
            new Color32(0, 0, 0, 255),       // 0x2F

            new Color32(206, 205, 158, 255), // 0x30
            new Color32(143, 177, 189, 255), // 0x31
            new Color32(162, 163, 209, 255), // 0x32
            new Color32(185, 152, 209, 255), // 0x33
            new Color32(204, 146, 189, 255), // 0x34
            new Color32(218, 148, 152, 255), // 0x35
            new Color32(219, 158, 108, 255), // 0x36
            new Color32(207, 170, 73, 255),  // 0x37
            new Color32(188, 184, 53, 255),  // 0x38
            new Color32(165, 195, 53, 255),  // 0x39
            new Color32(146, 202, 73, 255),  // 0x3A
            new Color32(132, 199, 111, 255), // 0x3B
            new Color32(131, 190, 154, 255), // 0x3C
            new Color32(144, 143, 104, 255), // 0x3D
            new Color32(0, 0, 0, 255),       // 0x3E
            new Color32(0, 0, 0, 255)        // 0x3F
        };

        // NES palette (emphasis = 100) - Blue Emphasis
        public static Color32[] LUT_100 = new Color32[]
        {
            new Color32(80, 70, 129, 255),   // 0x00
            new Color32(0, 18, 187, 255),    // 0x01
            new Color32(30, 0, 209, 255),    // 0x02
            new Color32(69, 0, 187, 255),    // 0x03
            new Color32(97, 0, 131, 255),    // 0x04
            new Color32(106, 0, 55, 255),    // 0x05
            new Color32(93, 0, 0, 255),      // 0x06
            new Color32(60, 16, 0, 255),     // 0x07
            new Color32(17, 39, 0, 255),     // 0x08
            new Color32(0, 59, 0, 255),      // 0x09
            new Color32(0, 66, 0, 255),      // 0x0A
            new Color32(0, 59, 55, 255),     // 0x0B
            new Color32(0, 41, 131, 255),    // 0x0C
            new Color32(0, 0, 0, 255),       // 0x0D
            new Color32(0, 0, 0, 255),       // 0x0E
            new Color32(0, 0, 0, 255),       // 0x0F

            new Color32(141, 133, 213, 255), // 0x10
            new Color32(7, 67, 255, 255),    // 0x11
            new Color32(64, 35, 255, 255),   // 0x12
            new Color32(119, 6, 255, 255),   // 0x13
            new Color32(161, 0, 232, 255),   // 0x14
            new Color32(178, 0, 131, 255),   // 0x15
            new Color32(164, 21, 26, 255),   // 0x16
            new Color32(124, 53, 0, 255),    // 0x17
            new Color32(66, 85, 0, 255),     // 0x18
            new Color32(12, 113, 0, 255),    // 0x19
            new Color32(0, 126, 0, 255),     // 0x1A
            new Color32(0, 120, 97, 255),    // 0x1B
            new Color32(0, 98, 202, 255),    // 0x1C
            new Color32(0, 0, 15, 255),      // 0x1D
            new Color32(0, 0, 0, 255),       // 0x1E
            new Color32(0, 0, 0, 255),       // 0x1F

            new Color32(214, 210, 255, 255), // 0x20
            new Color32(64, 145, 255, 255),  // 0x21
            new Color32(123, 110, 255, 255), // 0x22
            new Color32(181, 79, 255, 255),  // 0x23
            new Color32(228, 62, 255, 255),  // 0x24
            new Color32(252, 66, 242, 255),  // 0x25
            new Color32(244, 87, 131, 255),  // 0x26
            new Color32(206, 119, 39, 255),  // 0x27
            new Color32(148, 154, 0, 255),   // 0x28
            new Color32(89, 186, 2, 255),    // 0x29
            new Color32(42, 202, 67, 255),   // 0x2A
            new Color32(19, 199, 169, 255),  // 0x2B
            new Color32(27, 178, 255, 255),  // 0x2C
            new Color32(57, 55, 104, 255),   // 0x2D
            new Color32(0, 0, 0, 255),       // 0x2E
            new Color32(0, 0, 0, 255),       // 0x2F

            new Color32(209, 212, 255, 255), // 0x30
            new Color32(146, 188, 255, 255), // 0x31
            new Color32(168, 174, 255, 255), // 0x32
            new Color32(192, 160, 255, 255), // 0x33
            new Color32(213, 152, 255, 255), // 0x34
            new Color32(224, 152, 255, 255), // 0x35
            new Color32(223, 160, 242, 255), // 0x36
            new Color32(210, 173, 203, 255), // 0x37
            new Color32(187, 187, 180, 255), // 0x38
            new Color32(164, 201, 181, 255), // 0x39
            new Color32(143, 208, 204, 255), // 0x3A
            new Color32(132, 208, 243, 255), // 0x3B
            new Color32(132, 201, 255, 255), // 0x3C
            new Color32(146, 149, 225, 255), // 0x3D
            new Color32(0, 0, 0, 255),       // 0x3E
            new Color32(0, 0, 0, 255)        // 0x3F
        };

        // NES palette (emphasis = 101) - Blue Red Emphasis
        public static Color32[] LUT_101 = new Color32[]
        {
            new Color32(79, 59, 79, 255),    // 0x00
            new Color32(0, 6, 138, 255),     // 0x01
            new Color32(29, 0, 160, 255),    // 0x02
            new Color32(66, 0, 142, 255),    // 0x03
            new Color32(93, 0, 95, 255),     // 0x04
            new Color32(102, 0, 30, 255),    // 0x05
            new Color32(93, 0, 0, 255),      // 0x06
            new Color32(60, 13, 0, 255),     // 0x07
            new Color32(17, 36, 0, 255),     // 0x08
            new Color32(0, 51, 0, 255),      // 0x09
            new Color32(0, 55, 0, 255),      // 0x0A
            new Color32(0, 48, 17, 255),     // 0x0B
            new Color32(0, 30, 82, 255),     // 0x0C
            new Color32(0, 0, 0, 255),       // 0x0D
            new Color32(0, 0, 0, 255),       // 0x0E
            new Color32(0, 0, 0, 255),       // 0x0F

            new Color32(143, 115, 147, 255), // 0x10
            new Color32(8, 49, 217, 255),    // 0x11
            new Color32(66, 17, 255, 255),   // 0x12
            new Color32(117, 0, 240, 255),   // 0x13
            new Color32(156, 0, 180, 255),   // 0x14
            new Color32(174, 0, 94, 255),    // 0x15
            new Color32(165, 15, 3, 255),    // 0x16
            new Color32(125, 47, 0, 255),    // 0x17
            new Color32(67, 79, 0, 255),     // 0x18
            new Color32(16, 102, 0, 255),    // 0x19
            new Color32(0, 110, 0, 255),     // 0x1A
            new Color32(0, 102, 44, 255),    // 0x1B
            new Color32(0, 80, 135, 255),    // 0x1C
            new Color32(0, 0, 0, 255),       // 0x1D
            new Color32(0, 0, 0, 255),       // 0x1E
            new Color32(0, 0, 0, 255),       // 0x1F

            new Color32(219, 185, 228, 255), // 0x20
            new Color32(69, 120, 255, 255),  // 0x21
            new Color32(128, 86, 255, 255),  // 0x22
            new Color32(182, 60, 255, 255),  // 0x23
            new Color32(228, 47, 255, 255),  // 0x24
            new Color32(251, 53, 188, 255),  // 0x25
            new Color32(246, 74, 90, 255),   // 0x26
            new Color32(208, 107, 0, 255),   // 0x27
            new Color32(150, 141, 0, 255),   // 0x28
            new Color32(95, 168, 0, 255),    // 0x29
            new Color32(50, 180, 14, 255),   // 0x2A
            new Color32(27, 174, 103, 255),  // 0x2B
            new Color32(32, 153, 201, 255),  // 0x2C
            new Color32(59, 42, 64, 255),    // 0x2D
            new Color32(0, 0, 0, 255),       // 0x2E
            new Color32(0, 0, 0, 255),       // 0x2F

            new Color32(217, 185, 230, 255), // 0x30
            new Color32(154, 161, 253, 255), // 0x31
            new Color32(176, 147, 255, 255), // 0x32
            new Color32(199, 135, 255, 255), // 0x33
            new Color32(218, 129, 255, 255), // 0x34
            new Color32(230, 130, 221, 255), // 0x35
            new Color32(230, 138, 181, 255), // 0x36
            new Color32(216, 151, 141, 255), // 0x37
            new Color32(194, 165, 118, 255), // 0x38
            new Color32(171, 177, 118, 255), // 0x39
            new Color32(151, 183, 138, 255), // 0x3A
            new Color32(140, 182, 173, 255), // 0x3B
            new Color32(140, 174, 214, 255), // 0x3C
            new Color32(153, 126, 164, 255), // 0x3D
            new Color32(0, 0, 0, 255),       // 0x3E
            new Color32(0, 0, 0, 255)        // 0x3F
        };

        // NES palette (emphasis = 110) - Blue Green Emphasis
        public static Color32[] LUT_110 = new Color32[]
        {
            new Color32(55, 71, 79, 255),    // 0x00
            new Color32(0, 18, 142, 255),    // 0x01
            new Color32(5, 0, 160, 255),     // 0x02
            new Color32(45, 0, 138, 255),    // 0x03
            new Color32(73, 0, 82, 255),     // 0x04
            new Color32(82, 0, 17, 255),     // 0x05
            new Color32(73, 0, 0, 255),      // 0x06
            new Color32(46, 17, 0, 255),     // 0x07
            new Color32(9, 39, 0, 255),      // 0x08
            new Color32(0, 59, 0, 255),      // 0x09
            new Color32(0, 66, 0, 255),      // 0x0A
            new Color32(0, 58, 30, 255),     // 0x0B
            new Color32(0, 41, 95, 255),     // 0x0C
            new Color32(0, 0, 0, 255),       // 0x0D
            new Color32(0, 0, 0, 255),       // 0x0E
            new Color32(0, 0, 0, 255),       // 0x0F

            new Color32(109, 133, 141, 255), // 0x10
            new Color32(0, 66, 219, 255),    // 0x11
            new Color32(32, 35, 250, 255),   // 0x12
            new Color32(87, 6, 229, 255),    // 0x13
            new Color32(129, 0, 160, 255),   // 0x14
            new Color32(146, 1, 73, 255),    // 0x15
            new Color32(137, 23, 0, 255),    // 0x16
            new Color32(104, 54, 0, 255),    // 0x17
            new Color32(56, 85, 0, 255),     // 0x18
            new Color32(1, 114, 0, 255),     // 0x19
            new Color32(0, 126, 0, 255),     // 0x1A
            new Color32(0, 118, 58, 255),    // 0x1B
            new Color32(0, 96, 148, 255),    // 0x1C
            new Color32(0, 0, 0, 255),       // 0x1D
            new Color32(0, 0, 0, 255),       // 0x1E
            new Color32(0, 0, 0, 255),       // 0x1F

            new Color32(177, 209, 215, 255), // 0x20
            new Color32(36, 143, 255, 255),  // 0x21
            new Color32(86, 109, 255, 255),  // 0x22
            new Color32(145, 77, 255, 255),  // 0x23
            new Color32(192, 61, 251, 255),  // 0x24
            new Color32(215, 67, 163, 255),  // 0x25
            new Color32(211, 88, 65, 255),   // 0x26
            new Color32(179, 120, 0, 255),   // 0x27
            new Color32(129, 153, 0, 255),   // 0x28
            new Color32(71, 185, 0, 255),    // 0x29
            new Color32(23, 201, 20, 255),   // 0x2A
            new Color32(0, 196, 109, 255),   // 0x2B
            new Color32(5, 175, 207, 255),   // 0x2C
            new Color32(38, 54, 57, 255),    // 0x2D
            new Color32(0, 0, 0, 255),       // 0x2E
            new Color32(0, 0, 0, 255),       // 0x2F

            new Color32(177, 210, 211, 255), // 0x30
            new Color32(116, 185, 236, 255), // 0x31
            new Color32(135, 171, 255, 255), // 0x32
            new Color32(159, 158, 255, 255), // 0x33
            new Color32(180, 150, 233, 255), // 0x34
            new Color32(191, 151, 198, 255), // 0x35
            new Color32(191, 159, 157, 255), // 0x36
            new Color32(180, 171, 122, 255), // 0x37
            new Color32(160, 185, 102, 255), // 0x38
            new Color32(136, 199, 103, 255), // 0x39
            new Color32(116, 206, 126, 255), // 0x3A
            new Color32(105, 205, 161, 255), // 0x3B
            new Color32(105, 198, 202, 255), // 0x3C
            new Color32(119, 147, 148, 255), // 0x3D
            new Color32(0, 0, 0, 255),       // 0x3E
            new Color32(0, 0, 0, 255)        // 0x3F
        };

        // NES palette (emphasis = 111) - Blue Green Red Emphasis
        public static Color32[] LUT_111 = new Color32[]
        {
            new Color32(61, 61, 61, 255),    // 0x00
            new Color32(0, 8, 124, 255),     // 0x01
            new Color32(11, 0, 141, 255),    // 0x02
            new Color32(48, 0, 124, 255),    // 0x03
            new Color32(74, 0, 76, 255),     // 0x04
            new Color32(84, 0, 11, 255),     // 0x05
            new Color32(74, 0, 0, 255),      // 0x06
            new Color32(48, 14, 0, 255),     // 0x07
            new Color32(11, 36, 0, 255),     // 0x08
            new Color32(0, 52, 0, 255),      // 0x09
            new Color32(0, 56, 0, 255),      // 0x0A
            new Color32(0, 48, 11, 255),     // 0x0B
            new Color32(0, 31, 76, 255),     // 0x0C
            new Color32(0, 0, 0, 255),       // 0x0D
            new Color32(0, 0, 0, 255),       // 0x0E
            new Color32(0, 0, 0, 255),       // 0x0F

            new Color32(118, 118, 118, 255), // 0x10
            new Color32(0, 51, 195, 255),    // 0x11
            new Color32(41, 20, 227, 255),   // 0x12
            new Color32(92, 0, 211, 255),    // 0x13
            new Color32(132, 0, 152, 255),   // 0x14
            new Color32(149, 0, 65, 255),    // 0x15
            new Color32(140, 18, 0, 255),    // 0x16
            new Color32(107, 49, 0, 255),    // 0x17
            new Color32(59, 80, 0, 255),     // 0x18
            new Color32(8, 103, 0, 255),     // 0x19
            new Color32(0, 111, 0, 255),     // 0x1A
            new Color32(0, 103, 34, 255),    // 0x1B
            new Color32(0, 81, 125, 255),    // 0x1C
            new Color32(0, 0, 0, 255),       // 0x1D
            new Color32(0, 0, 0, 255),       // 0x1E
            new Color32(0, 0, 0, 255),       // 0x1F

            new Color32(189, 189, 189, 255), // 0x20
            new Color32(48, 123, 255, 255),  // 0x21
            new Color32(98, 90, 255, 255),   // 0x22
            new Color32(153, 63, 255, 255),  // 0x23
            new Color32(198, 51, 238, 255),  // 0x24
            new Color32(221, 57, 149, 255),  // 0x25
            new Color32(217, 78, 52, 255),   // 0x26
            new Color32(185, 110, 0, 255),   // 0x27
            new Color32(135, 143, 0, 255),   // 0x28
            new Color32(80, 170, 0, 255),    // 0x29
            new Color32(35, 182, 0, 255),    // 0x2A
            new Color32(12, 176, 84, 255),   // 0x2B
            new Color32(16, 155, 181, 255),  // 0x2C
            new Color32(44, 44, 44, 255),    // 0x2D
            new Color32(0, 0, 0, 255),       // 0x2E
            new Color32(0, 0, 0, 255),       // 0x2F

            new Color32(189, 189, 189, 255), // 0x30
            new Color32(128, 164, 214, 255), // 0x31
            new Color32(148, 150, 234, 255), // 0x32
            new Color32(171, 139, 234, 255), // 0x33
            new Color32(190, 133, 214, 255), // 0x34
            new Color32(201, 134, 179, 255), // 0x35
            new Color32(201, 142, 139, 255), // 0x36
            new Color32(190, 154, 104, 255), // 0x37
            new Color32(171, 168, 84, 255),  // 0x38
            new Color32(148, 180, 84, 255),  // 0x39
            new Color32(128, 186, 104, 255), // 0x3A
            new Color32(117, 185, 139, 255), // 0x3B
            new Color32(117, 177, 179, 255), // 0x3C
            new Color32(129, 129, 129, 255), // 0x3D
            new Color32(0, 0, 0, 255),       // 0x3E
            new Color32(0, 0, 0, 255)        // 0x3F
        };
    }
}
