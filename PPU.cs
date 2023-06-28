using System.Reflection.Metadata.Ecma335;

namespace Emulation
{
    public class PPU
    {
        const int PATTERN_TABLE_0_BASE_ADDRESS = 0x0000; // Address of the first name table
        const int PATTERN_TABLE_1_BASE_ADDRESS = 0x1000; // Address of the first name table
        const int NAME_TABLE_0_BASE_ADDRESS = 0x2000; // Address of the first name table
        const int NAME_TABLE_1_BASE_ADDRESS = 0x2400; // Address of the second name table
        const int NAME_TABLE_2_BASE_ADDRESS = 0x2800; // Address of the third name table
        const int NAME_TABLE_3_BASE_ADDRESS = 0x2c00; // Address of the fourth name table
        const int ATTRIBUTE_TABLE_BASE_ADDRESS = 0x23C0; // Address of the attribute table
        const int PALETTE_TABLE_BASE_ADDRESS = 0x3F00;

        const int OAM_SIZE = 0x1000;
        const int PATTERN_TABLE_SIZE = 0x1000;
        const int NAME_TABLE_SIZE = 0x0400;
        const int PALETTE_RAM_SIZE = 0x20;

        // Width of the screen in pixels
        const int SCREEN_WIDTH = 256;

        // Height of the screen in pixels
        const int SCREEN_HEIGHT = 240;

        public const int DOTS_PER_SCANLINE = 341;
        public const int SCANLINES_PER_FRAME = 262;
        const int VBLANK_START_SCANLINE = 241;

        // PPUCONTROL Flags
        const byte VRAM_ADDRESS_INCREMENT_FLAG = 1 << 2;
        const byte SPRITE_PATTERN_TABLE_ADDRESS_FLAG = 1 << 3;
        const byte BACKGROUND_PATTERN_TABLE_ADDRESS_FLAG = 1 << 4;
        const byte GENERATE_NMI_FLAG = 1 << 7;

        // PPUMASK Flags
        const byte SHOW_SPRITES = 1 << 2;
        const byte SHOW_BACKGROUND = 1 << 3;

        // PPUSTATUS Flags
        const byte SPRITE0_HIT_FLAG = 1 << 6;
        const byte IN_VBLANK_FLAG = 1 << 7;

        private int dot;
        private int scanline;

        private byte[] oam = new byte[OAM_SIZE]; // Object Attribute Memory
        private byte[] patternTable0 = new byte[PATTERN_TABLE_SIZE]; // Pattern Table 0
        private byte[] patternTable1 = new byte[PATTERN_TABLE_SIZE]; // Pattern Table 1
        private byte[] nameTable0 = new byte[NAME_TABLE_SIZE]; // Nametable 0
        private byte[] nameTable1 = new byte[NAME_TABLE_SIZE]; // Nametable 1
        private byte[] nameTable2 = new byte[NAME_TABLE_SIZE]; // Nametable 2
        private byte[] nameTable3 = new byte[NAME_TABLE_SIZE]; // Nametable 3
        private byte[] paletteRAM = new byte[PALETTE_RAM_SIZE]; // Palette RAM

        // PPU registers
        public byte ppuControl; // PPU Control Register (0x2000)
        private byte ppuMask; // PPU Mask Register (0x2001)
        public byte ppuStatus; // PPU Status Register (0x2002)
        private byte oamAddress; // OAM Address Register (0x2003)
        private byte ppudataBuffer; // Internal read buffer for PPUDATA

        // Screen buffer to store the rendered pixels
        private byte[] screenBuffer = new byte[SCREEN_WIDTH * SCREEN_HEIGHT * 3];

        // PPU registers
        private ushort v; // Current VRAM address (15 bits)
        private ushort t; // Temporary VRAM address (15 bits)
        private byte x; // Fine X scroll (3 bits)
        private bool w; // Write toggle flag

        // Open bus value
        private byte openBus;

        private Emulator? emulator;

