namespace Emulation
{
    public class CPU
    {
        private int remainingCycles;

        // Internal Registers
        public byte A; // Accumulator
        public byte X, Y; // General-purpose registers
        public byte S; // Stack pointer
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

        private byte dmaPage;
        private byte dmaAddress;
        private int dmaCycleCounter;

        // next instruction to execute
        Action pendingOperation = () => { };

        // Other CPU components and functions
        private Emulator emulator = null!;
        private Memory memory = null!;
        private PPU ppu = null!;

        public void Initialize(Emulator emulator, Memory memory, PPU ppu)
        {
            this.emulator = emulator;
            this.memory = memory;
            this.ppu = ppu;

            // Initialize registers and flags
            A = 0;
            X = 0;
            Y = 0;
            S = 0xFD;
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
            // Stall execution for required cycle count
            remainingCycles--;
            if (remainingCycles > 0)
            {
                return;
            }

            // Display debug information
            //Debug.DisplayInstruction(this);

            // Execute pending operation
            pendingOperation();

            // Check if remaining cycles since executed operations can add new operations
            if (remainingCycles > 0)
                return;

            // Check if an NMI is pending and handle it if necessary
            if (emulator.isNmiPending)
            {
                NMI_Begin();
                emulator.isNmiPending = false;
                return;
            }

            // Lookup next operation
            byte opcode = ReadMemory(PC++);
            switch (opcode)
            {
                // Register Transfers
                case 0xAA: TAX(); break;
                case 0x8A: TXA(); break;
                case 0xA8: TAY(); break;
                case 0x98: TYA(); break;
                case 0xBA: TSX(); break;
                case 0x9A: TXS(); break;
                case 0x68: PLA(); break;
                case 0x48: PHA(); break;
                case 0x28: PLP(); break;
                case 0x08: PHP(); break;

                // Load and Store Operations
                case 0xA9: LDA_Immediate(); break;
                case 0xA5: LDA_ZeroPage(); break;
                case 0xB5: LDA_ZeroPageX(); break;
                case 0xAD: LDA_Absolute(); break;
                case 0xBD: LDA_AbsoluteX(); break;
                case 0xB9: LDA_AbsoluteY(); break;
                case 0xA1: LDA_IndirectX(); break;
                case 0xB1: LDA_IndirectY(); break;
                case 0xA2: LDX_Immediate(); break;
                case 0xA6: LDX_ZeroPage(); break;
                case 0xB6: LDX_ZeroPageY(); break;
                case 0xAE: LDX_Absolute(); break;
                case 0xBE: LDX_AbsoluteY(); break;
                case 0xA0: LDY_Immediate(); break;
                case 0xA4: LDY_ZeroPage(); break;
                case 0xB4: LDY_ZeroPageX(); break;
                case 0xAC: LDY_Absolute(); break;
                case 0xBC: LDY_AbsoluteX(); break;
                case 0x85: STA_ZeroPage(); break;
                case 0x95: STA_ZeroPageX(); break;
                case 0x8D: STA_Absolute(); break;
                case 0x9D: STA_AbsoluteX(); break;
                case 0x99: STA_AbsoluteY(); break;
                case 0x81: STA_IndirectX(); break;
                case 0x91: STA_IndirectY(); break;
                case 0x86: STX_ZeroPage(); break;
                case 0x96: STX_ZeroPageY(); break;
                case 0x8E: STX_Absolute(); break;
                case 0x84: STY_ZeroPage(); break;
                case 0x94: STY_ZeroPageX(); break;
                case 0x8C: STY_Absolute(); break;

                // Arithmetic and Logical Operations
                case 0x69: ADC_Immediate(); break;
                case 0x65: ADC_ZeroPage(); break;
                case 0x75: ADC_ZeroPageX(); break;
                case 0x6D: ADC_Absolute(); break;
                case 0x7D: ADC_AbsoluteX(); break;
                case 0x79: ADC_AbsoluteY(); break;
                case 0x61: ADC_IndirectX(); break;
                case 0x71: ADC_IndirectY(); break;
                case 0xE9: SBC_Immediate(); break;
                case 0xE5: SBC_ZeroPage(); break;
                case 0xF5: SBC_ZeroPageX(); break;
                case 0xED: SBC_Absolute(); break;
                case 0xFD: SBC_AbsoluteX(); break;
                case 0xF9: SBC_AbsoluteY(); break;
                case 0xE1: SBC_IndirectX(); break;
                case 0xF1: SBC_IndirectY(); break;
                case 0x29: AND_Immediate(); break;
                case 0x25: AND_ZeroPage(); break;
                case 0x35: AND_ZeroPageX(); break;
                case 0x2D: AND_Absolute(); break;
                case 0x3D: AND_AbsoluteX(); break;
                case 0x39: AND_AbsoluteY(); break;
                case 0x21: AND_IndirectX(); break;
                case 0x31: AND_IndirectY(); break;
                case 0x09: ORA_Immediate(); break;
                case 0x05: ORA_ZeroPage(); break;
                case 0x15: ORA_ZeroPageX(); break;
                case 0x0D: ORA_Absolute(); break;
                case 0x1D: ORA_AbsoluteX(); break;
                case 0x19: ORA_AbsoluteY(); break;
                case 0x01: ORA_IndirectX(); break;
                case 0x11: ORA_IndirectY(); break;
                case 0x49: EOR_Immediate(); break;
                case 0x45: EOR_ZeroPage(); break;
                case 0x55: EOR_ZeroPageX(); break;
                case 0x4D: EOR_Absolute(); break;
                case 0x5D: EOR_AbsoluteX(); break;
                case 0x59: EOR_AbsoluteY(); break;
                case 0x41: EOR_IndirectX(); break;
                case 0x51: EOR_IndirectY(); break;
                case 0x24: BIT_ZeroPage(); break;
                case 0x2C: BIT_Absolute(); break;

                // Increment and Decrement Operations
                case 0xE6: INC_ZeroPage(); break;
                case 0xEE: INC_Absolute(); break;
                case 0xF6: INC_ZeroPageX(); break;
                case 0xFE: INC_AbsoluteX(); break;
                case 0xC6: DEC_ZeroPage(); break;
                case 0xD6: DEC_ZeroPageX(); break;
                case 0xCE: DEC_Absolute(); break;
                case 0xDE: DEC_AbsoluteX(); break;
                case 0xE8: INX(); break;
                case 0xC8: INY(); break;
                case 0xCA: DEX(); break;
                case 0x88: DEY(); break;

                // Shift Operations
                case 0x0A: ASL(); break;
                case 0x06: ASL_ZeroPage(); break;
                case 0x16: ASL_ZeroPageX(); break;
                case 0x0E: ASL_Absolute(); break;
                case 0x1E: ASL_AbsoluteX(); break;
                case 0x4A: LSR(); break;
                case 0x46: LSR_ZeroPage(); break;
                case 0x56: LSR_ZeroPageX(); break;
                case 0x4E: LSR_Absolute(); break;
                case 0x5E: LSR_AbsoluteX(); break;
                case 0x2A: ROL(); break;
                case 0x26: ROL_ZeroPage(); break;
                case 0x36: ROL_ZeroPageX(); break;
                case 0x2E: ROL_Absolute(); break;
                case 0x3E: ROL_AbsoluteX(); break;
                case 0x6A: ROR(); break;
                case 0x66: ROR_ZeroPage(); break;
                case 0x76: ROR_ZeroPageX(); break;
                case 0x6E: ROR_Absolute(); break;
                case 0x7E: ROR_AbsoluteX(); break;

                // Compare Operations
                case 0xC9: CMP_Immediate(); break;
                case 0xC5: CMP_ZeroPage(); break;
                case 0xD5: CMP_ZeroPageX(); break;
                case 0xCD: CMP_Absolute(); break;
                case 0xDD: CMP_AbsoluteX(); break;
                case 0xD9: CMP_AbsoluteY(); break;
                case 0xC1: CMP_IndirectX(); break;
                case 0xD1: CMP_IndirectY(); break;
                case 0xE0: CPX_Immediate(); break;
                case 0xE4: CPX_ZeroPage(); break;
                case 0xEC: CPX_Absolute(); break;
                case 0xC0: CPY_Immediate(); break;
                case 0xC4: CPY_ZeroPage(); break;
                case 0xCC: CPY_Absolute(); break;

                // Branching Operations
                case 0xD0: BNE_Relative(); break;
                case 0xF0: BEQ_Relative(); break;
                case 0x10: BPL_Relative(); break;
                case 0x30: BMI_Relative(); break;
                case 0x90: BCC_Relative(); break;
                case 0xB0: BCS_Relative(); break;
                case 0x50: BVC_Relative(); break;
                case 0x70: BVS_Relative(); break;

                // Jump and Call Operations
                case 0x4C: JMP_Absolute(); break;
                case 0x6C: JMP_Indirect(); break;
                case 0x20: JSR_Absolute(); break;
                case 0x60: RTS(); break;
                case 0x40: RTI(); break;

                // Status Flag Operations
                case 0x18: CLC(); break;
                case 0x38: SEC(); break;
                case 0x58: CLI(); break;
                case 0x78: SEI(); break;
                case 0xD8: CLD(); break;
                case 0xF8: SED(); break;
                case 0xB8: CLV(); break;

                // System Functions
                case 0x00: BRK(); break;
                case 0xEA: NOP(); break;

                // Unofficial Instructions
                case 0xEB: SBC_Immediate(); break;
                case 0x1A:
                case 0x3A:
                case 0x5A:
                case 0x7A:
                case 0xDA:
                case 0xFA: NOP(); break;
                case 0x80:
                case 0x82:
                case 0x89:
                case 0xC2:
                case 0xE2: NOP_Immediate(); break;
                case 0x04:
                case 0x44:
                case 0x64: NOP_ZeroPage(); break;
                case 0x14: // NOP 2-byte Zero-Page, X (unofficial)
                case 0x34:
                case 0x54:
                case 0x74:
                case 0xD4:
                case 0xF4: NOP_ZeroPageX(); break;
                case 0x0C: NOP_Absolute(); break;
                case 0x1C:
                case 0x3C:
                case 0x5C:
                case 0x7C:
                case 0xDC:
                case 0xFC: NOP_AbsoluteX(); break;
                case 0x02:
                case 0x12:
                case 0x22:
                case 0x32:
                case 0x42:
                case 0x52:
                case 0x62:
                case 0x72:
                case 0x92:
                case 0xB2:
                case 0xD2:
                case 0xF2: STP(); break;
                case 0x4B: ALR_Immediate(); break;
                case 0x0B:
                case 0x2B: ANC_Immediate(); break;
                case 0x8B: ANE_Immediate(); break;
                case 0x6B: ARR_Immediate(); break;
                case 0xCB: AXS_Immediate(); break;
                case 0xBB: LAS_AbsoluteY(); break;
                case 0xA7: LAX_ZeroPage(); break;
                case 0xB7: LAX_ZeroPageY(); break;
                case 0xAF: LAX_Absolute(); break;
                case 0xBF: LAX_AbsoluteY(); break;
                case 0xA3: LAX_IndirectX(); break;
                case 0xB3: LAX_IndirectY(); break;
                case 0xAB: LXA_Immediate(); break;
                case 0x87: SAX_ZeroPage(); break;
                case 0x97: SAX_ZeroPageY(); break;
                case 0x8F: SAX_Absolute(); break;
                case 0x83: SAX_IndirectX(); break;
                case 0x9F: SHA_AbsoluteY(); break;
                case 0x93: SHA_IndirectY(); break;
                case 0x9E: SHX_AbsoluteY(); break;
                case 0x9C: SHY_AbsoluteX(); break;
                case 0x9B: TAS_AbsoluteY(); break;
                case 0xC7: DCP_ZeroPage(); break;
                case 0xD7: DCP_ZeroPageX(); break;
                case 0xCF: DCP_Absolute(); break;
                case 0xDF: DCP_AbsoluteX(); break;
                case 0xDB: DCP_AbsoluteY(); break;
                case 0xC3: DCP_IndirectX(); break;
                case 0xD3: DCP_IndirectY(); break;
                case 0xE7: ISC_ZeroPage(); break;
                case 0xF7: ISC_ZeroPageX(); break;
                case 0xEF: ISC_Absolute(); break;
                case 0xFF: ISC_AbsoluteX(); break;
                case 0xFB: ISC_AbsoluteY(); break;
                case 0xE3: ISC_IndirectX(); break;
                case 0xF3: ISC_IndirectY(); break;
                case 0x27: RLA_ZeroPage(); break;
                case 0x37: RLA_ZeroPageX(); break;
                case 0x2F: RLA_Absolute(); break;
                case 0x3F: RLA_AbsoluteX(); break;
                case 0x3B: RLA_AbsoluteY(); break;
                case 0x23: RLA_IndirectX(); break;
                case 0x33: RLA_IndirectY(); break;
                case 0x67: RRA_ZeroPage(); break;
                case 0x77: RRA_ZeroPageX(); break;
                case 0x6F: RRA_Absolute(); break;
                case 0x7F: RRA_AbsoluteX(); break;
                case 0x7B: RRA_AbsoluteY(); break;
                case 0x63: RRA_IndirectX(); break;
                case 0x73: RRA_IndirectY(); break;
                case 0x07: SLO_ZeroPage(); break;
                case 0x17: SLO_ZeroPageX(); break;
                case 0x0F: SLO_Absolute(); break;
                case 0x1F: SLO_AbsoluteX(); break;
                case 0x1B: SLO_AbsoluteY(); break;
                case 0x03: SLO_IndirectX(); break;
                case 0x13: SLO_IndirectY(); break;
                case 0x47: SRE_ZeroPage(); break;
                case 0x57: SRE_ZeroPageX(); break;
                case 0x4F: SRE_Absolute(); break;
                case 0x5F: SRE_AbsoluteX(); break;
                case 0x5B: SRE_AbsoluteY(); break;
                case 0x43: SRE_IndirectX(); break;
                case 0x53: SRE_IndirectY(); break;

                default:
                    throw new NotImplementedException($"Opcode {opcode:X2} is not implemented.");
            }
        }

