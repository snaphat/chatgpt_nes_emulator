namespace Emulation
{
    public static class Globals
    {
        // Header constants
        public const int HORIZONTAL_MIRRORING = 0;
        public const int VERTICAL_MIRRORING = 1;

        // PPU Constants
        public const int ATTRIBUTE_TABLE_START = 0x23C0; // Address of the attribute table
        public const int PATTERN_TABLE_0_START = 0x0000;
        public const int PATTERN_TABLE_0_END = 0x1000;
        public const int PATTERN_TABLE_1_START = 0x1000;
        public const int PATTERN_TABLE_1_END = 0x2000;
        public const int NAME_TABLE_0_START = 0x2000;
        public const int NAME_TABLE_0_END = 0x23FF;
        public const int NAME_TABLE_1_START = 0x2400;
        public const int NAME_TABLE_1_END = 0x27FF;
        public const int NAME_TABLE_2_START = 0x2800;
        public const int NAME_TABLE_2_END = 0x2BFF;
        public const int NAME_TABLE_3_START = 0x2C00;
        public const int NAME_TABLE_3_END = 0x2FFF;
        public const int PALETTE_TABLE_START = 0x3F00;
        public const int PALETTE_TABLE_SPRITE_START = 0x3F10;
        public const int PALETTE_TABLE_END = 0x3F20;

        public const int OAM_SIZE = 0x1000;
        public const int PATTERN_TABLE_SIZE = 0x1000;
        public const int NAME_TABLE_SIZE = 0x0400;
        public const int PALETTE_RAM_SIZE = 0x20;

        // Width of the screen in pixels
        public const int SCREEN_WIDTH = 256;

        // Height of the screen in pixels
        public const int SCREEN_HEIGHT = 240;

        public const int DOTS_PER_SCANLINE = 341;
        public const int SCANLINES_PER_FRAME = 262;
        public const int VBLANK_START_SCANLINE = 241;

        // PPUCONTROL Flags
        public const int VRAM_ADDRESS_INCREMENT_FLAG = 1 << 2;
        public const int SPRITE_PATTERN_TABLE_ADDRESS_FLAG = 1 << 3;
        public const int BACKGROUND_PATTERN_TABLE_ADDRESS_FLAG = 1 << 4;
        public const int SPRITE_SIZE_FLAG = 1 << 5;
        public const int GENERATE_NMI_FLAG = 1 << 7;

        // PPUMASK Flags
        public const int SHOW_BACKGROUND_IN_LEFTMOST_8_PIXELS = 1 << 1;
        public const int SHOW_SPRITES_IN_LEFTMOST_8_PIXELS = 1 << 2;
        public const int SHOW_BACKGROUND = 1 << 3;
        public const int SHOW_SPRITES = 1 << 4;

        // PPUSTATUS Flags
        public const int SPRITE0_HIT_FLAG = 1 << 6;
        public const int IN_VBLANK_FLAG = 1 << 7;

        // OAM Attribute Flags
        public const int SPRITE_PALETTE = 0x3;
        public const int SPRITE_PRIORITY_FLAG = 1 << 5;
        public const int FLIP_SPRITE_HORIZONTALLY_FLAG = 1 << 6;
        public const int FLIP_SPRITE_VERTICALLY_FLAG = 1 << 7;
    }
}
