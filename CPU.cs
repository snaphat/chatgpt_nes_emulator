using System;
using System.Net;

public class CPU
{
    // Registers
    private byte A; // Accumulator
    private byte X, Y; // General-purpose registers
    private byte SP; // Stack pointer
    private ushort PC; // Program counter
    private byte P; // Processor status register

    // Status Flags
    private bool C; // Carry flag
    private bool Z; // Zero flag
    private bool I; // Interrupt disable flag
    private bool D; // Decimal mode flag
    private bool B; // Break flag
    private bool N; // Negative flag

    // Other CPU components and functions
    private Memory memory;

    public CPU(Memory memory)
    {
        this.memory = memory;

        // Initialize registers and flags
        A = 0;
        X = 0;
        Y = 0;
        SP = 0xFD;
        P = 0x00; // Processor status register (default value)
        C = false;
        Z = false;
        I = true;
        D = false;
        B = false;
        N = false;

        // Set the program counter (PC) to the Reset Vector address
        byte lowByte = memory.Read(0xFFFC);
        byte highByte = memory.Read(0xFFFD);
        PC = (ushort)((highByte << 8) | lowByte);

        // Simulate power-on/reset behavior by skipping the first read cycle
        ReadMemory(PC);
        PC++;

        // Disable interrupts
        I = true;

        // Initialize stack memory (if applicable)
        memory.ClearStack();
    }

    // Functions to update status flags
    private void UpdateZeroFlag(byte value)
    {
        Z = value == 0;
    }

    private void UpdateNegativeFlag(byte value)
    {
        N = (value & 0x80) != 0;
    }

    private void UpdateZeroAndNegativeFlags(byte value)
    {
        UpdateZeroFlag(value);
        UpdateNegativeFlag(value);
    }

    private void UpdateCarryFlag(byte value1, byte value2)
    {
        C = value1 >= value2;
    }

    private void DisplayInstructionOpcode(ushort address, byte opcode)
    {
        string aHex = A.ToString("X2");
        string xHex = X.ToString("X2");
        string yHex = Y.ToString("X2");
        string sHex = SP.ToString("X2");
        string addressHex = address.ToString("X2"); // Convert the opcode to a two-digit hexadecimal string
        string opcodeHex = opcode.ToString("X2"); // Convert the opcode to a two-digit hexadecimal string
        Console.WriteLine("A:" + aHex + " X:" + xHex + " Y:" + yHex + " S:" + sHex + " $00:" + addressHex + ": " + Debug.OpToStr(opcode));
    }

    public int cycles;

