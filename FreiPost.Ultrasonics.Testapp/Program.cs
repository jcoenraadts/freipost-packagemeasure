using System;

namespace FreiPost.Ultrasonics.Testapp
{ 
    public class Program
    {
        static void Main(string[] args)
        {
            var us = new UltrasonicSensor();

            double x_mm, y_mm, z_mm;

            while (true)
            {
                us.Measure(out x_mm, out y_mm, out z_mm);
                Console.WriteLine("X: " + x_mm.ToString() +
                    " - Y: " + y_mm.ToString() +
                    " - Z: " + z_mm.ToString());
                
                System.Threading.Thread.Sleep(500);
            }
        }
    }
}
