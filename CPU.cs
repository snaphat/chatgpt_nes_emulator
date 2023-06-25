namespace Emulation
{
    public class CPU
    {
        // Registers
        public byte A; // Accumulator
        public byte X, Y; // General-purpose registers
        public byte SP; // Stack pointer
        public ushort PC; // Program counter
        public byte P; // Processor status register

        // Status Flags
        public bool N; // Negative flag
        public bool V; // Overflow flag
        public bool B; // Break flag
        public bool D; // Decimal mode flag
        public bool I; // Interrupt disable flag
        public bool Z; // Zero flag
        public bool C; // Carry flag

        // Other CPU components and functions
        private Emulator? emulator;
        private Memory? memory;
        private PPU? ppu;

        public void Initialize(Emulator emulator, Memory memory, PPU ppu)
        {
            this.emulator = emulator;
            this.memory = memory;
            this.ppu = ppu;

            // Initialize registers and flags
            A = 0;
            X = 0;
            Y = 0;
            SP = 0xFD;
            N = false;
            V = false;
            B = false;
            D = false;
            I = true;
            Z = false;
            C = false;

            // Set the program counter (PC) to the Reset Vector address
            byte lowByte = memory.Read(0xFFFC);
            byte highByte = memory.Read(0xFFFD);
            PC = (ushort)((highByte << 8) | lowByte);

            // Simulate power-on/reset behavior by skipping the first read cycle
            //ReadMemory(PC);
            //PC++;

            // Disable interrupts
            I = true;
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

        public void ExecuteNextInstruction()
        {
            Debug.DisplayInstruction(this);
            byte opcode = ReadMemory(PC++);
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
                case 0xEB: // SBC Immediate (Unofficial)
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
                    ASL();
                    break;
                case 0x06: // ASL Zero Page
                    ASL(ZeroPage());
                    break;
                case 0x0E: // ASL Absolute
                    ASL(Absolute());
                    break;
                case 0x16: // ASL Zero Page, X
                    ASL(ZeroPageX());
                    break;
                case 0x1E: // ASL Absolute, X
                    ASL(AbsoluteX());
                    break;
                case 0x4A: // LSR Accumulator
                    LSR();
                    break;
                case 0x46: // LSR Zero Page
                    LSR(ZeroPage());
                    break;
                case 0x4E: // LSR Absolute
                    LSR(Absolute());
                    break;
                case 0x56: // LSR Zero Page, X
                    LSR(ZeroPageX());
                    break;
                case 0x5E: // LSR Absolute, X
                    LSR(AbsoluteX());
                    break;
                case 0x2A: // ROL Accumulator
                    ROL();
                    break;
                case 0x26: // ROL Zero Page
                    ROL(ZeroPage());
                    break;
                case 0x2E: // ROL Absolute
                    ROL(Absolute());
                    break;
                case 0x36: // ROL Zero Page, X
                    ROL(ZeroPageX());
                    break;
                case 0x3E: // ROL Absolute, X
                    ROL(AbsoluteX());
                    break;
                case 0x6A: // ROR Accumulator
                    ROR();
                    break;
                case 0x66: // ROR Zero Page
                    ROR(ZeroPage());
                    break;
                case 0x6E: // ROR Absolute
                    ROR(Absolute());
                    break;
                case 0x76: // ROR Zero Page, X
                    ROR(ZeroPageX());
                    break;
                case 0x7E: // ROR Absolute, X
                    ROR(AbsoluteX());
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
                case 0x70: // BVS
                    BVS(Relative());
                    break;
                case 0x50: // BVC
                    BVC(Relative());
                    break;

                // Jump/Call Operations
                case 0x4C: // JMP Absolute
                    JMP(Absolute());
                    break;
                case 0x6C: // JMP Indirect
                    JMP(Indirect());
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
                case 0xB8: // CLV
                    CLV();
                    break;

                // System Functions
                case 0x00: // BRK
                    BRK();
                    break;
                case 0xEA: // NOP (official)
                case 0x1A: // NOP 1-byte (unofficial)
                case 0x3A: // "
                case 0x5A: // "
                case 0x7A: // "
                case 0xDA: // "
                case 0xFA: // "
                    NOP();
                    break;
                case 0x80: // NOP 2-byte Immediate (unofficial)
                case 0x82: // "
                case 0xC2: // "
                case 0xE2: // "
                case 0x89: // "
                case 0x04: // NOP 2-byte Zero-Page (unofficial)
                case 0x44: // "
                case 0x64: // "
                case 0x14: // NOP 2-byte Zero-Page, X (unofficial)
                case 0x34: // "
                case 0x54: // "
                case 0x74: // "
                case 0xD4: // "
                case 0xF4: // "
                    NOPNOP();
                    break;
                case 0x0C: // NOP 3-byte Absolute (unofficial)
                case 0x1C: // NOP 3-byte Absolute, X (unofficial)
                case 0x3C: // "
                case 0x5C: // "
                case 0x7C: // "
                case 0xDC: // "
                case 0xFC: // "
                    NOPNOPNOP();
                    break;

                case 0x02: // STP (unofficial)
                case 0x12: // "
                case 0x22: // "
                case 0x32: // "
                case 0x42: // "
                case 0x52: // "
                case 0x62: // "
                case 0x72: // "
                case 0x92: // "
                case 0xB2: // "
                case 0xD2: // "
                case 0xF2: // "
                    STP();
                    break;

                case 0x4B:
                    ALR(Immediate());
                    break;

                case 0x0B:
                case 0x2B:
                    ANC(Immediate());
                    break;

                case 0x8B:
                    ANE(Immediate());
                    break;

                case 0x6B:
                    ARR(Immediate());
                    break;

                case 0xCB: // AXS Immediate
                    AXS(Immediate());
                    break;

                case 0xBB: // LAS Absolute, Y
                    LAS(AbsoluteY());
                    break;

                case 0xA7: // LAX Zero Page
                    LAX(ZeroPage());
                    break;
                case 0xB7: // LAX Zero Page, Y
                    LAX(ZeroPageY());
                    break;
                case 0xAF: // LAX Absolute
                    LAX(Absolute());
                    break;
                case 0xBF: // LAX Absolute, Y
                    LAX(AbsoluteY());
                    break;
                case 0xA3: // LAX (Zero Page, X)
                    LAX(IndirectX());
                    break;
                case 0xB3: // LAX (Zero Page), Y
                    LAX(IndirectY());
                    break;

                case 0xAB: // LXA Immediate
                    LXA(Immediate());
                    break;

                case 0x87: // SAX Zero Page
                    SAX(ZeroPage());
                    break;
                case 0x97: // SAX Zero Page, Y
                    SAX(ZeroPageY());
                    break;
                case 0x8F: // SAX Absolute
                    SAX(Absolute());
                    break;
                case 0x83: // SAX (Zero Page, X)
                    SAX(IndirectX());
                    break;

                case 0x9F: // SHA Absolute, Y
                    SHA(AbsoluteY());
                    break;
                case 0x93: // SHA Indirect, Y
                    SHA(IndirectY());
                    break;

                case 0x9E: // SHX Absolute, Y
                    SHX(AbsoluteY());
                    break;

                case 0x9C: // SHY Absolute, X
                    SHY(AbsoluteX());
                    break;

                case 0x9B: // TAS Absolute, Y
                    TAS(AbsoluteY());
                    break;

                case 0xC7: // DCP Zero Page
                    DCP(ZeroPage());
                    break;
                case 0xD7: // DCP Zero Page, X
                    DCP(ZeroPageX());
                    break;
                case 0xCF: // DCP Absolute
                    DCP(Absolute());
                    break;
                case 0xDF: // DCP Absolute, X
                    DCP(AbsoluteX());
                    break;
                case 0xDB: // DCP Absolute, Y
                    DCP(AbsoluteY());
                    break;
                case 0xC3: // DCP Indirect, X
                    DCP(IndirectX());
                    break;
                case 0xD3: // DCP Indirect, Y
                    DCP(IndirectY());
                    break;

                case 0xE7: // ISC Zero Page
                    ISC(ZeroPage());
                    break;
                case 0xF7: // ISC Zero Page, X
                    ISC(ZeroPageX());
                    break;
                case 0xEF: // ISC Absolute
                    ISC(Absolute());
                    break;
                case 0xFF: // ISC Absolute, X
                    ISC(AbsoluteX());
                    break;
                case 0xFB: // ISC Absolute, Y
                    ISC(AbsoluteY());
                    break;
                case 0xE3: // ISC Indirect, X
                    ISC(IndirectX());
                    break;
                case 0xF3: // ISC Indirect, Y
                    ISC(IndirectY());
                    break;

                case 0x27: // RLA Zero Page
                    RLA(ZeroPage());
                    break;
                case 0x37: // RLA Zero Page, X
                    RLA(ZeroPageX());
                    break;
                case 0x2F: // RLA Absolute
                    RLA(Absolute());
                    break;
                case 0x3F: // RLA Absolute, X
                    RLA(AbsoluteX());
                    break;
                case 0x3B: // RLA Absolute, Y
                    RLA(AbsoluteY());
                    break;
                case 0x23: // RLA (Zero Page, X)
                    RLA(IndirectX());
                    break;
                case 0x33: // RLA (Zero Page), Y
                    RLA(IndirectY());
                    break;

                case 0x67: // RRA Zero Page
                    RRA(ZeroPage());
                    break;
                case 0x77: // RRA Zero Page, X
                    RRA(ZeroPageX());
                    break;
                case 0x6F: // RRA Absolute
                    RRA(Absolute());
                    break;
                case 0x7F: // RRA Absolute, X
                    RRA(AbsoluteX());
                    break;
                case 0x7B: // RRA Absolute, Y
                    RRA(AbsoluteY());
                    break;
                case 0x63: // RRA (Zero Page, X)
                    RRA(IndirectX());
                    break;
                case 0x73: // RRA (Zero Page), Y
                    RRA(IndirectY());
                    break;

                case 0x07: // SLO Zero Page
                    SLO(ZeroPage());
                    break;
                case 0x17: // SLO Zero Page, X
                    SLO(ZeroPageX());
                    break;
                case 0x0F: // SLO Absolute
                    SLO(Absolute());
                    break;
                case 0x1F: // SLO Absolute, X
                    SLO(AbsoluteX());
                    break;
                case 0x1B: // SLO Absolute, Y
                    SLO(AbsoluteY());
                    break;
                case 0x03: // SLO (Indirect, X)
                    SLO(IndirectX());
                    break;
                case 0x13: // SLO (Indirect), Y
                    SLO(IndirectY());
                    break;

                case 0x47: // SRE Zero Page
                    SRE(ZeroPage());
                    break;
                case 0x57: // SRE Zero Page, X
                    SRE(ZeroPageX());
                    break;
                case 0x4F: // SRE Absolute
                    SRE(Absolute());
                    break;
                case 0x5F: // SRE Absolute, X
                    SRE(AbsoluteX());
                    break;
                case 0x5B: // SRE Absolute, Y
                    SRE(AbsoluteY());
                    break;
                case 0x43: // SRE (Indirect, X)
                    SRE(IndirectX());
                    break;
                case 0x53: // SRE (Indirect), Y
                    SRE(IndirectY());
                    break;

                default:
                    throw new NotImplementedException($"Opcode {opcode:X2} is not implemented.");
            }
        }

        // Helper functions for reading from and writing to memory
        public byte ReadMemory(ushort address, bool isDebugRead = false)
        {
            return memory.Read(address, isDebugRead);
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
            byte flags = PopStack();
            P = (byte)((P & 0x60) | (flags & 0xCF)); // Preserve bit 4 (B flag) and bit 5 (unused flag) and update the rest with the stack value
        }

        private void PHP()
        {
            PushStack((byte)(P | 0x30)); // Push bit 4 (B flag) and bit 5 (unused flag) set to 1 onto the stack
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
            V = ((A ^ sum) & (value ^ sum) & 0x80) != 0;  // Update overflow flag
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
            V = ((A ^ difference) & ((byte)~value ^ difference) & 0x80) != 0;  // Update overflow flag
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
            N = (value & 0x80) != 0;
            V = (value & 0x40) != 0;
            Z = result == 0;
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
        private void ASL()
        {
            C = (A & 0x80) != 0;
            A <<= 1;
            UpdateZeroAndNegativeFlags(A);
        }

        private void ASL(ushort address)
        {
            byte value = ReadMemory(address);
            C = (value & 0x80) != 0;
            value <<= 1;
            WriteMemory(address, value);
            UpdateZeroAndNegativeFlags(value);
        }

        private void LSR()
        {
            C = (A & 0x01) != 0;
            A >>= 1;
            UpdateZeroAndNegativeFlags(A);
        }

        private void LSR(ushort address)
        {
            byte value = ReadMemory(address);
            C = (value & 0x01) != 0;
            value >>= 1;
            WriteMemory(address, value);
            UpdateZeroAndNegativeFlags(value);
        }

        private void ROL()
        {
            bool newC = (A & 0x80) != 0;
            A <<= 1;
            if (C)
                A |= 0x01;
            C = newC;
            UpdateZeroAndNegativeFlags(A);
        }

        private void ROL(ushort address)
        {
            byte value = ReadMemory(address);
            bool newC = (value & 0x80) != 0;
            value <<= 1;
            if (C)
                value |= 0x01;
            WriteMemory(address, value);
            C = newC;
            UpdateZeroAndNegativeFlags(value);
        }

        private void ROR()
        {
            bool newC = (A & 0x01) != 0;
            A >>= 1;
            if (C)
                A |= 0x80;
            C = newC;
            UpdateZeroAndNegativeFlags(A);
        }

        private void ROR(ushort address)
        {
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

        private void BVC(sbyte offset)
        {
            if (!V)
                PC += (ushort)offset;
        }

        private void BVS(sbyte offset)
        {
            if (!V)
                PC += (ushort)offset;
        }

        // Compare Operations
        private void CMP(byte value)
        {
            ushort result = (byte)(A - value);
            UpdateZeroAndNegativeFlags((byte)result);
            C = A >= value;
        }

        private void CMP(ushort address)
        {
            byte value = ReadMemory(address);
            ushort result = (byte)(A - value);
            UpdateZeroAndNegativeFlags((byte)result);
            C = A >= value;
        }

        private void CPX(byte value)
        {
            ushort result = (byte)(X - value);
            UpdateZeroAndNegativeFlags((byte)result);
            C = X >= value;
        }

        private void CPX(ushort address)
        {
            byte value = ReadMemory(address);
            ushort result = (byte)(X - value);
            UpdateZeroAndNegativeFlags((byte)result);
            C = X >= value;
        }

        private void CPY(byte value)
        {
            ushort result = (ushort)(Y - value);
            UpdateZeroAndNegativeFlags((byte)result);
            C = Y >= value;
        }

        private void CPY(ushort address)
        {
            byte value = ReadMemory(address);
            ushort result = (ushort)(Y - value);
            UpdateZeroAndNegativeFlags((byte)result);
            C = Y >= value;
        }

        // Jump/Call Operations
        private void JMP(ushort address)
        {
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
            byte flags = PopStack();
            P = (byte)((P & 0x60) | (flags & 0xCF)); // Preserve bit 4 (B flag) and bit 5 (unused flag) and update the rest with the stack value
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

        private void CLV()
        {
            V = false;
        }

        private void BRK()
        {
            PC++; // Increment PC to point to the next instruction
            PushStack((byte)(PC >> 8)); // Push high byte of PC onto the stack
            PushStack((byte)(PC & 0xFF)); // Push low byte of PC onto the stack
            PushStack((byte)(P | 0x30)); // Push bit 4 (B flag) and bit 5 (unused flag) set to 1 onto the stack
            I = true; // Set Interrupt flag to disable further interrupts
            PC = (ushort)(ReadMemory(0xFFFE) | (ReadMemory(0xFFFF) << 8)); // Set PC to the interrupt vector address
        }

        private static void NOP()
        {
            // Do nothing
        }

        private void NOPNOP()
        {
            SP++; // Skip byte
        }

        private void NOPNOPNOP()
        {
            SP += 2; // Skip 2 bytes
        }

        private static void STP()
        {
            throw new InvalidOperationException("STP Instruction encountered.");
        }

        private void ALR(byte operand)
        {
            AND(operand);
            LSR();
        }

        private void ANC(byte operand)
        {
            AND(operand);
            UpdateZeroAndNegativeFlags(A);
            C = (A & 0x80) != 0; // Set the carry flag based on the value of the 7th bit of A
        }

        private void ANE(byte operand)
        {
            A = (byte)(A & X & operand);
            UpdateZeroAndNegativeFlags(A);
        }

        private void ARR(byte operand)
        {
            AND(operand);
            ROR();
            UpdateZeroAndNegativeFlags(A);
            C = (A & 0x40) != 0; // Set bit 6 of A as the carry flag
            V = ((A & 0x40) ^ ((A & 0x20) << 1)) != 0; // Set bit 6 xor bit 5 of A as the overflow flag
        }

        private void AXS(byte operand)
        {
            int result = (A & X) - operand;
            X = (byte)(result & 0xFF);
            UpdateZeroAndNegativeFlags(X);
            C = result >= 0; // Set the carry flag based on the result without borrow
        }

        private void LAS(ushort address)
        {
            byte value = ReadMemory(address);
            byte result = (byte)(value & SP);
            A = result;
            X = result;
            SP = result;
            UpdateZeroAndNegativeFlags(result);
        }

        private void LAX(ushort address)
        {
            LDA(address);
            TAX();
        }

        private void LXA(byte operand)
        {
            byte result = (byte)(A & operand);
            A = result;
            X = result;
            UpdateZeroAndNegativeFlags(result);
        }

        private void SAX(ushort address)
        {
            byte result = (byte)(A & X);
            WriteMemory(address, result);
        }

        private void SHA(ushort address)
        {
            byte result = (byte)(A & X & ((address >> 8) + 1));
            WriteMemory(address, result);
        }

        private void SHX(ushort address)
        {
            byte result = (byte)(X & ((address >> 8) + 1));
            WriteMemory(address, result);
        }

        private void SHY(ushort address)
        {
            byte result = (byte)(Y & ((address >> 8) + 1));
            WriteMemory(address, result);
        }

        private void TAS(ushort address)
        {
            byte result = (byte)(A & X);
            SP = result;
            result &= (byte)((address >> 8) + 1);
            WriteMemory(address, result);
        }

        private void DCP(ushort address)
        {
            DEC(address);
            CMP(address);
        }

        private void ISC(ushort address)
        {
            INC(address);
            SBC(address);
        }

        private void RLA(ushort address)
        {
            ROL(address);
            AND(address);
        }

        private void RRA(ushort address)
        {
            ROR(address);
            ADC(address);
        }

        private void SLO(ushort address)
        {
            ASL(address);
            ORA(address);
        }

        private void SRE(ushort address)
        {
            LSR(address);
            EOR(address);
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
            PushStack((byte)((P | 0x20) & 0xEF)); // Push bit 4 (B flag) set to 0 and bit 5 (unused flag) set to 1 onto the stack

            // Disable interrupts
            I = true;

            // Set the program counter (PC) to the NMI Vector address
            byte lowByte = memory.Read(0xFFFA);
            byte highByte = memory.Read(0xFFFB);
            PC = (ushort)((highByte << 8) | lowByte);
        }
    }
}