        public void Initialize(Emulator emulator, Memory memory)
        {
            this.emulator = emulator;

            if (!memory.mirrorVertical)
            {
                nameTable1 = nameTable0;
                nameTable3 = nameTable2;
            }
            else
            {
                nameTable2 = nameTable0;
                nameTable3 = nameTable1;
            }
        }

        public byte[] GetScreenBuffer()
        {
            return screenBuffer;
        }

        // Read a byte from the specified PPU register
        public byte ReadRegister(ushort address, bool isDebugRead = false)
        {
            switch (address)
            {
                case 0x2000: // PPU Control Register
                    break;

                case 0x2001: // PPU Mask Register
                    break;

                case 0x2002: // PPU Status Register
                    if (!isDebugRead)
                    {
                        // Read and clear the vertical blank flag in the status register
                        openBus = ppuStatus;
                        ppuStatus = (byte)(ppuStatus & ~IN_VBLANK_FLAG);

                        // Reset the address latch
                        w = false;
                    }
                    else
                    {
                        openBus = (byte)ppuStatus;
                    }
                    break;

                case 0x2003: // OAM Address Register
                    break;

                case 0x2004: // OAM Data Register
                    openBus = oam[oamAddress];
                    break;

                case 0x2005: // PPU Scroll Register
                case 0x2006: // PPU Address Register
                    break;

                case 0x2007: // VRAM Data Register
                    if (v >= 0x0000 && v <= 0x3EFF)
                    {
                        // Read from internal read buffer and update the buffer with the new value
                        openBus = ppudataBuffer;
                        if (!isDebugRead)
                        {
                            ppudataBuffer = ReadVRAM(v);

                            // Increment the VRAM address based on the VRAM increment mode
                            v += (ushort)((ppuControl & VRAM_ADDRESS_INCREMENT_FLAG) != 0 ? 32 : 1);
                        }
                    }
                    else
                    {
                        // Read directly from VRAM and update the internal buffer
                        openBus = ReadVRAM(v);
                        if (!isDebugRead)
                        {
                            ppudataBuffer = openBus;

                            // Increment the VRAM address based on the VRAM increment mode
                            v += (ushort)((ppuControl & VRAM_ADDRESS_INCREMENT_FLAG) != 0 ? 32 : 1);
                        }
                    }
                    v &= 0x7FFF; // Handle VRAM address overflow
                    break;

                default:
                    // Invalid register address
                    break;
            }

            return openBus;
        }

        // Write a byte value to the specified PPU register
        public void WriteRegister(ushort address, byte value)
        {
            openBus = value;
            switch (address)
            {
                case 0x2000: // PPU Control Register
                    ppuControl = (byte)(0xFC & value); // ingore bits 1-2 for storing ppuControl
                    t = (ushort)((t & 0xF3FF) | ((value & 0x03) << 10)); // Update bits 10-11 of t with bits 1-2 of value
                    break;

                case 0x2001: // PPU Mask Register
                    ppuMask = value;
                    break;

                case 0x2003: // OAM Address Register
                    oamAddress = value;
                    break;

                case 0x2004: // OAM Data Register
                    oam[oamAddress] = value;
                    oamAddress++;
                    oamAddress &= 0xFF;
                    break;

                case 0x2005: // PPU Scroll Register
                    if (!w)
                    {
                        // First write to PPUSCROLL
                        x = (byte)(value & 0x07); // Store fine X scroll
                        t = (ushort)((t & 0xFFE0) | (value >> 3)); // Update coarse Y scroll in temporary VRAM address (t)
                        w = true;
                    }
                    else
                    {
                        // Second write to PPUSCROLL
                        t = (ushort)((t & 0x8FFF) | ((value & 0x07) << 12)); // Update coarse X scroll in temporary VRAM address (t)
                        t = (ushort)((t & 0xFC1F) | ((value & 0xF8) << 2)); // Update fine Y scroll in temporary VRAM address (t)
                        w = false;
                    }
                    break;

                case 0x2006: // PPU Address Register
                    if (!w)
                    {
                        // First write to PPUADDR
                        t = (ushort)((t & 0x00FF) | ((value & 0x3F) << 8)); // Clear upper bits of temporary VRAM address (t)
                        w = true;
                    }
                    else
                    {
                        // Second write to PPUADDR
                        t = (ushort)((t & 0xFF00) | (value & 0xFF)); // Preserve the lower bits of temporary VRAM address (t)
                        v = t; // Copy temporary VRAM address (t) to current VRAM address (v)
                        w = false;
                    }
                    break;

                case 0x2007: // VRAM Data Register
                    WriteVRAM(v, value);
                    // Increment v after writing
                    v += (ushort)(((ppuControl & VRAM_ADDRESS_INCREMENT_FLAG) != 0) ? 32 : 1);
                    // Handle wrapping
                    v &= 0x7FFF; // Apply a bitwise AND operation to limit the address within the VRAM address space
                    break;

                default:
                    // Invalid register address
                    break;
            }
        }

