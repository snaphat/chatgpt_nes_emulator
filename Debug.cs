using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

public static class Debug
{
    public enum AddressingMode
    {
        Implicit,
        Immediate,
        ZeroPage,
        ZeroPageX,
        ZeroPageY,
        Relative,
        Absolute,
        AbsoluteX,
        AbsoluteY,
        Indirect,
        IndexedIndirect,
        IndirectIndexed
        // Add other addressing modes if needed
    }

    public static readonly Dictionary<byte, (string mnemonic, AddressingMode mode)> opcodeMap = new()
    {
        { 0x00, ("BRK", AddressingMode.Implicit) },
        { 0x01, ("ORA", AddressingMode.IndexedIndirect) }, //  (Indirect, X)
        { 0x05, ("ORA", AddressingMode.ZeroPage) }, // Zero Page
        { 0x06, ("ASL", AddressingMode.ZeroPage) }, // Zero Page
        { 0x08, ("PHP", AddressingMode.Implicit) },
        { 0x09, ("ORA", AddressingMode.Immediate) }, // Immediate 
        { 0x0A, ("ASL", AddressingMode.Implicit) }, // Accumulator
        { 0x0D, ("ORA", AddressingMode.Absolute) }, // Absolute
        { 0x0E, ("ASL", AddressingMode.Absolute) }, // Absolute
        { 0x10, ("BPL", AddressingMode.Relative) }, // Relative
        { 0x11, ("ORA", AddressingMode.IndirectIndexed) }, //  (Indirect), Y
        { 0x15, ("ORA", AddressingMode.ZeroPageX) }, // Zero Page, X 
        { 0x16, ("ASL", AddressingMode.ZeroPageX) }, // Zero Page, X 
        { 0x18, ("CLC", AddressingMode.Implicit) },
        { 0x19, ("ORA", AddressingMode.AbsoluteY) }, // Absolute, Y
        { 0x1D, ("ORA", AddressingMode.AbsoluteX) }, // Absolute, X
        { 0x1E, ("ASL", AddressingMode.AbsoluteX) }, // Absolute, X
        { 0x20, ("JSR", AddressingMode.Absolute) }, // Absolute
        { 0x21, ("AND", AddressingMode.IndexedIndirect) }, //  (Indirect, X)
        { 0x24, ("BIT", AddressingMode.ZeroPage) }, // Zero Page
        { 0x25, ("AND", AddressingMode.ZeroPage) }, // Zero Page
        { 0x26, ("ROL", AddressingMode.ZeroPage) }, // Zero Page
        { 0x28, ("PLP", AddressingMode.Implicit) },
        { 0x29, ("AND", AddressingMode.Immediate) }, // Immediate 
        { 0x2A, ("ROL", AddressingMode.Implicit) }, // Accumulator
        { 0x2C, ("BIT", AddressingMode.Absolute) }, // Absolute
        { 0x2D, ("AND", AddressingMode.Absolute) }, // Absolute
        { 0x2E, ("ROL", AddressingMode.Absolute) }, // Absolute
        { 0x30, ("BMI", AddressingMode.Relative) }, // Relative
        { 0x31, ("AND", AddressingMode.IndirectIndexed) }, //  (Indirect), Y
        { 0x35, ("AND", AddressingMode.ZeroPageX) }, // Zero Page, X 
        { 0x36, ("ROL", AddressingMode.ZeroPageX) }, // Zero Page, X 
        { 0x38, ("SEC", AddressingMode.Implicit) },
        { 0x39, ("AND", AddressingMode.AbsoluteY) }, // Absolute, Y
        { 0x3D, ("AND", AddressingMode.AbsoluteX) }, // Absolute, X
        { 0x3E, ("ROL", AddressingMode.AbsoluteX) }, // Absolute, X
        { 0x40, ("RTI", AddressingMode.Implicit) },
        { 0x41, ("EOR", AddressingMode.IndexedIndirect) }, //  (Indirect, X)
        { 0x45, ("EOR", AddressingMode.ZeroPage) }, // Zero Page
        { 0x46, ("LSR", AddressingMode.ZeroPage) }, // Zero Page
        { 0x48, ("PHA", AddressingMode.Implicit) },
        { 0x49, ("EOR", AddressingMode.Immediate) }, // Immediate 
        { 0x4A, ("LSR", AddressingMode.Implicit) }, // Accumulator
        { 0x4C, ("JMP", AddressingMode.Absolute) }, // Absolute
        { 0x4D, ("EOR", AddressingMode.Absolute) }, // Absolute
        { 0x4E, ("LSR", AddressingMode.Absolute) }, // Absolute
        { 0x50, ("BVC", AddressingMode.Relative) }, // Relative
        { 0x51, ("EOR", AddressingMode.IndirectIndexed) }, //  (Indirect), Y
        { 0x55, ("EOR", AddressingMode.ZeroPageX) }, // Zero Page, X 
        { 0x56, ("LSR", AddressingMode.ZeroPageX) }, // Zero Page, X 
        { 0x58, ("CLI", AddressingMode.Implicit) },
        { 0x59, ("EOR", AddressingMode.AbsoluteY) }, // Absolute, Y
        { 0x5D, ("EOR", AddressingMode.AbsoluteX) }, // Absolute, X
        { 0x5E, ("LSR", AddressingMode.AbsoluteX) }, // Absolute, X
        { 0x60, ("RTS", AddressingMode.Implicit) },
        { 0x61, ("ADC", AddressingMode.IndexedIndirect) }, //  (Indirect, X)
        { 0x65, ("ADC", AddressingMode.ZeroPage) }, // Zero Page
        { 0x66, ("ROR", AddressingMode.ZeroPage) }, // Zero Page
        { 0x68, ("PLA", AddressingMode.Implicit) },
        { 0x69, ("ADC", AddressingMode.Immediate) }, // Immediate 
        { 0x6A, ("ROR", AddressingMode.Implicit) }, // Accumulator
        { 0x6C, ("JMP", AddressingMode.Indirect) }, // (Indirect)
        { 0x6D, ("ADC", AddressingMode.Absolute) }, // Absolute
        { 0x6E, ("ROR", AddressingMode.Absolute) }, // Absolute
        { 0x70, ("BVS", AddressingMode.Relative) }, // Relative
        { 0x71, ("ADC", AddressingMode.IndirectIndexed) }, //  (Indirect), Y
        { 0x75, ("ADC", AddressingMode.ZeroPageX) }, // Zero Page, X 
        { 0x76, ("ROR", AddressingMode.ZeroPageX) }, // Zero Page, X 
        { 0x78, ("SEI", AddressingMode.Implicit) },
        { 0x79, ("ADC", AddressingMode.AbsoluteY) }, // Absolute, Y
        { 0x7D, ("ADC", AddressingMode.AbsoluteX) }, // Absolute, X
        { 0x7E, ("ROR", AddressingMode.AbsoluteX) }, // Absolute, X
        { 0x81, ("STA", AddressingMode.IndexedIndirect) }, //  (Indirect, X)
        { 0x84, ("STY", AddressingMode.ZeroPage) }, // Zero Page
        { 0x85, ("STA", AddressingMode.ZeroPage) }, // Zero Page
        { 0x86, ("STX", AddressingMode.ZeroPage) }, // Zero Page
        { 0x88, ("DEY", AddressingMode.Implicit) },
        { 0x8A, ("TXA", AddressingMode.Implicit) },
        { 0x8C, ("STY", AddressingMode.Absolute) }, // Absolute
        { 0x8D, ("STA", AddressingMode.Absolute) }, // Absolute
        { 0x8E, ("STX", AddressingMode.Absolute) }, // Absolute
        { 0x90, ("BCC", AddressingMode.Relative) }, // Relative
        { 0x91, ("STA", AddressingMode.IndirectIndexed) }, //  (Indirect), Y
        { 0x94, ("STY", AddressingMode.ZeroPageX) }, // Zero Page, X 
        { 0x95, ("STA", AddressingMode.ZeroPageX) }, // Zero Page, X 
        { 0x96, ("STX", AddressingMode.ZeroPageY) }, // Zero Page, Y
        { 0x98, ("TYA", AddressingMode.Implicit) },
        { 0x99, ("STA", AddressingMode.AbsoluteY) }, // Absolute, Y
        { 0x9A, ("TXS", AddressingMode.Implicit) },
        { 0x9D, ("STA", AddressingMode.AbsoluteX) }, // Absolute, X
        { 0xA0, ("LDY", AddressingMode.Immediate) }, // Immediate 
        { 0xA1, ("LDA", AddressingMode.IndexedIndirect) }, //  (Indirect, X)
        { 0xA2, ("LDX", AddressingMode.Immediate) }, // Immediate 
        { 0xA4, ("LDY", AddressingMode.ZeroPage) }, // Zero Page
        { 0xA5, ("LDA", AddressingMode.ZeroPage) }, // Zero Page
        { 0xA6, ("LDX", AddressingMode.ZeroPage) }, // Zero Page
        { 0xA8, ("TAY", AddressingMode.Implicit) },
        { 0xA9, ("LDA", AddressingMode.Immediate) }, // Immediate 
        { 0xAA, ("TAX", AddressingMode.Implicit) },
        { 0xAC, ("LDY", AddressingMode.Absolute) }, // Absolute
        { 0xAD, ("LDA", AddressingMode.Absolute) }, // Absolute
        { 0xAE, ("LDX", AddressingMode.Absolute) }, // Absolute
        { 0xB0, ("BCS", AddressingMode.Relative) }, // Relative
        { 0xB1, ("LDA", AddressingMode.IndirectIndexed) }, //  (Indirect), Y
        { 0xB4, ("LDY", AddressingMode.ZeroPageX) }, // Zero Page, X 
        { 0xB5, ("LDA", AddressingMode.ZeroPageX) }, // Zero Page, X 
        { 0xB6, ("LDX", AddressingMode.ZeroPageY) }, // Zero Page, Y
        { 0xB8, ("CLV", AddressingMode.Implicit) },
        { 0xB9, ("LDA", AddressingMode.AbsoluteY) }, // Absolute, Y
        { 0xBA, ("TSX", AddressingMode.Implicit) },
        { 0xBC, ("LDY", AddressingMode.AbsoluteX) }, // Absolute, X
        { 0xBD, ("LDA", AddressingMode.AbsoluteX) }, // Absolute, X
        { 0xBE, ("LDX", AddressingMode.AbsoluteY) }, // Absolute, Y
        { 0xC0, ("CPY", AddressingMode.Immediate) }, // Immediate 
        { 0xC1, ("CMP", AddressingMode.IndexedIndirect) }, //  (Indirect, X)
        { 0xC4, ("CPY", AddressingMode.ZeroPage) }, // Zero Page
        { 0xC5, ("CMP", AddressingMode.ZeroPage) }, // Zero Page
        { 0xC6, ("DEC", AddressingMode.ZeroPage) }, // Zero Page
        { 0xC8, ("INY", AddressingMode.Implicit) },
        { 0xC9, ("CMP", AddressingMode.Immediate) }, // Immediate 
        { 0xCA, ("DEX", AddressingMode.Implicit) },
        { 0xCC, ("CPY", AddressingMode.Absolute) }, // Absolute
        { 0xCD, ("CMP", AddressingMode.Absolute) }, // Absolute
        { 0xCE, ("DEC", AddressingMode.Absolute) }, // Absolute
        { 0xD0, ("BNE", AddressingMode.Relative) }, // Relative
        { 0xD1, ("CMP", AddressingMode.IndirectIndexed) }, //  (Indirect), Y
        { 0xD5, ("CMP", AddressingMode.ZeroPageX) }, // Zero Page, X 
        { 0xD6, ("DEC", AddressingMode.ZeroPageX) }, // Zero Page, X 
        { 0xD8, ("CLD", AddressingMode.Implicit) },
        { 0xD9, ("CMP", AddressingMode.AbsoluteY) }, // Absolute, Y
        { 0xDD, ("CMP", AddressingMode.AbsoluteX) }, // Absolute, X
        { 0xDE, ("DEC", AddressingMode.AbsoluteX) }, // Absolute, X
        { 0xE0, ("CPX", AddressingMode.Immediate) }, // Immediate 
        { 0xE1, ("SBC", AddressingMode.IndexedIndirect) }, //  (Indirect, X)
        { 0xE4, ("CPX", AddressingMode.ZeroPage) }, // Zero Page
        { 0xE5, ("SBC", AddressingMode.ZeroPage) }, // Zero Page
        { 0xE6, ("INC", AddressingMode.ZeroPage) }, // Zero Page
        { 0xE8, ("INX", AddressingMode.Implicit) },
        { 0xE9, ("SBC", AddressingMode.Immediate) }, // Immediate 
        { 0xEA, ("NOP", AddressingMode.Implicit) },
        { 0xEC, ("CPX", AddressingMode.Absolute) }, // Absolute
        { 0xED, ("SBC", AddressingMode.Absolute) }, // Absolute
        { 0xEE, ("INC", AddressingMode.Absolute) }, // Absolute
        { 0xF0, ("BEQ", AddressingMode.Relative) }, // Relative
        { 0xF1, ("SBC", AddressingMode.IndirectIndexed) }, //  (Indirect), Y
        { 0xF5, ("SBC", AddressingMode.ZeroPageX) }, // Zero Page, X 
        { 0xF6, ("INC", AddressingMode.ZeroPageX) }, // Zero Page, X 
        { 0xF8, ("SED", AddressingMode.Implicit) },
        { 0xF9, ("SBC", AddressingMode.AbsoluteY) }, // Absolute, Y
        { 0xFD, ("SBC", AddressingMode.AbsoluteX) }, // Absolute, X
        { 0xFE, ("INC", AddressingMode.AbsoluteX) } // Absolute, X
    };

