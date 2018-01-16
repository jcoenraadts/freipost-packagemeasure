using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreiPost.Ultrasonics
{
    public class UltrasonicSensor
    {
        #region constants

        private const double clockFrequency = 8000000;  //Hz
        private const double speedOfSound = 340;        //m/s

        #endregion

        private HU320.HU320 usbI;

        public UltrasonicSensor()
        {
            usbI = new HU320.HU320();            
        }

        private bool LEDOn
        {
            set
            {
                byte portVal = value ? (byte)0x00 : (byte)0xFF;
                usbI.GPIO_writePort('C', portVal, (byte)(1 << 4));
            }
            get
            {
                byte portVal;
                usbI.GPIO_readPort('C', out portVal);
                return ((portVal & (1 << 4)) != 0);
            }
        }

        /// <summary>
        /// Invokation of this method the hardware device to sample the ultrasonic devices
        /// The returned data is the distance from the sensor to the surface, and has no knowledge of the size of the table
        /// </summary>
        /// <param name="X_mm">Distance from the X-axis sensor to the surface, in mm</param>
        /// <param name="Y_mm">Distance from the Y-axis sensor to the surface, in mm</param>
        /// <param name="Z_mm">Distance from the Z-axis sensor to the surface, in mm</param>
        /// <returns>The X,Y,Z distances from the sensor to the surface</returns>
        public void Measure(out double X_mm, out double Y_mm, out double Z_mm)
        {
            this.LEDOn = true;
            byte[] data = usbI.user_startSampling();
            this.LEDOn = false;

            X_mm = Y_mm = Z_mm = 0;

            if (data.Length == 0)
                return;

            X_mm = calibrateTimeToDistance_mm(BitConverter.ToUInt16(data, 0));
            Y_mm = calibrateTimeToDistance_mm(BitConverter.ToUInt16(data, 2));
            Z_mm = calibrateTimeToDistance_mm(BitConverter.ToUInt16(data, 4));
        }

        /// <summary>
        /// Converts the time measured by the microcontroller in counts at the 
        /// clock frequency into a distance, based on the speed of sound
        /// </summary>
        /// <param name="timerCounts"></param>
        /// <returns></returns>
        private double calibrateTimeToDistance_mm(UInt16 timerCounts)
        {
            return ((double)timerCounts) / 2 * (1 / clockFrequency) * speedOfSound * 1000;
        }

        /// <summary>
        /// Closes the reference to the ultrasonic USB interface
        /// It is good practice to call this when the program is complete
        /// </summary>
        public void Close()
        {
            try
            {
                if (usbI != null)
                    usbI.close();
            }
            catch (Exception e)
            {
                //do nothing here as we don't really care
            }
        }

    }
}