        private byte ReadVRAM(ushort address)
        {
            if (address is >= PATTERN_TABLE_0_BASE_ADDRESS and < PATTERN_TABLE_0_BASE_ADDRESS + PATTERN_TABLE_SIZE)
            {
                // Accessing Pattern Table 0
                return patternTable0[address];
            }
            else if (address is >= PATTERN_TABLE_1_BASE_ADDRESS and < PATTERN_TABLE_1_BASE_ADDRESS + PATTERN_TABLE_SIZE)
            {
                // Accessing Pattern Table 1
                return patternTable1[address & (PATTERN_TABLE_SIZE - 1)];
            }
            else if (address is >= NAME_TABLE_0_BASE_ADDRESS and < NAME_TABLE_0_BASE_ADDRESS + NAME_TABLE_SIZE)
            {
                // Accessing Nametable 0
                return nameTable0[address & (NAME_TABLE_SIZE - 1)];
            }
            else if (address is >= NAME_TABLE_1_BASE_ADDRESS and < NAME_TABLE_1_BASE_ADDRESS + NAME_TABLE_SIZE)
            {
                // Accessing Nametable 1
                return nameTable1[address & (NAME_TABLE_SIZE - 1)];
            }
            else if (address is >= NAME_TABLE_2_BASE_ADDRESS and < NAME_TABLE_2_BASE_ADDRESS + NAME_TABLE_SIZE)
            {
                // Accessing Nametable 2
                return nameTable2[address & (NAME_TABLE_SIZE - 1)];
            }
            else if (address is >= NAME_TABLE_3_BASE_ADDRESS and < NAME_TABLE_3_BASE_ADDRESS + NAME_TABLE_SIZE)
            {
                // Accessing Nametable 3
                return nameTable3[address & (NAME_TABLE_SIZE - 1)];
            }
            else if (address is >= PALETTE_TABLE_BASE_ADDRESS and < PALETTE_TABLE_BASE_ADDRESS + (PALETTE_RAM_SIZE * 8))
            {
                // Accessing Palette RAM
                // Handle address mirrors of $3F00/$3F04/$3F08/$3F0C to $3F10/$3F14/$3F18/$3F1C
                int realPaletteAddress = address & (PALETTE_RAM_SIZE - 1);
                if (realPaletteAddress % 4 == 0) realPaletteAddress &= ~0x10;
                return paletteRAM[realPaletteAddress];
            }

            return 0x00; // Default value if the address is not within any of the defined regions
        }

