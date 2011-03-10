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
                if (!File.Exists(Ssettings.LogsDir + "/" + GetDate() + ".txt"))
                {
                    FileStream fs = File.Create(Ssettings.LogsDir + "/" + GetDate() + ".txt");
                    fs.Close();
                }
                FileStream file = new FileStream(Ssettings.LogsDir + "/" + GetDate() + ".txt", FileMode.Append, FileAccess.Write);
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
            String DateFormat = "";
            if (usedots)
            {
                DateFormat = "hh:mm:ss";
            }
            else
            {
                DateFormat = "hh-mm-ss";
            }
            return DateTime.Now.ToString(DateFormat);
        }
        public string GetDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }
    }
}