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

        const int NameTableWidth = 32;
        const int NameTableHeight = 30;
        const int NameTableSize = NameTableWidth * NameTableHeight;
        const ushort NameTableAddress = 0x2000; // Address of the first name table
        const int AttributeTableAddress = 0x23C0; // Address of the attribute table
        const int PaletteMemoryAddress = 0x3F00;

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
        private byte[] screenBuffer = new byte[SCREEN_WIDTH * SCREEN_HEIGHT * 3];

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

        public void Initialize()
        {
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
                        t = (ushort)((t & 0xFF00) | (value & 0xFF)); // Preserve the lower bits of temporary VRAM address (t)
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
            if (address >= 0x3F00)
            {
                int realPaletteAddress = 0x3F00 + (address & 0x1F);
                if (realPaletteAddress % 4 == 0)
                {
                    realPaletteAddress &= ~0x10;
                }
                address = (ushort)realPaletteAddress;
            }

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
                return nameTable0[address & 0x03FF];
            }
            else if (address >= 0x2400 && address <= 0x27FF)
            {
                // Accessing Nametable 1
                return nameTable1[address & 0x03FF];
            }
            else if (address >= 0x2800 && address <= 0x2BFF)
            {
                // Accessing Nametable 2
                return nameTable2[address & 0x03FF];
            }
            else if (address >= 0x2C00 && address <= 0x2FFF)
            {
                // Accessing Nametable 3
                return nameTable3[address & 0x03FF];
            }
            else if (address >= 0x3F00 && address <= 0x3FFF)
            {
                // Accessing Palette RAM
                return paletteRAM[address - 0x3F00];
            }

            return 0x00; // Default value if the address is not within any of the defined regions
        }

        // Write a byte value to VRAM at the current VRAM address and increment the address
        public void WriteVRAM(ushort address, byte value)
        {
            if (address >= 0x3F00)
            {
                int realPaletteAddress = 0x3F00 + (address & 0x1F);
                if (realPaletteAddress % 4 == 0)
                {
                    realPaletteAddress &= ~0x10;
                }
                address = (ushort)realPaletteAddress;
            }

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
                nameTable0[address & 0x03FF] = value;
            }
            else if (address >= 0x2400 && address <= 0x27FF)
            {
                // Writing to Nametable 1
                nameTable1[address & 0x03FF] = value;
            }
            else if (address >= 0x2800 && address <= 0x2BFF)
            {
                // Accessing Nametable 2
                nameTable2[address & 0x03FF] = value;
            }
            else if (address >= 0x2C00 && address <= 0x2FFF)
            {
                // Accessing Nametable 3
                nameTable3[address & 0x03FF] = value;
            }
            else if (address >= 0x3F00 && address <= 0x3FFF)
            {
                // Writing to Palette RAM
                paletteRAM[address - 0x3F00] = value;
            }
        }

        // Calculate the name table address
        int CalculateNameTableAddress(int x, int y)
        {
            int tableX = x / 8; // Each tile is 8 pixels wide
            int tableY = y / 8; // Each tile is 8 pixels high

            int offset = tableY * NameTableWidth + tableX;
            return NameTableAddress + offset;
        }

        byte FetchPixelData(byte tileID, int pixelRow, int pixelCol)
        {
            // Calculate the tile's address in the pattern table 1
            int tileAddress = 0x1000 + (tileID * 16);

            // Read the data for each plane of the tile from the pattern table
            byte plane1 = ReadVRAM((ushort)(tileAddress + pixelRow));
            byte plane2 = ReadVRAM((ushort)(tileAddress + pixelRow + 8));

            // Combine the data from both planes to determine the color of the pixel
            byte pixel1 = (byte)((plane1 >> (7 - pixelCol)) & 0x01); // Get the pixel from plane 1 in reverse order
            byte pixel2 = (byte)((plane2 >> (7 - pixelCol)) & 0x01); // Get the pixel from plane 2 in reverse order
            byte pixel = (byte)(pixel1 | (pixel2 << 1)); // Combine the pixels from both planes

            return pixel;
        }

        // Fetch the attribute data based on the name table address
        byte FetchAttributeData(int nameTableAddress)
        {
            // Find the corresponding attribute table address
            int attributeTableAddress = nameTableAddress + 0x3C0;

            // Calculate the tile coordinates
            int tileIndex = nameTableAddress - 0x2000; // calculate the tile index relative to the start of the nametable
            int tileX = tileIndex % 32; // calculate the X coordinate of the tile within the nametable
            int tileY = tileIndex / 32; // calculate the Y coordinate of the tile within the nametable


            // Find the attribute byte
            int attributeByteAddress = attributeTableAddress + (tileY / 4) * 8 + (tileX / 4);
            byte attributeByte = ReadVRAM((ushort)attributeByteAddress);

            // Extract the correct bits
            int offset = ((tileY % 4) / 2) * 2 + (tileX % 4) / 2;
            int mask = 3 << offset;
            byte palette = (byte)((attributeByte & mask) >> offset);

            return palette;
        }

        // Get the color for a single pixel based on the pixel data and attribute data
        byte[] GetPixelColor(byte pixelData, byte attributeData)
        {
            int paletteIndex = (pixelData & 0x03); // Mask the pixel data to ensure it's 2 bits

            // Apply the attribute data to determine the correct palette index
            int paletteOffset = (attributeData & 0x03) * 4;

            // Fetch the color from the correct palette and color index
            int finalPaletteIndex = PaletteMemoryAddress + paletteOffset + paletteIndex;
            byte paletteColor = ReadVRAM((ushort)finalPaletteIndex);

            return ColorMap.LUT[paletteColor];
        }

        // Render a single pixel at the specified position
        void RenderPixel(int x, int y)
        {
            // Calculate the name table address for the current coordinates
            int nameTableAddress = CalculateNameTableAddress(x, y);

            // Read the tile ID from the name table
            byte tileID = ReadVRAM((ushort)nameTableAddress);
            if (tileID == 0x45)
                Console.WriteLine();

            // Calculate the tile position within the tile (0-7)
            int tileX = x % 8;
            int tileY = y % 8;

            // Fetch the pixel data for the current tile and position
            byte pixelData = FetchPixelData(tileID, tileY, tileX);

            // Fetch the attribute data for the tile
            byte attributeData = FetchAttributeData(nameTableAddress);

            // Get the color for the current pixel
            byte[] pixelColor = GetPixelColor(pixelData, attributeData);

            if (tileID == 0x45)
                Console.WriteLine(pixelData);

            // Calculate the index in the screen buffer based on the scanline and pixel position
            int index = (y * SCREEN_WIDTH * 3) + (x * 3);

            // Set the RGB values in the screen buffer at the calculated index
            screenBuffer[index] = pixelColor[2];     // Red component
            screenBuffer[index + 1] = pixelColor[1]; // Green component
            screenBuffer[index + 2] = pixelColor[0]; // Blue component
        }

        // Render the background
        void RenderBackground(int currentScanline)
        {
            for (int x = 0; x < SCREEN_WIDTH; x++) // Assuming a screen width of 256 pixels
            {
                RenderPixel(x, currentScanline);
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
                if (scanline < SCREEN_HEIGHT)
                    RenderBackground(scanline);

                // Render the sprites for the current scanline
                //RenderSprites(scanline);

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
