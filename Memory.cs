using System;

public class Memory
{
    private byte[] ram = new byte[0x0800]; // 2KB of RAM
    private byte[] prgRom; // PRG-ROM data
    private byte[] chrRom; // CHR-ROM data

    private PPU ppu;

    public Memory()
    {
        for (int i = 0; i < ram.Length; i++)
        {
            ram[i] = 0xFF;
        }
    }

    // Read a byte from the specified address in memory
    public byte Read(ushort address)
    {
        if (address < 0x2000)
        {
            // Access RAM
            return ram[address % 0x0800];
        }
        else if (address >= 0x2000 && address <= 0x3FFF)
        {
            // Access PPU registers
            return ppu.ReadRegister(address);
        }
        else if (address == 0x4014)
        {
            // Invalid read of register
            return ppu.ReadRegister(address);
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

    public void SetPPU(PPU ppu)
    {
        this.ppu = ppu;
    }

    // Set the PRG-ROM and CHR-ROM data
    public void SetROMData(byte[] prgRomData, byte[] chrRomData)
    {
        prgRom = prgRomData;
        chrRom = chrRomData;
    }
}
