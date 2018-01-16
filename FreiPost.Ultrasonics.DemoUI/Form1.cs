using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FreiPost.Ultrasonics;
using FreiPost.SharpIR;
using Freipost.HAL;

namespace FreiPost.Ultrasonics.DemoUI
{
    public partial class Form1 : Form
    {
        private UltrasonicSensor us;

        private Timer timer;

        private int maxRange = 800;     //mm, sets bar lengths on UI

        private delegate void updateUIDelegate(int mass_g, int x_mm, int y_mm, int z_mm);
        private updateUIDelegate _updateUIDelegate;

                
        public Form1()
        {
            InitializeComponent();                     

            this.FormClosing += Form1_FormClosing;
            _updateUIDelegate = new updateUIDelegate(updateUI);
            
            label1.Text = "X";
            label2.Text = "Y";
            label3.Text = "Z";
            label4.Text = "";
            label5.Text = "";
            label6.Text = "";

            progressBar1.Maximum = maxRange;
            progressBar2.Maximum = maxRange;
            progressBar3.Maximum = maxRange;

            this.Text = "Freipost Hardware Demo";
            try
            {
                us = new UltrasonicSensor();
            }
            catch (Exception e)
            {
                MessageBox.Show("Device could not be found " + e.Message);
                this.Close();
            }

            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            double x_mm, y_mm, z_mm;
            us.Measure(out x_mm, out y_mm, out z_mm);
            updateUI(0, (int)x_mm, (int)y_mm, (int)z_mm);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (us != null)
                us.Close();
        }
        
        private void updateUI(int mass_g, int x_mm, int y_mm, int z_mm)
        {
            if (this.InvokeRequired)
            {   
                //as we should be on the UI thread
                this.Invoke(new Action(() => updateUI(mass_g, x_mm, y_mm, z_mm)));
                return;
            }

            //coerce
            x_mm = Math.Max(Math.Min(x_mm, maxRange), 0);
            y_mm = Math.Max(Math.Min(y_mm, maxRange), 0);
            z_mm = Math.Max(Math.Min(z_mm, maxRange), 0);

            progressBar1.Value = (int)x_mm;
            progressBar2.Value = (int)y_mm;
            progressBar3.Value = (int)z_mm;

            label4.Text = x_mm.ToString("0.0") + " mm";
            label5.Text = y_mm.ToString("0.0") + " mm";
            label6.Text = z_mm.ToString("0.0") + " mm";

            massLabel.Text = ((double)mass_g / 1000).ToString("0.000") + "kg";
        }
    }
}