    public void ExecuteNextInstruction()
    {
        byte opcode = ReadMemory(PC++);
        DisplayInstructionOpcode((ushort)(PC - 1), opcode);
        switch (opcode)
        {
            // Transfer instructions
            case 0xAA: // TAX
                TAX();
                break;
            case 0xA8: // TAY
                TAY();
                break;
            case 0xBA: // TSX
                TSX();
                break;
            case 0x8A: // TXA
                TXA();
                break;
            case 0x9A: // TXS
                TXS();
                break;
            case 0x98: // TYA
                TYA();
                break;
            case 0x68: // PLA
                PLA();
                break;
            case 0x48: // PHA
                PHA();
                break;
            case 0x28: // PLP
                PLP();
                break;
            case 0x08: // PHP
                PHP();
                break;

            // Load/Store Operations
            case 0xA9: // LDA Immediate
                LDA(Immediate());
                break;
            case 0xA5: // LDA Zero Page
                LDA(ZeroPage());
                break;
            case 0xAD: // LDA Absolute
                LDA(Absolute());
                break;
            case 0xB5: // LDA Zero Page, X
                LDA(ZeroPageX());
                break;
            case 0xBD: // LDA Absolute, X
                LDA(AbsoluteX());
                break;
            case 0xB9: // LDA Absolute, Y
                LDA(AbsoluteY());
                break;
            case 0xA1: // LDA Indirect, X
                LDA(IndirectX());
                break;
            case 0xB1: // LDA Indirect, Y
                LDA(IndirectY());
                break;
            case 0xA2: // LDX Immediate
                LDX(Immediate());
                break;
            case 0xA6: // LDX Zero Page
                LDX(ZeroPage());
                break;
            case 0xAE: // LDX Absolute
                LDX(Absolute());
                break;
            case 0xB6: // LDX Zero Page, Y
                LDX(ZeroPageY());
                break;
            case 0xBE: // LDX Absolute, Y
                LDX(AbsoluteY());
                break;
            case 0xA0: // LDY Immediate
                LDY(Immediate());
                break;
            case 0xA4: // LDY Zero Page
                LDY(ZeroPage());
                break;
            case 0xAC: // LDY Absolute
                LDY(Absolute());
                break;
            case 0xB4: // LDY Zero Page, X
                LDY(ZeroPageX());
                break;
            case 0xBC: // LDY Absolute, X
                LDY(AbsoluteX());
                break;
            case 0x85: // STA Zero Page
                STA(ZeroPage());
                break;
            case 0x8D: // STA Absolute
                STA(Absolute());
                break;
            case 0x95: // STA Zero Page, X
                STA(ZeroPageX());
                break;
            case 0x9D: // STA Absolute, X
                STA(AbsoluteX());
                break;
            case 0x99: // STA Absolute, Y
                STA(AbsoluteY());
                break;
            case 0x81: // STA Indirect, X
                STA(IndirectX());
                break;
            case 0x91: // STA Indirect, Y
                STA(IndirectY());
                break;
            case 0x86: // STX Zero Page
                STX(ZeroPage());
                break;
            case 0x8E: // STX Absolute
                STX(Absolute());
                break;
            case 0x96: // STX Zero Page, Y
                STX(ZeroPageY());
                break;
            case 0x84: // STY Zero Page
                STY(ZeroPage());
                break;
            case 0x8C: // STY Absolute
                STY(Absolute());
                break;
            case 0x94: // STY Zero Page, X
                STY(ZeroPageX());
                break;

            // Arithmetic Operations
            case 0x69: // ADC Immediate
                ADC(Immediate());
                break;
            case 0x65: // ADC Zero Page
                ADC(ZeroPage());
                break;
            case 0x6D: // ADC Absolute
                ADC(Absolute());
                break;
            case 0x75: // ADC Zero Page, X
                ADC(ZeroPageX());
                break;
            case 0x7D: // ADC Absolute, X
                ADC(AbsoluteX());
                break;
            case 0x79: // ADC Absolute, Y
                ADC(AbsoluteY());
                break;
            case 0x61: // ADC Indirect, X
                ADC(IndirectX());
                break;
            case 0x71: // ADC Indirect, Y
                ADC(IndirectY());
                break;
            case 0xE9: // SBC Immediate
                SBC(Immediate());
                break;
            case 0xE5: // SBC Zero Page
                SBC(ZeroPage());
                break;
            case 0xED: // SBC Absolute
                SBC(Absolute());
                break;
            case 0xF5: // SBC Zero Page, X
                SBC(ZeroPageX());
                break;
            case 0xFD: // SBC Absolute, X
                SBC(AbsoluteX());
                break;
            case 0xF9: // SBC Absolute, Y
                SBC(AbsoluteY());
                break;
            case 0xE1: // SBC Indirect, X
                SBC(IndirectX());
                break;
            case 0xF1: // SBC Indirect, Y
                SBC(IndirectY());
                break;
            case 0x29: // AND Immediate
                AND(Immediate());
                break;
            case 0x25: // AND Zero Page
                AND(ZeroPage());
                break;
            case 0x2D: // AND Absolute
                AND(Absolute());
                break;
            case 0x35: // AND Zero Page, X
                AND(ZeroPageX());
                break;
            case 0x3D: // AND Absolute, X
                AND(AbsoluteX());
                break;
            case 0x39: // AND Absolute, Y
                AND(AbsoluteY());
                break;
            case 0x21: // AND Indirect, X
                AND(IndirectX());
                break;
            case 0x31: // AND Indirect, Y
                AND(IndirectY());
                break;
            case 0x49: // EOR Immediate
                EOR(Immediate());
                break;
            case 0x45: // EOR Zero Page
                EOR(ZeroPage());
                break;
            case 0x4D: // EOR Absolute
                EOR(Absolute());
                break;
            case 0x55: // EOR Zero Page, X
                EOR(ZeroPageX());
                break;
            case 0x5D: // EOR Absolute, X
                EOR(AbsoluteX());
                break;
            case 0x59: // EOR Absolute, Y
                EOR(AbsoluteY());
                break;
            case 0x41: // EOR Indirect, X
                EOR(IndirectX());
                break;
            case 0x51: // EOR Indirect, Y
                EOR(IndirectY());
                break;
            case 0x09: // ORA Immediate
                ORA(Immediate());
                break;
            case 0x05: // ORA Zero Page
                ORA(ZeroPage());
                break;
            case 0x0D: // ORA Absolute
                ORA(Absolute());
                break;
            case 0x15: // ORA Zero Page, X
                ORA(ZeroPageX());
                break;
            case 0x1D: // ORA Absolute, X
                ORA(AbsoluteX());
                break;
            case 0x19: // ORA Absolute, Y
                ORA(AbsoluteY());
                break;
            case 0x01: // ORA Indirect, X
                ORA(IndirectX());
                break;
            case 0x11: // ORA Indirect, Y
                ORA(IndirectY());
                break;
            case 0x24: // BIT Zero Page
                BIT(ZeroPage());
                break;
            case 0x2C: // BIT Absolute
                BIT(Absolute());
                break;

            // Increment/Decrement Operations
            case 0xE6: // INC Zero Page
                INC(ZeroPage());
                break;
            case 0xEE: // INC Absolute
                INC(Absolute());
                break;
            case 0xF6: // INC Zero Page, X
                INC(ZeroPageX());
                break;
            case 0xFE: // INC Absolute, X
                INC(AbsoluteX());
                break;
            case 0xC6: // DEC Zero Page
                DEC(ZeroPage());
                break;
            case 0xCE: // DEC Absolute
                DEC(Absolute());
                break;
            case 0xD6: // DEC Zero Page, X
                DEC(ZeroPageX());
                break;
            case 0xDE: // DEC Absolute, X
                DEC(AbsoluteX());
                break;
            case 0xE8: // INX
                INX();
                break;
            case 0xC8: // INY
                INY();
                break;
            case 0xCA: // DEX
                DEX();
                break;
            case 0x88: // DEY
                DEY();
                break;

            // Shift Operations
            case 0x0A: // ASL Accumulator
                ASL_Accumulator();
                break;
            case 0x06: // ASL Zero Page
                ASL_ZeroPage();
                break;
            case 0x0E: // ASL Absolute
                ASL_Absolute();
                break;
            case 0x16: // ASL Zero Page, X
                ASL_ZeroPageX();
                break;
            case 0x1E: // ASL Absolute, X
                ASL_AbsoluteX();
                break;
            case 0x4A: // LSR Accumulator
                LSR_Accumulator();
                break;
            case 0x46: // LSR Zero Page
                LSR_ZeroPage();
                break;
            case 0x4E: // LSR Absolute
                LSR_Absolute();
                break;
            case 0x56: // LSR Zero Page, X
                LSR_ZeroPageX();
                break;
            case 0x5E: // LSR Absolute, X
                LSR_AbsoluteX();
                break;
            case 0x2A: // ROL Accumulator
                ROL_Accumulator();
                break;
            case 0x26: // ROL Zero Page
                ROL_ZeroPage();
                break;
            case 0x2E: // ROL Absolute
                ROL_Absolute();
                break;
            case 0x36: // ROL Zero Page, X
                ROL_ZeroPageX();
                break;
            case 0x3E: // ROL Absolute, X
                ROL_AbsoluteX();
                break;
            case 0x6A: // ROR Accumulator
                ROR_Accumulator();
                break;
            case 0x66: // ROR Zero Page
                ROR_ZeroPage();
                break;
            case 0x6E: // ROR Absolute
                ROR_Absolute();
                break;
            case 0x76: // ROR Zero Page, X
                ROR_ZeroPageX();
                break;
            case 0x7E: // ROR Absolute, X
                ROR_AbsoluteX();
                break;

            // Compare Operations
            case 0xC9: // CMP Immediate
                CMP(Immediate());
                break;
            case 0xC5: // CMP Zero Page
                CMP(ZeroPage());
                break;
            case 0xCD: // CMP Absolute
                CMP(Absolute());
                break;
            case 0xD5: // CMP Zero Page, X
                CMP(ZeroPageX());
                break;
            case 0xDD: // CMP Absolute, X
                CMP(AbsoluteX());
                break;
            case 0xD9: // CMP Absolute, Y
                CMP(AbsoluteY());
                break;
            case 0xC1: // CMP Indirect, X
                CMP(IndirectX());
                break;
            case 0xD1: // CMP Indirect, Y
                CMP(IndirectY());
                break;
            case 0xE0: // CPX Immediate
                CPX(Immediate());
                break;
            case 0xE4: // CPX Zero Page
                CPX(ZeroPage());
                break;
            case 0xEC: // CPX Absolute
                CPX(Absolute());
                break;
            case 0xC0: // CPY Immediate
                CPY(Immediate());
                break;
            case 0xC4: // CPY Zero Page
                CPY(ZeroPage());
                break;
            case 0xCC: // CPY Absolute
                CPY(Absolute());
                break;

            // Branching Operations
            case 0x10: // BPL
                BPL(Relative());
                break;
            case 0x30: // BMI
                BMI(Relative());
                break;
            case 0x90: // BCC
                BCC(Relative());
                break;
            case 0xB0: // BCS
                BCS(Relative());
                break;
            case 0xD0: // BNE
                BNE(Relative());
                break;
            case 0xF0: // BEQ
                BEQ(Relative());
                break;

            // Jump/Call Operations
            case 0x4C: // JMP Absolute
                JMP_Absolute();
                break;
            case 0x6C: // JMP Indirect
                JMP_Indirect();
                break;
            case 0x20: // JSR
                JSR(Absolute());
                break;
            case 0x60: // RTS
                RTS();
                break;
            case 0x40: // RTI
                RTI();
                break;

            // Status Flag Operations
            case 0x18: // CLC
                CLC();
                break;
            case 0x38: // SEC
                SEC();
                break;
            case 0x58: // CLI
                CLI();
                break;
            case 0x78: // SEI
                SEI();
                break;
            case 0xD8: // CLD
                CLD();
                break;
            case 0xF8: // SED
                SED();
                break;

            // System Functions
            case 0x00: // BRK
                BRK();
                break;
            case 0xEA: // NOP
                NOP();
                break;

            default:
                throw new NotImplementedException($"Opcode {opcode:X2} is not implemented.");
        }
        cycles++;
    }

