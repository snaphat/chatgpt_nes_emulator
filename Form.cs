using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Emulation
{
    public partial class Form : System.Windows.Forms.Form
    {
        private Emulator emulator = null!;

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AllocConsole();

        public Form()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();

            // Create an instance of the emulator and load the ROM
            const string romFilePath = "Background.nes"; /* Provide the path to the ROM file */
            emulator = new Emulator(romFilePath, this);

            // Setup the PictureBox
            pictureBox.Width = 256;
            pictureBox.Height = 240;
            pictureBox.BackColor = Color.Red;
            pictureBox.Paint += PictureBox1_Paint;

            // Start the CPU and PPU processing on the same thread threads
            Thread thread = new(emulator.Run);
            thread.Start();
        }

        private void PictureBox1_Paint(object? sender, PaintEventArgs e)
        {
            PPU ppu = emulator.GetPPU();
            byte[] screenBuffer = ppu.GetScreenBuffer();

            // Create a Bitmap object to hold the NES frame with Format32bppRgb pixel format
            Bitmap frameBitmap = new(256, 240, PixelFormat.Format24bppRgb);

            // Lock the bitmap data for faster manipulation
            BitmapData bitmapData = frameBitmap.LockBits(new Rectangle(0, 0, frameBitmap.Width, frameBitmap.Height),
                                                         ImageLockMode.WriteOnly, frameBitmap.PixelFormat);

            // Copy the pixel data from the screen buffer to the bitmap data
            Marshal.Copy(screenBuffer, 0, bitmapData.Scan0, screenBuffer.Length);

            // Unlock the bitmap data
            frameBitmap.UnlockBits(bitmapData);

            // Draw the NES frame onto the PictureBox
            e.Graphics.DrawImage(frameBitmap, 0, 0);
        }
    }
}
