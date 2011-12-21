using System;
using System.IO;

namespace MineWorld
{
    public partial class MineWorldServer
    {
        public void LogWrite(string consoletext)
        {
            string text = "[" + GetTime(true) + "] ";

            if (!File.Exists(Ssettings.LogsDir + "/" + GetDate(false) + ".txt"))
            {
                FileStream fs = File.Create(Ssettings.LogsDir + "/" + GetDate(false) + ".txt");
                fs.Close();
            }
            FileStream file = new FileStream(Ssettings.LogsDir + "/" + GetDate(false) + ".txt", FileMode.Append,
                                             FileAccess.Write);
            StreamWriter sw = new StreamWriter(file);
            sw.WriteLine(text + consoletext);
            sw.Close();
            file.Close();
        }

        public string GetTime(bool usedots)
        {
            string dateFormat = usedots ? "hh:mm:ss" : "hh-mm-ss";
            return DateTime.Now.ToString(dateFormat);
        }

        public string GetDate(bool usedots)
        {
            string dateFormat = usedots ? "yyyy:MM:dd" : "yyyy-MM-dd";
            return DateTime.Now.ToString(dateFormat);
        }
    }
}