        // Write a byte value to VRAM at the current VRAM address and increment the address
        public void WriteVRAM(ushort address, byte value)
        {
            if (address is >= PATTERN_TABLE_0_BASE_ADDRESS and < PATTERN_TABLE_0_BASE_ADDRESS + PATTERN_TABLE_SIZE)
            {
                // Writing to Pattern Table 0
                patternTable0[address] = value;
            }
            else if (address is >= PATTERN_TABLE_1_BASE_ADDRESS and < PATTERN_TABLE_1_BASE_ADDRESS + PATTERN_TABLE_SIZE)
            {
                // Writing to Pattern Table 1
                patternTable1[address & (PATTERN_TABLE_SIZE - 1)] = value;
            }
            else if (address is >= NAME_TABLE_0_BASE_ADDRESS and < NAME_TABLE_0_BASE_ADDRESS + NAME_TABLE_SIZE)
            {
                // Writing to Nametable 0
                nameTable0[address & (NAME_TABLE_SIZE - 1)] = value;
            }
            else if (address is >= NAME_TABLE_1_BASE_ADDRESS and < NAME_TABLE_1_BASE_ADDRESS + NAME_TABLE_SIZE)
            {
                // Writing to Nametable 1
                nameTable1[address & (NAME_TABLE_SIZE - 1)] = value;
            }
            else if (address is >= NAME_TABLE_2_BASE_ADDRESS and < NAME_TABLE_2_BASE_ADDRESS + NAME_TABLE_SIZE)
            {
                // Accessing Nametable 2
                nameTable2[address & (NAME_TABLE_SIZE - 1)] = value;
            }
            else if (address is >= NAME_TABLE_3_BASE_ADDRESS and < NAME_TABLE_3_BASE_ADDRESS + NAME_TABLE_SIZE)
            {
                // Accessing Nametable 3
                nameTable3[address & (NAME_TABLE_SIZE - 1)] = value;
            }
            else if (address is >= PALETTE_TABLE_BASE_ADDRESS and < PALETTE_TABLE_BASE_ADDRESS + (PALETTE_RAM_SIZE * 8))
            {
                // Writing to Palette RAM
                // Handle address mirrors of $3F00/$3F04/$3F08/$3F0C to $3F10/$3F14/$3F18/$3F1C
                int realPaletteAddress = address & (PALETTE_RAM_SIZE - 1);
                if (realPaletteAddress % 4 == 0) realPaletteAddress &= ~0x10;
                paletteRAM[realPaletteAddress] = value;
            }
        }

        // Render a single pixel at the specified position
        public void RenderPixel(int x, int y)
        {
            // Calculate the name table address for the current coordinates
            ushort nameTableAddress = (ushort)(NAME_TABLE_0_BASE_ADDRESS | (v & 0x0FFF));

            // Compute the tile index
            byte tileIndex = ReadVRAM(nameTableAddress);

            // Fetch the pixel data for the current tile and position
            ushort patternTableAddress = (ushort)(((ppuControl & BACKGROUND_PATTERN_TABLE_ADDRESS_FLAG) != 0 ? 0x1000 : 0x0000) | (tileIndex << 4) | (v >> 12)); // Use the fine Y scroll for the row within the tile
            byte patternDataLo = ReadVRAM(patternTableAddress);
            byte patternDataHi = ReadVRAM((ushort)(patternTableAddress + 8));

            // Select the correct pixel within the tile
            byte pixelData = (byte)(((patternDataHi >> (7 - this.x)) & 1) << 1 | ((patternDataLo >> (7 - this.x)) & 1)); // Use the fine X scroll for the column within the tile

            // Compute the attribute table address
            ushort attributeTableAddress = (ushort)(ATTRIBUTE_TABLE_BASE_ADDRESS | (nameTableAddress & 0xC00) | 0x3C0 | ((nameTableAddress >> 4) & 0x38) | ((nameTableAddress >> 2) & 0x07));

            // Read the attribute byte
            byte attributeByte = ReadVRAM(attributeTableAddress);

            // Calculate the tile's relative position within its attribute cell
            int relativeTileX = (nameTableAddress & 0x02) >> 1;
            int relativeTileY = (nameTableAddress & 0x40) >> 6;

            // Calculate the attribute data offset based on the tile's relative position
            int offset = (relativeTileY * 2 + relativeTileX) * 2;

            // Extract the correct bits
            byte attributeData = (byte)((attributeByte >> offset) & 0x03);

            int paletteIndex = pixelData & 0x03; // Mask the pixel data to ensure it's 2 bits

            // Apply the attribute data to determine the correct palette index
            int paletteOffset = (attributeData & 0x03) * 4;

            // Fetch the color from the correct palette and color index
            int finalPaletteIndex = PALETTE_TABLE_BASE_ADDRESS + paletteOffset + paletteIndex;
            byte paletteColor = ReadVRAM((ushort)finalPaletteIndex);

            // Calculate the index in the screen buffer based on the scanline and pixel position
            int index = (y * SCREEN_WIDTH * 3) + (x * 3);

            // Lookup pixel color
            var pixelColor = ColorMap.LUT[paletteColor];

            // Set the RGB values in the screen buffer at the calculated index
            screenBuffer[index] = pixelColor[2];     // Red component
            screenBuffer[index + 1] = pixelColor[1]; // Green component
            screenBuffer[index + 2] = pixelColor[0]; // Blue component
        }

