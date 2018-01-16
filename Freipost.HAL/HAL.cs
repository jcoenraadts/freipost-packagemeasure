using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace Freipost.HAL
{
    public class HAL
    {
        private const int BAUD_RATE = 9600;
        private SerialPort P;

        public event DataReceivedEvent DataReceived;    //The calling class can subscribe to this event
        public delegate void DataReceivedEvent(int mass_g, int x_mm, int y_mm, int z_mm);

        public HAL(string comport)
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

                    char[] seperators = { ':', ',' };
                    string[] parts = line.Split(seperators);

                    //response is STAT:mass_g,x_mm,y_mm,z_mm\n
                    int mass_g = Int32.Parse(parts[1]);
                    int x = Int32.Parse(parts[2]);
                    int y = Int32.Parse(parts[3]);
                    int z = Int32.Parse(parts[4]);
                    
                    if (DataReceived != null)
                        DataReceived(mass_g, x, y, z);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Exception in HAL.Measure: " + e.Message);
                }
            }
        }

        public void Close()
        {
            if (P.IsOpen)
                P.Close();
        }
    }
}