    // Helper functions for reading from and writing to memory
    private byte ReadMemory(ushort address)
    {
        return memory.Read(address);
    }

    private void WriteMemory(ushort address, byte value)
    {
        memory.Write(address, value);
    }

    // Addressing Modes
    private byte Immediate()
    {
        return ReadMemory(PC++);
    }

    private ushort ZeroPage()
    {
        return ReadMemory(PC++);
    }

    private ushort ZeroPageX()
    {
        return (ushort)(ReadMemory(PC++) + X);
    }

    private ushort ZeroPageY()
    {
        return (ushort)(ReadMemory(PC++) + Y);
    }

    private ushort Absolute()
    {
        ushort address = ReadMemory(PC++);
        address |= (ushort)(ReadMemory(PC++) << 8);
        return address;
    }

    private ushort AbsoluteX()
    {
        ushort address = (ushort)(ReadMemory(PC++) | (ReadMemory(PC++) << 8));
        return (ushort)(address + X);
    }

    private ushort AbsoluteY()
    {
        ushort address = (ushort)(ReadMemory(PC++) | (ReadMemory(PC++) << 8));
        return (ushort)(address + Y);
    }

    private ushort Indirect()
    {
        ushort indirectAddress = ReadMemory(PC++);
        indirectAddress |= (ushort)(ReadMemory(PC++) << 8);
        ushort address = ReadMemory(indirectAddress);
        address |= (ushort)(ReadMemory((ushort)(indirectAddress + 1)) << 8);
        return address;
    }

