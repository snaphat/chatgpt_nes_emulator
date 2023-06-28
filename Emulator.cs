namespace Emulation
{
    public class Emulator
    {
        private readonly CPU cpu;
        private readonly PPU ppu;
        private readonly PictureBox pictureBox;

        public bool isNmiPending;

        public Emulator(string romFilePath, PictureBox pictureBox)
        {
            // Initialize Memory, PPU, APU, and other components
            var memory = new Memory();
            cpu = new CPU();
            ppu = new PPU();
            this.pictureBox = pictureBox;

            memory.Initialize(ppu);
            memory.LoadROM(romFilePath);
            cpu.Initialize(this, memory, ppu);
            ppu.Initialize(this, memory);
        }

        public PPU GetPPU()
        {
            return ppu;
        }

        public void Run()
        {
            while (true)
            {
                // Execute a single CPU instruction
                cpu.ExecuteNextInstruction();

                // Execute three PPU cycles (since the PPU runs at three times the speed of the CPU)
                ppu.RenderCycle();
                ppu.RenderCycle();
                ppu.RenderCycle();

                // If we've completed a frame, render the screen
                if (ppu.ShouldRenderFrame())
                {
                    pictureBox.Invoke((MethodInvoker)delegate
                    {
                        pictureBox.Invalidate();
                    });
                }
            }
        }
    }
}
