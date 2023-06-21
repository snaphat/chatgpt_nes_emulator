﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Emulation
{
    public partial class Form : System.Windows.Forms.Form
    {
        private Emulator emulator;

        private readonly object cpuLock = new object();
        private readonly object ppuLock = new object();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public Form()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();

            // Create an instance of the emulator and load the ROM
            string romFilePath = "scanline.nes"; /* Provide the path to the ROM file */
            emulator = new Emulator(romFilePath);

            // Setup the PictureBox
            pictureBox1.Width = 256;
            pictureBox1.Height = 240;
            pictureBox1.BackColor = Color.Red;
            pictureBox1.Paint += pictureBox1_Paint;

            emulator.pictureBox = pictureBox1;

            // Start the CPU and PPU processing on separate threads
            Thread cpuThread = new Thread(emulator.CPURun);
            Thread ppuThread = new Thread(emulator.PPURun);
            cpuThread.Start();
            ppuThread.Start();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            PPU ppu = emulator.GetPPU();
            byte[] screenBuffer = ppu.GetScreenBuffer();

            // Create a Bitmap object to hold the NES frame
            Bitmap frameBitmap = new Bitmap(256, 240, PixelFormat.Format8bppIndexed);

            // Lock the bitmap data for faster manipulation
            BitmapData bitmapData = frameBitmap.LockBits(new Rectangle(0, 0, frameBitmap.Width, frameBitmap.Height),
                                                        ImageLockMode.WriteOnly, frameBitmap.PixelFormat);

            // Copy the pixel data from the screen buffer to the bitmap data
            System.Runtime.InteropServices.Marshal.Copy(screenBuffer, 0, bitmapData.Scan0, screenBuffer.Length);

            // Unlock the bitmap data
            frameBitmap.UnlockBits(bitmapData);

            // Draw the NES frame onto the PictureBox
            e.Graphics.DrawImage(frameBitmap, 0, 0);
        }
    }
}
