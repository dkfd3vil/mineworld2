using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MineWorldServerManager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadServerConfig();
        }





        private void loadServerConfig()
        {
            Dictionary<String, String> settings = parseConfig("serverconfigs/server.config.txt");
            TXTservername.Text = settings["servername"];
            TXTmaxplayers.Text = settings["maxplayers"];
            TXTmotd.Text = settings["motd"];
            TXTlevelname.Text = settings["levelname"];
            TXTautosave.Text=settings["autosave"];
            TXTlight.Text = settings["lightsteps"];

            CHKpublic.Checked = bool.Parse(settings["public"]);
            CHKproxy.Checked = bool.Parse(settings["proxy"]);
            CHKannounce.Checked = bool.Parse(settings["autoannounce"]);
            CHKlogging.Checked = bool.Parse(settings["logs"]);
        }

        private void BTNsavebasic_Click(object sender, EventArgs e)
        {
            Dictionary<String, Setting> newsettings = new Dictionary<string, Setting>();
            newsettings.Add("servername", new Setting(TXTservername.Text, "Set the servername"));
            newsettings.Add("maxplayers", new Setting(TXTmaxplayers.Text, "Max players for the server"));
            newsettings.Add("public", new Setting(CHKpublic.Checked.ToString(), "Registers this server to the online masterserver\ndon't forget to forward port 5565!"));
            newsettings.Add("proxy", new Setting(CHKproxy.Checked.ToString(), "Enable this if the server is public and behind a proxy"));
            newsettings.Add("autoannounce", new Setting(CHKannounce.Checked.ToString(), "If true, the public server list will be notified immediately when your server goes up - otherwise it will take a couple mintes"));
            newsettings.Add("levelname", new Setting(TXTlevelname.Text, "If set, this level will be loaded on server start, otherwise the server generates a new random map"));
            newsettings.Add("motd", new Setting(TXTmotd.Text.Replace("\n", "%%"), "This is the welcome message on the server. %%=newline, [name] is username"));
            newsettings.Add("autosave", new Setting(TXTautosave.Text, "Number of minutes between autosaves (zero for disable)"));
            newsettings.Add("logs", new Setting(CHKlogging.Checked.ToString(), "Enables server logging"));
            newsettings.Add("lightsteps", new Setting(TXTlight.Text, "The transition time from day to night in multiples of 10 seconds"));
            writeConfig(newsettings, "serverConfigs/server.config.txt");
        }

        private Dictionary<String, String> parseConfig(String configfile)
        {
            String[] raw = File.ReadAllLines(configfile);
            Dictionary<String, String> output = new Dictionary<string, string>();

            foreach (String line in raw)
            {
                String rline = line.Trim();
                if ((rline != "")&&(rline.Substring(0, 1) != "#"))
                {
                    String[] part = rline.Split("=".ToCharArray());
                    if (part.Length > 1)
                    {
                        output.Add(part[0].Trim(), part[1].Trim());
                    }
                    else
                    {
                        output.Add(part[0].Trim(), "");
                    }
                }
            }
            return output;
        }
        private void writeConfig(Dictionary<String, Setting> values, String file)
        {
            String output = "";
            foreach (KeyValuePair<String, Setting> s in values)
            {
                output += "# " + s.Value.comment + "\n";
                output += s.Key + " = " + s.Value.value + "\n\n";
            }
            File.WriteAllText(file, output);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            loadServerConfig();
        }
    }
    public class Setting
    {
        public string value;
        public string comment;
        public Setting(String val, String comm)
        {
            this.value = val;
            this.comment = comm;
        }
    }
}
