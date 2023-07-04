namespace Emulation
{
    using System.Runtime.CompilerServices;
    using static Globals;
    public class Memory
    {
        //private byte[] chrRom; // CHR-ROM data

        public PPU ppu = null!;

        public int mapper;
        public int mirrorArrangement;
        public bool fourScreenMirroring;

        private readonly byte[] memory = new byte[0x10000]; // 64 KB, the size of NES addressable memory

        public void Initialize(PPU ppu)
        {
            this.ppu = ppu;
            for (int i = 0; i < memory.Length; i++)
            {
                memory[i] = 0xFF;
            }
        }

        // Read a byte from the specified address in memory
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte DebugRead(ushort address)
        {
            if (address < 0x2000)
            {
                // Access RAM
                return memory[address & 0x07FF];
            }
            else if (address <= 0x3FFF)
            {
                // Access PPU registers
                return ppu.DebugReadRegister(address);
            }
            // Access PRG-ROM
            return memory[address];
        }

        // Read a byte from the specified address in memory
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Read(ushort address)
        {
            if (address < 0x2000)
            {
                // Access RAM
                return memory[address & 0x07FF];
            }
            else if (address <= 0x3FFF)
            {
                // Access PPU registers
                return (byte)ppu.ReadRegister(address);
            }
            // Access PRG-ROM
            return memory[address];
        }

        // Write a byte value to the specified address in memory
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ushort address, byte value)
        {
            if (address < 0x2000)
            {
                // Write to RAM
                memory[address & 0x07FF] = value;
            }
            else if (address <= 0x3FFF)
            {
                // Write to PPU registers
                ppu.WriteRegister(address, value);
            }
        }

        // Set the PRG-ROM and CHR-ROM data
        private void SetROMData(byte[] prgRomData, byte[] chrRomData, int mapperNumber, int mirrorArrangement, bool fourScreenMirroring)
        {
            this.mirrorArrangement = mirrorArrangement;
            this.fourScreenMirroring = fourScreenMirroring;
            switch (mapperNumber)
            {
                case 0x00:
                    // Mapper 0 - Handle the logic to load data into VRAM for Mapper 0
                    // ...
                    mapper = 0;
                    WriteCHRROMToVRAM_Mapper0(chrRomData);
                    break;
                default:
                    throw new Exception($"Unsupported mapper: {mapperNumber}");
            }

            // Continue with the remaining code for setting PRG-ROM and CHR-ROM data
            if (prgRomData.Length == 0x4000)
            {
                Array.Copy(prgRomData, 0, memory, 0x8000, 0x4000); // Copy first 16KB to 0x8000
                Array.Copy(prgRomData, 0, memory, 0xC000, 0x4000); // Copy second 16KB to 0xC000
            }
            else if (prgRomData.Length == 0x8000)
            {
                Array.Copy(prgRomData, 0, memory, 0x8000, 0x8000); // Copy 32KB to 0x8000
            }
        }

        public void LoadROM(string romFilePath)
        {
            byte[] romData = File.ReadAllBytes(romFilePath);

            // Check if the ROM file is in the iNES format
            bool iNESFormat = false;
            bool NES20Format = false;
            int mirrorArrangement = default;

            if (romData.Length >= 16 && romData[0] == 'N' && romData[1] == 'E' && romData[2] == 'S' && romData[3] == 0x1A)
            {
                iNESFormat = true;

                // Check if the ROM file is in the NES 2.0 format
                if ((romData[7] & 0x0C) == 0x08)
                {
                    NES20Format = true;
                }

                // Extract the mirroring type from the 6th byte of the ROM data
                byte flags6 = romData[6];

                if (iNESFormat)
                {
                    switch (flags6 & 0x01)
                    {
                        case 0: // Horizontal Mirroring
                            mirrorArrangement = HORIZONTAL_MIRRORING;
                            break;
                        case 1: // Vertical Mirroring
                            mirrorArrangement = VERTICAL_MIRRORING;
                            break;
                    }
                }
            }

            // Extract the PRG-ROM and CHR-ROM data
            int prgRomOffset = iNESFormat ? 16 : 0;  // Adjust the offset based on the header format
            int prgRomSize = romData[4] * 16384;  // PRG-ROM size (in 16KB units)
            int chrRomSize = romData[5] * 8192;   // CHR-ROM size (in 8KB units)

            byte[] prgRomData = new byte[prgRomSize];
            Array.Copy(romData, prgRomOffset, prgRomData, 0, prgRomSize);

            byte[] chrRomData = new byte[chrRomSize];
            Array.Copy(romData, prgRomOffset + prgRomSize, chrRomData, 0, chrRomSize);

            // Determine the mapper number
            byte mapperNumber;

            if (iNESFormat)
            {
                mapperNumber = (byte)(((romData[6] & 0xF0) >> 4) | (romData[7] & 0xF0));
            }
            else if (NES20Format)
            {
                // Extract the mapper number from the NES 2.0 header
                mapperNumber = (byte)((romData[8] & 0x0F) | ((romData[9] & 0xF0) >> 4));
            }
            else
            {
                Console.WriteLine("Invalid ROM file format.");
                return;
            }

            // Set the PRG-ROM, CHR-ROM, mapper number and mirroring type in the memory
            SetROMData(prgRomData, chrRomData, mapperNumber, mirrorArrangement, fourScreenMirroring);
        }

        private void WriteCHRROMToVRAM_Mapper0(byte[] chrRomData)
        {
            // Write CHR-ROM data to VRAM for Mapper 0

            // CHR-ROM data size per bank in bytes
            const int chrRomBankSize = 0x2000;

            // Calculate the number of CHR-ROM banks
            int numChrRomBanks = chrRomData.Length / chrRomBankSize;

            // Write each CHR-ROM bank to VRAM
            for (int bank = 0; bank < numChrRomBanks; bank++)
            {
                int vramBankAddress = bank * chrRomBankSize;

                // Copy the CHR-ROM bank data to VRAM using the provided Write API
                for (int offset = 0; offset < chrRomBankSize; offset++)
                {
                    ushort vramAddress = (ushort)(vramBankAddress + offset);
                    byte data = chrRomData[(bank * chrRomBankSize) + offset];
                    ppu.WriteVRAM(vramAddress, data);
                }
            }
        }
    }
}
