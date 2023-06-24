namespace Emulation
{
    class Emulator
    {
        private readonly CPU cpu;
        private readonly PPU ppu;

        public Emulator(string romFilePath)
        {
            // Initialize Memory, PPU, APU, and other components
            var memory = new Memory();
            cpu = new CPU(memory);
            ppu = new PPU(memory, cpu);
            memory.ppu = ppu;

            LoadROM(romFilePath, memory);
            cpu.Initialize();
            ppu.Initialize();
        }

        private void LoadROM(string romFilePath, Memory memory)
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
            memory.SetROMData(prgRomData, chrRomData, mapperNumber, mirrorVertical, fourScreenMirroring);
        }

        public PPU GetPPU()
        {
            return ppu;
        }

        private readonly object cpuLock = new();
        private readonly object ppuLock = new();

        // CPU thread
        public void CPURun()
        {
            while (true)
            {
                lock (cpuLock)
                {
                    cpu.ExecuteNextInstruction();

                    // Check if an NMI is pending and handle it if necessary
                    if (ppu.isNmiPending)
                    {
                        cpu.HandleNMI();
                        ppu.isNmiPending = false;
                    }
                }

                //Thread.Sleep(1); // Add a delay to control the execution speed
            }
        }

        public PictureBox? pictureBox;

        // PPU thread
        public void PPURun()
        {
            while (true)
            {
                lock (ppuLock)
                {
                    // Render a frame using the PPU
                    ppu.RenderFrame(pictureBox);
                }

                // Trigger the PictureBox to be redrawn on the UI thread
                pictureBox.Invoke((MethodInvoker)delegate
                {
                    pictureBox.Invalidate();
                });
            }
        }
    }
}
