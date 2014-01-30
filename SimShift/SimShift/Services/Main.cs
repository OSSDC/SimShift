﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimShift.Controllers;
using SimShift.Data;
using SimShift.Models;

namespace SimShift.Services
{
    public class Main
    {
        private static bool requiresSetup = true;

        private static Dictionary<JoyControls, double> AxisFeedback = new Dictionary<JoyControls, double>();
        private static Dictionary<JoyControls, bool> ButtonFeedback = new Dictionary<JoyControls, bool>(); 

        public static List<JoystickInput> RawJoysticksIn = new List<JoystickInput>();
        public static List<JoystickOutput> RawJoysticksOut = new List<JoystickOutput>();

        public static Ets2DataMiner Telemetry;
        public static Ets2Engine Engine;

        // Modules
        public static Antistall Antistall;
        public static Transmission Transmission;

        public static ControlChain Controls;

        public static bool Running { get; private set; }

        public static void Setup()
        {
            if (requiresSetup)
            {
                Engine = new Ets2Engine(3550);
                Antistall = new Antistall();
                Transmission = new Transmission();
                Telemetry = new Ets2DataMiner();
                Controls = new ControlChain();

                var ps3 = JoystickInputDevice.Search("Motion").FirstOrDefault();
                var ps3Controller = new JoystickInput(ps3);

                var vJoy = new JoystickOutput();

                RawJoysticksIn.Add(ps3Controller);
                RawJoysticksOut.Add(vJoy);

                requiresSetup = false;
            }
        }

        public static void Start()
        {
            if (requiresSetup)
                Setup();
            //
            if (!Running)
            {
                Telemetry.DataReceived += tick;
                Running = true;
            }
        }

        public static void Stop()
        {
            if (Running)
            {
                Telemetry.DataReceived -= tick;
                Running = false;
            }
        }

        public static void tick(object sender, EventArgs e)
        {
            Antistall.TickTelemetry(Telemetry);
            Transmission.TickTelemetry(Telemetry);

            Controls.Tick();
        }

        #region Control mapping
        public static bool GetButtonIn(JoyControls c)
        {
            switch(c)
            {
                    // Unimplemented as of now.
                case Services.JoyControls.Gear1:
                case Services.JoyControls.Gear2:
                case Services.JoyControls.Gear3:
                case Services.JoyControls.Gear4:
                case Services.JoyControls.Gear5:
                case Services.JoyControls.Gear6:
                case Services.JoyControls.GearR:
                    return false;

                    // PS3 (via DS3 tool) L1/R1
                case Services.JoyControls.GearDown:
                    return RawJoysticksIn[0].GetButton(4);
                case Services.JoyControls.GearUp:
                    return RawJoysticksIn[0].GetButton(5);

                default:
                    return false;
            }
            // Map user config -> controller
        }

        public static double GetAxisIn(JoyControls c)
        {
            switch(c)
            {
                case Services.JoyControls.Throttle:
                    return 1- RawJoysticksIn[0].GetAxis(2)/Math.Pow(2,15);

                case Services.JoyControls.Brake:
                    return (RawJoysticksIn[0].GetAxis(2) - Math.Pow(2, 15)) / Math.Pow(2, 15);

                case Services.JoyControls.Clutch:
                    return 0.0;
                    
                default:
                    return 0.0;
            }
        }

        public static void SetButtonOut(JoyControls c, bool value)
        {
            switch (c)
            {
                default:
                    break;

                case Services.JoyControls.Gear1:
                    RawJoysticksOut[0].SetButton(1, value);
                    break;

                case Services.JoyControls.Gear2:
                    RawJoysticksOut[0].SetButton(2, value);
                    break;

                case Services.JoyControls.Gear3:
                    RawJoysticksOut[0].SetButton(3, value);
                    break;

                case Services.JoyControls.Gear4:
                    RawJoysticksOut[0].SetButton(4, value);
                    break;

                case Services.JoyControls.Gear5:
                    RawJoysticksOut[0].SetButton(5, value);
                    break;

                case Services.JoyControls.Gear6:
                    RawJoysticksOut[0].SetButton(6, value);
                    break;

                case Services.JoyControls.GearR:
                    RawJoysticksOut[0].SetButton(7, value);
                    break;

                case Services.JoyControls.GearRange1:
                    RawJoysticksOut[0].SetButton(8, value);
                    break;

                case Services.JoyControls.GearRange2:
                    RawJoysticksOut[0].SetButton(9, value);
                    break;

            }

            if (ButtonFeedback.ContainsKey(c)) ButtonFeedback[c] = value;
        else ButtonFeedback.Add(c, value);
        }

        public static void SetAxisOut(JoyControls c, double value)
        {
            switch(c)
            {
                default:
                    break;

                case Services.JoyControls.Throttle:
                    RawJoysticksOut[0].SetAxis(HID_USAGES.HID_USAGE_X, value);
                    break;

                case Services.JoyControls.Brake:
                    RawJoysticksOut[0].SetAxis(HID_USAGES.HID_USAGE_Y, value);
                    break;

                case Services.JoyControls.Clutch:
                    RawJoysticksOut[0].SetAxis(HID_USAGES.HID_USAGE_Z, value);
                    break;
            }

            if (AxisFeedback.ContainsKey(c)) AxisFeedback[c] = value;
            else AxisFeedback.Add(c, value);
        }
        #endregion

        public static double GetAxisOut(JoyControls ctrl)
        {
            if (AxisFeedback.ContainsKey(ctrl)) return AxisFeedback[ctrl];
            return 0;
        }

        public static bool GetButtonOut(JoyControls ctrl)
        {
            if (ButtonFeedback.ContainsKey(ctrl)) return ButtonFeedback[ctrl];
            return false;
        }
    }
}
