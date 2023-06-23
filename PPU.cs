namespace Emulation
{
    public class PPU
    {
        private byte[] oam = new byte[0x0100]; // Object Attribute Memory

        private byte[] patternTable0 = new byte[0x1000]; // Pattern Table 0
        private byte[] patternTable1 = new byte[0x1000]; // Pattern Table 1
        private byte[] nameTable0 = new byte[0x0400]; // Nametable 0
        private byte[] nameTable1 = new byte[0x0400]; // Nametable 1
        private byte[] nameTable2 = new byte[0x0400]; // Nametable 2
        private byte[] nameTable3 = new byte[0x0400]; // Nametable 3
        private byte[] paletteRAM = new byte[0x0040]; // Palette RAM


        // PPU registers
        private volatile byte ppuControl; // PPU Control Register (0x2000)
        private volatile byte ppuMask; // PPU Mask Register (0x2001)
        private volatile int ppuStatus; // PPU Status Register (0x2002)
        private volatile byte oamAddress; // OAM Address Register (0x2003)
        private bool ppuLatch;  // Address latch for PPU Address Register (0x2006) and Scroll Register (0x2005)
        
        private byte ppudataBuffer; // Internal read buffer for PPUDATA

        // Maximum number of sprites on the screen
        const int MAX_SPRITES = 64;

        // Height of a sprite in pixels
        const int SPRITE_HEIGHT = 8;

        // Width of a sprite in pixels
        const int SPRITE_WIDTH = 8;

        // Sprite attribute bit indicating vertical flip
        const byte SPRITE_ATTRIBUTE_VERTICAL_FLIP = 0x80;

        // Sprite attribute bits for palette selection
        const byte SPRITE_ATTRIBUTE_PALETTE = 0x03;

        // Size of a pattern table row for a sprite
        const int SPRITE_PATTERN_TABLE_ROW_SIZE = 16;

        // Number of bytes of sprite data per scanline
        const int SPRITE_DATA_PER_SCANLINE = 4;

        // Width of the screen in pixels
        const int SCREEN_WIDTH = 256;

        // Height of the screen in pixels
        const int SCREEN_HEIGHT = 240;

        const int SCANLINE_COUNT = 262;
        const int VBLANK_START_SCANLINE = 241;

        const byte VBLANK_FLAG = 1 << 7;
        const byte SPRITE0_HIT_FLAG = 1 << 6;
        const byte NMI_FLAG = 1 << 7;

        // Screen buffer to store the rendered pixels
        private byte[] screenBuffer = new byte[SCREEN_WIDTH * SCREEN_HEIGHT];
        private bool[] isSprite = new bool[SCREEN_WIDTH * SCREEN_HEIGHT];
        private byte[] colorIndex = new byte[SCREEN_WIDTH * SCREEN_HEIGHT];
        private bool[] isBackground = new bool[SCREEN_WIDTH * SCREEN_HEIGHT];

        // PPU registers
        private ushort v; // Current VRAM address (15 bits)
        private ushort t; // Temporary VRAM address (15 bits)
        private byte x; // Fine X scroll (3 bits)
        private bool w; // Write toggle flag

        // Open bus value
        private byte openBus;

        private Memory memory;
        private CPU cpu;

        public PPU(Memory memory, CPU cpu)
        {
            this.memory = memory;
            this.cpu = cpu;
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
                        openBus = (byte)Interlocked.And(ref ppuStatus, ~VBLANK_FLAG);

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
                            v += (ushort)((ppuControl & 0x04) != 0 ? 32 : 1);
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
                            v += (ushort)((ppuControl & 0x04) != 0 ? 32 : 1);
                        }
                    }
                    v &= 0x7FFF; // Handle VRAM address overflow
                    break;

                case 0x4014: // DMA Register
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
                        t = (ushort)((t & 0x00FF) | ((value & 0xFF) << 8)); // Preserve the lower bits of temporary VRAM address (t)
                        v = t; // Copy temporary VRAM address (t) to current VRAM address (v)
                        w = false;
                    }
                    break;

                case 0x2007: // VRAM Data Register
                    WriteVRAM(v, value);
                    // Increment v after writing
                    v += (ushort)(((ppuControl & 0x04) != 0) ? 32 : 1);
                    // Handle wrapping
                    v &= 0x7FFF; // Apply a bitwise AND operation to limit the address within the VRAM address space
                    break;

                case 0x4014: // DMA Register
                    // Perform DMA transfer from CPU memory to OAM
                    ushort cpuAddress = (ushort)(value << 8);
                    for (int i = 0; i < 256; i++)
                    {
                        oam[oamAddress] = memory.Read(cpuAddress);
                        oamAddress++;
                        oamAddress &= 0xFF;
                        cpuAddress++;
                    }
                    // The DMA transfer takes 513 or 514 cycles to complete
                    // You may need to account for the cycles spent on the DMA transfer
                    // by adding extra cycles to the CPU's cycle count.
                    // cpu.StallCycles += 513 + (cpu.Cycles % 2);
                    break;

                default:
                    // Invalid register address
                    break;
            }
        }

        private byte ReadVRAM(ushort address)
        {
            if (address >= 0x0000 && address <= 0x0FFF)
            {
                // Accessing Pattern Table 0
                return patternTable0[address];
            }
            else if (address >= 0x1000 && address <= 0x1FFF)
            {
                // Accessing Pattern Table 1
                return patternTable1[address - 0x1000];
            }
            else if (address >= 0x2000 && address <= 0x23FF)
            {
                // Accessing Nametable 0
                return nameTable0[address - 0x2000];
            }
            else if (address >= 0x2400 && address <= 0x27FF)
            {
                // Accessing Nametable 1
                return nameTable1[address - 0x2400];
            }
            else if (address >= 0x2800 && address <= 0x2BFF)
            {
                // Accessing Nametable 2
                return nameTable2[address - 0x2800];
            }
            else if (address >= 0x2C00 && address <= 0x2FFF)
            {
                // Accessing Nametable 3
                return nameTable3[address - 0x2C00];
            }
            else if (address >= 0x3F00 && address <= 0x3FFF)
            {
                // Accessing Palette RAM
                return paletteRAM[address - 0x3F00];
            }

            return 0x00; // Default value if the address is not within any of the defined regions
        }

        // Write a byte value to VRAM at the current VRAM address and increment the address
        private void WriteVRAM(ushort address, byte value)
        {
            if (address >= 0x0000 && address <= 0x0FFF)
            {
                // Writing to Pattern Table 0
                patternTable0[address] = value;
            }
            else if (address >= 0x1000 && address <= 0x1FFF)
            {
                // Writing to Pattern Table 1
                patternTable1[address - 0x1000] = value;
            }
            else if (address >= 0x2000 && address <= 0x23FF)
            {
                // Writing to Nametable 0
                nameTable0[address - 0x2000] = value;
            }
            else if (address >= 0x2400 && address <= 0x27FF)
            {
                // Writing to Nametable 1
                nameTable1[address - 0x2400] = value;
            }
            else if (address >= 0x2800 && address <= 0x2BFF)
            {
                // Writing to Nametable 2
                nameTable2[address - 0x2800] = value;
            }
            else if (address >= 0x2C00 && address <= 0x2FFF)
            {
                // Writing to Nametable 3
                nameTable3[address - 0x2C00] = value;
            }
            else if (address >= 0x3F00 && address <= 0x3FFF)
            {
                // Writing to Palette RAM
                paletteRAM[address - 0x3F00] = value;
            }
        }

        // Draw a scanline on the screen using the provided pixel colors
        private void DrawScanline(int scanline, byte[] colors)
        {
            // Draw the pixels on the screen
            for (int i = 0; i < SCREEN_WIDTH; i++)
            {
                byte colorIndex = colors[i / 32];
                byte color = ReadVRAM((ushort)(0x3F00 | (colorIndex & 0x3F))); // Read color from palette RAM via ReadVRAM

                // Calculate the index in the screen buffer based on the scanline and pixel position
                int index = (scanline * SCREEN_WIDTH) + i;

                // Set the color value in the screen buffer at the calculated index
                screenBuffer[index] = color;
            }
        }

        // Render the background using name tables
        private void RenderBackground(int scanline)
        {
            if (scanline > SCREEN_HEIGHT - 1)
                return;

            // Calculate the name table base address
            // Step 1: Read control registers
            ushort nameTableBaseAddress = (ushort)((ReadRegister(0x2000) & 0x03) * 0x0400);
            ushort patternTableBaseAddress = (ushort)(((ReadRegister(0x2000) & 0x10) != 0) ? 0x1000 : 0x0000);


            // Calculate the tile index offset
            byte tileIndexOffset = (byte)((scanline / 8) * 32);

            // Calculate the attribute offset
            ushort attributeOffset = (ushort)(((scanline / 32) * 8) + 0x03C0);

            // Step 2: Calculate scroll position
            byte coarseScrollX = (byte)(ppuScrollX >> 3);
            byte fineScrollX = (byte)(ppuScrollX & 0x07);
            byte coarseScrollY = (byte)(ppuScrollY >> 3);
            byte fineScrollY = (byte)(ppuScrollY & 0x07);


            // Calculate the name table index and tile column
            byte tileColumn = (byte)(coarseScrollX % 32);

            // Read the attribute byte for the current scanline
            byte attributeByte = ReadVRAM((ushort)(nameTableBaseAddress + attributeOffset + (coarseScrollY / 4) * 8 + (coarseScrollX / 4)));

            // Calculate the attribute value
            byte attributeShift = (byte)(((coarseScrollX & 0x02) >> 1) + ((coarseScrollY & 0x02) << 1) * 2);
            byte attribute = (byte)(((attributeByte >> attributeShift) & 0x03) << 2);

            // Calculate the tile index for the current scanline
            //ushort tileIndex = ReadVRAM((ushort)(nameTableBaseAddress + tileIndexOffset + tileColumn));
            ushort tileIndex = ReadVRAM((ushort)(nameTableBaseAddress + tileIndexOffset + tileColumn + (coarseScrollY * 32)));


            // Calculate the pattern address for the current scanline
            ushort patternAddress = (ushort)(patternTableBaseAddress + tileIndex * 16 + fineScrollY);

            // Read the pattern data for the current scanline
            byte patternLow = ReadVRAM(patternAddress);
            byte patternHigh = ReadVRAM((ushort)(patternAddress + 8));

            byte[] pixels = new byte[8];

            // Render the pixels for the current scanline
            for (int i = 0; i < 8; i++)
            {
                // Calculate the bit position of the pixel within the pattern byte
                byte bitPosition = (byte)(7 - fineScrollX);

                // Extract the color value (0-3) for the pixel
                byte pixelLow = (byte)((patternLow >> bitPosition) & 0x01);
                byte pixelHigh = (byte)((patternHigh >> bitPosition) & 0x01);

                int paletteIndex = ((pixelHigh << 1) | pixelLow) + attribute;

                // Calculate the effective palette index based on the attribute
                int effectivePaletteIndex = ((paletteIndex & 0x03) | (attribute << 2)) & 0x0F;

                // Read the color value from the VRAM using the effective palette index
                byte colorIndex = ReadVRAM((ushort)(0x3F00 + effectivePaletteIndex));

                pixels[i] = colorIndex;

                fineScrollX = (byte)((fineScrollX + 1) % 8);
            }

            // Draw the pixels on the screen
            DrawScanline(scanline, pixels);

            // Increment the tile column
            tileColumn++;
            if (tileColumn == 32)
            {
                tileColumn = 0;
            }

            // Calculate the VRAM address of the next tile
            ushort nextTileIndex = (ushort)(nameTableBaseAddress + tileIndexOffset + tileColumn);
            tileIndex = ReadVRAM(nextTileIndex);

            // Calculate the pattern address of the next tile
            patternAddress = (ushort)(patternTableBaseAddress + tileIndex * 16 + fineScrollY);

            // Read the pattern data for the next tile
            patternLow = ReadVRAM(patternAddress);
            patternHigh = ReadVRAM((ushort)(patternAddress + 8));

            fineScrollX = (byte)(ppuScrollX & 0x07);

            // Render the pixels for the current scanline
            for (int i = 0; i < 8; i++)
            {
                // Calculate the bit position of the pixel within the pattern byte
                byte bitPosition = (byte)(7 - fineScrollX);

                // Extract the color value (0-3) for the pixel
                byte pixelLow = (byte)((patternLow >> bitPosition) & 0x01);
                byte pixelHigh = (byte)((patternHigh >> bitPosition) & 0x01);

                int paletteIndex = ((pixelHigh << 1) | pixelLow) + attribute;

                // Calculate the effective palette index based on the attribute
                int effectivePaletteIndex = ((paletteIndex & 0x03) | (attribute << 2)) & 0x0F;

                // Read the color value from the VRAM using the effective palette index
                byte colorIndex = ReadVRAM((ushort)(0x3F00 + effectivePaletteIndex));

                pixels[i] = colorIndex;

                fineScrollX = (byte)((fineScrollX + 1) % 8);
            }

            // Draw the next set of pixels on the screen
            DrawScanline(scanline, pixels);

            // Update the PPU state for the next scanline
            ppuScrollX = (byte)((ppuScrollX + 1) % 512);
            ppuScrollY = (byte)((ppuScrollY + 1) % 240);
        }

        private void RenderSprites(int scanline)
        {
            // Boolean variable to track sprite 0 hit
            bool sprite0Hit = false;

            // Check if the current scanline is within the visible screen area
            if (scanline < SCREEN_HEIGHT)
            {
                // Iterate through the sprites in OAM
                for (int i = 0; i < MAX_SPRITES; i++)
                {
                    // Get the sprite attributes from OAM
                    byte y = oam[i * 4]; // Sprite Y position
                    byte tileIndex = oam[i * 4 + 1]; // Sprite tile index
                    byte attributes = oam[i * 4 + 2]; // Sprite attributes
                    byte x = oam[i * 4 + 3]; // Sprite X position

                    // Check if the sprite intersects with the current scanline
                    //if (y <= scanline && y + SPRITE_HEIGHT > scanline)
                    {
                        // Calculate the Y offset within the sprite tile
                        int yOffset = scanline - y;

                        // If the sprite is vertically flipped, calculate the Y offset from the bottom of the sprite tile
                        if ((attributes & SPRITE_ATTRIBUTE_VERTICAL_FLIP) != 0)
                        {
                            yOffset = SPRITE_HEIGHT - 1 - yOffset;
                        }

                        // Calculate the pattern table row address for the current scanline
                        ushort patternTableAddressRow = (ushort)(((ppuControl & 0x08) != 0) ? 0x1000 : 0x0000);
                        patternTableAddressRow += (ushort)(tileIndex * SPRITE_PATTERN_TABLE_ROW_SIZE + yOffset);

                        // Read the sprite tile data from the pattern table
                        byte tileLow = ReadVRAM(patternTableAddressRow);
                        byte tileHigh = ReadVRAM((ushort)(patternTableAddressRow + 8));

                        // Render the sprite pixel by pixel
                        for (int xOffset = 0; xOffset < SPRITE_WIDTH; xOffset++)
                        {
                            // Calculate the X position within the screen buffer
                            int screenX = x + xOffset;

                            // Check if the pixel is within the visible screen area
                            if (screenX >= 0 && screenX < SCREEN_WIDTH)
                            {
                                // Check if this is sprite 0 and the background/sprites are visible
                                if (i == 0 && ((ppuMask & (1 << 3)) != 0) && ((ppuMask & (1 << 4)) != 0) && screenX >= 8)
                                {
                                    sprite0Hit = true;
                                }

                                // Calculate the bit position of the pixel within the tile byte
                                byte bitPosition = (byte)(7 - xOffset);

                                // Extract the color value (0-3) for the pixel
                                byte pixelLow = (byte)((tileLow >> bitPosition) & 0x01);
                                byte pixelHigh = (byte)((tileHigh >> bitPosition) & 0x01);

                                // Combine the low and high bits to get the final pixel value
                                byte pixelValue = (byte)((pixelHigh << 1) | pixelLow);

                                // Only render the sprite pixel if it is not transparent
                                if (pixelValue != 0)
                                {
                                    // Calculate the palette index based on the pixel value and sprite attributes
                                    //int paletteIndex = ((pixelValue - 1) & 0x03) + ((attributes & 0x03) << 2);
                                    int paletteIndex = ((pixelValue & 0x03) + 4) + ((attributes & 0x03) << 2);

                                    // Get the color from VRAM using the palette index
                                    byte colorIndex = ReadVRAM((ushort)(0x3F00 + paletteIndex));

                                    // Set the pixel color in the screen buffer
                                    screenBuffer[scanline * SCREEN_WIDTH + screenX] = colorIndex;
                                }
                            }
                        }
                    }
                }
            }

            // Set or unset sprite 0 hit flag
            if (sprite0Hit && ((ppuControl & NMI_FLAG) == 0) && scanline < SCREEN_HEIGHT)
            {
                Interlocked.Or(ref ppuStatus, SPRITE0_HIT_FLAG);
            }
            else
            {
                Interlocked.And(ref ppuStatus, ~SPRITE0_HIT_FLAG);
            }
        }

        public bool isNmiPending;
        int cycles = 0;

        public void RenderFrame(PictureBox pictureBox)
        {
            for (int scanline = 0; scanline < SCANLINE_COUNT; scanline++)
            {
                while (cpu.cycles < cycles + 113) ;

                // Render the background for the current scanline
                RenderBackground(scanline);

                // Render the sprites for the current scanline
                RenderSprites(scanline);

                // Check if we reached the start of the vertical blanking period
                if (scanline == VBLANK_START_SCANLINE)
                {
                    // Set VBlank flag
                    Interlocked.Or(ref ppuStatus, VBLANK_FLAG);

                    // If the NMI interrupt is enabled, trigger the interrupt
                    if ((ppuControl & NMI_FLAG) != 0)
                    {
                        isNmiPending = true;
                    }
                }

                // Trigger the PictureBox to be redrawn on the UI thread
                pictureBox.Invoke((MethodInvoker)delegate
                {
                    pictureBox.Invalidate();
                });
                cycles = cpu.cycles;
            }

        }
    }
}
