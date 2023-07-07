
namespace Emulation
{
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using static Globals;

    public class PPU
    {
        private int dot;
        private int scanline;

        private readonly int[] oam = new int[OAM_SIZE]; // Object Attribute Memory
        private const int VRAM_SIZE = 0x4000;
        private readonly int[] vram = new int[VRAM_SIZE];

        // PPU registers
        public int ppuControl; // PPU Control Register (0x2000)
        private int ppuMask; // PPU Mask Register (0x2001)
        public int ppuStatus; // PPU Status Register (0x2002)
        private int oamAddress; // OAM Address Register (0x2003)
        private int ppudataBuffer; // Internal read buffer for PPUDATA

        // Screen buffer to store the rendered pixels
        private readonly Color32[] screenBuffer = new Color32[SCREEN_WIDTH * SCREEN_HEIGHT];
        private readonly int[] previousPaletteColor = new int[SCREEN_WIDTH * SCREEN_HEIGHT]; // for caching the previous palette color

        // PPU registers
        private int v; // Current VRAM address (15 bits)
        private int t; // Temporary VRAM address (15 bits)
        private int x; // Fine X scroll (3 bits)
        private bool w; // Write toggle flag

        // Open bus value
        private int openBus;

        // State to avoid recomputations
        private int backgroundPatternDataLo;
        private int backgroundPatternDataHi;
        private int backgroundPaletteTableOffset;
        private int backgroundPaletteTableOffsetNext;
        private int spriteHeight = 8;
        private readonly ulong[,] spritesPerDot = new ulong[SCREEN_HEIGHT, SCREEN_WIDTH];

        private Emulator emulator = null!;
        private Memory memory = null!;