    private ushort IndirectX()
    {
        byte zeroPageAddress = (byte)(ReadMemory(PC++) + X);
        ushort address = ReadMemory(zeroPageAddress);
        address |= (ushort)(ReadMemory((byte)(zeroPageAddress + 1)) << 8);
        return address;
    }

    private ushort IndirectY()
    {
        byte zeroPageAddress = ReadMemory(PC++);
        ushort address = ReadMemory(zeroPageAddress);
        address |= (ushort)(ReadMemory((byte)(zeroPageAddress + 1)) << 8);
        return (ushort)(address + Y);
    }

    private sbyte Relative()
    {
        return (sbyte)ReadMemory(PC++);
    }

    private void TAX()
    {
        X = A;
        UpdateZeroAndNegativeFlags(X);
    }

    private void TXA()
    {
        A = X;
        UpdateZeroAndNegativeFlags(X);
    }

    private void TAY()
    {
        Y = A;
        UpdateZeroAndNegativeFlags(X);
    }

    private void TYA()
    {
        A = Y;
        UpdateZeroAndNegativeFlags(X);
    }

    private void TSX()
    {
        X = SP;
        UpdateZeroAndNegativeFlags(X);
    }

    private void TXS()
    {
        SP = X;
    }

    private void PLA()
    {
        A = PopStack();
        UpdateZeroAndNegativeFlags(X);
    }

