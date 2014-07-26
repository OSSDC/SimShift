﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SimShift.Data;
using SimShift.Services;

namespace SimShift.Dialogs
{
    public partial class dlPlotter : Form
    {
        private ucPlotter plot;
        private Timer updater;

        public dlPlotter()
        {
            InitializeComponent();

            plot = new ucPlotter(4, new float[] { 1, 1, 1000, 2500 });
            plot.Dock = DockStyle.Fill;
            Controls.Add(plot);

            Main.Data.DataReceived += Data_DataReceived;
        }

        void Data_DataReceived(object sender, EventArgs e)
        {
            var miner = Main.Data.Active as Ets2DataMiner;
            var tel = Main.Data.Telemetry;// miner.MyTelemetry;
            var pwr = Main.Drivetrain.CalculatePower(tel.EngineRpm, Main.GetAxisOut(JoyControls.Throttle));
            var data = new double[4]
                           {
                               Main.GetAxisOut(JoyControls.Throttle), 
                               Main.GetAxisOut(JoyControls.Clutch), 
                               //0.5 - tel.gameSteer/2,
                               pwr - 1000,
                               tel.EngineRpm
                               - 2500
                           };

            plot.Add(data.ToList());
        }
    }
}