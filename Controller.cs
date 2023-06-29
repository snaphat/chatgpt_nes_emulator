public class Controller
{
    private ushort buttonState1; // Button state for controller 1
    private ushort buttonState2; // Button state for controller 2
    private ushort controllerState1; // Current controller state for controller 1
    private ushort controllerState2; // Current controller state for controller 2

    // Constants for buttons
    public const ushort BUTTON_A = 0x01;
    public const ushort BUTTON_B = 0x02;
    public const ushort BUTTON_SELECT = 0x04;
    public const ushort BUTTON_START = 0x08;
    public const ushort BUTTON_UP = 0x10;
    public const ushort BUTTON_DOWN = 0x20;
    public const ushort BUTTON_LEFT = 0x40;
    public const ushort BUTTON_RIGHT = 0x80;

    // Constructor
    public Controller()
    {
        // Initialize button states
        buttonState1 = 0;
        buttonState2 = 0;
    }

    // Set button state
    public void SetButtonState(int controller, ushort button, bool pressed)
    {
        if (controller == 1)
        {
            if (pressed)
            {
                buttonState1 |= button;
            }
            else
            {
                buttonState1 &= (ushort)~button;
            }
        }
        else if (controller == 2)
        {
            if (pressed)
            {
                buttonState2 |= button;
            }
            else
            {
                buttonState2 &= (ushort)~button;
            }
        }
    }

    // Create a method to read from controller 1
    public byte DebugReadController1()
    {
        byte value = (byte)(controllerState1 & 1);
        return value;
    }

    // Create a method to read from controller 2
    public byte DebugReadController2()
    {
        byte value = (byte)(controllerState2 & 1);
        return value;
    }

    // Create a method to read from controller 1
    public byte ReadController1()
    {
        byte value = (byte)(controllerState1 & 1);
        controllerState1 >>= 1;
        return value;
    }

    // Create a method to read from controller 2
    public byte ReadController2()
    {
        byte value = (byte)(controllerState2 & 1);
        controllerState2 >>= 1;
        return value;
    }

    // Create a method to write to the controller 1
    public void WriteController1(byte value)
    {
        if ((value & 1) == 1)
        {
            // if bit 0 is set to 1, latch the controller states
            controllerState1 = buttonState1;
        }
    }

    // Create a method to write to the controller 2
    public void WriteController2(byte value)
    {
        if ((value & 1) == 1)
        {
            // if bit 0 is set to 1, latch the controller states
            controllerState2 = buttonState2;
        }
    }
}
