using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace FreiPost.SharpIR
{
    public class IrSensors
    {
        private const int BAUD_RATE = 9600;
        private SerialPort P;

        public event DistanceReceivedEvent DistanceReceived;    //The calling class can subscribe to this event
        public delegate void DistanceReceivedEvent(int x, int y, int z);

        public IrSensors(string comport)
        {
            P = new SerialPort(comport, BAUD_RATE);
            P.Open();

            Task.Run(new Action(measure));
        }

        private void measure()
        {
            while (true)
            {
                try
                {
                    string line = P.ReadLine();
                    System.Diagnostics.Debug.WriteLine(line);

                    char[] seperators = { ',' };
                    string[] parts = line.Split(seperators);

                
                    //response is x_mm,y_mm,z_mm\r\n
                    int x = Int32.Parse(parts[0]) * 10;
                    int y = Int32.Parse(parts[1]) * 10;
                    int z = Int32.Parse(parts[2]) * 10;

                    if (DistanceReceived != null)
                        DistanceReceived(x, y, z);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Exception in IR.Measure: " + e.Message);
                }
            }
        }

        public void Close()
        {
            if(P.IsOpen)
                P.Close();
        }
    }
}
