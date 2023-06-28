namespace Emulation
{
    public class Emulator
    {
        private readonly CPU cpu;
        private readonly PPU ppu;
        private readonly Controller controller;
        private readonly Form form;

        public bool isNmiPending;

        public Emulator(string romFilePath, Form form)
        {
            // Initialize Memory, PPU, APU, and other components
            var memory = new Memory();
            cpu = new CPU();
            ppu = new PPU();
            controller = new Controller();
            this.form = form;

            memory.Initialize(ppu);
            memory.LoadROM(romFilePath);
            cpu.Initialize(this, memory, ppu, controller);
            ppu.Initialize(this, memory);

            // Add event handlers for key press and key release
            form.KeyDown += Form_KeyDown;
            form.KeyUp += Form_KeyUp;
        }

        private void Form_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    controller.SetButtonState(1, Controller.BUTTON_UP, true);
                    break;
                case Keys.Down:
                    controller.SetButtonState(1, Controller.BUTTON_DOWN, true);
                    break;
                case Keys.Left:
                    controller.SetButtonState(1, Controller.BUTTON_LEFT, true);
                    break;
                case Keys.Right:
                    controller.SetButtonState(1, Controller.BUTTON_RIGHT, true);
                    break;
                case Keys.Z:
                    controller.SetButtonState(1, Controller.BUTTON_B, true);
                    break;
                case Keys.X:
                    controller.SetButtonState(1, Controller.BUTTON_A, true);
                    break;
                case Keys.ShiftKey:
                    controller.SetButtonState(1, Controller.BUTTON_SELECT, true);
                    break;
                case Keys.ControlKey:
                    controller.SetButtonState(1, Controller.BUTTON_START, true);
                    break;
            }
        }

        private void Form_KeyUp(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    controller.SetButtonState(1, Controller.BUTTON_UP, false);
                    break;
                case Keys.Down:
                    controller.SetButtonState(1, Controller.BUTTON_DOWN, false);
                    break;
                case Keys.Left:
                    controller.SetButtonState(1, Controller.BUTTON_LEFT, false);
                    break;
                case Keys.Right:
                    controller.SetButtonState(1, Controller.BUTTON_RIGHT, false);
                    break;
                case Keys.Z:
                    controller.SetButtonState(1, Controller.BUTTON_B, false);
                    break;
                case Keys.X:
                    controller.SetButtonState(1, Controller.BUTTON_A, false);
                    break;
                case Keys.ShiftKey:
                    controller.SetButtonState(1, Controller.BUTTON_SELECT, false);
                    break;
                case Keys.ControlKey:
                    controller.SetButtonState(1, Controller.BUTTON_START, false);
                    break;
            }
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
                    form.pictureBox.Invoke((MethodInvoker)delegate
                    {
                        form.pictureBox.Invalidate();
                    });
                }
            }
        }
    }
}
