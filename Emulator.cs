namespace Emulation
{
    class Emulator
    {
        private readonly CPU cpu;
        private readonly Memory memory;
        private readonly PPU ppu;

        public Emulator(string romFilePath)
        {
            // Initialize Memory, PPU, APU, and other components
            memory = new Memory();
            LoadROM(romFilePath);

            // Initialize CPU with the Memory instance
            cpu = new CPU(memory);
            ppu = new PPU(memory, cpu);
            memory.SetPPU(ppu);
        }

        private void LoadROM(string romFilePath)
        {
            byte[] romData = File.ReadAllBytes(romFilePath);

            // Check if the ROM file is in the INES format
            if (romData.Length < 16 || romData[0] != 0x4E || romData[1] != 0x45 || romData[2] != 0x53 || romData[3] != 0x1A)
            {
                Console.WriteLine("Invalid ROM file. Please provide a ROM file in the INES format.");
                return;
            }

            // Determine the size of the PRG-ROM and CHR-ROM
            int prgRomSize = romData[4] * 16384;  // PRG-ROM size (in 16KB units)
            int chrRomSize = romData[5] * 8192;   // CHR-ROM size (in 8KB units)

            // Extract the PRG-ROM data
            byte[] prgRomData = new byte[prgRomSize];
            Array.Copy(romData, 16, prgRomData, 0, prgRomSize);

            // Extract the CHR-ROM data
            byte[] chrRomData = new byte[chrRomSize];
            Array.Copy(romData, 16 + prgRomSize, chrRomData, 0, chrRomSize);

            // Set the PRG-ROM and CHR-ROM data in the memory
            memory.SetROMData(prgRomData, chrRomData);
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