        public void RenderCycle()
        {
            if ((ppuMask & SHOW_BACKGROUND) != 0)
            {
                // Check if we're rendering a visible scanline
                if (scanline < SCREEN_HEIGHT)
                {
                    // Perform cycle-based rendering operations here

                    // Render a pixel for each dot on a visible scanline
                    if (dot < SCREEN_WIDTH)
                    {

                        RenderPixel(dot, scanline);

                        // Increment fine X scroll
                        if (x < 7)
                        {
                            x++;
                        }
                        else
                        {
                            x = 0; // Reset fine X scroll
                                   // Increment coarse X scroll
                            if ((v & 0x1F) == 31) // If coarse X == 31
                            {
                                v = (ushort)(v & ~0x1F); // coarse X = 0
                                v ^= 0x400; // Switch horizontal nametable
                            }
                            else
                            {
                                v++; // coarse X++
                            }
                        }
                    }
                    else if (dot == 256)
                    {
                        // Increment fine Y scroll
                        if ((v & 0x7000) != 0x7000) // if fine Y < 7
                        {
                            v += 0x1000; // fine Y++
                        }
                        else
                        {
                            v = (ushort)(v & ~0x7000); // fine Y = 0
                            int y = (v & 0x03E0) >> 5; // let y = coarse Y
                            if (y == 29)
                            {
                                y = 0;  // coarse Y = 0
                                v ^= 0x0800; // switch vertical nametable
                            }
                            else if (y == 31)
                            {
                                y = 0;  // coarse Y = 0, nametable not switched
                            }
                            else
                            {
                                y++;  // coarse Y++
                            }
                            v = (ushort)((v & ~0x03E0) | (y << 5)); // put coarse Y back into v
                        }
                    }
                    else if (dot == 257)
                    {
                        // At dot 257 of each scanline, copy horizontal position from t to v
                        // i.e., v: ....F.. ...EDCBA = t: ....F.. ...EDCBA
                        v = (ushort)((v & ~0x041F) | (t & 0x041F));
                    }
                }

                if (scanline == 261)
                {
                    if (dot is >= 280 and <= 304)
                    {
                        // At dots 280 to 304 of the pre-render scanline, copy vertical position from t to v
                        // i.e., v: IHGF.ED CBA..... = t: IHGF.ED CBA.....
                        v = (ushort)((v & ~0x7BE0) | (t & 0x7BE0));
                    }

                    if (dot >= 339)
                    {
                        // At the end of the pre-render scanline, copy t into v
                        v = t;
                    }
                }
            }

            // Check if we reached the start of the vertical blanking period (dot 0 of scanline 241)
            if (scanline == VBLANK_START_SCANLINE && dot == 0)
            {
                // Set VBlank flag
                ppuStatus |= IN_VBLANK_FLAG;

                // If the NMI interrupt is enabled, trigger the interrupt
                if ((ppuControl & GENERATE_NMI_FLAG) != 0)
                {
                    emulator.isNmiPending = true;
                }
            }

            // Update dot and scanline
            dot++;
            if (dot >= DOTS_PER_SCANLINE)
            {
                dot = 0;
                scanline++;
                if (scanline >= SCANLINES_PER_FRAME)
                {
                    scanline = 0;
                }
            }
        }

        public bool ShouldRenderFrame()
        {
            // A frame should be rendered when we're at the start of a new frame (dot 0, scanline 0)
            return dot == 0 && scanline == 0;
        }
    }
}