    private void PHA()
    {
        PushStack(A);
    }

    private void PLP()
    {
        P = (byte)(PopStack() | 0x20); // Set bit 5 (unused) to 1
    }

    private void PHP()
    {
        PushStack((byte)(P | 0x10)); // Set bit 4 (unused) to 1
    }

    // Arithmetic and Logical Operations
    private void LDA(byte value)
    {
        A = value;
        UpdateZeroAndNegativeFlags(A);
    }

    private void LDA(ushort address)
    {
        byte value = ReadMemory(address);
        A = value;
        UpdateZeroAndNegativeFlags(A);
    }

    private void LDX(byte value)
    {
        X = value;
        UpdateZeroAndNegativeFlags(X);
    }

    private void LDX(ushort address)
    {
        byte value = ReadMemory(address);
        X = value;
        UpdateZeroAndNegativeFlags(X);
    }

    private void LDY(byte value)
    {
        Y = value;
        UpdateZeroAndNegativeFlags(Y);
    }

    private void LDY(ushort address)
    {
        byte value = ReadMemory(address);
        Y = value;
        UpdateZeroAndNegativeFlags(Y);
    }

    private void STA(ushort address)
    {
        WriteMemory(address, A);
    }

    private void STX(ushort address)
    {
        WriteMemory(address, X);
    }

    private void STY(ushort address)
    {
        WriteMemory(address, Y);
    }

    private void ADC(byte value)
    {
        int sum = A + value + (C ? 1 : 0);
        C = sum > 0xFF;  // Update carry flag based on carry-out from bit 7
        A = (byte)sum;   // Store the lower 8 bits of the sum in the accumulator
        UpdateZeroAndNegativeFlags(A);
    }

    private void ADC(ushort address)
    {
        byte value = ReadMemory(address);
        ADC(value);
    }

    private void SBC(byte value)
    {
        int difference = A - value - (C ? 0 : 1);
        C = difference >= 0;
        A = (byte)(difference & 0xFF);
        UpdateZeroAndNegativeFlags(A);
    }

    private void SBC(ushort address)
    {
        byte value = ReadMemory(address);
        SBC(value);
    }

