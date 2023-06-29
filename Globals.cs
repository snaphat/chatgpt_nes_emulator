namespace Emulation
{
    public class Globals
    {
        // Header constants
        public const int HORIZONTAL_MIRRORING = 0;
        public const int VERTICAL_MIRRORING = 1;

        // CPU Constants
        public const int BREAK_FLAG = 0x10;
        public const int ALWAYS_HIGH_FLAG = 0x20;

        // PPU Constants
        public const int PATTERN_TABLE_0_BASE_ADDRESS = 0x0000; // Address of the first name table
        public const int PATTERN_TABLE_1_BASE_ADDRESS = 0x1000; // Address of the first name table
        public const int NAME_TABLE_0_BASE_ADDRESS = 0x2000; // Address of the first name table
        public const int NAME_TABLE_1_BASE_ADDRESS = 0x2400; // Address of the second name table
        public const int NAME_TABLE_2_BASE_ADDRESS = 0x2800; // Address of the third name table
        public const int NAME_TABLE_3_BASE_ADDRESS = 0x2c00; // Address of the fourth name table
        public const int ATTRIBUTE_TABLE_BASE_ADDRESS = 0x23C0; // Address of the attribute table
        public const int PALETTE_TABLE_BASE_ADDRESS = 0x3F00;
        public const int PALETTE_TABLE_SPRITE_BASE_ADDRESS = PALETTE_TABLE_BASE_ADDRESS + 0x10;

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
        public const byte VRAM_ADDRESS_INCREMENT_FLAG = 1 << 2;
        public const byte SPRITE_PATTERN_TABLE_ADDRESS_FLAG = 1 << 3;
        public const byte BACKGROUND_PATTERN_TABLE_ADDRESS_FLAG = 1 << 4;
        public const byte SPRITE_SIZE_FLAG = 1 << 5;
        public const byte GENERATE_NMI_FLAG = 1 << 7;

        // PPUMASK Flags
        public const byte SHOW_BACKGROUND_IN_LEFTMOST_8_PIXELS = 1 << 1;
        public const byte SHOW_SPRITES_IN_LEFTMOST_8_PIXELS = 1 << 2;
        public const byte SHOW_BACKGROUND = 1 << 3;
        public const byte SHOW_SPRITES = 1 << 4;

        // PPUSTATUS Flags
        public const byte SPRITE0_HIT_FLAG = 1 << 6;
        public const byte IN_VBLANK_FLAG = 1 << 7;

        // OAM Attribute Flags
        public const byte SPRITE_PRIORITY_FLAG = 1 << 5;
        public const byte FLIP_SPRITE_HORIZONTALLY_FLAG = 1 << 6;
        public const byte FLIP_SPRITE_VERTICALLY_FLAG = 1 << 7;
    }
}
