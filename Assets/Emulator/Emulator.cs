namespace Emulation
{
    public class Emulator
    {
        private readonly CPU cpu;
        private readonly PPU ppu;
        private readonly Controller controller;

        public bool isNmiPending;

        public Emulator(string romFilePath)
        {
            // Initialize Memory, PPU, APU, and other components
            var memory = new Memory();
            cpu = new CPU();
            ppu = new PPU();
            controller = new Controller();
            //this.form = form;

            memory.Initialize(ppu);
            memory.LoadROM(romFilePath);
            cpu.Initialize(this, memory, ppu, controller);
            ppu.Initialize(this, memory);
        }


        public PPU GetPPU()
        {
            return ppu;
        }

        public Controller GetController()
        {
            return controller;
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
                    break;
                }
            }
        }
    }
}
