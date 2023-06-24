namespace Emulation
{
    public class Memory
    {
        private readonly byte[] ram = new byte[0x0800]; // 2KB of RAM
        private byte[] prgRom; // PRG-ROM data
        //private byte[] chrRom; // CHR-ROM data

        public PPU? ppu;

        public int mapper;
        public bool mirrorVertical;
        public bool fourScreenMirroring;

        public Memory()
        {
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
                if (address == 0 && value == 4)
                {
                    var a = address % 0x0800;
                    a = a + a;
                }
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

        public void ClearStack()
        {
            Array.Clear(ram, 0x0100, 0x0100); // Clear the stack memory region
        }

        // Set the PRG-ROM and CHR-ROM data
        public void SetROMData(byte[] prgRomData, byte[] chrRomData, int mapperNumber, bool mirrorVertical, bool fourScreenMirroring)
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
