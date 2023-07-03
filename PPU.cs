﻿
namespace Emulation
{
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using static Globals;

    public class PPU
    {
        private int dot;
        private int scanline;

        private readonly byte[] oam = new byte[OAM_SIZE]; // Object Attribute Memory
        private const int VRAM_SIZE = 0x4000;
        private readonly byte[] vram = new byte[VRAM_SIZE];

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
        public byte fine_x = 0;
        private bool w; // Write toggle flag

        // Open bus value
        private byte openBus;

        // State to avoid recomputations
        private byte patternDataLo;
        private byte patternDataHi;
        private int paletteTableOffset;
        private int screenIndex;

        private int spriteHeight = 8;
        private readonly ulong[,] spritesPerDot = new ulong[SCREEN_HEIGHT, SCREEN_WIDTH];

        private Emulator emulator = null!;
        private Memory memory = null!;

        public void Initialize(Emulator emulator, Memory memory)
        {
            this.emulator = emulator;
            this.memory = memory;
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
                        temp = vram[v];
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
                        ppudataBuffer = vram[v];

                        // Increment the VRAM address based on the VRAM increment mode
                        v += (ushort)((ppuControl & VRAM_ADDRESS_INCREMENT_FLAG) != 0 ? 32 : 1);
                    }
                    else
                    {
                        // Read directly from VRAM and update the internal buffer
                        openBus = vram[v];
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
                    ppuControl = (byte)(0xFC & value); // ignore bits 1-2 for storing ppuControl
                    int newSpriteHeight = (ppuControl & SPRITE_SIZE_FLAG) != 0 ? 16 : 8;
                    if (newSpriteHeight != spriteHeight)
                    {
                        if (newSpriteHeight == 16)
                            IncreaseCachedSpritesPerDot();
                        else
                            ReduceCachedSpritesPerDot();
                    }
                    spriteHeight = newSpriteHeight;

                    t = (ushort)((t & 0xF3FF) | ((value & 0x03) << 10)); // Update bits 10-11 of t with bits 1-2 of value
                    break;

                case 0x2001: // PPU Mask Register
                    ppuMask = value;
                    break;

                case 0x2003: // OAM Address Register
                    oamAddress = value;
                    break;

                case 0x2004: // OAM Data Register
                    int spriteIndex = oamAddress / 4;
                    int attributeIndex = oamAddress % 4;
                    if (attributeIndex == 0 && oam[oamAddress] != value) // Check if the Y address has changed
                    {
                        int oldY = oam[oamAddress];
                        int newY = value;
                        int x = oam[oamAddress + 3];
                        CacheSpritesPerDot(spriteIndex, oldY, newY, x, x);
                    }
                    else if (attributeIndex == 3 && oam[oamAddress] != value) // Check if the X address has changed
                    {
                        int y = oam[oamAddress - 3];
                        int oldX = oam[oamAddress];
                        int newX = value;
                        CacheSpritesPerDot(spriteIndex, y, y, oldX, newX);
                    }

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

        public void WriteVRAM(ushort address, byte value)
        {
            if (address >= PATTERN_TABLE_0_START && address < PATTERN_TABLE_0_END)
            {
                vram[address] = value;
            }
            else if (address >= PATTERN_TABLE_1_START && address < PATTERN_TABLE_1_END)
            {
                vram[address] = value;
            }
            else if ((address >= NAME_TABLE_0_START && address < NAME_TABLE_0_END) || (address >= NAME_TABLE_2_START && address < NAME_TABLE_2_END))
            {
                if (memory.mirrorArrangement == HORIZONTAL_MIRRORING)
                {
                    // Mirror horizontally - left to right
                    vram[address] = value;
                    vram[address + NAME_TABLE_1_START - NAME_TABLE_0_START] = value;  // Horizontal Mirror
                }
                else
                {
                    // Mirror vertically - top to bottom
                    vram[address] = value;
                    vram[address + NAME_TABLE_2_START - NAME_TABLE_0_START] = value;  // Vertical Mirror
                }
            }
            else if ((address >= NAME_TABLE_1_START && address < NAME_TABLE_1_END) || (address >= NAME_TABLE_3_START && address < NAME_TABLE_3_END))
            {
                if (memory.mirrorArrangement == HORIZONTAL_MIRRORING)
                {
                    // Mirror horizontally - right to left
                    vram[address] = value;
                    vram[address - NAME_TABLE_1_START + NAME_TABLE_0_START] = value;  // Horizontal Mirror
                }
                else
                {
                    // Mirror vertically - bottom to top
                    vram[address] = value;
                    vram[address - NAME_TABLE_3_START + NAME_TABLE_1_START] = value;  // Vertical Mirror
                }
            }
            else if (address >= PALETTE_TABLE_START && address < PALETTE_TABLE_END)
            {
                // Handle mirroring in Palette Table
                if (address % 4 == 0)
                {
                    // 0x3F00, 0x3F04, 0x3F08, 0x3F0C mirror to 0x3F10, 0x3F14, 0x3F18, 0x3F1C
                    ushort mirrorAddress = (ushort)(address ^ 0x10);
                    vram[address] = value;
                    vram[mirrorAddress] = value;
                }
                else
                {
                    vram[address] = value;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReduceCachedSpritesPerDot()
        {
            for (int i = 0; i < 64; i++)
            {
                int yAddress = oam[i * 4];
                int xAddress = oam[(i * 4) + 3];
                ulong bitmask = ~(1UL << i); // Create a bitmask with the i-th bit cleared

                for (int y = yAddress + 8; y < yAddress + 16 && y < SCREEN_HEIGHT; y++) // Ensure y is within the valid scanline range
                {
                    for (int x = xAddress; x < xAddress + 8 && x < SCREEN_WIDTH; x++) // Ensure x is within the valid dot range
                    {
                        spritesPerDot[y, x] &= bitmask;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncreaseCachedSpritesPerDot()
        {
            for (int i = 0; i < 64; i++)
            {
                int yAddress = oam[i * 4];
                int xAddress = oam[(i * 4) + 3];
                ulong bitmask = 1UL << i; // Create a bitmask with only the i-th bit set

                for (int y = yAddress + 8; y < yAddress + 16 && y < SCREEN_HEIGHT; y++) // Ensure y is within the valid scanline range
                {
                    for (int x = xAddress; x < xAddress + 8 && x < SCREEN_WIDTH; x++) // Ensure x is within the valid dot range
                    {
                        spritesPerDot[y, x] |= bitmask;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CacheSpritesPerDot(int i, int oldY, int newY, int oldX, int newX)
        {
            // Calculate the bit mask for the sprite index
            ulong spriteMask = 1UL << i;

            // Calculate the dot range based on old and new X and Y coordinates
            int endX = oldX + 8;
            if (SCREEN_WIDTH < endX) endX = SCREEN_WIDTH;
            int endY = oldY + spriteHeight;
            if (SCREEN_HEIGHT < endY) endY = SCREEN_HEIGHT;

            // Remove sprite index from all dots within the old valid range
            for (int y = oldY; y < endY; y++)
            {
                for (int x = oldX; x < endX; x++)
                {
                    spritesPerDot[y, x] &= ~spriteMask;
                }
            }

            // Calculate the dot range for the new X and Y coordinates
            endX = newX + 8;
            if (SCREEN_WIDTH < endX) endX = SCREEN_WIDTH;
            endY = newY + spriteHeight;
            if (SCREEN_HEIGHT < endY) endY = SCREEN_HEIGHT;

            // Add sprite index to all dots within the new valid range
            for (int y = newY; y < endY; y++)
            {
                for (int x = newX; x < endX; x++)
                {
                    spritesPerDot[y, x] |= spriteMask;
                }
            }
        }

        public void Start8PixelTile()
        {
            // Calculate the name table address for the current coordinates
            var nameTableAddress = (ushort)(NAME_TABLE_0_START | (v & 0x0FFF));

            // Compute the tile index
            var tileIndex = vram[nameTableAddress];

            // Fetch the pixel data for the current tile and position
            var patternTableAddress = ((ppuControl & BACKGROUND_PATTERN_TABLE_ADDRESS_FLAG) != 0 ? 0x1000 : 0x0000) | (tileIndex << 4) | (v >> 12); // Use the fine Y scroll for the row within the tile

            // Get pattern table bytes
            patternDataLo = vram[patternTableAddress];
            patternDataHi = vram[patternTableAddress + 8];

            // Compute the attribute table address
            var attributeTableAddress = (nameTableAddress & 0x3c00) | 0x3C0 | ((nameTableAddress >> 4) & 0x38) | ((nameTableAddress >> 2) & 0x07);

            // Read the attribute byte
            var attributeByte = vram[attributeTableAddress];

            // Extract the correct bits
            paletteTableOffset = PALETTE_TABLE_START + (((attributeByte >> ((((nameTableAddress & 0x40) >> 6) * 2) + ((nameTableAddress & 0x02) >> 1)) * 2) & 0x03) * 4);
        }

        public int RenderBackground()
        {
            // Select the correct pixel within the tile
            var paletteIndex = (((patternDataHi << x) & 0x80) >> 6) | (((patternDataLo << x) & 0x80) >> 7); // Use the fine X scroll for the column within the tile

            // Shift the pattern data registers each cycle to mimic the hardware shift registers
            patternDataHi <<= 1;
            patternDataLo <<= 1;

            // Check if the pixel is transparent
            if (paletteIndex == 0)
            {
                screenBuffer[screenIndex] = 0;     // Blue component
                screenBuffer[screenIndex + 1] = 0; // Green component
                screenBuffer[screenIndex + 2] = 0; // Red component
                return 0;
            }

            // Fetch the color from the correct palette and color index
            var paletteColor = vram[paletteTableOffset + paletteIndex];

            // Lookup pixel color
            var pixelColor = ColorMap.LUT[0];

            // Set the RGB values in the screen buffer at the calculated index
            screenBuffer[screenIndex] = pixelColor[2];     // Blue component
            screenBuffer[screenIndex + 1] = pixelColor[1]; // Green component
            screenBuffer[screenIndex + 2] = pixelColor[0]; // Red component

            // Return palette color before lookup
            return paletteColor;
        }

        public void RenderSprite(int backgroundPaletteColor)
        {
            ulong spriteMask = spritesPerDot[scanline, dot];

            int i = 0;
            while (spriteMask != 0)
            {
                int trailingZeros = BitOperations.TrailingZeroCount(spriteMask);
                spriteMask >>= trailingZeros + 1;
                i += trailingZeros;

                // Get sprite X and Y from OAM
                var spriteY = oam[(i * 4) + 0];
                var spriteX = oam[(i * 4) + 3];

                // Get sprite tile and attributes from OAM
                var spriteTile = oam[(i * 4) + 1];
                var spriteAttributes = oam[(i * 4) + 2];

                // Compute the tile row
                var row = scanline - spriteY;

                // Flip vertically if the attribute bit is set
                if ((spriteAttributes & FLIP_SPRITE_VERTICALLY_FLAG) != 0)
                    row = spriteHeight - 1 - row;

                // Compute the pattern table address
                var patternTableAddress = ((ppuControl & SPRITE_PATTERN_TABLE_ADDRESS_FLAG) != 0 ? 0x1000 : 0x0000) | (spriteTile << 4) | row;

                // Read the pattern data
                var patternDataLo = vram[patternTableAddress];
                var patternDataHi = vram[patternTableAddress + 8];

                // Flip horizontally if the attribute bit is set
                var x = (spriteAttributes & FLIP_SPRITE_HORIZONTALLY_FLAG) != 0 ? 7 - (dot - spriteX) : dot - spriteX;

                // Compute the pixel data
                var pixelData = ((patternDataHi >> (7 - x)) & 1) << 1 | ((patternDataLo >> (7 - x)) & 1);

                // Skip transparent pixels (palette index 0)
                if (pixelData == 0)
                {
                    i++;
                    continue;
                }
                // Check sprite priority
                var spriteIsBehindBackground = (spriteAttributes & SPRITE_PRIORITY_FLAG) != 0;
                if (spriteIsBehindBackground && backgroundPaletteColor != 0)
                {
                    i++;
                    continue;
                }

                // Compute the palette address
                var paletteAddress = PALETTE_TABLE_SPRITE_START + ((spriteAttributes & 0x03) << 2) + pixelData;

                // Fetch the color from the palette
                var paletteColor = vram[paletteAddress];

                // Check for sprite 0 hit
                if (i == 0 && backgroundPaletteColor != 0)
                {
                    ppuStatus |= SPRITE0_HIT_FLAG;
                }

                // Lookup pixel color
                var pixelColor = ColorMap.LUT[paletteColor];

                // Set the RGB values in the screen buffer at the calculated index
                screenBuffer[screenIndex] = pixelColor[2];     // Blue component
                screenBuffer[screenIndex + 1] = pixelColor[1]; // Green component
                screenBuffer[screenIndex + 2] = pixelColor[0]; // Red component

                break;
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
                        int tileOffset = dot % 8;

                        int backgroundPaletteColor = 0;

                        // Calculate the index in the screen buffer based on the scanline and pixel position
                        screenIndex = (scanline * SCREEN_WIDTH * 3) + (dot * 3);

                        if ((ppuMask & SHOW_BACKGROUND) != 0 && (dot >= 8 || (ppuMask & SHOW_BACKGROUND_IN_LEFTMOST_8_PIXELS) != 0))
                        {
                            // Select the correct pixel within the tile
                            var paletteIndex = (((patternDataHi << x) & 0x80) >> 6) | (((patternDataLo << x) & 0x80) >> 7); // Use the fine X scroll for the column within the tile

                            // Shift the pattern data registers each cycle to mimic the hardware shift registers
                            patternDataHi <<= 1;
                            patternDataLo <<= 1;

                            // Check if the pixel is transparent
                            if (paletteIndex == 0)
                            {
                                screenBuffer[screenIndex] = 0;     // Blue component
                                screenBuffer[screenIndex + 1] = 0; // Green component
                                screenBuffer[screenIndex + 2] = 0; // Red component
                            }
                            else
                            {
                                // Fetch the color from the correct palette and color index
                                backgroundPaletteColor = vram[paletteTableOffset + paletteIndex];

                                // Lookup pixel color
                                var pixelColor = ColorMap.LUT[backgroundPaletteColor];

                                // Set the RGB values in the screen buffer at the calculated index
                                screenBuffer[screenIndex] = pixelColor[2];     // Blue component
                                screenBuffer[screenIndex + 1] = pixelColor[1]; // Green component
                                screenBuffer[screenIndex + 2] = pixelColor[0]; // Red component
                            }
                        }
                        if ((ppuMask & SHOW_SPRITES) != 0 && (dot >= 8 || (ppuMask & SHOW_SPRITES_IN_LEFTMOST_8_PIXELS) != 0))
                            RenderSprite(backgroundPaletteColor);

                        // Every 8 cycles (dots), increment v and start a new tile.
                        if (tileOffset == 7)
                        {
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
                            // Start new 8x8 tile segment
                            Start8PixelTile();
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
