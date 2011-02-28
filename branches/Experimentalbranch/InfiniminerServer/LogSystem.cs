using System;
using System.IO;

namespace MineWorld
{
    public partial class MineWorldServer
    {
        public void LogWrite(string consoletext)
        {
            DateTime dt = DateTime.Now;
            string minute;
            string second;

            if (dt.Minute < 10)
            {
                minute = 0 + dt.Minute.ToString();
            }
            else
            {
                minute = dt.Minute.ToString();
            }

            if (dt.Second < 10)
            {
                second = 0 + dt.Second.ToString();
            }
            else
            {
                second = dt.Second.ToString();
            }

            string text = "[" + dt.Hour + ":" + minute + ":" + second + "] ";

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
    }
}