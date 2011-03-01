using System;
using System.IO;

namespace MineWorld
{
    public partial class MineWorldServer
    {
        public void LogWrite(string consoletext)
        {
            DateTime dt = DateTime.Now;

            string text = "[" + GetTime(true) + "] ";

            try
            {
                if (!File.Exists(Ssettings.LogsDir + "/" + dt.Year + "-" + dt.Month + "-" + dt.Day + ".txt"))
                {
                    FileStream fs = File.Create(Ssettings.LogsDir + "/" + dt.Year + "-" + dt.Month + "-" + dt.Day + ".txt");
                    fs.Close();
                }
                FileStream file = new FileStream(Ssettings.LogsDir + "/" + dt.Year + "-" + dt.Month + "-" + dt.Day + ".txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(file);
                sw.WriteLine(text + consoletext);
                sw.Close();
                file.Close();
            }
            catch
            {

            }
        }

        public string GetTime(bool usedots)
        {
            DateTime temp = DateTime.Now;
            string time;
            string minute;
            string second;
            string hour;

            if (temp.Minute < 10)
            {
                minute = 0 + temp.Minute.ToString();
            }
            else
            {
                minute = temp.Minute.ToString();
            }

            if (temp.Second < 10)
            {
                second = 0 + temp.Second.ToString();
            }
            else
            {
                second = temp.Second.ToString();
            }

            if (temp.Hour < 10)
            {
                hour = 0 + temp.Hour.ToString();
            }
            else
            {
                hour = temp.Hour.ToString();
            }

            if (usedots == true)
            {
                time = hour + ":" + minute + ":" + second;
            }
            else
            {
                time = hour + "-" + minute + "-" + second;
            }

            return time;
        }
    }
}