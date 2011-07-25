using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MineWorld
{
    public class FPSCounter
    {
        int frameCount;
        double frameRate;
        DateTime lastFPScheck;
        string message;

        public FPSCounter()
        {
            frameRate = 0;
            frameCount = 100;
            lastFPScheck = DateTime.Now;
            message = "";
        }

        public void Update()
        {
            frameCount = frameCount + 1;
            // Fps for the server
            if (lastFPScheck <= DateTime.Now - TimeSpan.FromMilliseconds(1000))
            {
                lastFPScheck = DateTime.Now;
                frameRate = frameCount;
                if (frameCount <= 20)
                {
                    message = "Heavy load: " + frameCount + " FPS";
                }
                else
                {
                    message = "Normal load: " + frameCount + " FPS";
                }
                frameCount = 0;
            }
            else
            {
                //It isnt the time yet emtpy string
                message = "";
            }
        }

        public void DisplayFps()
        {
            if (message != "")
            {
                //TODO IMPLENT FPS COUNTER SERVER
            }
        }
    }
}