    private void AND(byte value)
    {
        A &= value;
        UpdateZeroAndNegativeFlags(A);
    }

    private void AND(ushort address)
    {
        byte value = ReadMemory(address);
        AND(value);
    }

    private void EOR(byte value)
    {
        A ^= value;
        UpdateZeroAndNegativeFlags(A);
    }

    private void EOR(ushort address)
    {
        byte value = ReadMemory(address);
        EOR(value);
    }

    private void ORA(byte value)
    {
        A |= value;
        UpdateZeroAndNegativeFlags(A);
    }

    private void ORA(ushort address)
    {
        byte value = ReadMemory(address);
        A |= value;
        UpdateZeroAndNegativeFlags(A);
    }

    private void BIT(byte value)
    {
        byte result = (byte)(A & value);
        UpdateZeroAndNegativeFlags(result);
    }

    private void BIT(ushort address)
    {
        byte value = ReadMemory(address);
        BIT(value);
    }

    // Increment and Decrement Operations
    private void INC(ushort address)
    {
        byte value = ReadMemory(address);
        value++;
        WriteMemory(address, value);
        UpdateZeroAndNegativeFlags(value);
    }

    private void DEC(ushort address)
    {
        byte value = ReadMemory(address);
        value--;
        WriteMemory(address, value);
        UpdateZeroAndNegativeFlags(value);
    }

    private void INX()
    {
        X++;
        UpdateZeroAndNegativeFlags(X);
    }

    private void INY()
    {
        Y++;
        UpdateZeroAndNegativeFlags(Y);
    }

    private void DEX()
    {
        X--;
        UpdateZeroAndNegativeFlags(X);
    }

    private void DEY()
    {
        Y--;
        UpdateZeroAndNegativeFlags(Y);
    }

    // Shift Operations
    private void ASL_Accumulator()
    {
        C = (A & 0x80) != 0;
        A <<= 1;
        UpdateZeroAndNegativeFlags(A);
    }

    private void ASL_ZeroPage()
    {
        byte address = ReadMemory(PC++);
        byte value = ReadMemory(address);
        C = (value & 0x80) != 0;
        value <<= 1;
        WriteMemory(address, value);
        UpdateZeroAndNegativeFlags(value);
    }

    private void ASL_Absolute()
    {
        ushort address = Absolute();
        byte value = ReadMemory(address);
        C = (value & 0x80) != 0;
        value <<= 1;
        WriteMemory(address, value);
        UpdateZeroAndNegativeFlags(value);
    }

    private void ASL_ZeroPageX()
    {
        byte address = (byte)(ReadMemory(PC++) + X);
        byte value = ReadMemory(address);
        C = (value & 0x80) != 0;
        value <<= 1;
        WriteMemory(address, value);
        UpdateZeroAndNegativeFlags(value);
    }

    private void ASL_AbsoluteX()
    {
        ushort address = (ushort)(Absolute() + X);
        byte value = ReadMemory(address);
        C = (value & 0x80) != 0;
        value <<= 1;
        WriteMemory(address, value);
        UpdateZeroAndNegativeFlags(value);
    }

    private void LSR_Accumulator()
    {
        C = (A & 0x01) != 0;
        A >>= 1;
        UpdateZeroAndNegativeFlags(A);
    }

    private void LSR_ZeroPage()
    {
        byte address = ReadMemory(PC++);
        byte value = ReadMemory(address);
        C = (value & 0x01) != 0;
        value >>= 1;
        WriteMemory(address, value);
        UpdateZeroAndNegativeFlags(value);
    }

    private void LSR_Absolute()
    {
        ushort address = Absolute();
        byte value = ReadMemory(address);
        C = (value & 0x01) != 0;
        value >>= 1;
        WriteMemory(address, value);
        UpdateZeroAndNegativeFlags(value);
    }

    private void LSR_ZeroPageX()
    {
        byte address = (byte)(ReadMemory(PC++) + X);
        byte value = ReadMemory(address);
        C = (value & 0x01) != 0;
        value >>= 1;
        WriteMemory(address, value);
        UpdateZeroAndNegativeFlags(value);
    }

