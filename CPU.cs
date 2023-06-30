namespace Emulation
{
    using System;
    public class CPU
    {
        private int remainingCycles;

        // Internal Registers
        public byte A; // Accumulator
        public byte X, Y; // General-purpose registers
        public byte S; // Stack pointer
        public ushort PC; // Program counter

        // Processor Status Flags (Register P)
        public bool N; // Negative flag
        public bool V; // Overflow flag
        // public bool U; // Unused
        // public bool B; // Break flag
        public bool D; // Decimal mode flag
        public bool I; // Interrupt disable flag
        public bool Z; // Zero flag
        public bool C; // Carry flag

        private byte dmaPage;
        private byte dmaAddress;
        public int dmaCycleCounter;

        private int opcode;

        // Allocate an array for the opcode jump table
        private readonly Action[] opcodeExecuteJumpTable = new Action[258];

        // Allocate an array for the opcode jump table
        private readonly Action[] opcodeSetCyclesJumpTable = new Action[258];

        private void InitializeSetCyclesJumpTable()
        {
            // Register Transfers
            opcodeSetCyclesJumpTable[0xAA] = TAX_SetCycles;
            opcodeSetCyclesJumpTable[0x8A] = TXA_SetCycles;
            opcodeSetCyclesJumpTable[0xA8] = TAY_SetCycles;
            opcodeSetCyclesJumpTable[0x98] = TYA_SetCycles;
            opcodeSetCyclesJumpTable[0xBA] = TSX_SetCycles;
            opcodeSetCyclesJumpTable[0x9A] = TXS_SetCycles;
            opcodeSetCyclesJumpTable[0x68] = PLA_SetCycles;
            opcodeSetCyclesJumpTable[0x48] = PHA_SetCycles;
            opcodeSetCyclesJumpTable[0x28] = PLP_SetCycles;
            opcodeSetCyclesJumpTable[0x08] = PHP_SetCycles;

            // Load and Store Operations
            opcodeSetCyclesJumpTable[0xA9] = LDA_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0xA5] = LDA_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0xB5] = LDA_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0xAD] = LDA_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0xBD] = LDA_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0xB9] = LDA_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0xA1] = LDA_IndirectX_SetCycles;
            opcodeSetCyclesJumpTable[0xB1] = LDA_IndirectY_SetCycles;
            opcodeSetCyclesJumpTable[0xA2] = LDX_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0xA6] = LDX_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0xB6] = LDX_ZeroPageY_SetCycles;
            opcodeSetCyclesJumpTable[0xAE] = LDX_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0xBE] = LDX_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0xA0] = LDY_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0xA4] = LDY_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0xB4] = LDY_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0xAC] = LDY_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0xBC] = LDY_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0x85] = STA_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x95] = STA_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0x8D] = STA_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0x9D] = STA_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0x99] = STA_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0x81] = STA_IndirectX_SetCycles;
            opcodeSetCyclesJumpTable[0x91] = STA_IndirectY_SetCycles;
            opcodeSetCyclesJumpTable[0x86] = STX_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x96] = STX_ZeroPageY_SetCycles;
            opcodeSetCyclesJumpTable[0x8E] = STX_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0x84] = STY_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x94] = STY_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0x8C] = STY_Absolute_SetCycles;

            // Arithmetic and Logical Operations
            opcodeSetCyclesJumpTable[0x69] = ADC_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0x65] = ADC_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x75] = ADC_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0x6D] = ADC_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0x7D] = ADC_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0x79] = ADC_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0x61] = ADC_IndirectX_SetCycles;
            opcodeSetCyclesJumpTable[0x71] = ADC_IndirectY_SetCycles;
            opcodeSetCyclesJumpTable[0xE9] = SBC_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0xE5] = SBC_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0xF5] = SBC_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0xED] = SBC_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0xFD] = SBC_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0xF9] = SBC_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0xE1] = SBC_IndirectX_SetCycles;
            opcodeSetCyclesJumpTable[0xF1] = SBC_IndirectY_SetCycles;
            opcodeSetCyclesJumpTable[0x29] = AND_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0x25] = AND_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x35] = AND_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0x2D] = AND_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0x3D] = AND_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0x39] = AND_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0x21] = AND_IndirectX_SetCycles;
            opcodeSetCyclesJumpTable[0x31] = AND_IndirectY_SetCycles;
            opcodeSetCyclesJumpTable[0x09] = ORA_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0x05] = ORA_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x15] = ORA_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0x0D] = ORA_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0x1D] = ORA_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0x19] = ORA_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0x01] = ORA_IndirectX_SetCycles;
            opcodeSetCyclesJumpTable[0x11] = ORA_IndirectY_SetCycles;
            opcodeSetCyclesJumpTable[0x49] = EOR_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0x45] = EOR_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x55] = EOR_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0x4D] = EOR_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0x5D] = EOR_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0x59] = EOR_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0x41] = EOR_IndirectX_SetCycles;
            opcodeSetCyclesJumpTable[0x51] = EOR_IndirectY_SetCycles;
            opcodeSetCyclesJumpTable[0x24] = BIT_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x2C] = BIT_Absolute_SetCycles;

            // Increment and Decrement Operations
            opcodeSetCyclesJumpTable[0xE6] = INC_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0xEE] = INC_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0xF6] = INC_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0xFE] = INC_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0xC6] = DEC_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0xD6] = DEC_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0xCE] = DEC_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0xDE] = DEC_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0xE8] = INX_SetCycles;
            opcodeSetCyclesJumpTable[0xC8] = INY_SetCycles;
            opcodeSetCyclesJumpTable[0xCA] = DEX_SetCycles;
            opcodeSetCyclesJumpTable[0x88] = DEY_SetCycles;
            opcodeSetCyclesJumpTable[0x0A] = ASL_SetCycles;

            // Shift Operations
            opcodeSetCyclesJumpTable[0x06] = ASL_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x16] = ASL_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0x0E] = ASL_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0x1E] = ASL_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0x4A] = LSR_SetCycles;
            opcodeSetCyclesJumpTable[0x46] = LSR_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x56] = LSR_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0x4E] = LSR_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0x5E] = LSR_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0x2A] = ROL_SetCycles;
            opcodeSetCyclesJumpTable[0x26] = ROL_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x36] = ROL_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0x2E] = ROL_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0x3E] = ROL_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0x6A] = ROR_SetCycles;
            opcodeSetCyclesJumpTable[0x66] = ROR_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x76] = ROR_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0x6E] = ROR_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0x7E] = ROR_AbsoluteX_SetCycles;

            // Compare Operations
            opcodeSetCyclesJumpTable[0xC9] = CMP_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0xC5] = CMP_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0xD5] = CMP_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0xCD] = CMP_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0xDD] = CMP_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0xD9] = CMP_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0xC1] = CMP_IndirectX_SetCycles;
            opcodeSetCyclesJumpTable[0xD1] = CMP_IndirectY_SetCycles;
            opcodeSetCyclesJumpTable[0xE0] = CPX_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0xE4] = CPX_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0xEC] = CPX_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0xC0] = CPY_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0xC4] = CPY_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0xCC] = CPY_Absolute_SetCycles;

            // Branching Operations
            opcodeSetCyclesJumpTable[0xD0] = BNE_Relative_SetCycles;
            opcodeSetCyclesJumpTable[0xF0] = BEQ_Relative_SetCycles;
            opcodeSetCyclesJumpTable[0x10] = BPL_Relative_SetCycles;
            opcodeSetCyclesJumpTable[0x30] = BMI_Relative_SetCycles;
            opcodeSetCyclesJumpTable[0x90] = BCC_Relative_SetCycles;
            opcodeSetCyclesJumpTable[0xB0] = BCS_Relative_SetCycles;
            opcodeSetCyclesJumpTable[0x50] = BVC_Relative_SetCycles;
            opcodeSetCyclesJumpTable[0x70] = BVS_Relative_SetCycles;

            // Jump and Call Operations
            opcodeSetCyclesJumpTable[0x4C] = JMP_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0x6C] = JMP_Indirect_SetCycles;
            opcodeSetCyclesJumpTable[0x20] = JSR_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0x60] = RTS_SetCycles;
            opcodeSetCyclesJumpTable[0x40] = RTI_SetCycles;

            // Status Flag Operations
            opcodeSetCyclesJumpTable[0x18] = CLC_SetCycles;
            opcodeSetCyclesJumpTable[0x38] = SEC_SetCycles;
            opcodeSetCyclesJumpTable[0x58] = CLI_SetCycles;
            opcodeSetCyclesJumpTable[0x78] = SEI_SetCycles;
            opcodeSetCyclesJumpTable[0xD8] = CLD_SetCycles;
            opcodeSetCyclesJumpTable[0xF8] = SED_SetCycles;
            opcodeSetCyclesJumpTable[0xB8] = CLV_SetCycles;

            // System Functions
            opcodeSetCyclesJumpTable[0x00] = BRK_SetCycles;
            opcodeSetCyclesJumpTable[0xEA] = NOP_SetCycles;

            // Unofficial Instructions
            opcodeSetCyclesJumpTable[0xEB] = SBC_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0x1A] = NOP_SetCycles;
            opcodeSetCyclesJumpTable[0x3A] = NOP_SetCycles;
            opcodeSetCyclesJumpTable[0x5A] = NOP_SetCycles;
            opcodeSetCyclesJumpTable[0x7A] = NOP_SetCycles;
            opcodeSetCyclesJumpTable[0xDA] = NOP_SetCycles;
            opcodeSetCyclesJumpTable[0xFA] = NOP_SetCycles;
            opcodeSetCyclesJumpTable[0x80] = NOP_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0x82] = NOP_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0x89] = NOP_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0xC2] = NOP_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0xE2] = NOP_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0x04] = NOP_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x44] = NOP_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x64] = NOP_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x14] = NOP_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0x34] = NOP_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0x54] = NOP_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0x74] = NOP_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0xD4] = NOP_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0xF4] = NOP_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0x0C] = NOP_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0x1C] = NOP_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0x3C] = NOP_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0x5C] = NOP_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0x7C] = NOP_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0xDC] = NOP_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0xFC] = NOP_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0x02] = STP;
            opcodeSetCyclesJumpTable[0x12] = STP;
            opcodeSetCyclesJumpTable[0x22] = STP;
            opcodeSetCyclesJumpTable[0x32] = STP;
            opcodeSetCyclesJumpTable[0x42] = STP;
            opcodeSetCyclesJumpTable[0x52] = STP;
            opcodeSetCyclesJumpTable[0x62] = STP;
            opcodeSetCyclesJumpTable[0x72] = STP;
            opcodeSetCyclesJumpTable[0x92] = STP;
            opcodeSetCyclesJumpTable[0xB2] = STP;
            opcodeSetCyclesJumpTable[0xD2] = STP;
            opcodeSetCyclesJumpTable[0xF2] = STP;
            opcodeSetCyclesJumpTable[0x4B] = ALR_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0x0B] = ANC_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0x2B] = ANC_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0x8B] = ANE_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0x6B] = ARR_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0xCB] = AXS_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0xBB] = LAS_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0xA7] = LAX_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0xB7] = LAX_ZeroPageY_SetCycles;
            opcodeSetCyclesJumpTable[0xAF] = LAX_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0xBF] = LAX_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0xA3] = LAX_IndirectX_SetCycles;
            opcodeSetCyclesJumpTable[0xB3] = LAX_IndirectY_SetCycles;
            opcodeSetCyclesJumpTable[0xAB] = LXA_Immediate_SetCycles;
            opcodeSetCyclesJumpTable[0x87] = SAX_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x97] = SAX_ZeroPageY_SetCycles;
            opcodeSetCyclesJumpTable[0x8F] = SAX_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0x83] = SAX_IndirectX_SetCycles;
            opcodeSetCyclesJumpTable[0x9F] = SHA_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0x93] = SHA_IndirectY_SetCycles;
            opcodeSetCyclesJumpTable[0x9E] = SHX_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0x9C] = SHY_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0x9B] = TAS_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0xC7] = DCP_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0xD7] = DCP_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0xCF] = DCP_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0xDF] = DCP_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0xDB] = DCP_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0xC3] = DCP_IndirectX_SetCycles;
            opcodeSetCyclesJumpTable[0xD3] = DCP_IndirectY_SetCycles;
            opcodeSetCyclesJumpTable[0xE7] = ISC_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0xF7] = ISC_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0xEF] = ISC_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0xFF] = ISC_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0xFB] = ISC_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0xE3] = ISC_IndirectX_SetCycles;
            opcodeSetCyclesJumpTable[0xF3] = ISC_IndirectY_SetCycles;
            opcodeSetCyclesJumpTable[0x27] = RLA_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x37] = RLA_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0x2F] = RLA_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0x3F] = RLA_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0x3B] = RLA_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0x23] = RLA_IndirectX_SetCycles;
            opcodeSetCyclesJumpTable[0x33] = RLA_IndirectY_SetCycles;
            opcodeSetCyclesJumpTable[0x67] = RRA_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x77] = RRA_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0x6F] = RRA_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0x7F] = RRA_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0x7B] = RRA_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0x63] = RRA_IndirectX_SetCycles;
            opcodeSetCyclesJumpTable[0x73] = RRA_IndirectY_SetCycles;
            opcodeSetCyclesJumpTable[0x07] = SLO_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x17] = SLO_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0x0F] = SLO_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0x1F] = SLO_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0x1B] = SLO_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0x03] = SLO_IndirectX_SetCycles;
            opcodeSetCyclesJumpTable[0x13] = SLO_IndirectY_SetCycles;
            opcodeSetCyclesJumpTable[0x47] = SRE_ZeroPage_SetCycles;
            opcodeSetCyclesJumpTable[0x57] = SRE_ZeroPageX_SetCycles;
            opcodeSetCyclesJumpTable[0x4F] = SRE_Absolute_SetCycles;
            opcodeSetCyclesJumpTable[0x5F] = SRE_AbsoluteX_SetCycles;
            opcodeSetCyclesJumpTable[0x5B] = SRE_AbsoluteY_SetCycles;
            opcodeSetCyclesJumpTable[0x43] = SRE_IndirectX_SetCycles;
            opcodeSetCyclesJumpTable[0x53] = SRE_IndirectY_SetCycles;

            // Fill the rest of the jump table with a default handler for unknown opcodes
            for (int i = 0; i < opcodeExecuteJumpTable.Length; i++)
            {
                if (opcodeExecuteJumpTable[i] == null)
                {
                    opcodeExecuteJumpTable[i] = UnknownOpcodeHandler;
                }
            }
        }

        private void InitializeExecutionJumpTable()
        {
            opcodeExecuteJumpTable[0x100] = DMA_Execute;
            opcodeExecuteJumpTable[0x101] = NMI_Execute;

            // Register Transfers
            opcodeExecuteJumpTable[0xAA] = TAX_Execute;
            opcodeExecuteJumpTable[0x8A] = TXA_Execute;
            opcodeExecuteJumpTable[0xA8] = TAY_Execute;
            opcodeExecuteJumpTable[0x98] = TYA_Execute;
            opcodeExecuteJumpTable[0xBA] = TSX_Execute;
            opcodeExecuteJumpTable[0x9A] = TXS_Execute;
            opcodeExecuteJumpTable[0x68] = PLA_Execute;
            opcodeExecuteJumpTable[0x48] = PHA_Execute;
            opcodeExecuteJumpTable[0x28] = PLP_Execute;
            opcodeExecuteJumpTable[0x08] = PHP_Execute;

            // Load and Store Operations
            opcodeExecuteJumpTable[0xA9] = LDA_Immediate_Execute;
            opcodeExecuteJumpTable[0xA5] = LDA_ZeroPage_Execute;
            opcodeExecuteJumpTable[0xB5] = LDA_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0xAD] = LDA_Absolute_Execute;
            opcodeExecuteJumpTable[0xBD] = LDA_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0xB9] = LDA_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0xA1] = LDA_IndirectX_Execute;
            opcodeExecuteJumpTable[0xB1] = LDA_IndirectY_Execute;
            opcodeExecuteJumpTable[0xA2] = LDX_Immediate_Execute;
            opcodeExecuteJumpTable[0xA6] = LDX_ZeroPage_Execute;
            opcodeExecuteJumpTable[0xB6] = LDX_ZeroPageY_Execute;
            opcodeExecuteJumpTable[0xAE] = LDX_Absolute_Execute;
            opcodeExecuteJumpTable[0xBE] = LDX_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0xA0] = LDY_Immediate_Execute;
            opcodeExecuteJumpTable[0xA4] = LDY_ZeroPage_Execute;
            opcodeExecuteJumpTable[0xB4] = LDY_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0xAC] = LDY_Absolute_Execute;
            opcodeExecuteJumpTable[0xBC] = LDY_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0x85] = STA_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x95] = STA_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0x8D] = STA_Absolute_Execute;
            opcodeExecuteJumpTable[0x9D] = STA_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0x99] = STA_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0x81] = STA_IndirectX_Execute;
            opcodeExecuteJumpTable[0x91] = STA_IndirectY_Execute;
            opcodeExecuteJumpTable[0x86] = STX_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x96] = STX_ZeroPageY_Execute;
            opcodeExecuteJumpTable[0x8E] = STX_Absolute_Execute;
            opcodeExecuteJumpTable[0x84] = STY_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x94] = STY_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0x8C] = STY_Absolute_Execute;

            // Arithmetic and Logical Operations
            opcodeExecuteJumpTable[0x69] = ADC_Immediate_Execute;
            opcodeExecuteJumpTable[0x65] = ADC_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x75] = ADC_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0x6D] = ADC_Absolute_Execute;
            opcodeExecuteJumpTable[0x7D] = ADC_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0x79] = ADC_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0x61] = ADC_IndirectX_Execute;
            opcodeExecuteJumpTable[0x71] = ADC_IndirectY_Execute;
            opcodeExecuteJumpTable[0xE9] = SBC_Immediate_Execute;
            opcodeExecuteJumpTable[0xE5] = SBC_ZeroPage_Execute;
            opcodeExecuteJumpTable[0xF5] = SBC_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0xED] = SBC_Absolute_Execute;
            opcodeExecuteJumpTable[0xFD] = SBC_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0xF9] = SBC_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0xE1] = SBC_IndirectX_Execute;
            opcodeExecuteJumpTable[0xF1] = SBC_IndirectY_Execute;
            opcodeExecuteJumpTable[0x29] = AND_Immediate_Execute;
            opcodeExecuteJumpTable[0x25] = AND_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x35] = AND_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0x2D] = AND_Absolute_Execute;
            opcodeExecuteJumpTable[0x3D] = AND_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0x39] = AND_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0x21] = AND_IndirectX_Execute;
            opcodeExecuteJumpTable[0x31] = AND_IndirectY_Execute;
            opcodeExecuteJumpTable[0x09] = ORA_Immediate_Execute;
            opcodeExecuteJumpTable[0x05] = ORA_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x15] = ORA_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0x0D] = ORA_Absolute_Execute;
            opcodeExecuteJumpTable[0x1D] = ORA_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0x19] = ORA_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0x01] = ORA_IndirectX_Execute;
            opcodeExecuteJumpTable[0x11] = ORA_IndirectY_Execute;
            opcodeExecuteJumpTable[0x49] = EOR_Immediate_Execute;
            opcodeExecuteJumpTable[0x45] = EOR_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x55] = EOR_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0x4D] = EOR_Absolute_Execute;
            opcodeExecuteJumpTable[0x5D] = EOR_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0x59] = EOR_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0x41] = EOR_IndirectX_Execute;
            opcodeExecuteJumpTable[0x51] = EOR_IndirectY_Execute;
            opcodeExecuteJumpTable[0x24] = BIT_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x2C] = BIT_Absolute_Execute;

            // Increment and Decrement Operations
            opcodeExecuteJumpTable[0xE6] = INC_ZeroPage_Execute;
            opcodeExecuteJumpTable[0xEE] = INC_Absolute_Execute;
            opcodeExecuteJumpTable[0xF6] = INC_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0xFE] = INC_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0xC6] = DEC_ZeroPage_Execute;
            opcodeExecuteJumpTable[0xD6] = DEC_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0xCE] = DEC_Absolute_Execute;
            opcodeExecuteJumpTable[0xDE] = DEC_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0xE8] = INX_Execute;
            opcodeExecuteJumpTable[0xC8] = INY_Execute;
            opcodeExecuteJumpTable[0xCA] = DEX_Execute;
            opcodeExecuteJumpTable[0x88] = DEY_Execute;
            opcodeExecuteJumpTable[0x0A] = ASL_Execute;

            // Shift Operations
            opcodeExecuteJumpTable[0x06] = ASL_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x16] = ASL_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0x0E] = ASL_Absolute_Execute;
            opcodeExecuteJumpTable[0x1E] = ASL_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0x4A] = LSR_Execute;
            opcodeExecuteJumpTable[0x46] = LSR_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x56] = LSR_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0x4E] = LSR_Absolute_Execute;
            opcodeExecuteJumpTable[0x5E] = LSR_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0x2A] = ROL_Execute;
            opcodeExecuteJumpTable[0x26] = ROL_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x36] = ROL_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0x2E] = ROL_Absolute_Execute;
            opcodeExecuteJumpTable[0x3E] = ROL_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0x6A] = ROR_Execute;
            opcodeExecuteJumpTable[0x66] = ROR_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x76] = ROR_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0x6E] = ROR_Absolute_Execute;
            opcodeExecuteJumpTable[0x7E] = ROR_AbsoluteX_Execute;

            // Compare Operations
            opcodeExecuteJumpTable[0xC9] = CMP_Immediate_Execute;
            opcodeExecuteJumpTable[0xC5] = CMP_ZeroPage_Execute;
            opcodeExecuteJumpTable[0xD5] = CMP_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0xCD] = CMP_Absolute_Execute;
            opcodeExecuteJumpTable[0xDD] = CMP_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0xD9] = CMP_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0xC1] = CMP_IndirectX_Execute;
            opcodeExecuteJumpTable[0xD1] = CMP_IndirectY_Execute;
            opcodeExecuteJumpTable[0xE0] = CPX_Immediate_Execute;
            opcodeExecuteJumpTable[0xE4] = CPX_ZeroPage_Execute;
            opcodeExecuteJumpTable[0xEC] = CPX_Absolute_Execute;
            opcodeExecuteJumpTable[0xC0] = CPY_Immediate_Execute;
            opcodeExecuteJumpTable[0xC4] = CPY_ZeroPage_Execute;
            opcodeExecuteJumpTable[0xCC] = CPY_Absolute_Execute;

            // Branching Operations
            opcodeExecuteJumpTable[0xD0] = BNE_Relative_Execute;
            opcodeExecuteJumpTable[0xF0] = BEQ_Relative_Execute;
            opcodeExecuteJumpTable[0x10] = BPL_Relative_Execute;
            opcodeExecuteJumpTable[0x30] = BMI_Relative_Execute;
            opcodeExecuteJumpTable[0x90] = BCC_Relative_Execute;
            opcodeExecuteJumpTable[0xB0] = BCS_Relative_Execute;
            opcodeExecuteJumpTable[0x50] = BVC_Relative_Execute;
            opcodeExecuteJumpTable[0x70] = BVS_Relative_Execute;

            // Jump and Call Operations
            opcodeExecuteJumpTable[0x4C] = JMP_Absolute_Execute;
            opcodeExecuteJumpTable[0x6C] = JMP_Indirect_Execute;
            opcodeExecuteJumpTable[0x20] = JSR_Absolute_Execute;
            opcodeExecuteJumpTable[0x60] = RTS_Execute;
            opcodeExecuteJumpTable[0x40] = RTI_Execute;

            // Status Flag Operations
            opcodeExecuteJumpTable[0x18] = CLC_Execute;
            opcodeExecuteJumpTable[0x38] = SEC_Execute;
            opcodeExecuteJumpTable[0x58] = CLI_Execute;
            opcodeExecuteJumpTable[0x78] = SEI_Execute;
            opcodeExecuteJumpTable[0xD8] = CLD_Execute;
            opcodeExecuteJumpTable[0xF8] = SED_Execute;
            opcodeExecuteJumpTable[0xB8] = CLV_Execute;

            // System Functions
            opcodeExecuteJumpTable[0x00] = BRK_Execute;
            opcodeExecuteJumpTable[0xEA] = NOP_Execute;

            // Unofficial Instructions
            opcodeExecuteJumpTable[0xEB] = SBC_Immediate_Execute;
            opcodeExecuteJumpTable[0x1A] = NOP_Execute;
            opcodeExecuteJumpTable[0x3A] = NOP_Execute;
            opcodeExecuteJumpTable[0x5A] = NOP_Execute;
            opcodeExecuteJumpTable[0x7A] = NOP_Execute;
            opcodeExecuteJumpTable[0xDA] = NOP_Execute;
            opcodeExecuteJumpTable[0xFA] = NOP_Execute;
            opcodeExecuteJumpTable[0x80] = NOP_Immediate_Execute;
            opcodeExecuteJumpTable[0x82] = NOP_Immediate_Execute;
            opcodeExecuteJumpTable[0x89] = NOP_Immediate_Execute;
            opcodeExecuteJumpTable[0xC2] = NOP_Immediate_Execute;
            opcodeExecuteJumpTable[0xE2] = NOP_Immediate_Execute;
            opcodeExecuteJumpTable[0x04] = NOP_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x44] = NOP_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x64] = NOP_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x14] = NOP_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0x34] = NOP_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0x54] = NOP_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0x74] = NOP_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0xD4] = NOP_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0xF4] = NOP_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0x0C] = NOP_Absolute_Execute;
            opcodeExecuteJumpTable[0x1C] = NOP_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0x3C] = NOP_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0x5C] = NOP_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0x7C] = NOP_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0xDC] = NOP_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0xFC] = NOP_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0x02] = STP;
            opcodeExecuteJumpTable[0x12] = STP;
            opcodeExecuteJumpTable[0x22] = STP;
            opcodeExecuteJumpTable[0x32] = STP;
            opcodeExecuteJumpTable[0x42] = STP;
            opcodeExecuteJumpTable[0x52] = STP;
            opcodeExecuteJumpTable[0x62] = STP;
            opcodeExecuteJumpTable[0x72] = STP;
            opcodeExecuteJumpTable[0x92] = STP;
            opcodeExecuteJumpTable[0xB2] = STP;
            opcodeExecuteJumpTable[0xD2] = STP;
            opcodeExecuteJumpTable[0xF2] = STP;
            opcodeExecuteJumpTable[0x4B] = ALR_Immediate_Execute;
            opcodeExecuteJumpTable[0x0B] = ANC_Immediate_Execute;
            opcodeExecuteJumpTable[0x2B] = ANC_Immediate_Execute;
            opcodeExecuteJumpTable[0x8B] = ANE_Immediate_Execute;
            opcodeExecuteJumpTable[0x6B] = ARR_Immediate_Execute;
            opcodeExecuteJumpTable[0xCB] = AXS_Immediate_Execute;
            opcodeExecuteJumpTable[0xBB] = LAS_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0xA7] = LAX_ZeroPage_Execute;
            opcodeExecuteJumpTable[0xB7] = LAX_ZeroPageY_Execute;
            opcodeExecuteJumpTable[0xAF] = LAX_Absolute_Execute;
            opcodeExecuteJumpTable[0xBF] = LAX_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0xA3] = LAX_IndirectX_Execute;
            opcodeExecuteJumpTable[0xB3] = LAX_IndirectY_Execute;
            opcodeExecuteJumpTable[0xAB] = LXA_Immediate_Execute;
            opcodeExecuteJumpTable[0x87] = SAX_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x97] = SAX_ZeroPageY_Execute;
            opcodeExecuteJumpTable[0x8F] = SAX_Absolute_Execute;
            opcodeExecuteJumpTable[0x83] = SAX_IndirectX_Execute;
            opcodeExecuteJumpTable[0x9F] = SHA_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0x93] = SHA_IndirectY_Execute;
            opcodeExecuteJumpTable[0x9E] = SHX_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0x9C] = SHY_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0x9B] = TAS_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0xC7] = DCP_ZeroPage_Execute;
            opcodeExecuteJumpTable[0xD7] = DCP_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0xCF] = DCP_Absolute_Execute;
            opcodeExecuteJumpTable[0xDF] = DCP_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0xDB] = DCP_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0xC3] = DCP_IndirectX_Execute;
            opcodeExecuteJumpTable[0xD3] = DCP_IndirectY_Execute;
            opcodeExecuteJumpTable[0xE7] = ISC_ZeroPage_Execute;
            opcodeExecuteJumpTable[0xF7] = ISC_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0xEF] = ISC_Absolute_Execute;
            opcodeExecuteJumpTable[0xFF] = ISC_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0xFB] = ISC_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0xE3] = ISC_IndirectX_Execute;
            opcodeExecuteJumpTable[0xF3] = ISC_IndirectY_Execute;
            opcodeExecuteJumpTable[0x27] = RLA_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x37] = RLA_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0x2F] = RLA_Absolute_Execute;
            opcodeExecuteJumpTable[0x3F] = RLA_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0x3B] = RLA_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0x23] = RLA_IndirectX_Execute;
            opcodeExecuteJumpTable[0x33] = RLA_IndirectY_Execute;
            opcodeExecuteJumpTable[0x67] = RRA_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x77] = RRA_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0x6F] = RRA_Absolute_Execute;
            opcodeExecuteJumpTable[0x7F] = RRA_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0x7B] = RRA_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0x63] = RRA_IndirectX_Execute;
            opcodeExecuteJumpTable[0x73] = RRA_IndirectY_Execute;
            opcodeExecuteJumpTable[0x07] = SLO_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x17] = SLO_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0x0F] = SLO_Absolute_Execute;
            opcodeExecuteJumpTable[0x1F] = SLO_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0x1B] = SLO_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0x03] = SLO_IndirectX_Execute;
            opcodeExecuteJumpTable[0x13] = SLO_IndirectY_Execute;
            opcodeExecuteJumpTable[0x47] = SRE_ZeroPage_Execute;
            opcodeExecuteJumpTable[0x57] = SRE_ZeroPageX_Execute;
            opcodeExecuteJumpTable[0x4F] = SRE_Absolute_Execute;
            opcodeExecuteJumpTable[0x5F] = SRE_AbsoluteX_Execute;
            opcodeExecuteJumpTable[0x5B] = SRE_AbsoluteY_Execute;
            opcodeExecuteJumpTable[0x43] = SRE_IndirectX_Execute;
            opcodeExecuteJumpTable[0x53] = SRE_IndirectY_Execute;

            // Fill the rest of the jump table with a default handler for unknown opcodes
            for (int i = 0; i < opcodeExecuteJumpTable.Length; i++)
            {
                if (opcodeExecuteJumpTable[i] == null)
                {
                    opcodeExecuteJumpTable[i] = UnknownOpcodeHandler;
                }
            }
        }

        // Other CPU components and functions
        private Emulator emulator = null!;
        private Memory memory = null!;
        private PPU ppu = null!;
        private Controller controller = null!;

        public void Initialize(Emulator emulator, Memory memory, PPU ppu, Controller controller)
        {
            this.emulator = emulator;
            this.memory = memory;
            this.ppu = ppu;
            this.controller = controller;

            // Initialize registers and flags
            A = 0;
            X = 0;
            Y = 0;
            S = 0xFD;
            N = false;
            V = false;
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

            InitializeSetCyclesJumpTable();
            InitializeExecutionJumpTable();
        }

        private void UnknownOpcodeHandler()
        {
            throw new NotImplementedException($"Opcode {opcode:X2} is not implemented.");
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
            opcodeExecuteJumpTable[opcode]();

            // Check if remaining cycles since executed operations can add new operations
            if (remainingCycles > 0)
                return;

            // Check if an NMI is pending and handle it if necessary
            if (emulator.isNmiPending)
            {
                NMI_SetCycles();
                emulator.isNmiPending = false;
                return;
            }

            // Lookup next operation
            opcode = ReadMemory(PC);
            opcodeSetCyclesJumpTable[opcode]();
        }

        // Helper functions for reading from and writing to memory
        public byte DebugReadMemory(ushort address)
        {
            if (address == 0x4016)
            {
                return controller.DebugReadController1();
            }
            else if (address == 0x4017)
            {
                return controller.DebugReadController2();
            }
            else
            {
                return memory.DebugRead(address);
            }
        }

        private byte ReadMemory(ushort address)
        {
            if (address == 0x4016)
            {
                return controller.ReadController1();
            }
            else if (address == 0x4017)
            {
                return controller.ReadController2();
            }
            else
            {
                return memory.Read(address);
            }
        }

        private void WriteMemory(ushort address, byte value)
        {
            if (address == 0x4014)
            {
                DMA_SetCycles(value);
            }
            else if (address == 0x4016)
            {
                controller.WriteController1(value);
            }
            else if (address == 0x4017)
            {
                controller.WriteController2(value);
            }
            else
            {
                memory.Write(address, value);
            }
        }

        // Status flag conversions
        private byte StatusToByte(bool isBRK)
        {
            byte status = 0;
            if (N) status |= 0x80;
            if (V) status |= 0x40;
            status |= 0x20; // unused bit, always set to 1
            if (isBRK) status |= 0x10;
            if (D) status |= 0x08;
            if (I) status |= 0x04;
            if (Z) status |= 0x02;
            if (C) status |= 0x01;
            return status;
        }

        private void ByteToStatus(byte status)
        {
            N = (status & 0x80) > 0;
            V = (status & 0x40) > 0;
            // skip 0x20, the unused bit
            // skip 0x10, the B flag which is not a physical flag
            D = (status & 0x08) > 0;
            I = (status & 0x04) > 0;
            Z = (status & 0x02) > 0;
            C = (status & 0x01) > 0;
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
            return (byte)(ReadMemory(PC++) + X); // zero-page so ignore carry 
        }

        private ushort ZeroPageY()
        {
            return (byte)(ReadMemory(PC++) + Y); // zero-page so ignore carry 
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

        private ushort Indirect_Bugged() // doesn't go to next page
        {
            ushort indirectAddress = ReadMemory(PC++);
            indirectAddress |= (ushort)(ReadMemory(PC++) << 8);

            ushort address = ReadMemory(indirectAddress);
            ushort wrapAddress = (ushort)((indirectAddress & 0xFF00) | ((indirectAddress + 1) & 0x00FF));
            address |= (ushort)(ReadMemory(wrapAddress) << 8);

            return address;
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
            byte zeroPageAddress = (byte)(ReadMemory(PC++) + X); // zero-page so ignore carry 
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

        private void TAX_SetCycles()
        {
            remainingCycles = 2;
        }

        private void TAX_Execute()
        {
            PC++;
            TAX_Internal();
        }

        private void TAX_Internal()
        {
            X = A;
            UpdateZeroAndNegativeFlags(X);
        }

        private void TXA_SetCycles()
        {
            remainingCycles = 2;
        }

        private void TXA_Execute()
        {
            PC++;
            TXA_Internal();
        }

        private void TXA_Internal()
        {
            A = X;
            UpdateZeroAndNegativeFlags(A);
        }

        private void TAY_SetCycles()
        {
            remainingCycles = 2;
        }

        private void TAY_Execute()
        {
            PC++;
            TAY_Internal();
        }

        private void TAY_Internal()
        {
            Y = A;
            UpdateZeroAndNegativeFlags(Y);
        }

        private void TYA_SetCycles()
        {
            remainingCycles = 2;
        }

        private void TYA_Execute()
        {
            PC++;
            TYA_Internal();
        }

        private void TYA_Internal()
        {
            A = Y;
            UpdateZeroAndNegativeFlags(A);
        }

        private void TSX_SetCycles()
        {
            remainingCycles = 2;
        }

        private void TSX_Execute()
        {
            PC++;
            TSX_Internal();
        }

        private void TSX_Internal()
        {
            X = S;
            UpdateZeroAndNegativeFlags(X);
        }

        private void TXS_SetCycles()
        {
            remainingCycles = 2;
        }

        private void TXS_Execute()
        {
            PC++;
            TXS_Internal();
        }

        private void TXS_Internal()
        {
            S = X;
        }

        private void PLA_SetCycles()
        {
            remainingCycles = 4;
        }

        private void PLA_Execute()
        {
            PC++;
            PLA_Internal();
        }

        private void PLA_Internal()
        {
            A = PopStack();
            UpdateZeroAndNegativeFlags(A);
        }

        private void PHA_SetCycles()
        {
            remainingCycles = 3;
        }

        private void PHA_Execute()
        {
            PC++;
            PHA_Internal();
        }

        private void PHA_Internal()
        {
            PushStack(A);
        }

        private void PLP_SetCycles()
        {
            remainingCycles = 4;
        }

        private void PLP_Execute()
        {
            PC++;
            PLP_Internal();
        }

        private void PLP_Internal()
        {
            ByteToStatus(PopStack());
        }

        private void PHP_SetCycles()
        {
            remainingCycles = 3;
        }

        private void PHP_Execute()
        {
            PC++;
            PHP_Internal();
        }

        private void PHP_Internal()
        {
            PushStack(StatusToByte(true));
        }


        // Load and Store Operations
        private void LDA_Immediate_SetCycles()
        {
            remainingCycles = 2;
        }

        private void LDA_Immediate_Execute()
        {
            PC++;
            LDA_(Immediate());
        }

        private void LDA_ZeroPage_SetCycles()
        {
            remainingCycles = 3;
        }

        private void LDA_ZeroPage_Execute()
        {
            PC++;
            LDA_Internal(ZeroPage());
        }

        private void LDA_ZeroPageX_SetCycles()
        {
            remainingCycles = 4;
        }

        private void LDA_ZeroPageX_Execute()
        {
            PC++;
            LDA_Internal(ZeroPageX());
        }

        private void LDA_Absolute_SetCycles()
        {
            remainingCycles = 4;
        }

        private void LDA_Absolute_Execute()
        {
            PC++;
            LDA_Internal(Absolute());
        }

        private void LDA_AbsoluteX_SetCycles()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
        }

        private void LDA_AbsoluteX_Execute()
        {
            PC++;
            LDA_Internal(AbsoluteX());
        }

        private void LDA_AbsoluteY_SetCycles()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
        }

        private void LDA_AbsoluteY_Execute()
        {
            PC++;
            LDA_Internal(AbsoluteY());
        }

        private void LDA_IndirectX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void LDA_IndirectX_Execute()
        {
            PC++;
            LDA_Internal(IndirectX());
        }

        private void LDA_IndirectY_SetCycles()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_IndirectY()) remainingCycles++;
        }

        private void LDA_IndirectY_Execute()
        {
            PC++;
            LDA_Internal(IndirectY());
        }

        private void LDA_(byte value)
        {
            A = value;
            UpdateZeroAndNegativeFlags(A);
        }

        private void LDA_Internal(ushort address)
        {
            A = ReadMemory(address);
            UpdateZeroAndNegativeFlags(A);
        }

        private void LDX_Immediate_SetCycles()
        {
            remainingCycles = 2;
        }

        private void LDX_Immediate_Execute()
        {
            PC++;
            LDX_Internal(Immediate());
        }

        private void LDX_ZeroPage_SetCycles()
        {
            remainingCycles = 3;
        }

        private void LDX_ZeroPage_Execute()
        {
            PC++;
            LDX_Internal(ZeroPage());
        }

        private void LDX_ZeroPageY_SetCycles()
        {
            remainingCycles = 4;
        }

        private void LDX_ZeroPageY_Execute()
        {
            PC++;
            LDX_Internal(ZeroPageY());
        }

        private void LDX_Absolute_SetCycles()
        {
            remainingCycles = 4;
        }

        private void LDX_Absolute_Execute()
        {
            PC++;
            LDX_Internal(Absolute());
        }

        private void LDX_AbsoluteY_SetCycles()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
        }

        private void LDX_AbsoluteY_Execute()
        {
            PC++;
            LDX_Internal(AbsoluteY());
        }

        private void LDX_Internal(byte value)
        {
            X = value;
            UpdateZeroAndNegativeFlags(X);
        }

        private void LDX_Internal(ushort address)
        {
            X = ReadMemory(address);
            UpdateZeroAndNegativeFlags(X);
        }

        private void LDY_Immediate_SetCycles()
        {
            remainingCycles = 2;
        }

        private void LDY_Immediate_Execute()
        {
            PC++;
            LDY_Internal(Immediate());
        }

        private void LDY_ZeroPage_SetCycles()
        {
            remainingCycles = 3;
        }

        private void LDY_ZeroPage_Execute()
        {
            PC++;
            LDY_Internal(ZeroPage());
        }

        private void LDY_ZeroPageX_SetCycles()
        {
            remainingCycles = 4;
        }

        private void LDY_ZeroPageX_Execute()
        {
            PC++;
            LDY_Internal(ZeroPageX());
        }

        private void LDY_Absolute_SetCycles()
        {
            remainingCycles = 4;
        }

        private void LDY_Absolute_Execute()
        {
            PC++;
            LDY_Internal(Absolute());
        }

        private void LDY_AbsoluteX_SetCycles()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
        }

        private void LDY_AbsoluteX_Execute()
        {
            PC++;
            LDY_Internal(AbsoluteX());
        }

        private void LDY_Internal(byte value)
        {
            Y = value;
            UpdateZeroAndNegativeFlags(Y);
        }

        private void LDY_Internal(ushort address)
        {
            Y = ReadMemory(address);
            UpdateZeroAndNegativeFlags(Y);
        }

        private void STA_ZeroPage_SetCycles()
        {
            remainingCycles = 3;
        }

        private void STA_ZeroPage_Execute()
        {
            PC++;
            STA_(ZeroPage());
        }

        private void STA_ZeroPageX_SetCycles()
        {
            remainingCycles = 4;
        }

        private void STA_ZeroPageX_Execute()
        {
            PC++;
            STA_(ZeroPageX());
        }

        private void STA_Absolute_SetCycles()
        {
            remainingCycles = 4;
        }

        private void STA_Absolute_Execute()
        {
            PC++;
            STA_(Absolute());
        }

        private void STA_AbsoluteX_SetCycles()
        {
            remainingCycles = 5;
        }

        private void STA_AbsoluteX_Execute()
        {
            PC++;
            STA_(AbsoluteX());
        }

        private void STA_AbsoluteY_SetCycles()
        {
            remainingCycles = 5;
        }

        private void STA_AbsoluteY_Execute()
        {
            PC++;
            STA_(AbsoluteY());
        }

        private void STA_IndirectX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void STA_IndirectX_Execute()
        {
            PC++;
            STA_(IndirectX());
        }

        private void STA_IndirectY_SetCycles()
        {
            remainingCycles = 6;
        }

        private void STA_IndirectY_Execute()
        {
            PC++;
            STA_(IndirectY());
        }

        private void STA_(ushort address)
        {
            WriteMemory(address, A);
        }

        private void STX_ZeroPage_SetCycles()
        {
            remainingCycles = 3;
        }

        private void STX_ZeroPage_Execute()
        {
            PC++;
            STX_(ZeroPage());
        }

        private void STX_ZeroPageY_SetCycles()
        {
            remainingCycles = 4;
        }

        private void STX_ZeroPageY_Execute()
        {
            PC++;
            STX_(ZeroPageY());
        }

        private void STX_Absolute_SetCycles()
        {
            remainingCycles = 4;
        }

        private void STX_Absolute_Execute()
        {
            PC++;
            STX_(Absolute());
        }

        private void STX_(ushort address)
        {
            WriteMemory(address, X);
        }

        private void STY_ZeroPage_SetCycles()
        {
            remainingCycles = 3;
        }

        private void STY_ZeroPage_Execute()
        {
            PC++;
            STY_(ZeroPage());
        }

        private void STY_ZeroPageX_SetCycles()
        {
            remainingCycles = 4;
        }

        private void STY_ZeroPageX_Execute()
        {
            PC++;
            STY_(ZeroPageX());
        }

        private void STY_Absolute_SetCycles()
        {
            remainingCycles = 4;
        }

        private void STY_Absolute_Execute()
        {
            PC++;
            STY_(Absolute());
        }

        private void STY_(ushort address)
        {
            WriteMemory(address, Y);
        }

        // Arithmetic and Logical Operations
        private void ADC_Immediate_SetCycles()
        {
            remainingCycles = 2;
        }

        private void ADC_Immediate_Execute()
        {
            PC++;
            ADC_(Immediate());
        }

        private void ADC_ZeroPage_SetCycles()
        {
            remainingCycles = 3;
        }

        private void ADC_ZeroPage_Execute()
        {
            PC++;
            ADC_(ZeroPage());
        }

        private void ADC_ZeroPageX_SetCycles()
        {
            remainingCycles = 4;
        }

        private void ADC_ZeroPageX_Execute()
        {
            PC++;
            ADC_(ZeroPageX());
        }

        private void ADC_Absolute_SetCycles()
        {
            remainingCycles = 4;
        }

        private void ADC_Absolute_Execute()
        {
            PC++;
            ADC_(Absolute());
        }

        private void ADC_AbsoluteX_SetCycles()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
        }

        private void ADC_AbsoluteX_Execute()
        {
            PC++;
            ADC_(AbsoluteX());
        }

        private void ADC_AbsoluteY_SetCycles()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
        }

        private void ADC_AbsoluteY_Execute()
        {
            PC++;
            ADC_(AbsoluteY());
        }

        private void ADC_IndirectX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void ADC_IndirectX_Execute()
        {
            PC++;
            ADC_(IndirectX());
        }

        private void ADC_IndirectY_SetCycles()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_IndirectY()) remainingCycles++;
        }

        private void ADC_IndirectY_Execute()
        {
            PC++;
            ADC_(IndirectY());
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

        private void SBC_Immediate_SetCycles()
        {
            remainingCycles = 2;
        }

        private void SBC_Immediate_Execute()
        {
            PC++;
            SBC_(Immediate());
        }

        private void SBC_ZeroPage_SetCycles()
        {
            remainingCycles = 3;
        }

        private void SBC_ZeroPage_Execute()
        {
            PC++;
            SBC_(ZeroPage());
        }

        private void SBC_ZeroPageX_SetCycles()
        {
            remainingCycles = 4;
        }

        private void SBC_ZeroPageX_Execute()
        {
            PC++;
            SBC_(ZeroPageX());
        }

        private void SBC_Absolute_SetCycles()
        {
            remainingCycles = 4;
        }

        private void SBC_Absolute_Execute()
        {
            PC++;
            SBC_(Absolute());
        }

        private void SBC_AbsoluteX_SetCycles()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
        }

        private void SBC_AbsoluteX_Execute()
        {
            PC++;
            SBC_(AbsoluteX());
        }

        private void SBC_AbsoluteY_SetCycles()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
        }

        private void SBC_AbsoluteY_Execute()
        {
            PC++;
            SBC_(AbsoluteY());
        }

        private void SBC_IndirectX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void SBC_IndirectX_Execute()
        {
            PC++;
            SBC_(IndirectX());
        }

        private void SBC_IndirectY_SetCycles()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_IndirectY()) remainingCycles++;
        }

        private void SBC_IndirectY_Execute()
        {
            PC++;
            SBC_(IndirectY());
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

        private void AND_Immediate_SetCycles()
        {
            remainingCycles = 2;
        }

        private void AND_Immediate_Execute()
        {
            PC++;
            AND_(Immediate());
        }

        private void AND_ZeroPage_SetCycles()
        {
            remainingCycles = 3;
        }

        private void AND_ZeroPage_Execute()
        {
            PC++;
            AND_(ZeroPage());
        }

        private void AND_ZeroPageX_SetCycles()
        {
            remainingCycles = 4;
        }

        private void AND_ZeroPageX_Execute()
        {
            PC++;
            AND_(ZeroPageX());
        }

        private void AND_Absolute_SetCycles()
        {
            remainingCycles = 4;
        }

        private void AND_Absolute_Execute()
        {
            PC++;
            AND_(Absolute());
        }

        private void AND_AbsoluteX_SetCycles()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
        }

        private void AND_AbsoluteX_Execute()
        {
            PC++;
            AND_(AbsoluteX());
        }

        private void AND_AbsoluteY_SetCycles()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
        }

        private void AND_AbsoluteY_Execute()
        {
            PC++;
            AND_(AbsoluteY());
        }

        private void AND_IndirectX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void AND_IndirectX_Execute()
        {
            PC++;
            AND_(IndirectX());
        }

        private void AND_IndirectY_SetCycles()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_IndirectY()) remainingCycles++;
        }

        private void AND_IndirectY_Execute()
        {
            PC++;
            AND_(IndirectY());
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

        private void ORA_Immediate_SetCycles()
        {
            remainingCycles = 2;
        }

        private void ORA_Immediate_Execute()
        {
            PC++;
            ORA_(Immediate());
        }

        private void ORA_ZeroPage_SetCycles()
        {
            remainingCycles = 3;
        }

        private void ORA_ZeroPage_Execute()
        {
            PC++;
            ORA_(ZeroPage());
        }

        private void ORA_ZeroPageX_SetCycles()
        {
            remainingCycles = 4;
        }

        private void ORA_ZeroPageX_Execute()
        {
            PC++;
            ORA_(ZeroPageX());
        }

        private void ORA_Absolute_SetCycles()
        {
            remainingCycles = 4;
        }

        private void ORA_Absolute_Execute()
        {
            PC++;
            ORA_(Absolute());
        }

        private void ORA_AbsoluteX_SetCycles()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
        }

        private void ORA_AbsoluteX_Execute()
        {
            PC++;
            ORA_(AbsoluteX());
        }

        private void ORA_AbsoluteY_SetCycles()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
        }

        private void ORA_AbsoluteY_Execute()
        {
            PC++;
            ORA_(AbsoluteY());
        }

        private void ORA_IndirectX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void ORA_IndirectX_Execute()
        {
            PC++;
            ORA_(IndirectX());
        }

        private void ORA_IndirectY_SetCycles()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_IndirectY()) remainingCycles++;
        }

        private void ORA_IndirectY_Execute()
        {
            PC++;
            ORA_(IndirectY());
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

        private void EOR_Immediate_SetCycles()
        {
            remainingCycles = 2;
        }

        private void EOR_Immediate_Execute()
        {
            PC++;
            EOR_(Immediate());
        }

        private void EOR_ZeroPage_SetCycles()
        {
            remainingCycles = 3;
        }

        private void EOR_ZeroPage_Execute()
        {
            PC++;
            EOR_(ZeroPage());
        }

        private void EOR_ZeroPageX_SetCycles()
        {
            remainingCycles = 4;
        }

        private void EOR_ZeroPageX_Execute()
        {
            PC++;
            EOR_(ZeroPageX());
        }

        private void EOR_Absolute_SetCycles()
        {
            remainingCycles = 4;
        }

        private void EOR_Absolute_Execute()
        {
            PC++;
            EOR_(Absolute());
        }

        private void EOR_AbsoluteX_SetCycles()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
        }

        private void EOR_AbsoluteX_Execute()
        {
            PC++;
            EOR_(AbsoluteX());
        }

        private void EOR_AbsoluteY_SetCycles()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
        }

        private void EOR_AbsoluteY_Execute()
        {
            PC++;
            EOR_(AbsoluteY());
        }

        private void EOR_IndirectX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void EOR_IndirectX_Execute()
        {
            PC++;
            EOR_(IndirectX());
        }

        private void EOR_IndirectY_SetCycles()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_IndirectY()) remainingCycles++;
        }

        private void EOR_IndirectY_Execute()
        {
            PC++;
            EOR_(IndirectY());
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

        private void BIT_ZeroPage_SetCycles()
        {
            remainingCycles = 3;
        }

        private void BIT_ZeroPage_Execute()
        {
            PC++;
            BIT_(ZeroPage());
        }

        private void BIT_Absolute_SetCycles()
        {
            remainingCycles = 4;
        }

        private void BIT_Absolute_Execute()
        {
            PC++;
            BIT_(Absolute());
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
        private void INC_ZeroPage_SetCycles()
        {
            remainingCycles = 5;
        }

        private void INC_ZeroPage_Execute()
        {
            PC++;
            INC_(ZeroPage());
        }

        private void INC_Absolute_SetCycles()
        {
            remainingCycles = 6;
        }

        private void INC_Absolute_Execute()
        {
            PC++;
            INC_(Absolute());
        }

        private void INC_ZeroPageX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void INC_ZeroPageX_Execute()
        {
            PC++;
            INC_(ZeroPageX());
        }

        private void INC_AbsoluteX_SetCycles()
        {
            remainingCycles = 7;
        }

        private void INC_AbsoluteX_Execute()
        {
            PC++;
            INC_(AbsoluteX());
        }

        private void INC_(ushort address)
        {
            byte value = ReadMemory(address);
            value++;
            WriteMemory(address, value);
            UpdateZeroAndNegativeFlags(value);
        }

        private void DEC_ZeroPage_SetCycles()
        {
            remainingCycles = 5;
        }

        private void DEC_ZeroPage_Execute()
        {
            PC++;
            DEC_(ZeroPage());
        }

        private void DEC_ZeroPageX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void DEC_ZeroPageX_Execute()
        {
            PC++;
            DEC_(ZeroPageX());
        }

        private void DEC_Absolute_SetCycles()
        {
            remainingCycles = 6;
        }

        private void DEC_Absolute_Execute()
        {
            PC++;
            DEC_(Absolute());
        }

        private void DEC_AbsoluteX_SetCycles()
        {
            remainingCycles = 7;
        }

        private void DEC_AbsoluteX_Execute()
        {
            PC++;
            DEC_(AbsoluteX());
        }

        private void DEC_(ushort address)
        {
            byte value = ReadMemory(address);
            value--;
            WriteMemory(address, value);
            UpdateZeroAndNegativeFlags(value);
        }

        private void INX_SetCycles()
        {
            remainingCycles = 2;
        }

        private void INX_Execute()
        {
            PC++;
            X++;
            UpdateZeroAndNegativeFlags(X);
        }

        private void INY_SetCycles()
        {
            remainingCycles = 2;
        }

        private void INY_Execute()
        {
            PC++;
            Y++;
            UpdateZeroAndNegativeFlags(Y);
        }

        private void DEX_SetCycles()
        {
            remainingCycles = 2;
        }

        private void DEX_Execute()
        {
            PC++;
            X--;
            UpdateZeroAndNegativeFlags(X);
        }

        private void DEY_SetCycles()
        {
            remainingCycles = 2;
        }

        private void DEY_Execute()
        {
            PC++;
            Y--;
            UpdateZeroAndNegativeFlags(Y);
        }

        // Shift Operations
        private void ASL_SetCycles()
        {
            remainingCycles = 2;
        }

        private void ASL_Execute()
        {
            PC++;
            ASL_Internal();
        }

        private void ASL_ZeroPage_SetCycles()
        {
            remainingCycles = 5;
        }

        private void ASL_ZeroPage_Execute()
        {
            PC++;
            ASL_(ZeroPage());
        }

        private void ASL_ZeroPageX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void ASL_ZeroPageX_Execute()
        {
            PC++;
            ASL_(ZeroPageX());
        }

        private void ASL_Absolute_SetCycles()
        {
            remainingCycles = 6;
        }

        private void ASL_Absolute_Execute()
        {
            PC++;
            ASL_(Absolute());
        }

        private void ASL_AbsoluteX_SetCycles()
        {
            remainingCycles = 7;
        }

        private void ASL_AbsoluteX_Execute()
        {
            PC++;
            ASL_(AbsoluteX());
        }

        private void ASL_(ushort address)
        {
            byte value = ReadMemory(address);
            C = (value & 0x80) != 0;
            value <<= 1;
            WriteMemory(address, value);
            UpdateZeroAndNegativeFlags(value);
        }

        private void ASL_Internal()
        {
            C = (A & 0x80) != 0;
            A <<= 1;
            UpdateZeroAndNegativeFlags(A);
        }

        private void LSR_SetCycles()
        {
            remainingCycles = 2;
        }

        private void LSR_Execute()
        {
            PC++;
            LSR_Internal();
        }

        private void LSR_ZeroPage_SetCycles()
        {
            remainingCycles = 5;
        }

        private void LSR_ZeroPage_Execute()
        {
            PC++;
            LSR_(ZeroPage());
        }

        private void LSR_ZeroPageX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void LSR_ZeroPageX_Execute()
        {
            PC++;
            LSR_(ZeroPageX());
        }

        private void LSR_Absolute_SetCycles()
        {
            remainingCycles = 6;
        }

        private void LSR_Absolute_Execute()
        {
            PC++;
            LSR_(Absolute());
        }

        private void LSR_AbsoluteX_SetCycles()
        {
            remainingCycles = 7;
        }

        private void LSR_AbsoluteX_Execute()
        {
            PC++;
            LSR_(AbsoluteX());
        }

        private void LSR_(ushort address)
        {
            byte value = ReadMemory(address);
            C = (value & 0x01) != 0;
            value >>= 1;
            WriteMemory(address, value);
            UpdateZeroAndNegativeFlags(value);
        }

        private void LSR_Internal()
        {
            C = (A & 0x01) != 0;
            A >>= 1;
            UpdateZeroAndNegativeFlags(A);
        }

        private void ROL_SetCycles()
        {
            remainingCycles = 2;
        }

        private void ROL_Execute()
        {
            PC++;
            ROL_Internal();
        }

        private void ROL_ZeroPage_SetCycles()
        {
            remainingCycles = 5;
        }

        private void ROL_ZeroPage_Execute()
        {
            PC++;
            ROL_(ZeroPage());
        }

        private void ROL_ZeroPageX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void ROL_ZeroPageX_Execute()
        {
            PC++;
            ROL_(ZeroPageX());
        }

        private void ROL_Absolute_SetCycles()
        {
            remainingCycles = 6;
        }

        private void ROL_Absolute_Execute()
        {
            PC++;
            ROL_(Absolute());
        }

        private void ROL_AbsoluteX_SetCycles()
        {
            remainingCycles = 7;
        }

        private void ROL_AbsoluteX_Execute()
        {
            PC++;
            ROL_(AbsoluteX());
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

        private void ROL_Internal()
        {
            bool newC = (A & 0x80) != 0;
            A <<= 1;
            if (C)
                A |= 0x01;
            C = newC;
            UpdateZeroAndNegativeFlags(A);
        }

        private void ROR_SetCycles()
        {
            remainingCycles = 2;
        }

        private void ROR_Execute()
        {
            PC++;
            ROR_Internal();
        }

        private void ROR_ZeroPage_SetCycles()
        {
            remainingCycles = 5;
        }

        private void ROR_ZeroPage_Execute()
        {
            PC++;
            ROR_(ZeroPage());
        }

        private void ROR_ZeroPageX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void ROR_ZeroPageX_Execute()
        {
            PC++;
            ROR_(ZeroPageX());
        }

        private void ROR_Absolute_SetCycles()
        {
            remainingCycles = 6;
        }

        private void ROR_Absolute_Execute()
        {
            PC++;
            ROR_(Absolute());
        }

        private void ROR_AbsoluteX_SetCycles()
        {
            remainingCycles = 7;
        }

        private void ROR_AbsoluteX_Execute()
        {
            PC++;
            ROR_(AbsoluteX());
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

        private void ROR_Internal()
        {
            bool newC = (A & 0x01) != 0;
            A >>= 1;
            if (C)
                A |= 0x80;
            C = newC;
            UpdateZeroAndNegativeFlags(A);
        }

        // Compare Operations
        private void CMP_Immediate_SetCycles()
        {
            remainingCycles = 2;
        }

        private void CMP_Immediate_Execute()
        {
            PC++;
            CMP_(Immediate());
        }

        private void CMP_ZeroPage_SetCycles()
        {
            remainingCycles = 3;
        }

        private void CMP_ZeroPage_Execute()
        {
            PC++;
            CMP_(ZeroPage());
        }

        private void CMP_ZeroPageX_SetCycles()
        {
            remainingCycles = 4;
        }

        private void CMP_ZeroPageX_Execute()
        {
            PC++;
            CMP_(ZeroPageX());
        }

        private void CMP_Absolute_SetCycles()
        {
            remainingCycles = 4;
        }

        private void CMP_Absolute_Execute()
        {
            PC++;
            CMP_(Absolute());
        }

        private void CMP_AbsoluteX_SetCycles()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
        }

        private void CMP_AbsoluteX_Execute()
        {
            PC++;
            CMP_(AbsoluteX());
        }

        private void CMP_AbsoluteY_SetCycles()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
        }

        private void CMP_AbsoluteY_Execute()
        {
            PC++;
            CMP_(AbsoluteY());
        }

        private void CMP_IndirectX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void CMP_IndirectX_Execute()
        {
            PC++;
            CMP_(IndirectX());
        }

        private void CMP_IndirectY_SetCycles()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_IndirectY()) remainingCycles++;
        }

        private void CMP_IndirectY_Execute()
        {
            PC++;
            CMP_(IndirectY());
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

        private void CPX_Immediate_SetCycles()
        {
            remainingCycles = 2;
        }

        private void CPX_Immediate_Execute()
        {
            PC++;
            CPX_(Immediate());
        }

        private void CPX_ZeroPage_SetCycles()
        {
            remainingCycles = 3;
        }

        private void CPX_ZeroPage_Execute()
        {
            PC++;
            CPX_(ZeroPage());
        }

        private void CPX_Absolute_SetCycles()
        {
            remainingCycles = 4;
        }

        private void CPX_Absolute_Execute()
        {
            PC++;
            CPX_(Absolute());
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

        private void CPY_Immediate_SetCycles()
        {
            remainingCycles = 2;
        }

        private void CPY_Immediate_Execute()
        {
            PC++;
            CPY_(Immediate());
        }

        private void CPY_ZeroPage_SetCycles()
        {
            remainingCycles = 3;
        }

        private void CPY_ZeroPage_Execute()
        {
            PC++;
            CPY_(ZeroPage());
        }

        private void CPY_Absolute_SetCycles()
        {
            remainingCycles = 4;
        }

        private void CPY_Absolute_Execute()
        {
            PC++;
            CPY_(Absolute());
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

        private void BNE_Relative_SetCycles()
        {
            remainingCycles = 2; // +1 if branch succeeds, +2 if to a new page
            remainingCycles += BranchCyclesNeeded_Relative(!Z);
        }

        private void BNE_Relative_Execute()
        {
            PC++;
            BNE_(Relative());
        }

        private void BNE_(sbyte offset)
        {
            if (!Z)
                PC += (ushort)(short)offset;
        }

        private void BEQ_Relative_SetCycles()
        {
            remainingCycles = 2; // +1 if branch succeeds, +2 if to a new page
            remainingCycles += BranchCyclesNeeded_Relative(Z);
        }

        private void BEQ_Relative_Execute()
        {
            PC++;
            BEQ_(Relative());
        }

        private void BEQ_(sbyte offset)
        {
            if (Z)
                PC += (ushort)(short)offset;
        }

        private void BPL_Relative_SetCycles()
        {
            remainingCycles = 2; // +1 if branch succeeds, +2 if to a new page
            remainingCycles += BranchCyclesNeeded_Relative(!N);
        }

        private void BPL_Relative_Execute()
        {
            PC++;
            BPL_(Relative());
        }

        private void BPL_(sbyte offset)
        {
            if (!N)
                PC += (ushort)(short)offset;
        }

        private void BMI_Relative_SetCycles()
        {
            remainingCycles = 2; // +1 if branch succeeds, +2 if to a new page
            remainingCycles += BranchCyclesNeeded_Relative(N);
        }

        private void BMI_Relative_Execute()
        {
            PC++;
            BMI_(Relative());
        }

        private void BMI_(sbyte offset)
        {
            if (N)
                PC += (ushort)(short)offset;
        }

        private void BCC_Relative_SetCycles()
        {
            remainingCycles = 2; // +1 if branch succeeds, +2 if to a new page
            remainingCycles += BranchCyclesNeeded_Relative(!C);
        }

        private void BCC_Relative_Execute()
        {
            PC++;
            BCC_(Relative());
        }

        private void BCC_(sbyte offset)
        {
            if (!C)
                PC += (ushort)(short)offset;
        }

        private void BCS_Relative_SetCycles()
        {
            remainingCycles = 2; // +1 if branch succeeds, +2 if to a new page
            remainingCycles += BranchCyclesNeeded_Relative(C);
        }

        private void BCS_Relative_Execute()
        {
            PC++;
            BCS_(Relative());
        }

        private void BCS_(sbyte offset)
        {
            if (C)
                PC += (ushort)(short)offset;
        }

        private void BVC_Relative_SetCycles()
        {
            remainingCycles = 2; // +1 if branch succeeds, +2 if to a new page
            remainingCycles += BranchCyclesNeeded_Relative(!V);
        }

        private void BVC_Relative_Execute()
        {
            PC++;
            BVC_(Relative());
        }

        private void BVC_(sbyte offset)
        {
            if (!V)
                PC += (ushort)(short)offset;
        }

        private void BVS_Relative_SetCycles()
        {
            remainingCycles = 2; // +1 if branch succeeds, +2 if to a new page
            remainingCycles += BranchCyclesNeeded_Relative(V);
        }

        private void BVS_Relative_Execute()
        {
            PC++;
            BVS_(Relative());
        }

        private void BVS_(sbyte offset)
        {
            if (V)
                PC += (ushort)(short)offset;
        }

        private void JMP_Absolute_SetCycles()
        {
            remainingCycles = 3;
        }

        private void JMP_Absolute_Execute()
        {
            PC++;
            JMP_(Absolute());
        }

        private void JMP_(ushort address)
        {
            PC = address;
        }

        private void JMP_Indirect_SetCycles()
        {
            remainingCycles = 5;
        }

        private void JMP_Indirect_Execute()
        {
            PC++;
            JMP_(Indirect_Bugged());
        }

        private void JSR_Absolute_SetCycles()
        {
            remainingCycles = 6;
        }

        private void JSR_Absolute_Execute()
        {
            PC++;
            JSR_(Absolute());
        }

        private void JSR_(ushort address)
        {
            PushStack((byte)((PC - 1) >> 8));
            PushStack((byte)(PC - 1));
            PC = address;
        }


        private void RTS_SetCycles()
        {
            remainingCycles = 6;
        }

        private void RTS_Execute()
        {
            PC++;
            PC = (ushort)(PopStack() | (PopStack() << 8));
            PC++;
        }

        private void RTI_SetCycles()
        {
            remainingCycles = 6;
        }

        private void RTI_Execute()
        {
            PC++;
            ByteToStatus(PopStack());
            PC = (ushort)(PopStack() | (PopStack() << 8));
        }

        private void CLC_SetCycles()
        {
            remainingCycles = 2;
        }

        private void CLC_Execute()
        {
            PC++;
            C = false;
        }

        private void SEC_SetCycles()
        {
            remainingCycles = 2;
        }

        private void SEC_Execute()
        {
            PC++;
            C = true;
        }

        private void CLI_SetCycles()
        {
            remainingCycles = 2;
        }

        private void CLI_Execute()
        {
            PC++;
            I = false;
        }

        private void SEI_SetCycles()
        {
            remainingCycles = 2;
        }

        private void SEI_Execute()
        {
            PC++;
            I = true;
        }

        private void CLD_SetCycles()
        {
            remainingCycles = 2;
        }

        private void CLD_Execute()
        {
            PC++;
            D = false;
        }

        private void SED_SetCycles()
        {
            remainingCycles = 2;
        }

        private void SED_Execute()
        {
            PC++;
            D = true;
        }

        private void CLV_SetCycles()
        {
            remainingCycles = 2;
        }

        private void CLV_Execute()
        {
            PC++;
            V = false;
        }

        private void BRK_SetCycles()
        {
            remainingCycles = 7;
        }

        private void BRK_Execute()
        {
            PC++;
            PC++;
            PushStack((byte)(PC >> 8));
            PushStack((byte)(PC & 0xFF));
            PushStack(StatusToByte(true));
            I = true;
            PC = (ushort)(ReadMemory(0xFFFE) | (ReadMemory(0xFFFF) << 8));
        }

        private void NOP_SetCycles()
        {
            remainingCycles = 2;
        }

        private void NOP_Execute()
        {
            PC++;
        }

        private void NOP_Immediate_SetCycles()
        {
            remainingCycles = 2;
        }

        private void NOP_Immediate_Execute()
        {
            PC++;
            _ = Immediate();
        }

        private void NOP_ZeroPage_SetCycles()
        {
            remainingCycles = 3;
        }

        private void NOP_ZeroPage_Execute()
        {
            PC++;
            _ = ZeroPage();
        }

        private void NOP_ZeroPageX_SetCycles()
        {
            remainingCycles = 4;
        }

        private void NOP_ZeroPageX_Execute()
        {
            PC++;
            _ = ZeroPageX();
        }

        private void NOP_Absolute_SetCycles()
        {
            remainingCycles = 4;
        }

        private void NOP_Absolute_Execute()
        {
            PC++;
            _ = Absolute();
        }

        private void NOP_AbsoluteX_SetCycles()
        {
            remainingCycles = 4;
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
        }

        private void NOP_AbsoluteX_Execute()
        {
            PC++;
            _ = AbsoluteX();
        }

        private static void STP()
        {
            throw new InvalidOperationException("STP Instruction encountered.");
        }

        private void ALR_Immediate_SetCycles()
        {
            remainingCycles = 2;
        }

        private void ALR_Immediate_Execute()
        {
            PC++;
            ALR_(Immediate());
        }

        private void ALR_(byte operand)
        {
            AND_(operand);
            LSR_Internal();
        }

        private void ANC_Immediate_SetCycles()
        {
            remainingCycles = 2;
        }

        private void ANC_Immediate_Execute()
        {
            PC++;
            ANC_(Immediate());
        }

        private void ANC_(byte operand)
        {
            AND_(operand);
            UpdateZeroAndNegativeFlags(A);
            C = (A & 0x80) != 0; // Set the carry flag based on the value of the 7th bit of A
        }

        private void ANE_Immediate_SetCycles()
        {
            remainingCycles = 2;
        }

        private void ANE_Immediate_Execute()
        {
            PC++;
            ANE_(Immediate());
        }

        private void ANE_(byte operand)
        {
            A = (byte)(A & X & operand);
            UpdateZeroAndNegativeFlags(A);
        }

        private void ARR_Immediate_SetCycles()
        {
            remainingCycles = 2;
        }

        private void ARR_Immediate_Execute()
        {
            PC++;
            ARR_(Immediate());
        }

        private void ARR_(byte operand)
        {
            AND_(operand);
            ROR_Internal();
            UpdateZeroAndNegativeFlags(A);
            C = (A & 0x40) != 0; // Set bit 6 of A as the carry flag
            V = ((A & 0x40) ^ ((A & 0x20) << 1)) != 0; // Set bit 6 xor bit 5 of A as the overflow flag
        }

        private void AXS_Immediate_SetCycles()
        {
            remainingCycles = 2;
        }

        private void AXS_Immediate_Execute()
        {
            PC++;
            AXS_(Immediate());
        }

        private void AXS_(byte operand)
        {
            int result = (A & X) - operand;
            X = (byte)(result & 0xFF);
            UpdateZeroAndNegativeFlags(X);
            C = result >= 0; // Set the carry flag based on the result without borrow
        }

        private void LAS_AbsoluteY_SetCycles()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
        }

        private void LAS_AbsoluteY_Execute()
        {
            PC++;
            LAS_(AbsoluteY());
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

        private void LAX_ZeroPage_SetCycles()
        {
            remainingCycles = 3;
        }

        private void LAX_ZeroPage_Execute()
        {
            PC++;
            LAX_(ZeroPage());
        }

        private void LAX_ZeroPageY_SetCycles()
        {
            remainingCycles = 4;
        }

        private void LAX_ZeroPageY_Execute()
        {
            PC++;
            LAX_(ZeroPageY());
        }

        private void LAX_Absolute_SetCycles()
        {
            remainingCycles = 4;
        }

        private void LAX_Absolute_Execute()
        {
            PC++;
            LAX_(Absolute());
        }

        private void LAX_AbsoluteY_SetCycles()
        {
            remainingCycles = 4; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
        }

        private void LAX_AbsoluteY_Execute()
        {
            PC++;
            LAX_(AbsoluteY());
        }

        private void LAX_IndirectX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void LAX_IndirectX_Execute()
        {
            PC++;
            LAX_(IndirectX());
        }

        private void LAX_IndirectY_SetCycles()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_IndirectY()) remainingCycles++;
        }

        private void LAX_IndirectY_Execute()
        {
            PC++;
            LAX_(IndirectY());
        }

        private void LAX_(ushort address)
        {
            LDA_Internal(address);
            TAX_Internal();
        }

        private void LXA_Immediate_SetCycles()
        {
            remainingCycles = 2;
        }

        private void LXA_Immediate_Execute()
        {
            PC++;
            LXA_(Immediate());
        }

        private void LXA_(byte operand)
        {
            byte result = (byte)(A & operand);
            A = result;
            X = result;
            UpdateZeroAndNegativeFlags(result);
        }

        private void SAX_ZeroPage_SetCycles()
        {
            remainingCycles = 3;
        }

        private void SAX_ZeroPage_Execute()
        {
            PC++;
            SAX_(ZeroPage());
        }

        private void SAX_ZeroPageY_SetCycles()
        {
            remainingCycles = 4;
        }

        private void SAX_ZeroPageY_Execute()
        {
            PC++;
            SAX_(ZeroPageY());
        }

        private void SAX_Absolute_SetCycles()
        {
            remainingCycles = 4;
        }

        private void SAX_Absolute_Execute()
        {
            PC++;
            SAX_(Absolute());
        }

        private void SAX_IndirectX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void SAX_IndirectX_Execute()
        {
            PC++;
            SAX_(IndirectX());
        }

        private void SAX_(ushort address)
        {
            byte result = (byte)(A & X);
            WriteMemory(address, result);
        }

        private void SHA_AbsoluteY_SetCycles()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
        }

        private void SHA_AbsoluteY_Execute()
        {
            PC++;
            SHA_(AbsoluteY());
        }

        private void SHA_IndirectY_SetCycles()
        {
            remainingCycles = 6;
        }

        private void SHA_IndirectY_Execute()
        {
            PC++;
            SHA_(IndirectY());
        }

        private void SHA_(ushort address)
        {
            byte result = (byte)(A & X & ((address >> 8) + 1));
            WriteMemory(address, result);
        }

        private void SHX_AbsoluteY_SetCycles()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
        }

        private void SHX_AbsoluteY_Execute()
        {
            PC++;
            SHX_(AbsoluteY());
        }

        private void SHX_(ushort address)
        {
            byte result = (byte)(X & ((address >> 8) + 1));
            WriteMemory(address, result);
        }

        private void SHY_AbsoluteX_SetCycles()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteX()) remainingCycles++;
        }

        private void SHY_AbsoluteX_Execute()
        {
            PC++;
            SHY_(AbsoluteX());
        }

        private void SHY_(ushort address)
        {
            byte result = (byte)(Y & ((address >> 8) + 1));
            WriteMemory(address, result);
        }

        private void TAS_AbsoluteY_SetCycles()
        {
            remainingCycles = 5; // +1 if page boundary is crossed
            if (IsPageBoundaryCrossed_AbsoluteY()) remainingCycles++;
        }

        private void TAS_AbsoluteY_Execute()
        {
            PC++;
            TAS_(AbsoluteY());
        }

        private void TAS_(ushort address)
        {
            byte result = (byte)(A & X);
            S = result;
            result &= (byte)((address >> 8) + 1);
            WriteMemory(address, result);
        }

        private void DCP_ZeroPage_SetCycles()
        {
            remainingCycles = 5;
        }

        private void DCP_ZeroPage_Execute()
        {
            PC++;
            DCP_(ZeroPage());
        }

        private void DCP_ZeroPageX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void DCP_ZeroPageX_Execute()
        {
            PC++;
            DCP_(ZeroPageX());
        }

        private void DCP_Absolute_SetCycles()
        {
            remainingCycles = 6;
        }

        private void DCP_Absolute_Execute()
        {
            PC++;
            DCP_(Absolute());
        }

        private void DCP_AbsoluteX_SetCycles()
        {
            remainingCycles = 7;
        }

        private void DCP_AbsoluteX_Execute()
        {
            PC++;
            DCP_(AbsoluteX());
        }

        private void DCP_AbsoluteY_SetCycles()
        {
            remainingCycles = 7;
        }

        private void DCP_AbsoluteY_Execute()
        {
            PC++;
            DCP_(AbsoluteY());
        }

        private void DCP_IndirectX_SetCycles()
        {
            remainingCycles = 8;
        }

        private void DCP_IndirectX_Execute()
        {
            PC++;
            DCP_(IndirectX());
        }

        private void DCP_IndirectY_SetCycles()
        {
            remainingCycles = 8;
        }

        private void DCP_IndirectY_Execute()
        {
            PC++;
            DCP_(IndirectY());
        }

        private void DCP_(ushort address)
        {
            DEC_(address);
            CMP_(address);
        }

        private void ISC_ZeroPage_SetCycles()
        {
            remainingCycles = 5;
        }

        private void ISC_ZeroPage_Execute()
        {
            PC++;
            ISC_(ZeroPage());
        }

        private void ISC_ZeroPageX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void ISC_ZeroPageX_Execute()
        {
            PC++;
            ISC_(ZeroPageX());
        }

        private void ISC_Absolute_SetCycles()
        {
            remainingCycles = 6;
        }

        private void ISC_Absolute_Execute()
        {
            PC++;
            ISC_(Absolute());
        }

        private void ISC_AbsoluteX_SetCycles()
        {
            remainingCycles = 7;
        }

        private void ISC_AbsoluteX_Execute()
        {
            PC++;
            ISC_(AbsoluteX());
        }

        private void ISC_AbsoluteY_SetCycles()
        {
            remainingCycles = 7;
        }

        private void ISC_AbsoluteY_Execute()
        {
            PC++;
            ISC_(AbsoluteY());
        }

        private void ISC_IndirectX_SetCycles()
        {
            remainingCycles = 8;
        }

        private void ISC_IndirectX_Execute()
        {
            PC++;
            ISC_(IndirectX());
        }

        private void ISC_IndirectY_SetCycles()
        {
            remainingCycles = 8;
        }

        private void ISC_IndirectY_Execute()
        {
            PC++;
            ISC_(IndirectY());
        }

        private void ISC_(ushort address)
        {
            INC_(address);
            SBC_(address);
        }

        private void RLA_ZeroPage_SetCycles()
        {
            remainingCycles = 5;
        }

        private void RLA_ZeroPage_Execute()
        {
            PC++;
            RLA_(ZeroPage());
        }

        private void RLA_ZeroPageX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void RLA_ZeroPageX_Execute()
        {
            PC++;
            RLA_(ZeroPageX());
        }

        private void RLA_Absolute_SetCycles()
        {
            remainingCycles = 6;
        }

        private void RLA_Absolute_Execute()
        {
            PC++;
            RLA_(Absolute());
        }

        private void RLA_AbsoluteX_SetCycles()
        {
            remainingCycles = 7;
        }

        private void RLA_AbsoluteX_Execute()
        {
            PC++;
            RLA_(AbsoluteX());
        }

        private void RLA_AbsoluteY_SetCycles()
        {
            remainingCycles = 7;
        }

        private void RLA_AbsoluteY_Execute()
        {
            PC++;
            RLA_(AbsoluteY());
        }

        private void RLA_IndirectX_SetCycles()
        {
            remainingCycles = 8;
        }

        private void RLA_IndirectX_Execute()
        {
            PC++;
            RLA_(IndirectX());
        }

        private void RLA_IndirectY_SetCycles()
        {
            remainingCycles = 8;
        }

        private void RLA_IndirectY_Execute()
        {
            PC++;
            RLA_(IndirectY());
        }

        private void RLA_(ushort address)
        {
            ROL_(address);
            AND_(address);
        }

        private void RRA_ZeroPage_SetCycles()
        {
            remainingCycles = 5;
        }

        private void RRA_ZeroPage_Execute()
        {
            PC++;
            RRA_(ZeroPage());
        }

        private void RRA_ZeroPageX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void RRA_ZeroPageX_Execute()
        {
            PC++;
            RRA_(ZeroPageX());
        }

        private void RRA_Absolute_SetCycles()
        {
            remainingCycles = 6;
        }

        private void RRA_Absolute_Execute()
        {
            PC++;
            RRA_(Absolute());
        }

        private void RRA_AbsoluteX_SetCycles()
        {
            remainingCycles = 7;
        }

        private void RRA_AbsoluteX_Execute()
        {
            PC++;
            RRA_(AbsoluteX());
        }

        private void RRA_AbsoluteY_SetCycles()
        {
            remainingCycles = 7;
        }

        private void RRA_AbsoluteY_Execute()
        {
            PC++;
            RRA_(AbsoluteY());
        }

        private void RRA_IndirectX_SetCycles()
        {
            remainingCycles = 8;
        }

        private void RRA_IndirectX_Execute()
        {
            PC++;
            RRA_(IndirectX());
        }

        private void RRA_IndirectY_SetCycles()
        {
            remainingCycles = 8;
        }

        private void RRA_IndirectY_Execute()
        {
            PC++;
            RRA_(IndirectY());
        }

        private void RRA_(ushort address)
        {
            ROR_(address);
            ADC_(address);
        }

        private void SLO_ZeroPage_SetCycles()
        {
            remainingCycles = 5;
        }

        private void SLO_ZeroPage_Execute()
        {
            PC++;
            SLO_(ZeroPage());
        }

        private void SLO_ZeroPageX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void SLO_ZeroPageX_Execute()
        {
            PC++;
            SLO_(ZeroPageX());
        }

        private void SLO_Absolute_SetCycles()
        {
            remainingCycles = 6;
        }

        private void SLO_Absolute_Execute()
        {
            PC++;
            SLO_(Absolute());
        }

        private void SLO_AbsoluteX_SetCycles()
        {
            remainingCycles = 7;
        }

        private void SLO_AbsoluteX_Execute()
        {
            PC++;
            SLO_(AbsoluteX());
        }

        private void SLO_AbsoluteY_SetCycles()
        {
            remainingCycles = 7;
        }

        private void SLO_AbsoluteY_Execute()
        {
            PC++;
            SLO_(AbsoluteY());
        }

        private void SLO_IndirectX_SetCycles()
        {
            remainingCycles = 8;
        }

        private void SLO_IndirectX_Execute()
        {
            PC++;
            SLO_(IndirectX());
        }

        private void SLO_IndirectY_SetCycles()
        {
            remainingCycles = 8;
        }

        private void SLO_IndirectY_Execute()
        {
            PC++;
            SLO_(IndirectY());
        }

        private void SLO_(ushort address)
        {
            ASL_(address);
            ORA_(address);
        }

        private void SRE_ZeroPage_SetCycles()
        {
            remainingCycles = 5;
        }

        private void SRE_ZeroPage_Execute()
        {
            PC++;
            SRE_(ZeroPage());
        }

        private void SRE_ZeroPageX_SetCycles()
        {
            remainingCycles = 6;
        }

        private void SRE_ZeroPageX_Execute()
        {
            PC++;
            SRE_(ZeroPageX());
        }

        private void SRE_Absolute_SetCycles()
        {
            remainingCycles = 6;
        }

        private void SRE_Absolute_Execute()
        {
            PC++;
            SRE_(Absolute());
        }

        private void SRE_AbsoluteX_SetCycles()
        {
            remainingCycles = 7;
        }

        private void SRE_AbsoluteX_Execute()
        {
            PC++;
            SRE_(AbsoluteX());
        }

        private void SRE_AbsoluteY_SetCycles()
        {
            remainingCycles = 7;
        }

        private void SRE_AbsoluteY_Execute()
        {
            PC++;
            SRE_(AbsoluteY());
        }

        private void SRE_IndirectX_SetCycles()
        {
            remainingCycles = 8;
        }

        private void SRE_IndirectX_Execute()
        {
            PC++;
            SRE_(IndirectX());
        }

        private void SRE_IndirectY_SetCycles()
        {
            remainingCycles = 8;
        }

        private void SRE_IndirectY_Execute()
        {
            PC++;
            SRE_(IndirectY());
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

        public void NMI_SetCycles()
        {
            remainingCycles = 7;
            opcode = 0x101;
        }

        public void NMI_Execute()
        {
            // Push the high byte of the program counter (PC) to the stack
            PushStack((byte)(PC >> 8));

            // Push the low byte of the program counter (PC) to the stack
            PushStack((byte)(PC & 0xFF));

            // Push the processor status register (P) to the stack
            PushStack(StatusToByte(false));

            // Disable interrupts
            I = true;

            // Set the program counter (PC) to the NMI Vector address
            byte lowByte = memory.Read(0xFFFA);
            byte highByte = memory.Read(0xFFFB);
            PC = (ushort)((highByte << 8) | lowByte);
        }

        // Begin the DMA transfer
        public void DMA_SetCycles(byte page)
        {
            dmaPage = page;
            dmaAddress = 0;
            dmaCycleCounter = 512;
            remainingCycles = 2;
            opcode = 0x100;
        }

        // Handle the DMA transfer
        public void DMA_Execute()
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
