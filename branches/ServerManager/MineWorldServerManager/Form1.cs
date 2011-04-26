using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

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
            loadMapConfig();
            loadAdminsBans();
        }

        public Process p;
        public BackgroundWorker bw;
        private void startServer()
        {
            p = new Process();
            p.StartInfo.FileName = "MWServer.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerAsync();
            LBLstatus.Text = "Server started";
        }

        private void textBox13_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                p.StandardInput.WriteLine(textBox13.Text);
                textBox13.Text = "";
            }
        }

        private void stopServer()
        {
            p.Kill();
            p = null;
            bw = null;
            LBLstatus.Text = "Server stopped";
        }
        public String consoleOutput = "";
        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                System.Threading.Thread.Sleep(10);
                if (p.HasExited)
                {
                    bw.CancelAsync();
                }
                String line = p.StandardOutput.ReadLine();
                consoleOutput += line + "\r\n";
                lastLine = line;
            }
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

        private void loadMapConfig()
        {
            Dictionary<String, String> settings = parseConfig("serverconfigs/map.config.txt");
            CHKadmin.Checked = bool.Parse(settings["includeadminblocks"]);
            CHKwater.Checked = bool.Parse(settings["includewater"]);
            CHKlava.Checked = bool.Parse(settings["includelava"]);
            CHKtrees.Checked = bool.Parse(settings["includetrees"]);
            TXTwaterfactor.Text = settings["waterspawns"];
            TXTlavafactor.Text = settings["lavaspawns"];
            TXTtreecount.Text = settings["treecount"];
            TXTorefactor.Text = settings["orefactor"];
            //todo: implement mapsize in mineworld
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

        private void button3_Click(object sender, EventArgs e)
        {
            loadMapConfig();
        }

        private void quitManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BTNstart_Click(object sender, EventArgs e)
        {
            startServer();
        }

        private void BTNsavemapconfig_Click(object sender, EventArgs e)
        {
            Dictionary<String, Setting> newsettings = new Dictionary<string, Setting>();
            newsettings.Add("includeadminblocks", new Setting(CHKadmin.Checked.ToString(), "Place a layer of indestructable admin blocks under the map"));
            newsettings.Add("includewater", new Setting(CHKwater.Checked.ToString(), "Enable random water placement in the map"));
            newsettings.Add("includelava", new Setting(CHKlava.Checked.ToString(), "Enable random lava placement in the map"));
            newsettings.Add("includetrees", new Setting(CHKtrees.Checked.ToString(), "Enable random tree placement in the map"));
            newsettings.Add("waterspawns", new Setting(TXTwaterfactor.Text, "Amount of water in map (0 for default random values)"));
            newsettings.Add("lavaspawns", new Setting(TXTlavafactor.Text, "Amount of lava in map (0 for default random values)"));
            newsettings.Add("treecount", new Setting(TXTtreecount.Text, "Amount of trees in the map (0 for default)"));
            newsettings.Add("orefactor", new Setting(TXTorefactor.Text, "The amount of ore in the map (0 for random)"));
            //Todo: insert mapsize setting here as soon as mineworld supports it
            writeConfig(newsettings, "serverConfigs/map.config.txt");
        }

        private void loadAdminsBans()
        {
            TXTadmins.Text = File.ReadAllText("ServerConfigs/admins.txt");
            TXTbans.Text = File.ReadAllText("ServerConfigs/banlist.txt");
        }

        private void BTNreloadadmin_Click(object sender, EventArgs e)
        {
            loadAdminsBans();
        }

        private void BTNsaveadmin_Click(object sender, EventArgs e)
        {
            File.Delete("ServerConfigs/admins.txt");
            File.Delete("ServerConfigs/banlist.txt");
            File.WriteAllText("ServerConfigs/admins.txt", TXTadmins.Text);
            File.WriteAllText("ServerConfigs/banlist.txt", TXTbans.Text);
        }
        public String lastLine = "";
        private void ReadStream_Tick(object sender, EventArgs e)
        {
            TXTconsole.Text = consoleOutput;
            TXTconsole.ScrollBars = ScrollBars.Vertical;
            TXTconsole.SelectionStart = TXTconsole.Text.Length;
            TXTconsole.ScrollToCaret();
            TXTconsole.Refresh();
            if (lastLine.Contains("DISCONNECT: "))
            {
                LSTplayers.Items.RemoveByKey(lastLine.Substring(12).Trim());
            }
            else
            {
                if (lastLine.Contains("CONNECT: "))
                {
                    String[] part = lastLine.Substring(9).Split("(".ToCharArray());
                    String username = part[0].Trim();
                    String IP = part[1].Replace(")","").Trim();
                    ListViewItem lvi = new ListViewItem(username);
                    lvi.SubItems.Add(IP);
                    lvi.SubItems.Add("None"); //team
                    
                    LSTplayers.Items.Add(lvi);
                }
                if (lastLine.Contains("SELECT_TEAM: "))
                {
                    String[] part = lastLine.Substring("SELECT_TEAM: ".Length).Split(",".ToCharArray());
                    String username = part[0].Trim();
                    String team = part[1].Trim();
                    LSTplayers.Items[username].SubItems[2].Text = team;
                }
            }

            lastLine = "";
        }

        private void BTNstop_Click(object sender, EventArgs e)
        {
            stopServer();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopServer();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startServer();
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
