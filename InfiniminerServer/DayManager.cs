using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;


namespace MineWorld
{
    public class DayManager
    {
        public float light = 1.0f;
        float prevlight = 1.0f;
        DateTime lastcalclight = DateTime.Now;
        public void Update()
        {
            TimeSpan timespanlastcalclight = DateTime.Now - lastcalclight;
            // Update time every minute
            if (timespanlastcalclight.Seconds > 15)
            {
                light = light - 0.1f;
                if (light < 0.0f)
                {
                    light = 1.0f;
                }
                lastcalclight = DateTime.Now;
            }
        }

        public bool Timechanged()
        {
            if (light == prevlight)
            {
                return false;
            }
            else
            {
                prevlight = light;
                return true;
            }
        }
    }
}