        // Helper functions for reading from and writing to memory
        public byte ReadMemory(ushort address, bool isDebugRead = false)
        {
            return memory.Read(address, isDebugRead);
        }

        public void WriteMemory(ushort address, byte value)
        {
            if (address == 0x4014)
            {
                DMA_Begin(value);
            }
            else
            {
                memory.Write(address, value);
            }
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

        private bool IsPageBoundaryCrossed_AbsoluteX()
        {
            ushort address = (ushort)(ReadMemory(PC) | (ReadMemory((ushort)(PC + 1)) << 8));
            ushort finalAddress = (ushort)(address + X);
            return (address & 0xFF00) != (finalAddress & 0xFF00);
        }

        private ushort AbsoluteY()
        {
            ushort address = (ushort)(ReadMemory(PC++) | (ReadMemory(PC++) << 8));
            return (ushort)(address + Y);
        }

        private bool IsPageBoundaryCrossed_AbsoluteY()
        {
            ushort address = (ushort)(ReadMemory(PC) | (ReadMemory((ushort)(PC + 1)) << 8));
            ushort finalAddress = (ushort)(address + Y);
            return (address & 0xFF00) != (finalAddress & 0xFF00);
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

        private bool IsPageBoundaryCrossed_IndirectY()
        {
            byte zeroPageAddress = ReadMemory(PC);
            ushort address = ReadMemory(zeroPageAddress);
            address |= (ushort)(ReadMemory((byte)(zeroPageAddress + 1)) << 8);
            ushort finalAddress = (ushort)(address + Y);
            return (address & 0xFF00) != (finalAddress & 0xFF00);
        }

        private sbyte Relative()
        {
            return (sbyte)ReadMemory(PC++);
        }

        private int BranchCyclesNeeded_Relative(bool condition)
        {
            sbyte offset = (sbyte)ReadMemory(PC);
            if (condition)
            {
                ushort newAddress = (ushort)(PC + offset);
                if ((PC & 0xFF00) != (newAddress & 0xFF00))
                {
                    // Crossing a page boundary
                    return 2;
                }
                return 1;
            }
            return 0;
        }

        private void TAX()
        {
            remainingCycles = 2;
            pendingOperation = TAX_;
        }

        private void TAX_()
        {
            X = A;
            UpdateZeroAndNegativeFlags(X);
        }

        private void TXA()
        {
            remainingCycles = 2;
            pendingOperation = TXA_;
        }

        private void TXA_()
        {
            A = X;
            UpdateZeroAndNegativeFlags(A);
        }

        private void TAY()
        {
            remainingCycles = 2;
            pendingOperation = TAY_;
        }

        private void TAY_()
        {
            Y = A;
            UpdateZeroAndNegativeFlags(Y);
        }

        private void TYA()
        {
            remainingCycles = 2;
            pendingOperation = TYA_;
        }

        private void TYA_()
        {
            A = Y;
            UpdateZeroAndNegativeFlags(A);
        }

        private void TSX()
        {
            remainingCycles = 2;
            pendingOperation = TSX_;
        }

        private void TSX_()
        {
            X = S;
            UpdateZeroAndNegativeFlags(X);
        }

        private void TXS()
        {
            remainingCycles = 2;
            pendingOperation = TXS_;
        }

        private void TXS_()
        {
            S = X;
        }

        private void PLA()
        {
            remainingCycles = 4;
            pendingOperation = PLA_;
        }

        private void PLA_()
        {
            A = PopStack();
            UpdateZeroAndNegativeFlags(A);
        }

        private void PHA()
        {
            remainingCycles = 3;
            pendingOperation = PHA_;
        }

        private void PHA_()
        {
            PushStack(A);
        }

        private void PLP()
        {
            remainingCycles = 4;
            pendingOperation = PLP_;
        }

        private void PLP_()
        {
            byte flags = PopStack();
            P = (byte)((P & 0x60) | (flags & 0xCF)); // Preserve bit 4 (B flag) and bit 5 (unused flag) and update the rest with the stack value
        }

        private void PHP()
        {
            remainingCycles = 3;
            pendingOperation = PHP_;
        }

        private void PHP_()
        {
            PushStack((byte)(P | 0x30)); // Push bit 4 (B flag) and bit 5 (unused flag) set to 1 onto the stack
        }

        // Load and Store Operations
        private void LDA_Immediate()
        {
            remainingCycles = 2;
            pendingOperation = () => LDA_(Immediate());
        }

        private void LDA_ZeroPage()
        {
            remainingCycles = 3;
            pendingOperation = () => LDA_(ZeroPage());
        }

        private void LDA_ZeroPageX()
        {
            remainingCycles = 4;
            pendingOperation = () => LDA_(ZeroPageX());
        }

        private void LDA_Absolute()
        {
            remainingCycles = 4;
            pendingOperation = () => LDA_(Absolute());
        }

        private void LDA_AbsoluteX()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
            pendingOperation = () => LDA_(AbsoluteX());
        }

        private void LDA_AbsoluteY()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
            pendingOperation = () => LDA_(AbsoluteY());
        }

