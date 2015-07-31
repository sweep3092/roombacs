using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;

namespace Roombacs
{
    struct OIMode
    {
        public const int OFF = 0;
        public const int PASSIVE = 1;
        public const int SAFE = 2;
        public const int FULL = 3;
    }

    public class RoombaControl : IDisposable
    {
        private SerialPort roombaPort;

        public RoombaControl(String port) : this(port, 115200)
        {
        }

        public RoombaControl(String port, int baudrate)
        {
            roombaPort = new SerialPort(
                port,
                baudrate,
                Parity.None,
                8,
                StopBits.One
            );

            roombaPort.Open();
        }

        public void Dispose()
        {
            Stop();
            Power();

            roombaPort.Close();
            roombaPort.Dispose();
        }

        public void Power()
        {
            write(133);
        }

        /* You must always send the Start command before sending any other commands to the OI. */
        public void Start()
        {
            write(128);
        }

        public void SetOIMode(int oimode)
        {
            switch (oimode)
            {
                case Roombacs.OIMode.OFF:
                    Power();
                    break;
                case Roombacs.OIMode.PASSIVE:
                    Start();
                    break;
                case Roombacs.OIMode.FULL:
                    write(132);
                    break;
                case Roombacs.OIMode.SAFE:
                    write(131);
                    break;
                default:
                    throw new ArgumentException("invalid oimode");                    
            }
        }

        public void Clean()
        {
            write(135);
        }
        public void Max()
        {
            write(136);
        }
        public void Spot()
        {
            write(134);
        }
        public void SeekDock()
        {
            write(143);
        }

        /* Actuator Commands */

        // 180度回転
        public void TurnInPlace()
        {
            byte[] bytes = { 0, 226, 0, 1 };
            Drive(bytes);
            // TODO: 現場で時間測る
            System.Threading.Thread.Sleep(1600);
            Stop();
        }

        public void GoAhead(int speed)
        {
            byte[] velocity_byte = serialize_velocity(speed);
            byte[] bytes = { velocity_byte[0], velocity_byte[1], 100, 0 };


            Drive(bytes);
        }

        public void GoBack(int speed)
        {
            byte[] velocity_byte = serialize_velocity(speed * -1);
            byte[] bytes = { velocity_byte[0], velocity_byte[1], 100, 0 };

            Drive(bytes);
        }

        public void TurnRight(int speed)
        {
            byte[] velocity_byte = serialize_velocity(speed);
            byte[] rev_velocity_byte = serialize_velocity(speed * -1);
            byte[] bytes = { rev_velocity_byte[0], rev_velocity_byte[1], velocity_byte[0], velocity_byte[1] };

            DriveDirect(bytes);
        }

        public void TurnLeft(int speed)
        {
            byte[] velocity_byte = serialize_velocity(speed);
            byte[] rev_velocity_byte = serialize_velocity(speed * -1);
            byte[] bytes = { velocity_byte[0], velocity_byte[1], rev_velocity_byte[0], rev_velocity_byte[1] };

            DriveDirect(bytes);
        }

        public void Stop()
        {
            byte[] bytes = { 0, 0, 128, 0 };
            Drive(bytes);
        }


        /* Support */

        public void LED(int color)
        {
            byte[] bytes = { 139, 4, 255, 128 };
            multi_write(bytes);
        }

        public int GetOIMode()
        {
            byte[] bytes = { 142, 35 };
            multi_write(bytes);

            return read_byte();
        }

        public int GetBatteryCharge()
        {
            byte[] bytes = { 142, 25 };
            multi_write(bytes);

            String a = Convert.ToString(read_byte(), 16);
            String b = Convert.ToString(read_byte(), 16);
            return Convert.ToInt32(a + b, 16);
        }
        public int GetBatteryCapacity()
        {
            byte[] bytes = { 142, 26 };
            multi_write(bytes);

            String a = Convert.ToString(read_byte(), 16);
            String b = Convert.ToString(read_byte(), 16);
            return Convert.ToInt32(a + b, 16);
        }


        public float GetBatteryPercentage()
        {
            float current_charge = (float)GetBatteryCharge();
            float capacity = (float)GetBatteryCapacity();

            float percentage = current_charge / capacity * 100;

            return percentage;
        }

        private byte[] serialize_velocity(int velocity)
        {
            if (velocity > 500 || velocity < -500)
            {
                throw new ArgumentOutOfRangeException("velocity must be between -500 and 500.");
            }

            if (velocity < 255 && velocity > 0)
            {
                byte[] simple = { 0, (byte)velocity };
                return simple;
            }

            String hexstr = Convert.ToString(velocity, 16);
            String highbyte = hexstr.Substring(hexstr.Length - 4, 2);
            String lowbyte = hexstr.Substring(hexstr.Length - 2, 2);

            byte[] ret = { (byte)Convert.ToInt32(highbyte, 16), (byte)Convert.ToInt32(lowbyte, 16) };

            return ret;
        }



        /* Write/Read Command */

        /* usage: [137] [Velocity high byte] [Velocity low byte] [Radius high byte] [Radius low byte] */
        private void Drive(byte[] bytes)
        {
            if (bytes.Length != 4)
            {
                throw new ArgumentException("4 arguments are required for drive command.");
            }
            byte[] drive_bytes = { 137, bytes[0], bytes[1], bytes[2], bytes[3] };
            multi_write(drive_bytes);
        }
        /* usage:  [145] [Right velocity high byte] [Right velocity low byte] [Left velocity high byte] [Left velocity low byte] */
        private void DriveDirect(byte[] bytes)
        {
            if (bytes.Length != 4)
            {
                throw new ArgumentException("4 arguments are required for drive_direct command.");
            }
            byte[] drive_bytes = { 145, bytes[0], bytes[1], bytes[2], bytes[3] };
            multi_write(drive_bytes);
        }

        private void multi_write(byte[] bytes)
        {
            roombaPort.Write(bytes, 0, bytes.Length);
        }

        private void write(byte command)
        {
            byte[] bytebuff = { command, 0 };
            roombaPort.Write(bytebuff, 0, 1);
        }

        private int read_byte()
        {
            return roombaPort.ReadByte();
        }
    }
}
