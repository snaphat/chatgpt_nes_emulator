
namespace Emulation
{
    using static Globals;

    public class PPU
    {
        private int dot;
        private int scanline;

        private readonly byte[] oam = new byte[OAM_SIZE]; // Object Attribute Memory
        private readonly byte[] patternTable0 = new byte[PATTERN_TABLE_SIZE]; // Pattern Table 0
        private readonly byte[] patternTable1 = new byte[PATTERN_TABLE_SIZE]; // Pattern Table 1
        private readonly byte[] nameTable0 = new byte[NAME_TABLE_SIZE]; // Nametable 0
        private byte[] nameTable1 = new byte[NAME_TABLE_SIZE]; // Nametable 1
        private byte[] nameTable2 = new byte[NAME_TABLE_SIZE]; // Nametable 2
        private byte[] nameTable3 = new byte[NAME_TABLE_SIZE]; // Nametable 3
        private readonly byte[] paletteRAM = new byte[PALETTE_RAM_SIZE]; // Palette RAM

        // PPU registers
        public byte ppuControl; // PPU Control Register (0x2000)
        private byte ppuMask; // PPU Mask Register (0x2001)
        public byte ppuStatus; // PPU Status Register (0x2002)
        private byte oamAddress; // OAM Address Register (0x2003)
        private byte ppudataBuffer; // Internal read buffer for PPUDATA

        // Screen buffer to store the rendered pixels
        private readonly byte[] screenBuffer = new byte[SCREEN_WIDTH * SCREEN_HEIGHT * 3];

        // PPU registers
        private ushort v; // Current VRAM address (15 bits)
        private ushort t; // Temporary VRAM address (15 bits)
        private byte x; // Fine X scroll (3 bits)
        private bool w; // Write toggle flag

        // Open bus value
        private byte openBus;

        // State to avoid recomputations
        private ushort nameTableAddress;
        private byte tileIndex;
        private ushort patternTableAddress;
        private byte patternDataLo;
        private byte patternDataHi;
        private byte pixelData;
        private ushort attributeTableAddress;
        private byte attributeByte;
        private byte attributeData;
        private int paletteIndex;
        private int paletteOffset;
        private int finalPaletteIndex;
        private byte paletteColor;
        private byte[] pixelColor;
        private int index;

        private List<byte>[] spritesPerScanline;

        private Emulator emulator = null!;

        public void Initialize(Emulator emulator, Memory memory)
        {
            this.emulator = emulator;

            if (memory.mirrorArrangement == HORIZONTAL_MIRRORING)
            {
                nameTable1 = nameTable0;
                nameTable3 = nameTable2;
            }
            else if (memory.mirrorArrangement == VERTICAL_MIRRORING)
            {
                nameTable2 = nameTable0;
                nameTable3 = nameTable1;
            }

            spritesPerScanline = new List<byte>[SCREEN_HEIGHT];
            for (int i = 0; i < SCREEN_HEIGHT; i++)
            {
                spritesPerScanline[i] = new List<byte>();
            }
        }

        public byte[] GetScreenBuffer()
        {
            return screenBuffer;
        }

        // Read a byte from the specified PPU register
        public byte DebugReadRegister(ushort address)
        {
            byte temp = openBus;
            switch (address)
            {
                case 0x2000: // PPU Control Register
                case 0x2001: // PPU Mask Register
                    break;

                case 0x2002: // PPU Status Register
                    temp = ppuStatus;
                    break;

                case 0x2003: // OAM Address Register
                    break;

                case 0x2004: // OAM Data Register
                    temp = oam[oamAddress];
                    break;

                case 0x2005: // PPU Scroll Register
                case 0x2006: // PPU Address Register
                    break;

                case 0x2007: // VRAM Data Register
                    if (v is >= 0x0000 and <= 0x3EFF)
                    {
                        // Read from internal read buffer and update the buffer with the new value
                        temp = ppudataBuffer;
                    }
                    else
                    {
                        // Read directly from VRAM and update the internal buffer
                        temp = ReadVRAM(v);
                    }
                    break;

                default:
                    // Invalid register address
                    break;
            }

            return temp;
        }