        private void LDA_IndirectX()
        {
            remainingCycles = 6;
            pendingOperation = () => LDA_(IndirectX());
        }

        private void LDA_IndirectY()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_IndirectY()) remainingCycles++;
            pendingOperation = () => LDA_(IndirectY());
        }

        private void LDA_(byte value)
        {
            A = value;
            UpdateZeroAndNegativeFlags(A);
        }

        private void LDA_(ushort address)
        {
            A = ReadMemory(address);
            UpdateZeroAndNegativeFlags(A);
        }

        private void LDX_Immediate()
        {
            remainingCycles = 2;
            pendingOperation = () => LDX_(Immediate());
        }

        private void LDX_ZeroPage()
        {
            remainingCycles = 3;
            pendingOperation = () => LDX_(ZeroPage());
        }

        private void LDX_ZeroPageY()
        {
            remainingCycles = 4;
            pendingOperation = () => LDX_(ZeroPageY());
        }

        private void LDX_Absolute()
        {
            remainingCycles = 4;
            pendingOperation = () => LDX_(Absolute());
        }

        private void LDX_AbsoluteY()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
            pendingOperation = () => LDX_(AbsoluteY());
        }

        private void LDX_(byte value)
        {
            X = value;
            UpdateZeroAndNegativeFlags(X);
        }

        private void LDX_(ushort address)
        {
            X = ReadMemory(address);
            UpdateZeroAndNegativeFlags(X);
        }

        private void LDY_Immediate()
        {
            remainingCycles = 2;
            pendingOperation = () => LDY_(Immediate());
        }

        private void LDY_ZeroPage()
        {
            remainingCycles = 3;
            pendingOperation = () => LDY_(ZeroPage());
        }

        private void LDY_ZeroPageX()
        {
            remainingCycles = 4;
            pendingOperation = () => LDY_(ZeroPageX());
        }

        private void LDY_Absolute()
        {
            remainingCycles = 4;
            pendingOperation = () => LDY_(Absolute());
        }

        private void LDY_AbsoluteX()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
            pendingOperation = () => LDY_(AbsoluteX());
        }

        private void LDY_(byte value)
        {
            Y = value;
            UpdateZeroAndNegativeFlags(Y);
        }

        private void LDY_(ushort address)
        {
            Y = ReadMemory(address);
            UpdateZeroAndNegativeFlags(Y);
        }

        private void STA_ZeroPage()
        {
            remainingCycles = 3;
            pendingOperation = () => STA_(ZeroPage());
        }

        private void STA_ZeroPageX()
        {
            remainingCycles = 4;
            pendingOperation = () => STA_(ZeroPageX());
        }

        private void STA_Absolute()
        {
            remainingCycles = 4;
            pendingOperation = () => STA_(Absolute());
        }

        private void STA_AbsoluteX()
        {
            remainingCycles = 5;
            pendingOperation = () => STA_(AbsoluteX());
        }

        private void STA_AbsoluteY()
        {
            remainingCycles = 5;
            pendingOperation = () => STA_(AbsoluteY());
        }

        private void STA_IndirectX()
        {
            remainingCycles = 6;
            pendingOperation = () => STA_(IndirectX());
        }

        private void STA_IndirectY()
        {
            remainingCycles = 6;
            pendingOperation = () => STA_(IndirectY());
        }

        private void STA_(ushort address)
        {
            WriteMemory(address, A);
        }

        private void STX_ZeroPage()
        {
            remainingCycles = 3;
            pendingOperation = () => STX_(ZeroPage());
        }

        private void STX_ZeroPageY()
        {
            remainingCycles = 4;
            pendingOperation = () => STX_(ZeroPageY());
        }

        private void STX_Absolute()
        {
            remainingCycles = 4;
            pendingOperation = () => STX_(Absolute());
        }

        private void STX_(ushort address)
        {
            WriteMemory(address, X);
        }

        private void STY_ZeroPage()
        {
            remainingCycles = 3;
            pendingOperation = () => STY_(ZeroPage());
        }

        private void STY_ZeroPageX()
        {
            remainingCycles = 4;
            pendingOperation = () => STY_(ZeroPageX());
        }

        private void STY_Absolute()
        {
            remainingCycles = 4;
            pendingOperation = () => STY_(Absolute());
        }

        private void STY_(ushort address)
        {
            WriteMemory(address, Y);
        }

        // Arithmetic and Logical Operations
        private void ADC_Immediate()
        {
            remainingCycles = 2;
            pendingOperation = () => ADC_(Immediate());
        }

        private void ADC_ZeroPage()
        {
            remainingCycles = 3;
            pendingOperation = () => ADC_(ZeroPage());
        }

        private void ADC_ZeroPageX()
        {
            remainingCycles = 4;
            pendingOperation = () => ADC_(ZeroPageX());
        }

        private void ADC_Absolute()
        {
            remainingCycles = 4;
            pendingOperation = () => ADC_(Absolute());
        }

        private void ADC_AbsoluteX()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
            pendingOperation = () => ADC_(AbsoluteX());
        }

        private void ADC_AbsoluteY()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
            pendingOperation = () => ADC_(AbsoluteY());
        }

        private void ADC_IndirectX()
        {
            remainingCycles = 6;
            pendingOperation = () => ADC_(IndirectX());
        }

        private void ADC_IndirectY()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_IndirectY()) remainingCycles++;
            pendingOperation = () => ADC_(IndirectY());
        }

        private void ADC_(byte value)
        {
            int sum = A + value + (C ? 1 : 0);
            C = sum > 0xFF;  // Update carry flag based on carry-out from bit 7
            V = ((A ^ sum) & (value ^ sum) & 0x80) != 0;  // Update overflow flag
            A = (byte)sum;   // Store the lower 8 bits of the sum in the accumulator
            UpdateZeroAndNegativeFlags(A);
        }

        private void ADC_(ushort address)
        {
            byte value = ReadMemory(address);
            ADC_(value);
        }

        private void SBC_Immediate()
        {
            remainingCycles = 2;
            pendingOperation = () => SBC_(Immediate());
        }

        private void SBC_ZeroPage()
        {
            remainingCycles = 3;
            pendingOperation = () => SBC_(ZeroPage());
        }

        private void SBC_ZeroPageX()
        {
            remainingCycles = 4;
            pendingOperation = () => SBC_(ZeroPageX());
        }

        private void SBC_Absolute()
        {
            remainingCycles = 4;
            pendingOperation = () => SBC_(Absolute());
        }

        private void SBC_AbsoluteX()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
            pendingOperation = () => SBC_(AbsoluteX());
        }

        private void SBC_AbsoluteY()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
            pendingOperation = () => SBC_(AbsoluteY());
        }

        private void SBC_IndirectX()
        {
            remainingCycles = 6;
            pendingOperation = () => SBC_(IndirectX());
        }

        private void SBC_IndirectY()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_IndirectY()) remainingCycles++;
            pendingOperation = () => SBC_(IndirectY());
        }

        private void SBC_(byte value)
        {
            int difference = A - value - (C ? 0 : 1);
            C = difference >= 0;
            V = ((A ^ difference) & ((byte)~value ^ difference) & 0x80) != 0;  // Update overflow flag
            A = (byte)(difference & 0xFF);
            UpdateZeroAndNegativeFlags(A);
        }

        private void SBC_(ushort address)
        {
            byte value = ReadMemory(address);
            SBC_(value);
        }

        private void AND_Immediate()
        {
            remainingCycles = 2;
            pendingOperation = () => AND_(Immediate());
        }

        private void AND_ZeroPage()
        {
            remainingCycles = 3;
            pendingOperation = () => AND_(ZeroPage());
        }

        private void AND_ZeroPageX()
        {
            remainingCycles = 4;
            pendingOperation = () => AND_(ZeroPageX());
        }

        private void AND_Absolute()
        {
            remainingCycles = 4;
            pendingOperation = () => AND_(Absolute());
        }

        private void AND_AbsoluteX()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
            pendingOperation = () => AND_(AbsoluteX());
        }

        private void AND_AbsoluteY()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
            pendingOperation = () => AND_(AbsoluteY());
        }

        private void AND_IndirectX()
        {
            remainingCycles = 6;
            pendingOperation = () => AND_(IndirectX());
        }

        private void AND_IndirectY()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_IndirectY()) remainingCycles++;
            pendingOperation = () => AND_(IndirectY());
        }

        private void AND_(byte value)
        {
            A &= value;
            UpdateZeroAndNegativeFlags(A);
        }

        private void AND_(ushort address)
        {
            byte value = ReadMemory(address);
            AND_(value);
        }

        private void ORA_Immediate()
        {
            remainingCycles = 2;
            pendingOperation = () => ORA_(Immediate());
        }

        private void ORA_ZeroPage()
        {
            remainingCycles = 3;
            pendingOperation = () => ORA_(ZeroPage());
        }

        private void ORA_ZeroPageX()
        {
            remainingCycles = 4;
            pendingOperation = () => ORA_(ZeroPageX());
        }

        private void ORA_Absolute()
        {
            remainingCycles = 4;
            pendingOperation = () => ORA_(Absolute());
        }

        private void ORA_AbsoluteX()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
            pendingOperation = () => ORA_(AbsoluteX());
        }

        private void ORA_AbsoluteY()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
            pendingOperation = () => ORA_(AbsoluteY());
        }

        private void ORA_IndirectX()
        {
            remainingCycles = 6;
            pendingOperation = () => ORA_(IndirectX());
        }

        private void ORA_IndirectY()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_IndirectY()) remainingCycles++;
            pendingOperation = () => ORA_(IndirectY());
        }

        private void ORA_(byte value)
        {
            A |= value;
            UpdateZeroAndNegativeFlags(A);
        }

        private void ORA_(ushort address)
        {
            byte value = ReadMemory(address);
            A |= value;
            UpdateZeroAndNegativeFlags(A);
        }

        private void EOR_Immediate()
        {
            remainingCycles = 2;
            pendingOperation = () => EOR_(Immediate());
        }

        private void EOR_ZeroPage()
        {
            remainingCycles = 3;
            pendingOperation = () => EOR_(ZeroPage());
        }

        private void EOR_ZeroPageX()
        {
            remainingCycles = 4;
            pendingOperation = () => EOR_(ZeroPageX());
        }

        private void EOR_Absolute()
        {
            remainingCycles = 4;
            pendingOperation = () => EOR_(Absolute());
        }

        private void EOR_AbsoluteX()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
            pendingOperation = () => EOR_(AbsoluteX());
        }

        private void EOR_AbsoluteY()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
            pendingOperation = () => EOR_(AbsoluteY());
        }

        private void EOR_IndirectX()
        {
            remainingCycles = 6;
            pendingOperation = () => EOR_(IndirectX());
        }

        private void EOR_IndirectY()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_IndirectY()) remainingCycles++;
            pendingOperation = () => EOR_(IndirectY());
        }

        private void EOR_(byte value)
        {
            A ^= value;
            UpdateZeroAndNegativeFlags(A);
        }

        private void EOR_(ushort address)
        {
            byte value = ReadMemory(address);
            EOR_(value);
        }

        private void BIT_ZeroPage()
        {
            remainingCycles = 3;
            pendingOperation = () => BIT_(ZeroPage());
        }

        private void BIT_Absolute()
        {
            remainingCycles = 4;
            pendingOperation = () => BIT_(Absolute());
        }

        private void BIT_(byte value)
        {
            byte result = (byte)(A & value);
            N = (value & 0x80) != 0;
            V = (value & 0x40) != 0;
            Z = result == 0;
        }

        private void BIT_(ushort address)
        {
            byte value = ReadMemory(address);
            BIT_(value);
        }

        // Increment and Decrement Operations
        private void INC_ZeroPage()
        {
            remainingCycles = 5;
            pendingOperation = () => INC_(ZeroPage());
        }

        private void INC_Absolute()
        {
            remainingCycles = 6;
            pendingOperation = () => INC_(Absolute());
        }

        private void INC_ZeroPageX()
        {
            remainingCycles = 6;
            pendingOperation = () => INC_(ZeroPageX());
        }

        private void INC_AbsoluteX()
        {
            remainingCycles = 7;
            pendingOperation = () => INC_(AbsoluteX());
        }

        private void INC_(ushort address)
        {
            byte value = ReadMemory(address);
            value++;
            WriteMemory(address, value);
            UpdateZeroAndNegativeFlags(value);
        }

        private void DEC_ZeroPage()
        {
            remainingCycles = 5;
            pendingOperation = () => DEC_(ZeroPage());
        }

        private void DEC_ZeroPageX()
        {
            remainingCycles = 6;
            pendingOperation = () => DEC_(ZeroPageX());
        }

        private void DEC_Absolute()
        {
            remainingCycles = 6;
            pendingOperation = () => DEC_(Absolute());
        }

        private void DEC_AbsoluteX()
        {
            remainingCycles = 7;
            pendingOperation = () => DEC_(AbsoluteX());
        }

        private void DEC_(ushort address)
        {
            byte value = ReadMemory(address);
            value--;
            WriteMemory(address, value);
            UpdateZeroAndNegativeFlags(value);
        }

        private void INX()
        {
            remainingCycles = 2;
            pendingOperation = INX_;
        }

        private void INX_()
        {
            X++;
            UpdateZeroAndNegativeFlags(X);
        }

        private void INY()
        {
            remainingCycles = 2;
            pendingOperation = INY_;
        }

        private void INY_()
        {
            Y++;
            UpdateZeroAndNegativeFlags(Y);
        }

        private void DEX()
        {
            remainingCycles = 2;
            pendingOperation = DEX_;
        }

        private void DEX_()
        {
            X--;
            UpdateZeroAndNegativeFlags(X);
        }

        private void DEY()
        {
            remainingCycles = 2;
            pendingOperation = DEY_;
        }

        private void DEY_()
        {
            Y--;
            UpdateZeroAndNegativeFlags(Y);
        }

        // Shift Operations
        private void ASL()
        {
            remainingCycles = 2;
            pendingOperation = ASL_;
        }

        private void ASL_ZeroPage()
        {
            remainingCycles = 5;
            pendingOperation = () => ASL_(ZeroPage());
        }

        private void ASL_ZeroPageX()
        {
            remainingCycles = 6;
            pendingOperation = () => ASL_(ZeroPageX());
        }

        private void ASL_Absolute()
        {
            remainingCycles = 6;
            pendingOperation = () => ASL_(Absolute());
        }

        private void ASL_AbsoluteX()
        {
            remainingCycles = 7;
            pendingOperation = () => ASL_(AbsoluteX());
        }

        private void ASL_(ushort address)
        {
            byte value = ReadMemory(address);
            C = (value & 0x80) != 0;
            value <<= 1;
            WriteMemory(address, value);
            UpdateZeroAndNegativeFlags(value);
        }

        private void ASL_()
        {
            C = (A & 0x80) != 0;
            A <<= 1;
            UpdateZeroAndNegativeFlags(A);
        }

        private void LSR()
        {
            remainingCycles = 2;
            pendingOperation = LSR_;
        }

        private void LSR_ZeroPage()
        {
            remainingCycles = 5;
            pendingOperation = () => LSR_(ZeroPage());
        }

        private void LSR_ZeroPageX()
        {
            remainingCycles = 6;
            pendingOperation = () => LSR_(ZeroPageX());
        }

        private void LSR_Absolute()
        {
            remainingCycles = 6;
            pendingOperation = () => LSR_(Absolute());
        }

        private void LSR_AbsoluteX()
        {
            remainingCycles = 7;
            pendingOperation = () => LSR_(AbsoluteX());
        }

        private void LSR_(ushort address)
        {
            byte value = ReadMemory(address);
            C = (value & 0x01) != 0;
            value >>= 1;
            WriteMemory(address, value);
            UpdateZeroAndNegativeFlags(value);
        }

        private void LSR_()
        {
            C = (A & 0x01) != 0;
            A >>= 1;
            UpdateZeroAndNegativeFlags(A);
        }

        private void ROL()
        {
            remainingCycles = 2;
            pendingOperation = ROL_;
        }

        private void ROL_ZeroPage()
        {
            remainingCycles = 5;
            pendingOperation = () => ROL_(ZeroPage());
        }

        private void ROL_ZeroPageX()
        {
            remainingCycles = 6;
            pendingOperation = () => ROL_(ZeroPageX());
        }

        private void ROL_Absolute()
        {
            remainingCycles = 6;
            pendingOperation = () => ROL_(Absolute());
        }

        private void ROL_AbsoluteX()
        {
            remainingCycles = 7;
            pendingOperation = () => ROL_(AbsoluteX());
        }

        private void ROL_(ushort address)
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

        private void ROL_()
        {
            bool newC = (A & 0x80) != 0;
            A <<= 1;
            if (C)
                A |= 0x01;
            C = newC;
            UpdateZeroAndNegativeFlags(A);
        }

        private void ROR()
        {
            remainingCycles = 2;
            pendingOperation = ROR_;
        }

        private void ROR_ZeroPage()
        {
            remainingCycles = 5;
            pendingOperation = () => ROR_(ZeroPage());
        }

        private void ROR_ZeroPageX()
        {
            remainingCycles = 6;
            pendingOperation = () => ROR_(ZeroPageX());
        }

        private void ROR_Absolute()
        {
            remainingCycles = 6;
            pendingOperation = () => ROR_(Absolute());
        }

        private void ROR_AbsoluteX()
        {
            remainingCycles = 7;
            pendingOperation = () => ROR_(AbsoluteX());
        }

        private void ROR_(ushort address)
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

        private void ROR_()
        {
            bool newC = (A & 0x01) != 0;
            A >>= 1;
            if (C)
                A |= 0x80;
            C = newC;
            UpdateZeroAndNegativeFlags(A);
        }

        // Compare Operations
        private void CMP_Immediate()
        {
            remainingCycles = 2;
            pendingOperation = () => CMP_(Immediate());
        }

        private void CMP_ZeroPage()
        {
            remainingCycles = 3;
            pendingOperation = () => CMP_(ZeroPage());
        }

        private void CMP_ZeroPageX()
        {
            remainingCycles = 4;
            pendingOperation = () => CMP_(ZeroPageX());
        }

        private void CMP_Absolute()
        {
            remainingCycles = 4;
            pendingOperation = () => CMP_(Absolute());
        }

        private void CMP_AbsoluteX()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
            pendingOperation = () => CMP_(AbsoluteX());
        }

        private void CMP_AbsoluteY()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
            pendingOperation = () => CMP_(AbsoluteY());
        }

        private void CMP_IndirectX()
        {
            remainingCycles = 6;
            pendingOperation = () => CMP_(IndirectX());
        }

        private void CMP_IndirectY()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_IndirectY()) remainingCycles++;
            pendingOperation = () => CMP_(IndirectY());
        }

        private void CMP_(byte value)
        {
            ushort result = (byte)(A - value);
            UpdateZeroAndNegativeFlags((byte)result);
            C = A >= value;
        }

        private void CMP_(ushort address)
        {
            byte value = ReadMemory(address);
            ushort result = (byte)(A - value);
            UpdateZeroAndNegativeFlags((byte)result);
            C = A >= value;
        }

        private void CPX_Immediate()
        {
            remainingCycles = 2;
            pendingOperation = () => CPX_(Immediate());
        }

        private void CPX_ZeroPage()
        {
            remainingCycles = 3;
            pendingOperation = () => CPX_(ZeroPage());
        }

        private void CPX_Absolute()
        {
            remainingCycles = 4;
            pendingOperation = () => CPX_(Absolute());
        }

        private void CPX_(byte value)
        {
            ushort result = (byte)(X - value);
            UpdateZeroAndNegativeFlags((byte)result);
            C = X >= value;
        }

        private void CPX_(ushort address)
        {
            byte value = ReadMemory(address);
            ushort result = (byte)(X - value);
            UpdateZeroAndNegativeFlags((byte)result);
            C = X >= value;
        }

        private void CPY_Immediate()
        {
            remainingCycles = 2;
            pendingOperation = () => CPY_(Immediate());
        }

        private void CPY_ZeroPage()
        {
            remainingCycles = 3;
            pendingOperation = () => CPY_(ZeroPage());
        }

        private void CPY_Absolute()
        {
            remainingCycles = 4;
            pendingOperation = () => CPY_(Absolute());
        }

        private void CPY_(byte value)
        {
            ushort result = (ushort)(Y - value);
            UpdateZeroAndNegativeFlags((byte)result);
            C = Y >= value;
        }

        private void CPY_(ushort address)
        {
            byte value = ReadMemory(address);
            ushort result = (ushort)(Y - value);
            UpdateZeroAndNegativeFlags((byte)result);
            C = Y >= value;
        }

        private void BNE_Relative()
        {
            remainingCycles = 2; // +1 if branch succeeds, +2 if to a new page
            remainingCycles += BranchCyclesNeeded_Relative(!Z);
            pendingOperation = () => BNE_(Relative());
        }

        private void BNE_(sbyte offset)
        {
            if (!Z)
                PC += (ushort)offset;
        }

        private void BEQ_Relative()
        {
            remainingCycles = 2; // +1 if branch succeeds, +2 if to a new page
            remainingCycles += BranchCyclesNeeded_Relative(Z);
            pendingOperation = () => BEQ_(Relative());
        }
        private void BEQ_(sbyte offset)
        {
            if (Z)
                PC += (ushort)offset;
        }

        // Branching Operations
        private void BPL_Relative()
        {
            remainingCycles = 2; // +1 if branch succeeds, +2 if to a new page
            remainingCycles += BranchCyclesNeeded_Relative(!N);
            pendingOperation = () => BPL_(Relative());
        }

        private void BPL_(sbyte offset)
        {
            if (!N)
                PC += (ushort)offset;
        }

        private void BMI_Relative()
        {
            remainingCycles = 2; // +1 if branch succeeds, +2 if to a new page
            remainingCycles += BranchCyclesNeeded_Relative(N);
            pendingOperation = () => BMI_(Relative());
        }

        private void BMI_(sbyte offset)
        {
            if (N)
                PC += (ushort)offset;
        }

        private void BCC_Relative()
        {
            remainingCycles = 2; // +1 if branch succeeds, +2 if to a new page
            remainingCycles += BranchCyclesNeeded_Relative(!C);
            pendingOperation = () => BCC_(Relative());
        }

        private void BCC_(sbyte offset)
        {
            if (!C)
                PC += (ushort)offset;
        }

        private void BCS_Relative()
        {
            remainingCycles = 2; // +1 if branch succeeds, +2 if to a new page
            remainingCycles += BranchCyclesNeeded_Relative(C);
            pendingOperation = () => BCS_(Relative());
        }

        private void BCS_(sbyte offset)
        {
            if (C)
                PC += (ushort)offset;
        }

        private void BVC_Relative()
        {
            remainingCycles = 2; // +1 if branch succeeds, +2 if to a new page
            remainingCycles += BranchCyclesNeeded_Relative(!V);
            pendingOperation = () => BVC_(Relative());
        }

        private void BVC_(sbyte offset)
        {
            if (!V)
                PC += (ushort)offset;
        }

        private void BVS_Relative()
        {
            remainingCycles = 2; // +1 if branch succeeds, +2 if to a new page
            remainingCycles += BranchCyclesNeeded_Relative(V);
            pendingOperation = () => BVS_(Relative());
        }

        private void BVS_(sbyte offset)
        {
            if (V)
                PC += (ushort)offset;
        }

        // Jump and Call Operations
        private void JMP_Absolute()
        {
            remainingCycles = 3;
            pendingOperation = () => JMP_(Absolute());
        }

        private void JMP_Indirect()
        {
            remainingCycles = 5;
            pendingOperation = () => JMP_(Indirect());
        }

        private void JMP_(ushort address)
        {
            PC = address;
        }

        private void JSR_Absolute()
        {
            remainingCycles = 6;
            pendingOperation = () => JSR_(Absolute());
        }

        private void JSR_(ushort address)
        {
            PushStack((byte)((PC - 1) >> 8));
            PushStack((byte)(PC - 1));
            PC = address;
        }

        private void RTS()
        {
            remainingCycles = 6;
            pendingOperation = RTS_;
        }

        private void RTS_()
        {
            PC = (ushort)(PopStack() | (PopStack() << 8));
            PC++;
        }

        private void RTI()
        {
            remainingCycles = 6;
            pendingOperation = RTI_;
        }

        private void RTI_()
        {
            byte flags = PopStack();
            P = (byte)((P & 0x60) | (flags & 0xCF)); // Preserve bit 4 (B flag) and bit 5 (unused flag) and update the rest with the stack value
            PC = (ushort)(PopStack() | (PopStack() << 8));
        }

        // Status Flag Operations
        private void CLC()
        {
            remainingCycles = 2;
            pendingOperation = CLC_;
        }

        private void CLC_()
        {
            C = false;
        }

        private void SEC()
        {
            remainingCycles = 2;
            pendingOperation = SEC_;
        }

        private void SEC_()
        {
            C = true;
        }

        private void CLI()
        {
            remainingCycles = 2;
            pendingOperation = CLI_;
        }

        private void CLI_()
        {
            I = false;
        }

        private void SEI()
        {
            remainingCycles = 2;
            pendingOperation = SEI_;
        }

        private void SEI_()
        {
            I = true;
        }

        private void CLD()
        {
            remainingCycles = 2;
            pendingOperation = CLD_;
        }

        private void CLD_()
        {
            D = false;
        }

        private void SED()
        {
            remainingCycles = 2;
            pendingOperation = SED_;
        }

        private void SED_()
        {
            D = true;
        }

        private void CLV()
        {
            remainingCycles = 2;
            pendingOperation = CLV_;
        }

        private void CLV_()
        {
            V = false;
        }

        // System Functions
        private void BRK()
        {
            remainingCycles = 7;
            pendingOperation = BRK_;
        }

        private void BRK_()
        {
            PC++; // Increment PC to point to the next instruction
            PushStack((byte)(PC >> 8)); // Push high byte of PC onto the stack
            PushStack((byte)(PC & 0xFF)); // Push low byte of PC onto the stack
            PushStack((byte)(P | 0x30)); // Push bit 4 (B flag) and bit 5 (unused flag) set to 1 onto the stack
            I = true; // Set Interrupt flag to disable further interrupts
            PC = (ushort)(ReadMemory(0xFFFE) | (ReadMemory(0xFFFF) << 8)); // Set PC to the interrupt vector address
        }

        private void NOP()
        {
            remainingCycles = 2;
            pendingOperation = NOP_;
        }

        private void NOP_()
        {
        }

        private void NOP_Immediate()
        {
            remainingCycles = 2;
            pendingOperation = NOP_Immediate_;
        }

        private void NOP_Immediate_()
        {
            _ = Immediate();
        }

        private void NOP_ZeroPage()
        {
            remainingCycles = 3;
            pendingOperation = NOP_ZeroPage_;
        }

        private void NOP_ZeroPage_()
        {
            _ = ZeroPage();
        }

        private void NOP_ZeroPageX()
        {
            remainingCycles = 4;
            pendingOperation = NOP_ZeroPageX_;
        }

        private void NOP_ZeroPageX_()
        {
            _ = ZeroPageX();
        }

        private void NOP_Absolute()
        {
            remainingCycles = 4;
            pendingOperation = NOP_Absolute_;
        }

        private void NOP_Absolute_()
        {
            _ = Absolute();
        }

        private void NOP_AbsoluteX()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
            pendingOperation = NOP_AbsoluteX_;
        }

        private void NOP_AbsoluteX_()
        {
            _ = AbsoluteX();
        }

        private static void STP()
        {
            throw new InvalidOperationException("STP Instruction encountered.");
        }

        private void ALR_Immediate()
        {
            remainingCycles = 2;
            pendingOperation = () => ALR_(Immediate());
        }
        private void ALR_(byte operand)
        {
            AND_(operand);
            LSR_();
        }

        private void ANC_Immediate()
        {
            remainingCycles = 2;
            pendingOperation = () => ANC_(Immediate());
        }

        private void ANC_(byte operand)
        {
            AND_(operand);
            UpdateZeroAndNegativeFlags(A);
            C = (A & 0x80) != 0; // Set the carry flag based on the value of the 7th bit of A
        }

        private void ANE_Immediate()
        {
            remainingCycles = 2;
            pendingOperation = () => ANE_(Immediate());
        }

        private void ANE_(byte operand)
        {
            A = (byte)(A & X & operand);
            UpdateZeroAndNegativeFlags(A);
        }

        private void ARR_Immediate()
        {
            remainingCycles = 2;
            pendingOperation = () => ARR_(Immediate());
        }

        private void ARR_(byte operand)
        {
            AND_(operand);
            ROR_();
            UpdateZeroAndNegativeFlags(A);
            C = (A & 0x40) != 0; // Set bit 6 of A as the carry flag
            V = ((A & 0x40) ^ ((A & 0x20) << 1)) != 0; // Set bit 6 xor bit 5 of A as the overflow flag
        }

        private void AXS_Immediate()
        {
            remainingCycles = 2;
            pendingOperation = () => AXS_(Immediate());
        }

        private void AXS_(byte operand)
        {
            int result = (A & X) - operand;
            X = (byte)(result & 0xFF);
            UpdateZeroAndNegativeFlags(X);
            C = result >= 0; // Set the carry flag based on the result without borrow
        }

        private void LAS_AbsoluteY()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
            pendingOperation = () => LAS_(AbsoluteY());
        }

        private void LAS_(ushort address)
        {
            byte value = ReadMemory(address);
            byte result = (byte)(value & S);
            A = result;
            X = result;
            S = result;
            UpdateZeroAndNegativeFlags(result);
        }

        private void LAX_ZeroPage()
        {
            remainingCycles = 3;
            pendingOperation = () => LAX_(ZeroPage());
        }

        private void LAX_ZeroPageY()
        {
            remainingCycles = 4;
            pendingOperation = () => LAX_(ZeroPageY());
        }

        private void LAX_Absolute()
        {
            remainingCycles = 4;
            pendingOperation = () => LAX_(Absolute());
        }

        private void LAX_AbsoluteY()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
            pendingOperation = () => LAX_(AbsoluteY());
        }

        private void LAX_IndirectX()
        {
            remainingCycles = 6;
            pendingOperation = () => LAX_(IndirectX());
        }

        private void LAX_IndirectY()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_IndirectY()) remainingCycles++;
            pendingOperation = () => LAX_(IndirectY());
        }

        private void LAX_(ushort address)
        {
            LDA_(address);
            TAX();
        }

        private void LXA_Immediate()
        {
            remainingCycles = 2;
            pendingOperation = () => LXA_(Immediate());
        }

        private void LXA_(byte operand)
        {
            byte result = (byte)(A & operand);
            A = result;
            X = result;
            UpdateZeroAndNegativeFlags(result);
        }

        private void SAX_ZeroPage()
        {
            remainingCycles = 3;
            pendingOperation = () => SAX_(ZeroPage());
        }

        private void SAX_ZeroPageY()
        {
            remainingCycles = 4;
            pendingOperation = () => SAX_(ZeroPageY());
        }

        private void SAX_Absolute()
        {
            remainingCycles = 4;
            pendingOperation = () => SAX_(Absolute());
        }

        private void SAX_IndirectX()
        {
            remainingCycles = 6;
            pendingOperation = () => SAX_(IndirectX());
        }

        private void SAX_(ushort address)
        {
            byte result = (byte)(A & X);
            WriteMemory(address, result);
        }

        private void SHA_AbsoluteY()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
            pendingOperation = () => SHA_(AbsoluteY());
        }

        private void SHA_IndirectY()
        {
            remainingCycles = 6;
            pendingOperation = () => SHA_(IndirectY());
        }

        private void SHA_(ushort address)
        {
            byte result = (byte)(A & X & ((address >> 8) + 1));
            WriteMemory(address, result);
        }

        private void SHX_AbsoluteY()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
            pendingOperation = () => SHX_(AbsoluteY());
        }

        private void SHX_(ushort address)
        {
            byte result = (byte)(X & ((address >> 8) + 1));
            WriteMemory(address, result);
        }

        private void SHY_AbsoluteX()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
            pendingOperation = () => SHY_(AbsoluteX());
        }

        private void SHY_(ushort address)
        {
            byte result = (byte)(Y & ((address >> 8) + 1));
            WriteMemory(address, result);
        }

        private void TAS_AbsoluteY()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
            pendingOperation = () => TAS_(AbsoluteY());
        }

        private void TAS_(ushort address)
        {
            byte result = (byte)(A & X);
            S = result;
            result &= (byte)((address >> 8) + 1);
            WriteMemory(address, result);
        }

        private void DCP_ZeroPage()
        {
            remainingCycles = 5;
            pendingOperation = () => DCP_(ZeroPage());
        }

        private void DCP_ZeroPageX()
        {
            remainingCycles = 6;
            pendingOperation = () => DCP_(ZeroPageX());
        }

        private void DCP_Absolute()
        {
            remainingCycles = 6;
            pendingOperation = () => DCP_(Absolute());
        }

        private void DCP_AbsoluteX()
        {
            remainingCycles = 7;
            pendingOperation = () => DCP_(AbsoluteX());
        }

        private void DCP_AbsoluteY()
        {
            remainingCycles = 7;
            pendingOperation = () => DCP_(AbsoluteY());
        }

        private void DCP_IndirectX()
        {
            remainingCycles = 8;
            pendingOperation = () => DCP_(IndirectX());
        }

        private void DCP_IndirectY()
        {
            remainingCycles = 8;
            pendingOperation = () => DCP_(IndirectY());
        }

        private void DCP_(ushort address)
        {
            DEC_(address);
            CMP_(address);
        }

        private void ISC_ZeroPage()
        {
            remainingCycles = 5;
            pendingOperation = () => ISC_(ZeroPage());
        }

        private void ISC_ZeroPageX()
        {
            remainingCycles = 6;
            pendingOperation = () => ISC_(ZeroPageX());
        }

        private void ISC_Absolute()
        {
            remainingCycles = 6;
            pendingOperation = () => ISC_(Absolute());
        }

        private void ISC_AbsoluteX()
        {
            remainingCycles = 7;
            pendingOperation = () => ISC_(AbsoluteX());
        }

        private void ISC_AbsoluteY()
        {
            remainingCycles = 7;
            pendingOperation = () => ISC_(AbsoluteY());
        }

        private void ISC_IndirectX()
        {
            remainingCycles = 8;
            pendingOperation = () => ISC_(IndirectX());
        }

        private void ISC_IndirectY()
        {
            remainingCycles = 8;
            pendingOperation = () => ISC_(IndirectY());
        }

        private void ISC_(ushort address)
        {
            INC_(address);
            SBC_(address);
        }

        private void RLA_ZeroPage()
        {
            remainingCycles = 5;
            pendingOperation = () => RLA_(ZeroPage());
        }

        private void RLA_ZeroPageX()
        {
            remainingCycles = 6;
            pendingOperation = () => RLA_(ZeroPageX());
        }

        private void RLA_Absolute()
        {
            remainingCycles = 6;
            pendingOperation = () => RLA_(Absolute());
        }

        private void RLA_AbsoluteX()
        {
            remainingCycles = 7;
            pendingOperation = () => RLA_(AbsoluteX());
        }

        private void RLA_AbsoluteY()
        {
            remainingCycles = 7;
            pendingOperation = () => RLA_(AbsoluteY());
        }

        private void RLA_IndirectX()
        {
            remainingCycles = 8;
            pendingOperation = () => RLA_(IndirectX());
        }

        private void RLA_IndirectY()
        {
            remainingCycles = 8;
            pendingOperation = () => RLA_(IndirectY());
        }

        private void RLA_(ushort address)
        {
            ROL_(address);
            AND_(address);
        }

        private void RRA_ZeroPage()
        {
            remainingCycles = 5;
            pendingOperation = () => RRA_(ZeroPage());
        }

        private void RRA_ZeroPageX()
        {
            remainingCycles = 6;
            pendingOperation = () => RRA_(ZeroPageX());
        }

        private void RRA_Absolute()
        {
            remainingCycles = 6;
            pendingOperation = () => RRA_(Absolute());
        }

        private void RRA_AbsoluteX()
        {
            remainingCycles = 7;
            pendingOperation = () => RRA_(AbsoluteX());
        }

        private void RRA_AbsoluteY()
        {
            remainingCycles = 7;
            pendingOperation = () => RRA_(AbsoluteY());
        }

        private void RRA_IndirectX()
        {
            remainingCycles = 8;
            pendingOperation = () => RRA_(IndirectX());
        }

        private void RRA_IndirectY()
        {
            remainingCycles = 8;
            pendingOperation = () => RRA_(IndirectY());
        }

        private void RRA_(ushort address)
        {
            ROR_(address);
            ADC_(address);
        }

        private void SLO_ZeroPage()
        {
            remainingCycles = 5;
            pendingOperation = () => SLO_(ZeroPage());
        }

        private void SLO_ZeroPageX()
        {
            remainingCycles = 6;
            pendingOperation = () => SLO_(ZeroPageX());
        }

        private void SLO_Absolute()
        {
            remainingCycles = 6;
            pendingOperation = () => SLO_(Absolute());
        }

        private void SLO_AbsoluteX()
        {
            remainingCycles = 7;
            pendingOperation = () => SLO_(AbsoluteX());
        }

        private void SLO_AbsoluteY()
        {
            remainingCycles = 7;
            pendingOperation = () => SLO_(AbsoluteY());
        }

        private void SLO_IndirectX()
        {
            remainingCycles = 8;
            pendingOperation = () => SLO_(IndirectX());
        }

        private void SLO_IndirectY()
        {
            remainingCycles = 8;
            pendingOperation = () => SLO_(IndirectY());
        }

        private void SLO_(ushort address)
        {
            ASL_(address);
            ORA_(address);
        }

        private void SRE_ZeroPage()
        {
            remainingCycles = 5;
            pendingOperation = () => SRE_(ZeroPage());
        }

        private void SRE_ZeroPageX()
        {
            remainingCycles = 6;
            pendingOperation = () => SRE_(ZeroPageX());
        }

        private void SRE_Absolute()
        {
            remainingCycles = 6;
            pendingOperation = () => SRE_(Absolute());
        }

        private void SRE_AbsoluteX()
        {
            remainingCycles = 7;
            pendingOperation = () => SRE_(AbsoluteX());
        }

        private void SRE_AbsoluteY()
        {
            remainingCycles = 7;
            pendingOperation = () => SRE_(AbsoluteY());
        }

        private void SRE_IndirectX()
        {
            remainingCycles = 8;
            pendingOperation = () => SRE_(IndirectX());
        }

        private void SRE_IndirectY()
        {
            remainingCycles = 8;
            pendingOperation = () => SRE_(IndirectY());
        }

        private void SRE_(ushort address)
        {
            LSR_(address);
            EOR_(address);
        }

        // Helper functions for stack operations
        private void PushStack(byte value)
        {
            WriteMemory((ushort)(0x0100 | S), value);
            S--;
        }

        private byte PopStack()
        {
            S++;
            return ReadMemory((ushort)(0x0100 | S));
        }

        public void NMI_Begin()
        {
            remainingCycles = 7;
            pendingOperation = NMI_Setup;
        }

        public void NMI_Setup()
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

        // Begin the DMA transfer
        public void DMA_Begin(byte page)
        {
            remainingCycles = 1;
            pendingOperation = () => DMA_Setup(page);
        }

        // Setup the DMA transfer
        public void DMA_Setup(byte page)
        {
            dmaPage = page;
            dmaAddress = 0;
            dmaCycleCounter = 512;
            remainingCycles = 1;
            pendingOperation = () => DMA_Transfer();
        }

        // Handle the DMA transfer
        public void DMA_Transfer()
        {
            // Only perform a read/write operation every 2 cycles
            if (dmaCycleCounter % 2 == 0)
            {
                byte value = ReadMemory((ushort)(dmaPage << 8 | dmaAddress));
                ppu.WriteRegister(0x2004, value);

                // Increment dmaAddress and handle CPU memory page boundary wrapping
                dmaAddress = (byte)((dmaAddress + 1) & 0xFF);
            }

            dmaCycleCounter--;

            // End the DMA transfer when all cycles are completed
            if (dmaCycleCounter > 0)
            {
                remainingCycles = 1;
            }
        }
    }
}
