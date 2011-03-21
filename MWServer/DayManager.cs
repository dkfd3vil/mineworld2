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
        bool goingup = false;
        float light = 1f;
        float prevlight = 1f;
        DateTime lastcalclight = DateTime.Now;

        public float Light
        {
            get
            {
                return light;
            }
            set
            {
                light = value;
            }
        }

        //TODO Load struct Serversettings for this
        public void Update(int dirty)
        {
            TimeSpan timespanlastcalclight = DateTime.Now - lastcalclight;

            if (timespanlastcalclight.Seconds > dirty)
            {
                if (goingup)
                {
                    light = light + 0.1f;
                }
                else
                {
                    light = light - 0.1f;
                }

                if (light <= 0.0f)
                {
                    light = 0.0f;
                    goingup = true;
                }
                if (light >= 1.0f)
                {
                    light = 1.0f;
                    goingup = false;
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