    private void LSR_AbsoluteX()
    {
        ushort address = (ushort)(Absolute() + X);
        byte value = ReadMemory(address);
        C = (value & 0x01) != 0;
        value >>= 1;
        WriteMemory(address, value);
        UpdateZeroAndNegativeFlags(value);
    }

    private void ROL_Accumulator()
    {
        bool newC = (A & 0x80) != 0;
        A <<= 1;
        if (C)
            A |= 0x01;
        C = newC;
        UpdateZeroAndNegativeFlags(A);
    }

    private void ROL_ZeroPage()
    {
        byte address = ReadMemory(PC++);
        byte value = ReadMemory(address);
        bool newC = (value & 0x80) != 0;
        value <<= 1;
        if (C)
            value |= 0x01;
        WriteMemory(address, value);
        C = newC;
        UpdateZeroAndNegativeFlags(value);
    }

    private void ROL_Absolute()
    {
        ushort address = Absolute();
        byte value = ReadMemory(address);
        bool newC = (value & 0x80) != 0;
        value <<= 1;
        if (C)
            value |= 0x01;
        WriteMemory(address, value);
        C = newC;
        UpdateZeroAndNegativeFlags(value);
    }

    private void ROL_ZeroPageX()
    {
        byte address = (byte)(ReadMemory(PC++) + X);
        byte value = ReadMemory(address);
        bool newC = (value & 0x80) != 0;
        value <<= 1;
        if (C)
            value |= 0x01;
        WriteMemory(address, value);
        C = newC;
        UpdateZeroAndNegativeFlags(value);
    }

    private void ROL_AbsoluteX()
    {
        ushort address = (ushort)(Absolute() + X);
        byte value = ReadMemory(address);
        bool newC = (value & 0x80) != 0;
        value <<= 1;
        if (C)
            value |= 0x01;
        WriteMemory(address, value);
        C = newC;
        UpdateZeroAndNegativeFlags(value);
    }

    private void ROR_Accumulator()
    {
        bool newC = (A & 0x01) != 0;
        A >>= 1;
        if (C)
            A |= 0x80;
        C = newC;
        UpdateZeroAndNegativeFlags(A);
    }

    private void ROR_ZeroPage()
    {
        byte address = ReadMemory(PC++);
        byte value = ReadMemory(address);
        bool newC = (value & 0x01) != 0;
        value >>= 1;
        if (C)
            value |= 0x80;
        WriteMemory(address, value);
        C = newC;
        UpdateZeroAndNegativeFlags(value);
    }

    private void ROR_Absolute()
    {
        ushort address = Absolute();
        byte value = ReadMemory(address);
        bool newC = (value & 0x01) != 0;
        value >>= 1;
        if (C)
            value |= 0x80;
        WriteMemory(address, value);
        C = newC;
        UpdateZeroAndNegativeFlags(value);
    }

    private void ROR_ZeroPageX()
    {
        byte address = (byte)(ReadMemory(PC++) + X);
        byte value = ReadMemory(address);
        bool newC = (value & 0x01) != 0;
        value >>= 1;
        if (C)
            value |= 0x80;
        WriteMemory(address, value);
        C = newC;
        UpdateZeroAndNegativeFlags(value);
    }

    private void ROR_AbsoluteX()
    {
        ushort address = (ushort)(Absolute() + X);
        byte value = ReadMemory(address);
        bool newC = (value & 0x01) != 0;
        value >>= 1;
        if (C)
            value |= 0x80;
        WriteMemory(address, value);
        C = newC;
        UpdateZeroAndNegativeFlags(value);
    }

    // Branching Operations
    private void BPL(sbyte offset)
    {
        if (!N)
            PC += (ushort)offset;
    }

    private void BMI(sbyte offset)
    {
        if (N)
            PC += (ushort)offset;
    }

    private void BCC(sbyte offset)
    {
        if (!C)
            PC += (ushort)offset;
    }

