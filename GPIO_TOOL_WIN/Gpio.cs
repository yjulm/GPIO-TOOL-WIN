using OpenLibSys;
using System;

namespace GPIO_TOOL_WIN
{
    internal class Gpio
    {
        private static Ols _ols;
        private byte _addresPort = 0x2e;
        private byte _dataPort = 0x2f;
        private ushort _gpioBaseAddress = 0;
        private ushort _gpioSet_6_Address = 0;
        //public static string gpbase = "a07";

        public bool Initialize()
        {
            _ols = new Ols();
            return _ols.GetStatus() == (uint)Ols.Status.NO_ERROR;
        }

        public void ExitSuperIo()
        {
            if (_ols != null)
            {
                _ols.WriteIoPortByte(0x2e, 0x02);
                _ols.WriteIoPortByte(0x2f, 0x02); //
            }
        }

        public void InitSuperIO()
        {
            //Enter MB PnP Mode
            _ols.WriteIoPortByte(0x2e, 0x87);
            _ols.WriteIoPortByte(0x2e, 0x01);
            _ols.WriteIoPortByte(0x2e, 0x55);
            _ols.WriteIoPortByte(0x2e, 0x55);
        }

        public void InitGpioReg()
        {
            //GPIO Set 5 Multi-Function Pin Selection Register (Index=29h, Default=00h)
            _ols.WriteIoPortByte(0x2e, 0x29);
            _ols.WriteIoPortByte(0x2f, 0x80); //GP63~GP67 Enabled by Index 29h <bit 7> = 1

            Console.WriteLine("init gpio end ...");
        }

        public string GetChipName()
        {
            int val;
            _ols.WriteIoPortByte(0x2e, 0x20); //Chip ID Byte 1 (Index=20h, Default=87h)
            val = _ols.ReadIoPortByte(0x2f) << 8;
            _ols.WriteIoPortByte(0x2e, 0x21); //Chip ID Byte 2 (Index=21h, Default=28h)
            val |= _ols.ReadIoPortByte(0x2f);
            Console.WriteLine("chip type :" + Convert.ToString(val, 16));
            return "IT" + Convert.ToString(val, 16);
        }

        public void GetEcBaseAddress()
        {
            _ols.WriteIoPortByte(0x2e, 0x07); //Logical Device Number (LDN, Index=07h) used to select logical devices
            _ols.WriteIoPortByte(0x2f, 0x07); //GPIO Configuration Registers (LDN=07h)

            int val;
            _ols.WriteIoPortByte(0x2e, 0x62); //Simple I/O Base Address MSB Register (Index=62h, Default=00h)
            val = _ols.ReadIoPortByte(0x2f) << 8;
            _ols.WriteIoPortByte(0x2e, 0x63); //Simple I/O Base Address LSB Register (Index=63h, Default=00h)
            val |= _ols.ReadIoPortByte(0x2f);

            _gpioBaseAddress = (ushort)val;
            _gpioSet_6_Address = (ushort)(val + 0x05);
        }

        public byte ReadGpioMode()
        {
            byte b = 0;
            try
            {
                //Simple I/O Set 1, 2, 3, 4, 5, 6, 7 and 8 Output Enable Registers
                //(Index=C8h, C9h, CAh, CBh, CCh, CDh, CEh and CFh, Default = 01h, 00h, 00h, 40h, 00h, 00h, 00h and 00h)
                _ols.WriteIoPortByte(0x2e, 0xcd);
                b = _ols.ReadIoPortByte(0x2f);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An error occured:\n" + ex.Message);
            }
            return b;
        }

        public void SetGpioMode(byte b)
        {
            try
            {
                _ols.WriteIoPortByte(0x2e, 0xcd);
                _ols.WriteIoPortByte(0x2f, b);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An error occured:\n" + ex.Message);
            }
        }


        public byte ReadGpioVal()
        {
            byte b = 0;
            try
            {
                b = _ols.ReadIoPortByte(_gpioSet_6_Address);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An error occured:\n" + ex.Message);
            }
            return b;
        }


        public void SetGpioOutValue(byte b)
        {
            try
            {
                _ols.WriteIoPortByte(_gpioSet_6_Address, b);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An error occured:\n" + ex.Message);
            }
        }
    }
}