    public static void DisplayInstruction(CPU cpu)
    {
        Console.WriteLine($"A:{cpu.A:X2} X:{cpu.X:X2} Y:{cpu.Y:X2} S:{cpu.SP:X2} P:{GetPFlags(cpu)} ${cpu.PC:X4}: {InstructionHexToString(cpu)}");
    }

    private static string GetPFlags(CPU cpu)
    {
        string flags = "";
        flags += cpu.N ? "N" : "n";
        flags += cpu.V ? "V" : "v";
        flags += 'u';
        flags += cpu.B ? "B" : "b";
        flags += cpu.D ? "D" : "d";
        flags += cpu.I ? "I" : "i";
        flags += cpu.Z ? "Z" : "z";
        flags += cpu.C ? "C" : "c";
        return flags;
    }

    public static string InstructionHexToString(CPU cpu)
    {
        byte opcode = cpu.ReadMemory(cpu.PC);

        if (opcodeMap.TryGetValue(opcode, out var opcodeData))
        {
            var (mnemonic, mode) = opcodeData;

            string instructionStr = mode switch
            {
                AddressingMode.Implicit => $"{opcode:X2}        {mnemonic}",
                AddressingMode.Immediate => $"{opcode:X2} {cpu.ReadMemory((ushort)(cpu.PC + 1)):X2}     {mnemonic} #{cpu.ReadMemory((ushort)(cpu.PC + 1)):X2}",
                AddressingMode.Absolute => $"{opcode:X2} {cpu.ReadMemory((ushort)(cpu.PC + 1)):X2} {cpu.ReadMemory((ushort)(cpu.PC + 2)):X2}  {mnemonic} ${BitConverter.ToUInt16(new[] { cpu.ReadMemory((ushort)(cpu.PC + 1)), cpu.ReadMemory((ushort)(cpu.PC + 2)) }):X4}",
                AddressingMode.Relative => $"{opcode:X2} {cpu.ReadMemory((ushort)(cpu.PC + 1)):X2}     {mnemonic} ${((ushort)(cpu.PC + 2) + (sbyte)cpu.ReadMemory((ushort)(cpu.PC + 1))):X4}",
                AddressingMode.AbsoluteX => $"{opcode:X2} {cpu.ReadMemory((ushort)(cpu.PC + 1)):X2} {cpu.ReadMemory((ushort)(cpu.PC + 2)):X2}  {mnemonic} ${BitConverter.ToUInt16(new[] { cpu.ReadMemory((ushort)(cpu.PC + 1)), cpu.ReadMemory((ushort)(cpu.PC + 2)) }):X4},X",
                AddressingMode.AbsoluteY => $"{opcode:X2} {cpu.ReadMemory((ushort)(cpu.PC + 1)):X2} {cpu.ReadMemory((ushort)(cpu.PC + 2)):X2}  {mnemonic} ${BitConverter.ToUInt16(new[] { cpu.ReadMemory((ushort)(cpu.PC + 1)), cpu.ReadMemory((ushort)(cpu.PC + 2)) }):X4},Y",
                AddressingMode.ZeroPage => $"{opcode:X2} {cpu.ReadMemory((ushort)(cpu.PC + 1)):X2}     {mnemonic} ${cpu.ReadMemory((ushort)(cpu.PC + 1)):X2}",
                AddressingMode.ZeroPageX => $"{opcode:X2} {cpu.ReadMemory((ushort)(cpu.PC + 1)):X2}     {mnemonic} ${cpu.ReadMemory((ushort)(cpu.PC + 1)):X2},X",
                AddressingMode.ZeroPageY => $"{opcode:X2} {cpu.ReadMemory((ushort)(cpu.PC + 1)):X2}     {mnemonic} ${cpu.ReadMemory((ushort)(cpu.PC + 1)):X2},Y",
                AddressingMode.IndexedIndirect => $"{opcode:X2} {cpu.ReadMemory((ushort)(cpu.PC + 1)):X2}     {mnemonic} (${cpu.ReadMemory((ushort)(cpu.PC + 1)):X2},X)",
                AddressingMode.IndirectIndexed => $"{opcode:X2} {cpu.ReadMemory((ushort)(cpu.PC + 1)):X2}     {mnemonic} (${cpu.ReadMemory((ushort)(cpu.PC + 1)):X2}),Y",
                // Handle other addressing modes...
                _ => $"Unknown addressing mode for opcode: {opcode:X2}"
            };

            if ((mnemonic == "STA" || mnemonic == "LDA" || mnemonic == "STX" || mnemonic == "LDX" || mnemonic == "STY" || mnemonic == "LDY") && mode != AddressingMode.Immediate)
            {
                ushort addressToPrint = GetAddressToPrint(cpu, mode);
                string previousValueStr = GetPreviousValueStr(cpu, addressToPrint);

                instructionStr = $"{instructionStr} = {previousValueStr}";
            }

            return instructionStr;
        }
        else
        {
            return $"Unknown opcode: {opcode:X2}";
        }
    }