    private void BCS(sbyte offset)
    {
        if (C)
            PC += (ushort)offset;
    }

    private void BNE(sbyte offset)
    {
        if (!Z)
            PC += (ushort)offset;
    }

    private void BEQ(sbyte offset)
    {
        if (Z)
            PC += (ushort)offset;
    }

    // Compare Operations
    private void CMP(byte value)
    {
        ushort result = (byte)(A - value);
        UpdateZeroAndNegativeFlags((byte)result);
        UpdateCarryFlag(A, value);
    }

    private void CMP(ushort address)
    {
        byte value = ReadMemory(address);
        ushort result = (byte)(A - value);
        UpdateZeroAndNegativeFlags((byte)result);
        UpdateCarryFlag(A, value);
    }

    private void CPX(byte value)
    {
        ushort result = (byte)(X - value);
        UpdateZeroAndNegativeFlags((byte)result);
        UpdateCarryFlag(X, value);
    }

    private void CPX(ushort address)
    {
        byte value = ReadMemory(address);
        ushort result = (byte)(X - value);
        UpdateZeroAndNegativeFlags((byte)result);
        UpdateCarryFlag(X, value);
    }

    private void CPY(byte value)
    {
        ushort result = (ushort)(Y - value);
        UpdateZeroAndNegativeFlags((byte)result);
        UpdateCarryFlag(Y, value);
    }

    private void CPY(ushort address)
    {
        byte value = ReadMemory(address);
        ushort result = (ushort)(Y - value);
        UpdateZeroAndNegativeFlags((byte)result);
        UpdateCarryFlag(Y, value);
    }

    // Jump/Call Operations
    private void JMP_Absolute()
    {
        PC = Absolute();
    }

    private void JMP_Indirect()
    {
        ushort address = Indirect();
        if (address == 0)
            throw new Exception("Shouldn't happen");
        PC = address;
    }

    private void JSR(ushort address)
    {
        PushStack((byte)((PC - 1) >> 8));
        PushStack((byte)(PC - 1));
        PC = address;
    }

    private void RTS()
    {
        PC = (ushort)(PopStack() | (PopStack() << 8));
        PC++;
    }

    private void RTI()
    {
        P = PopStack();
        PC = (ushort)(PopStack() | (PopStack() << 8));
    }

    // Status Flag Operations
    private void CLC()
    {
        C = false;
    }

    private void SEC()
    {
        C = true;
    }

    private void CLI()
    {
        I = false;
    }

    private void SEI()
    {
        I = true;
    }

    private void CLD()
    {
        D = false;
    }

    private void SED()
    {
        D = true;
    }

    // System Functions
    private void BRK()
    {
        PC++;
        PushStack((byte)((PC >> 8) & 0xFF));
        PushStack((byte)(PC & 0xFF));
        PushStack(P);
        I = true;
        PC = (ushort)((ReadMemory(0xFFFE) << 8) | ReadMemory(0xFFFD));
    }

    private void NOP()
    {
        // Do nothing
    }

    // Helper functions for stack operations
    private void PushStack(byte value)
    {
        WriteMemory((ushort)(0x0100 | SP), value);
        SP--;
    }

    private byte PopStack()
    {
        SP++;
        return ReadMemory((ushort)(0x0100 | SP));
    }

    public void HandleNMI()
    {
        // Push the high byte of the program counter (PC) to the stack
        PushStack((byte)(PC >> 8));

        // Push the low byte of the program counter (PC) to the stack
        PushStack((byte)(PC & 0xFF));

        // Push the processor status register (P) to the stack
        PushStack(P);

        // Disable interrupts
        I = true;

        // Set the program counter (PC) to the NMI Vector address
        byte lowByte = memory.Read(0xFFFA);
        byte highByte = memory.Read(0xFFFB);
        PC = (ushort)((highByte << 8) | lowByte);

        // Simulate power-on/reset behavior by skipping the first read cycle
        //ReadMemory(PC);
        //PC++;
    }
}
