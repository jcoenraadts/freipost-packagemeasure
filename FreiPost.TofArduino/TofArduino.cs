using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace FreiPost.TofArduino
{
    /// <summary>
    /// This class represents the three sensors connected to three arduinos, and three USB serial ports
    /// Each sensor (x, y, z) can be identified by it's serial port. Moving the devices to another computer will
    /// almost certainly change the port allocations.
    /// 
    /// This method (with three arduinos) is a convoluted method which was designed to be quick to manufacture and
    /// test, but not useful as a medium term solution
    /// 
    /// Internal functionality:
    /// - When the object is instantiated, the three serial ports are opened. An exception is raised if the port cannot be opened
    /// - Each port is read by a Task<> outside the main thread, and when data is available, a public property is updated
    /// - Also when each port is read, an event is raised containing the data
    /// 
    /// Usage:
    /// var tof = new TofArduino("COM5", "COM6", "COM7");
    /// int x_in_mm = tof.Distance_X;
    /// tof.close();  // optional, but preferred
    /// 
    /// Usage with Events:
    /// private void start()
    /// {
    ///     var tof = new TofArduino("COM5", "COM6", "COM7");
    ///     tof.DataReceived += new TofArduino.DataReceivedEvent(dataReceived);
    /// }
    /// 
    /// private void dataReceived(char axis, int distance)
    /// {
    ///     // axis is either X, Y, or Z. 
    ///     // distance is in mm
    /// }
    /// 
    /// </summary>
    public class TofArduino
    {
        private SerialPort portX, portY, portZ;
        private const int BAUD = 115200;
        private const string OUT_OF_RANGE = "out of range";
        private char[] SEPERATORS = { ' ' };
        private const int RANGE_INDEX = 1;
        private const char AXIS_X = 'X';
        private const char AXIS_Y = 'Y';
        private const char AXIS_Z = 'Z';

        public event DataReceivedEvent DataReceived;    
        public delegate void DataReceivedEvent(char axis, int diatance);

        public int Distance_X { get; private set; }
        public int Distance_Y { get; private set; }
        public int Distance_Z { get; private set; }

        public TofArduino(string xPortName, string yPortName, string zPortName)
        {
            portX = new SerialPort(xPortName, BAUD);
            portX.Open();
            Task.Run(() => measure(portX, AXIS_X));

            portY = new SerialPort(yPortName, BAUD);
            portY.Open();
            Task.Run(() => measure(portY, AXIS_Y));

            portZ = new SerialPort(zPortName, BAUD);
            portZ.Open();
            Task.Run(() => measure(portZ, AXIS_Z));
        }

        private void measure(SerialPort port, char axis)
        {
            // loop until the port is closed, then exit quietly
            while(port.IsOpen)
            {
                try
                {
                    string line = port.ReadLine();
                    if (line.Contains(OUT_OF_RANGE))
                        continue;

                    string[] parts = line.Split(SEPERATORS);
                    int distance = int.Parse(parts[RANGE_INDEX]);

                    // raise the event
                    if (DataReceived != null)
                        DataReceived(axis, distance);

                    //set the public prop
                    setProperty(axis, distance);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Exception in TOF Measure on " + port.PortName + ": " + e.Message);
                    System.Threading.Thread.Sleep(100);
                }
            }
        }

        private void setProperty(char axis, int distance)
        {
            switch(axis)
            {
                case AXIS_X:
                    Distance_X = distance;
                    break;
                case AXIS_Y:
                    Distance_Y = distance;
                    break;
                case AXIS_Z:
                    Distance_Z = distance;
                    break;
            }
        }
    }
}