        // Read a byte from the specified PPU register
        public byte ReadRegister(ushort address)
        {
            switch (address)
            {
                case 0x2000: // PPU Control Register
                case 0x2001: // PPU Mask Register
                    break;

                case 0x2002: // PPU Status Register
                    // Read and clear the vertical blank flag in the status register
                    openBus = ppuStatus;
                    ppuStatus = (byte)(ppuStatus & ~IN_VBLANK_FLAG);

                    // Reset the address latch
                    w = false;
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
                    if (v is >= 0x0000 and <= 0x3EFF)
                    {
                        // Read from internal read buffer and update the buffer with the new value
                        openBus = ppudataBuffer;
                        ppudataBuffer = ReadVRAM(v);

                        // Increment the VRAM address based on the VRAM increment mode
                        v += (ushort)((ppuControl & VRAM_ADDRESS_INCREMENT_FLAG) != 0 ? 32 : 1);
                    }
                    else
                    {
                        // Read directly from VRAM and update the internal buffer
                        openBus = ReadVRAM(v);
                        ppudataBuffer = openBus;

                        // Increment the VRAM address based on the VRAM increment mode
                        v += (ushort)((ppuControl & VRAM_ADDRESS_INCREMENT_FLAG) != 0 ? 32 : 1);
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
                    CacheSpritesPerScanline();
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

        public void CacheSpritesPerScanline()
        {
            // first clear all sprites from all scanlines
            for (int i = 0; i < SCREEN_HEIGHT; i++)
            {
                spritesPerScanline[i].Clear();
            }

            // then add sprites to scanlines they are visible on
            for (int i = 0; i < 64; i++)
            {
                byte spriteY = oam[(i * 4) + 0];
                int height = (ppuControl & SPRITE_SIZE_FLAG) != 0 ? 16 : 8;

                // add sprite index to all scanlines it is visible on
                for (int y = spriteY; y < spriteY + height && y < SCREEN_HEIGHT; y++)
                {
                    spritesPerScanline[y].Add((byte)i);
                }
            }
        }

        public void StartScanline()
        {
            // Calculate the name table address for the current coordinates
            nameTableAddress = (ushort)(NAME_TABLE_0_BASE_ADDRESS | (v & 0x0FFF));

            // Compute the tile index
            tileIndex = ReadVRAM(nameTableAddress);

            // Fetch the pixel data for the current tile and position
            patternTableAddress = (ushort)(((ppuControl & BACKGROUND_PATTERN_TABLE_ADDRESS_FLAG) != 0 ? 0x1000 : 0x0000) | (tileIndex << 4) | (v >> 12)); // Use the fine Y scroll for the row within the tile
            patternDataLo = ReadVRAM(patternTableAddress);
            patternDataHi = ReadVRAM((ushort)(patternTableAddress + 8));

            // Compute the attribute table address
            attributeTableAddress = (ushort)(ATTRIBUTE_TABLE_BASE_ADDRESS | (nameTableAddress & 0xC00) | 0x3C0 | ((nameTableAddress >> 4) & 0x38) | ((nameTableAddress >> 2) & 0x07));

            // Read the attribute byte
            attributeByte = ReadVRAM(attributeTableAddress);

            // Extract the correct bits
            attributeData = (byte)((attributeByte >> ((((nameTableAddress & 0x40) >> 6) * 2) + ((nameTableAddress & 0x02) >> 1)) * 2) & 0x03);

        }

        public byte RenderBackground()
        {
            // Implement background clipping
            if ((ppuMask & SHOW_BACKGROUND_IN_LEFTMOST_8_PIXELS) == 0 && dot < 8)
                return 0;

            // Select the correct pixel within the tile
            pixelData = (byte)(((patternDataHi >> (7 - x)) & 1) << 1 | ((patternDataLo >> (7 - x)) & 1)); // Use the fine X scroll for the column within the tile

            // Check if the pixel is transparent
            if (pixelData == 0)
            {
                screenBuffer[index] = 0;     // Blue component
                screenBuffer[index + 1] = 0; // Green component
                screenBuffer[index + 2] = 0; // Red component
                return 0;
            }

            paletteIndex = pixelData & 0x03; // Mask the pixel data to ensure it's 2 bits

            // Apply the attribute data to determine the correct palette index
            paletteOffset = (attributeData & 0x03) * 4;

            // Fetch the color from the correct palette and color index
            finalPaletteIndex = PALETTE_TABLE_BASE_ADDRESS + paletteOffset + paletteIndex;
            paletteColor = ReadVRAM((ushort)finalPaletteIndex);

            // Lookup pixel color
            pixelColor = ColorMap.LUT[paletteColor];

            // Set the RGB values in the screen buffer at the calculated index
            screenBuffer[index] = pixelColor[2];     // Blue component
            screenBuffer[index + 1] = pixelColor[1]; // Green component
            screenBuffer[index + 2] = pixelColor[0]; // Red component

            // Return palette color before lookup
            return paletteColor;
        }

        public void RenderSprite(byte backgroundPaletteColor)
        {
            foreach (byte i in spritesPerScanline[scanline]) // scanline is the current scanline being rendered
            {
                // Get sprite X and Y from OAM
                byte spriteY = oam[(i * 4) + 0];
                byte spriteX = oam[(i * 4) + 3];

                // Check if the dot is within the sprite's horizontal range
                if (dot < spriteX || dot >= spriteX + 8)
                    continue;

                // Check if the scanline is within the sprite's vertical range
                int height = (ppuControl & SPRITE_SIZE_FLAG) != 0 ? 16 : 8;
                if (scanline < spriteY || scanline >= spriteY + height)
                {
                    continue;
                }

                // Get sprite tile and attributes from OAM
                byte spriteTile = oam[(i * 4) + 1];
                byte spriteAttributes = oam[(i * 4) + 2];

                // Compute the tile row
                int row = scanline - spriteY;

                // Flip vertically if the attribute bit is set
                if ((spriteAttributes & FLIP_SPRITE_VERTICALLY_FLAG) != 0)
                    row = height - 1 - row;

                // Compute the pattern table address
                ushort patternTableAddress = (ushort)(((ppuControl & SPRITE_PATTERN_TABLE_ADDRESS_FLAG) != 0 ? 0x1000 : 0x0000) | (spriteTile << 4) | row);

                // Read the pattern data
                byte patternDataLo = ReadVRAM(patternTableAddress);
                byte patternDataHi = ReadVRAM((ushort)(patternTableAddress + 8));

                // Flip horizontally if the attribute bit is set
                int x = (spriteAttributes & FLIP_SPRITE_HORIZONTALLY_FLAG) != 0 ? 7 - (dot - spriteX) : dot - spriteX;

                // Compute the pixel data
                byte pixelData = (byte)(((patternDataHi >> (7 - x)) & 1) << 1 | ((patternDataLo >> (7 - x)) & 1));

                // Skip transparent pixels (palette index 0)
                if (pixelData == 0)
                    continue;

                // Check sprite priority
                bool spriteIsBehindBackground = (spriteAttributes & SPRITE_PRIORITY_FLAG) != 0;
                if (spriteIsBehindBackground && backgroundPaletteColor != 0)
                    continue;

                // Compute the palette address
                int paletteAddress = PALETTE_TABLE_SPRITE_BASE_ADDRESS + ((spriteAttributes & 0x03) << 2) + pixelData;

                // Fetch the color from the palette
                byte paletteColor = ReadVRAM((ushort)paletteAddress);

                // Check for sprite 0 hit
                if (i == 0 && backgroundPaletteColor != 0)
                {
                    ppuStatus |= SPRITE0_HIT_FLAG;
                }

                // Lookup pixel color
                var pixelColor = ColorMap.LUT[paletteColor];

                // Set the RGB values in the screen buffer at the calculated index
                screenBuffer[index] = pixelColor[2];     // Blue component
                screenBuffer[index + 1] = pixelColor[1]; // Green component
                screenBuffer[index + 2] = pixelColor[0]; // Red component
            }
        }

        public void RenderCycle()
        {
            if ((ppuMask & (SHOW_BACKGROUND | SHOW_SPRITES)) != 0)
            {
                // Check if we're rendering a visible scanline
                if (scanline < SCREEN_HEIGHT)
                {
                    // Perform cycle-based rendering operations here

                    // Render a pixel for each dot on a visible scanline
                    if (dot < SCREEN_WIDTH)
                    {
                        byte backgroundPaletteColor = 0;

                        // Calculate the index in the screen buffer based on the scanline and pixel position
                        index = (scanline * SCREEN_WIDTH * 3) + (dot * 3);

                        if ((ppuMask & SHOW_BACKGROUND) != 0)
                            backgroundPaletteColor = RenderBackground();
                        if ((ppuMask & SHOW_SPRITES) != 0 && (dot >= 8 || (ppuMask & SHOW_SPRITES_IN_LEFTMOST_8_PIXELS) != 0))
                            RenderSprite(backgroundPaletteColor);

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
                            // Start new scanline data
                            StartScanline();
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

                        // At the end of the pre-render scanline, clear sprite0 hit
                        ppuStatus = (byte)(ppuStatus & ~SPRITE0_HIT_FLAG);
                    }
                }
            }

            // Check if we reached the start of the vertical blanking period (dot 0 of scanline 241)
            if (scanline == VBLANK_START_SCANLINE && dot == 0)
            {
                shouldRender = true;

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

        bool shouldRender = false;

        public bool ShouldRenderFrame()
        {
            if (shouldRender)
            {
                shouldRender = false;
                return true;
            }
            return false;
        }
    }
}