        public void Initialize(Emulator emulator, Memory memory)
        {
            this.emulator = emulator;
            this.memory = memory;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color32[] GetScreenBuffer()
        {
            return screenBuffer;
        }

        // Read a byte from the specified PPU register
        public byte DebugReadRegister(uint address)
        {
            int temp = openBus;
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

            return (byte)temp;
        }

        // Read a byte from the specified PPU register
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadRegister(uint address)
        {
            switch (address & 0x7)
            {
                case 0x0: // PPU Control Register
                case 0x1: // PPU Mask Register
                    break;

                case 0x2: // PPU Status Register
                    // Read and clear the vertical blank flag in the status register
                    openBus = ppuStatus;
                    ppuStatus &= ~IN_VBLANK_FLAG;

                    // Reset the address latch
                    w = false;
                    break;

                case 0x3: // OAM Address Register
                    break;

                case 0x4: // OAM Data Register
                    openBus = oam[oamAddress];
                    break;

                case 0x5: // PPU Scroll Register
                case 0x6: // PPU Address Register
                    break;

                case 0x7: // VRAM Data Register
                    if (v is >= 0x0000 and <= 0x3EFF)
                    {
                        // Read from internal read buffer and update the buffer with the new value
                        openBus = ppudataBuffer;
                        ppudataBuffer = vram[v];

                        // Increment the VRAM address based on the VRAM increment mode
                        v += (ppuControl & VRAM_ADDRESS_INCREMENT_FLAG) != 0 ? 32 : 1;
                    }
                    else
                    {
                        // Read directly from VRAM and update the internal buffer
                        openBus = vram[v];
                        ppudataBuffer = vram[v];

                        // Increment the VRAM address based on the VRAM increment mode
                        v += (ppuControl & VRAM_ADDRESS_INCREMENT_FLAG) != 0 ? 32 : 1;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteRegister(int address, int value)
        {
            openBus = value;
            switch (address & 0x7)
            {
                case 0x0: // PPU Control Register
                    ppuControl = 0xFC & value; // ignore bits 1-2 for storing ppuControl
                    int newSpriteHeight = (ppuControl & SPRITE_SIZE_FLAG) != 0 ? 16 : 8;
                    if (newSpriteHeight != spriteHeight)
                    {
                        if (newSpriteHeight == 16)
                            IncreaseCachedSpritesPerDot();
                        else
                            ReduceCachedSpritesPerDot();
                    }
                    spriteHeight = newSpriteHeight;

                    t = (t & 0xF3FF) | ((value & 0x03) << 10); // Update bits 10-11 of t with bits 1-2 of value
                    break;

                case 0x1: // PPU Mask Register
                    ppuMask = value;
                    break;

                case 0x3: // OAM Address Register
                    oamAddress = value;
                    break;

                case 0x4: // OAM Data Register
                    if (oam[oamAddress] != value)
                    {
                        int attributeIndex = oamAddress & 3;
                        if (attributeIndex == 0)
                        {
                            int spriteIndex = oamAddress >> 2;
                            int oldY = oam[oamAddress];
                            int newY = value;
                            int x = oam[oamAddress + 3];
                            CacheSpritesPerDot(spriteIndex, oldY, newY, x, x);
                        }
                        else if (attributeIndex == 3)
                        {
                            int spriteIndex = oamAddress >> 2;
                            int y = oam[oamAddress - 3];
                            int oldX = oam[oamAddress];
                            int newX = value;
                            CacheSpritesPerDot(spriteIndex, y, y, oldX, newX);
                        }

                        oam[oamAddress] = value;
                    }

                    oamAddress++;
                    oamAddress &= 0xFF;
                    break;

                case 0x5: // PPU Scroll Register
                    if (!w)
                    {
                        // First write to PPUSCROLL
                        x = value & 0x07; // Store fine X scroll
                        t = (t & 0xFFE0) | (value >> 3); // Update coarse Y scroll in temporary VRAM address (t)
                        w = true;
                    }
                    else
                    {
                        // Second write to PPUSCROLL
                        t = (t & 0x8FFF) | ((value & 0x07) << 12); // Update coarse X scroll in temporary VRAM address (t)
                        t = (t & 0xFC1F) | ((value & 0xF8) << 2); // Update fine Y scroll in temporary VRAM address (t)
                        w = false;
                    }
                    break;

                case 0x6: // PPU Address Register
                    if (!w)
                    {
                        // First write to PPUADDR
                        t = (t & 0x00FF) | ((value & 0x3F) << 8); // Clear upper bits of temporary VRAM address (t)
                        w = true;
                    }
                    else
                    {
                        // Second write to PPUADDR
                        t = (t & 0xFF00) | (value & 0xFF); // Preserve the lower bits of temporary VRAM address (t)
                        v = t; // Copy temporary VRAM address (t) to current VRAM address (v)
                        w = false;
                    }
                    break;

                case 0x7: // VRAM Data Register
                    WriteVRAM(v, value);
                    // Increment v after writing
                    v += ((ppuControl & VRAM_ADDRESS_INCREMENT_FLAG) != 0) ? 32 : 1;
                    // Handle wrapping
                    v &= 0x7FFF; // Apply a bitwise AND operation to limit the address within the VRAM address space
                    break;

                default:
                    // Invalid register address
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVRAM(int address, int value)
        {
            if (address >= PATTERN_TABLE_0_START && address < PATTERN_TABLE_0_END)
            {
                vram[address] = value;
            }
            else if (address >= PATTERN_TABLE_1_START && address < PATTERN_TABLE_1_END)
            {
                vram[address] = value;
            }
            else if (address >= NAME_TABLE_0_START && address < NAME_TABLE_0_END)
            {
                vram[address] = value;
                if (memory.mirrorArrangement == HORIZONTAL_MIRRORING)
                {
                    // Mirror horizontally - left to right
                    vram[address + NAME_TABLE_SIZE] = value;
                }
                else if (memory.mirrorArrangement == VERTICAL_MIRRORING)
                {
                    // Mirror vertically - top to bottom
                    vram[address + (NAME_TABLE_SIZE * 2)] = value;
                }
            }
            else if (address >= NAME_TABLE_1_START && address < NAME_TABLE_1_END)
            {
                vram[address] = value;
                if (memory.mirrorArrangement == HORIZONTAL_MIRRORING)
                {
                    // Mirror horizontally - right to left
                    vram[address - NAME_TABLE_SIZE] = value;
                }
                else if (memory.mirrorArrangement == VERTICAL_MIRRORING)
                {
                    // Mirror vertically - bottom to top
                    vram[address + (NAME_TABLE_SIZE * 2)] = value;
                }
            }
            else if (address >= NAME_TABLE_2_START && address < NAME_TABLE_2_END)
            {
                vram[address] = value;
                if (memory.mirrorArrangement == HORIZONTAL_MIRRORING)
                {
                    // Mirror horizontally - left to right
                    vram[address + NAME_TABLE_SIZE] = value;
                }
                else if (memory.mirrorArrangement == VERTICAL_MIRRORING)
                {
                    // Mirror vertically - top to bottom
                    vram[address - (NAME_TABLE_SIZE * 2)] = value;
                }
            }
            else if (address >= NAME_TABLE_3_START && address < NAME_TABLE_3_END)
            {
                vram[address] = value;
                if (memory.mirrorArrangement == HORIZONTAL_MIRRORING)
                {
                    // Mirror horizontally - right to left
                    vram[address - NAME_TABLE_SIZE] = value;
                }
                else if (memory.mirrorArrangement == VERTICAL_MIRRORING)
                {
                    // Mirror vertically - bottom to top
                    vram[address - (NAME_TABLE_SIZE * 2)] = value;
                }
            }
            else if (address >= PALETTE_TABLE_START && address < PALETTE_TABLE_END)
            {
                // Handle mirroring in Palette Table
                vram[address] = value;
                if ((address & 3) == 0)
                {
                    // 0x3F00, 0x3F04, 0x3F08, 0x3F0C mirror to 0x3F10, 0x3F14, 0x3F18, 0x3F1C
                    var mirrorAddress = address ^ 0x10;
                    vram[mirrorAddress] = value;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReduceCachedSpritesPerDot()
        {
            for (int i = 0; i < 64; i++)
            {
                int yAddress = oam[i * 4] + 1;
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
                int yAddress = oam[i * 4] + 1;
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
            oldY++; // Sprites are rendered with 1 scanline delay
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
            newY++; // Sprites are rendered with 1 scanline delay
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenderCycle()
        {
            // Check if we're rendering a visible scanline
            if (scanline < SCREEN_HEIGHT)
            {
                if ((ppuMask & (SHOW_BACKGROUND | SHOW_SPRITES)) != 0)
                {
                    // Perform cycle-based rendering operations here
                    var backgroundPaletteIndex = 0;
                    var paletteColor = 0;

                    // Render a pixel for each dot on a visible scanline
                    if (dot < SCREEN_WIDTH)
                    {
                        // Render background
                        if ((ppuMask & SHOW_BACKGROUND) != 0 && (dot >= 8 || (ppuMask & SHOW_BACKGROUND_IN_LEFTMOST_8_PIXELS) != 0))
                        {
                            // Preload the pattern table data the start of a line
                            if (dot == 0)
                            {
                                // Calculate the name table address for the current coordinates
                                var nameTableAddress = NAME_TABLE_0_START | (v & 0x0FFF);

                                // Compute the tile index
                                var tileIndex = vram[nameTableAddress];

                                // Fetch the pixel data for the current tile and position
                                var patternTableAddress = ((ppuControl & BACKGROUND_PATTERN_TABLE_ADDRESS_FLAG) != 0 ? 0x1000 : 0x0000) | (tileIndex << 4) | (v >> 12); // Use the fine Y scroll for the row within the tile

                                // Get pattern table bytes
                                backgroundPatternDataLo = vram[patternTableAddress] << 8;
                                backgroundPatternDataHi = vram[patternTableAddress + 8] << 8;

                                // Compute the attribute table address
                                var attributeTableAddress = (nameTableAddress & 0x3c00) | 0x3C0 | ((nameTableAddress & 0x380) >> 4) | ((nameTableAddress & 0x1C) >> 2);

                                // Read the attribute byte
                                var backgroundPaletteAttributeByte = vram[attributeTableAddress];

                                // Extract the correct bits
                                backgroundPaletteTableOffsetNext = PALETTE_TABLE_START + (((backgroundPaletteAttributeByte >> ((((nameTableAddress & 0x40) >> 6) * 2) + ((nameTableAddress & 0x02) >> 1)) * 2) & 0x03) * 4);
                            }

                            var dotDiv8 = dot & 0x7;
                            if (dotDiv8 == 0) // Load the next table data Every 8th dot, which is every 8 pixels
                            {
                                // Calculate the name table address for the next coordinates
                                var nextNameTableAddress = NAME_TABLE_0_START | ((v + 1) & 0x1F) | (((v & ~0x1F) ^ (((v & 0x1F) == 0x1F) ? 0x400 : 0)) & 0xFFF);

                                // Compute the tile index
                                var tileIndex = vram[nextNameTableAddress];

                                // Fetch the pixel data for the current tile and position
                                var patternTableAddress = ((ppuControl & BACKGROUND_PATTERN_TABLE_ADDRESS_FLAG) != 0 ? 0x1000 : 0x0000) | (tileIndex << 4) | (v >> 12); // Use the fine Y scroll for the row within the tile

                                // Get pattern table bytes
                                backgroundPatternDataLo |= vram[patternTableAddress];
                                backgroundPatternDataHi |= vram[patternTableAddress + 8];

                                // Compute the attribute table address
                                var attributeTableAddress = (nextNameTableAddress & 0x3c00) | 0x3C0 | ((nextNameTableAddress & 0x380) >> 4) | ((nextNameTableAddress & 0x1C) >> 2);

                                // Read the attribute byte
                                var backgroundPaletteAttributeByte = vram[attributeTableAddress];

                                backgroundPaletteTableOffset = backgroundPaletteTableOffsetNext;

                                // Extract the correct bits
                                backgroundPaletteTableOffsetNext = PALETTE_TABLE_START + (((backgroundPaletteAttributeByte >> ((((nextNameTableAddress & 0x40) >> 6) * 2) + ((nextNameTableAddress & 0x02) >> 1)) * 2) & 0x03) * 4);
                            }

                            // Select the correct pixel within the tile
                            backgroundPaletteIndex = (((backgroundPatternDataHi << x) & 0x8000) >> 14) | (((backgroundPatternDataLo << x) & 0x8000) >> 15); // Use the fine X scroll for the column within the tile

                            // Shift the pattern data registers each cycle to mimic the hardware shift registers
                            backgroundPatternDataHi <<= 1;
                            backgroundPatternDataLo <<= 1;

                            // Fetch the color from the correct palette and color index
                            if (backgroundPaletteIndex == 0) // if index is 0, use the first entry of the palette table
                            {
                                paletteColor = vram[PALETTE_TABLE_START];
                            }
                            else if (dotDiv8 + x < 8) // Check for fine-x scroll
                            {
                                paletteColor = vram[backgroundPaletteTableOffset + backgroundPaletteIndex];
                            }
                            else
                            {
                                paletteColor = vram[backgroundPaletteTableOffsetNext + backgroundPaletteIndex];
                            }
                        }
                        // Render Sprites
                        if ((ppuMask & SHOW_SPRITES) != 0 && (dot >= 8 || (ppuMask & SHOW_SPRITES_IN_LEFTMOST_8_PIXELS) != 0))
                        {
                            ulong spriteMask = spritesPerDot[scanline, dot];

                            int spriteIndex = 0;
                            while (spriteMask != 0)
                            {
                                var trailingZeros = Unity.Mathematics.math.tzcnt(spriteMask);
                                spriteMask >>= trailingZeros;
                                spriteMask >>= 1; // Down-shift by 1 separately from trailing Zeros to avoid shifting by 64 bits, which will result in unexpected results.
                                spriteIndex += trailingZeros;

                                var oamEntry = spriteIndex * 4;

                                // Get sprite X, Y, sprite tile, and attributes from OAM
                                var spriteY = oam[oamEntry + 0] + 1; // Sprites are rendered with 1 scanline delay
                                var spriteTile = oam[oamEntry + 1];
                                var spriteAttributes = oam[oamEntry + 2];
                                var spriteX = oam[oamEntry + 3];

                                // Compute the tile row
                                var row = scanline - spriteY;

                                // Flip vertically if the attribute bit is set
                                if ((spriteAttributes & FLIP_SPRITE_VERTICALLY_FLAG) != 0)
                                    row = spriteHeight - 1 - row;

                                // Compute the pattern table address
                                var patternTableAddress = ((ppuControl & SPRITE_PATTERN_TABLE_ADDRESS_FLAG) != 0 ? 0x1000 : 0x0000) | (spriteTile << 4) | row;

                                // Read the pattern data
                                var spritePatternDataLo = vram[patternTableAddress];
                                var spritePatternDataHi = vram[patternTableAddress + 8];

                                // Flip horizontally if the attribute bit is set
                                var selectX = (spriteAttributes & FLIP_SPRITE_HORIZONTALLY_FLAG) != 0 ? 7 - (dot - spriteX) : dot - spriteX;

                                // Compute the pixel data
                                var spritePaletteIndex = (((spritePatternDataHi << selectX) & 0x80) >> 6) | (((spritePatternDataLo << selectX) & 0x80) >> 7);

                                // Skip transparent pixels (palette index 0)
                                if (spritePaletteIndex == 0)
                                {
                                    spriteIndex++;
                                    continue;
                                }

                                // Check for sprite 0 hit (check should be before checking sprite priority)
                                if (spriteIndex == 0 && backgroundPaletteIndex != 0)
                                {
                                    ppuStatus |= SPRITE0_HIT_FLAG;
                                }

                                // Check sprite priority
                                if (backgroundPaletteIndex != 0 && (spriteAttributes & SPRITE_PRIORITY_FLAG) != 0) // Priority (0: in front of background; 1: behind background)
                                {
                                    spriteIndex++;
                                    continue;
                                }

                                // Fetch the color for the sprite palette
                                paletteColor = vram[PALETTE_TABLE_SPRITE_START + ((spriteAttributes & SPRITE_PALETTE) << 2) + spritePaletteIndex];

                                break;
                            }
                        }

                        // Calculate the index in the palette buffer based on the scanline and pixel position
                        int index = ((SCREEN_HEIGHT - 1 - scanline) * SCREEN_WIDTH) + dot;

                        // Check if previous palette color has changed, if not don't update screen buffer
                        if (previousPaletteColor[index] != paletteColor)
                        {
                            previousPaletteColor[index] = paletteColor;

                            // Lookup pixel color
                            if (paletteColor != 0)
                            {
                                var pixelColor = ColorMap.LUT[paletteColor];

                                // Set the RGB values in the screen buffer at the calculated index
                                screenBuffer[index] = pixelColor;
                            }
                            else
                            {
                                // Set the RGB values in the screen buffer at the calculated index
                                screenBuffer[index] = new Color32(0, 0, 0, 255);
                            }
                        }

                        // Every 8 cycles (dots), increment v and start a new tile.
                        if ((dot & 7) == 7)
                        {
                            // Increment coarse X scroll
                            if ((v & 0x1F) == 31) // If coarse X == 31
                            {
                                v &= ~0x1F; // coarse X = 0
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
                            v &= ~0x7000; // fine Y = 0
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
                            v = (v & ~0x03E0) | (y << 5); // put coarse Y back into v
                        }
                    }
                    else if (dot == 257)
                    {
                        // At dot 257 of each scanline, copy horizontal position from t to v
                        // i.e., v: ....F.. ...EDCBA = t: ....F.. ...EDCBA
                        v = (v & ~0x041F) | (t & 0x041F);
                    }
                }
            }
            else if (scanline == 261 && (ppuMask & (SHOW_BACKGROUND | SHOW_SPRITES)) != 0)
            {
                if (dot is >= 280 and <= 304)
                {
                    // At dots 280 to 304 of the pre-render scanline, copy vertical position from t to v
                    // i.e., v: IHGF.ED CBA..... = t: IHGF.ED CBA.....
                    v = (v & ~0x7BE0) | (t & 0x7BE0);
                }

                if (dot >= 339)
                {
                    // At the end of the pre-render scanline, copy t into v
                    v = t;

                    // At the end of the pre-render scanline, clear sprite0 hit
                    ppuStatus &= ~SPRITE0_HIT_FLAG;
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

        bool shouldRender;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
