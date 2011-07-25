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
        int lightsteps = 0;
        float light = 1f;
        float prevlight = 1f;
        DateTime lastcalclight = DateTime.Now;

        public DayManager(int amount)
        {
            lightsteps = 0;
        }

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

        public void Update()
        {
            TimeSpan timespanlastcalclight = DateTime.Now - lastcalclight;

            if (timespanlastcalclight.Seconds > lightsteps)
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

        public void SetNight()
        {
            light = 0.0f;
        }

        public void SetDay()
        {
            light = 1.0f;
        }
    }
}