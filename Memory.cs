namespace Emulation
{
    public class Memory
    {
        private readonly byte[] ram = new byte[0x0800]; // 2KB of RAM
        private byte[]? prgRom; // PRG-ROM data
        //private byte[] chrRom; // CHR-ROM data

        public PPU? ppu;

        public int mapper;
        public bool mirrorVertical;
        public bool fourScreenMirroring;

        public void Initialize(PPU ppu)
        {
            this.ppu = ppu;
            for (int i = 0; i < ram.Length; i++)
            {
                ram[i] = 0xFF;
            }
        }

        // Read a byte from the specified address in memory
        public byte Read(ushort address, bool isDebugRead = false)
        {
            if (address < 0x2000)
            {
                // Access RAM
                return ram[address % 0x0800];
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                // Access PPU registers
                return ppu.ReadRegister(address, isDebugRead);
            }
            else if (address == 0x4014)
            {
                // Invalid read of register
                return ppu.ReadRegister(address, isDebugRead);
            }
            else if (address >= 0x8000 && address < 0xC000)
            {
                // Access the first 16KB of PRG-ROM
                int prgRomAddress = address - 0x8000;
                return prgRom[prgRomAddress];
            }
            else if (address >= 0xC000 && address <= 0xFFFF)
            {
                // Access the last 16KB of PRG-ROM
                int prgRomAddress = address - 0xC000 + (prgRom.Length - 0x4000);
                return prgRom[prgRomAddress];
            }

            // Default to returning 0x00 if no specific handling is implemented
            return 0x00;
        }

        // Write a byte value to the specified address in memory
        public void Write(ushort address, byte value)
        {
            if (address < 0x2000)
            {
                // Write to RAM
                ram[address % 0x0800] = value;
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                // Write to PPU registers
                ppu.WriteRegister(address, value);
            }
            else if (address == 0x4014)
            {
                // Perform DMA transfer
                ppu.WriteRegister(address, value);
            }
            else if (address >= 0x8000 && address < 0xC000)
            {
                // Handle writes to the first 16KB of PRG-ROM
                int prgRomAddress = address - 0x8000;
                prgRom[prgRomAddress] = value;
            }
            else if (address >= 0xC000 && address <= 0xFFFF)
            {
                // Handle writes to the last 16KB of PRG-ROM
                int prgRomAddress = address - 0xC000 + (prgRom.Length - 0x4000);
                prgRom[prgRomAddress] = value;
            }
        }

        // Set the PRG-ROM and CHR-ROM data
        private void SetROMData(byte[] prgRomData, byte[] chrRomData, int mapperNumber, bool mirrorVertical, bool fourScreenMirroring)
        {
            this.mirrorVertical = mirrorVertical;
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
            prgRom = prgRomData;
        }

        public void LoadROM(string romFilePath)
        {
            byte[] romData = File.ReadAllBytes(romFilePath);

            // Check if the ROM file is in the iNES format
            bool iNESFormat = false;
            bool NES20Format = false;
            bool fourScreenMirroring = false;
            bool mirrorVertical = false;

            if (romData.Length >= 16 && romData[0] == 'N' && romData[1] == 'E' && romData[2] == 'S' && romData[3] == 0x1A)
            {
                iNESFormat = true;

                // Check if the ROM file is in the NES 2.0 format
                if ((romData[7] & 0x0C) == 0x08)
                {
                    NES20Format = true;
                }

                // Extract the mirroring type from the 6th byte of the ROM data
                byte flags6 = romData[5];
                byte flags8 = romData[7];

                if (NES20Format)
                {
                    switch (flags8 & 0x03)
                    {
                        case 0: // Horizontal Mirroring
                            mirrorVertical = false;
                            break;
                        case 1: // Vertical Mirroring
                            mirrorVertical = true;
                            break;
                        case 2: // Four Screen Mirroring
                            fourScreenMirroring = true;
                            break;
                        case 3: // Mapper Controlled
                                // This needs to be handled in your mapper implementation
                            break;
                    }
                }
                else if (iNESFormat)
                {
                    mirrorVertical = (flags6 & 0x01) == 1; // If the 0th bit is 1, it's vertical mirroring
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
            SetROMData(prgRomData, chrRomData, mapperNumber, mirrorVertical, fourScreenMirroring);
        }

        private void WriteCHRROMToVRAM_Mapper0(byte[] chrRomData)
        {
            // Write CHR-ROM data to VRAM for Mapper 0

            // CHR-ROM data size per bank in bytes
            int chrRomBankSize = 0x2000;

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
                    byte data = chrRomData[bank * chrRomBankSize + offset];
                    ppu.WriteVRAM(vramAddress, data);
                }
            }
        }
    }
}