    private static ushort GetAddressToPrint(CPU cpu, AddressingMode mode)
    {
        return mode switch
        {
            AddressingMode.ZeroPage => cpu.ReadMemory((ushort)(cpu.PC + 1)),
            AddressingMode.ZeroPageX => (ushort)(cpu.ReadMemory((ushort)(cpu.PC + 1)) + cpu.X),
            AddressingMode.ZeroPageY => (ushort)(cpu.ReadMemory((ushort)(cpu.PC + 1)) + cpu.Y),
            AddressingMode.Absolute => BitConverter.ToUInt16(new[] { cpu.ReadMemory((ushort)(cpu.PC + 1)), cpu.ReadMemory((ushort)(cpu.PC + 2)) }),
            AddressingMode.AbsoluteX => (ushort)(BitConverter.ToUInt16(new[] { cpu.ReadMemory((ushort)(cpu.PC + 1)), cpu.ReadMemory((ushort)(cpu.PC + 2)) }) + cpu.X),
            AddressingMode.AbsoluteY => (ushort)(BitConverter.ToUInt16(new[] { cpu.ReadMemory((ushort)(cpu.PC + 1)), cpu.ReadMemory((ushort)(cpu.PC + 2)) }) + cpu.Y),
            AddressingMode.IndexedIndirect => cpu.ReadMemory((ushort)(cpu.ReadMemory((ushort)(cpu.PC + 1)) + cpu.X)),
            AddressingMode.IndirectIndexed => (ushort)(cpu.ReadMemory(cpu.ReadMemory((ushort)(cpu.PC + 1))) + cpu.Y),
            // Handle other addressing modes...
            _ => throw new InvalidOperationException($"Invalid addressing mode for getting address to print: {mode}")
        };
    }

    private static string GetPreviousValueStr(CPU cpu, ushort address)
    {
        byte previousValue = cpu.ReadMemory(address);
        return $"#${previousValue:X2}";
    }
}
