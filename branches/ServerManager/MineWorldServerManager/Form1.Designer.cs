namespace MineWorldServerManager
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backupWorldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.quitManagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.serverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.BTNstart = new System.Windows.Forms.ToolStripButton();
            this.BTNstop = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.LBLstatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.button2 = new System.Windows.Forms.Button();
            this.BTNsavebasic = new System.Windows.Forms.Button();
            this.TXTlight = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.CHKlogging = new System.Windows.Forms.CheckBox();
            this.TXTautosave = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.TXTmotd = new System.Windows.Forms.TextBox();
            this.TXTlevelname = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.CHKannounce = new System.Windows.Forms.CheckBox();
            this.CHKproxy = new System.Windows.Forms.CheckBox();
            this.CHKpublic = new System.Windows.Forms.CheckBox();
            this.TXTmaxplayers = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.TXTservername = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.BTNreloadmapconfig = new System.Windows.Forms.Button();
            this.BTNsavemapconfig = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.TXTtreecount = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.TXTlavafactor = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.TXTwaterfactor = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.CHKadmin = new System.Windows.Forms.CheckBox();
            this.CHKwater = new System.Windows.Forms.CheckBox();
            this.CHKtrees = new System.Windows.Forms.CheckBox();
            this.CHKlava = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.TXTorefactor = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.TXTmapsize = new System.Windows.Forms.ComboBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.BTNreloadadmin = new System.Windows.Forms.Button();
            this.BTNsaveadmin = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
            this.TXTbans = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.TXTadmins = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.LSTplayers = new System.Windows.Forms.ListView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.textBox13 = new System.Windows.Forms.TextBox();
            this.TXTconsole = new System.Windows.Forms.TextBox();
            this.ReadStream = new System.Windows.Forms.Timer(this.components);
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.serverToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(567, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveLogToolStripMenuItem,
            this.backupWorldToolStripMenuItem,
            this.toolStripSeparator1,
            this.quitManagerToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveLogToolStripMenuItem
            // 
            this.saveLogToolStripMenuItem.Name = "saveLogToolStripMenuItem";
            this.saveLogToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.saveLogToolStripMenuItem.Text = "Save log";
            // 
            // backupWorldToolStripMenuItem
            // 
            this.backupWorldToolStripMenuItem.Name = "backupWorldToolStripMenuItem";
            this.backupWorldToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.backupWorldToolStripMenuItem.Text = "Backup world";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(147, 6);
            // 
            // quitManagerToolStripMenuItem
            // 
            this.quitManagerToolStripMenuItem.Name = "quitManagerToolStripMenuItem";
            this.quitManagerToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.quitManagerToolStripMenuItem.Text = "Quit Manager";
            this.quitManagerToolStripMenuItem.Click += new System.EventHandler(this.quitManagerToolStripMenuItem_Click);
            // 
            // serverToolStripMenuItem
            // 
            this.serverToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolStripMenuItem,
            this.stopToolStripMenuItem});
            this.serverToolStripMenuItem.Name = "serverToolStripMenuItem";
            this.serverToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
            this.serverToolStripMenuItem.Text = "Server";
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.startToolStripMenuItem.Text = "Start";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.startToolStripMenuItem_Click);
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.stopToolStripMenuItem.Text = "Stop";
            this.stopToolStripMenuItem.Click += new System.EventHandler(this.stopToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BTNstart,
            this.BTNstop});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(567, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // BTNstart
            // 
            this.BTNstart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BTNstart.Image = ((System.Drawing.Image)(resources.GetObject("BTNstart.Image")));
            this.BTNstart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BTNstart.Name = "BTNstart";
            this.BTNstart.Size = new System.Drawing.Size(23, 22);
            this.BTNstart.Text = "toolStripButton1";
            this.BTNstart.Click += new System.EventHandler(this.BTNstart_Click);
            // 
            // BTNstop
            // 
            this.BTNstop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BTNstop.Image = ((System.Drawing.Image)(resources.GetObject("BTNstop.Image")));
            this.BTNstop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BTNstop.Name = "BTNstop";
            this.BTNstop.Size = new System.Drawing.Size(23, 22);
            this.BTNstop.Text = "toolStripButton1";
            this.BTNstop.Click += new System.EventHandler(this.BTNstop_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LBLstatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 368);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(567, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // LBLstatus
            // 
            this.LBLstatus.Name = "LBLstatus";
            this.LBLstatus.Size = new System.Drawing.Size(97, 17);
            this.LBLstatus.Text = "Application started";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 49);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(567, 319);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.button2);
            this.tabPage1.Controls.Add(this.BTNsavebasic);
            this.tabPage1.Controls.Add(this.TXTlight);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.CHKlogging);
            this.tabPage1.Controls.Add(this.TXTautosave);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.TXTmotd);
            this.tabPage1.Controls.Add(this.TXTlevelname);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.CHKannounce);
            this.tabPage1.Controls.Add(this.CHKproxy);
            this.tabPage1.Controls.Add(this.CHKpublic);
            this.tabPage1.Controls.Add(this.TXTmaxplayers);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.TXTservername);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(559, 293);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Basic settings";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(332, 264);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(114, 23);
            this.button2.TabIndex = 17;
            this.button2.Text = "Reload from config";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // BTNsavebasic
            // 
            this.BTNsavebasic.Location = new System.Drawing.Point(452, 264);
            this.BTNsavebasic.Name = "BTNsavebasic";
            this.BTNsavebasic.Size = new System.Drawing.Size(99, 23);
            this.BTNsavebasic.TabIndex = 16;
            this.BTNsavebasic.Text = "Save settings";
            this.BTNsavebasic.UseVisualStyleBackColor = true;
            this.BTNsavebasic.Click += new System.EventHandler(this.BTNsavebasic_Click);
            // 
            // TXTlight
            // 
            this.TXTlight.Location = new System.Drawing.Point(263, 230);
            this.TXTlight.Name = "TXTlight";
            this.TXTlight.Size = new System.Drawing.Size(288, 20);
            this.TXTlight.TabIndex = 15;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(260, 214);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(223, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Light transition time in multiplies of 10 seconds";
            // 
            // CHKlogging
            // 
            this.CHKlogging.AutoSize = true;
            this.CHKlogging.Location = new System.Drawing.Point(263, 170);
            this.CHKlogging.Name = "CHKlogging";
            this.CHKlogging.Size = new System.Drawing.Size(96, 17);
            this.CHKlogging.TabIndex = 13;
            this.CHKlogging.Text = "Enable logging";
            this.CHKlogging.UseVisualStyleBackColor = true;
            // 
            // TXTautosave
            // 
            this.TXTautosave.Location = new System.Drawing.Point(263, 133);
            this.TXTautosave.Name = "TXTautosave";
            this.TXTautosave.Size = new System.Drawing.Size(288, 20);
            this.TXTautosave.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(260, 117);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(130, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Auto save every * minutes";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(260, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Message of the day";
            // 
            // TXTmotd
            // 
            this.TXTmotd.Location = new System.Drawing.Point(263, 29);
            this.TXTmotd.Multiline = true;
            this.TXTmotd.Name = "TXTmotd";
            this.TXTmotd.Size = new System.Drawing.Size(288, 70);
            this.TXTmotd.TabIndex = 9;
            // 
            // TXTlevelname
            // 
            this.TXTlevelname.Location = new System.Drawing.Point(11, 230);
            this.TXTlevelname.Name = "TXTlevelname";
            this.TXTlevelname.Size = new System.Drawing.Size(232, 20);
            this.TXTlevelname.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 214);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Level name";
            // 
            // CHKannounce
            // 
            this.CHKannounce.AutoSize = true;
            this.CHKannounce.Location = new System.Drawing.Point(11, 182);
            this.CHKannounce.Name = "CHKannounce";
            this.CHKannounce.Size = new System.Drawing.Size(100, 17);
            this.CHKannounce.TabIndex = 6;
            this.CHKannounce.Text = "Auto Announce";
            this.CHKannounce.UseVisualStyleBackColor = true;
            // 
            // CHKproxy
            // 
            this.CHKproxy.AutoSize = true;
            this.CHKproxy.Location = new System.Drawing.Point(11, 149);
            this.CHKproxy.Name = "CHKproxy";
            this.CHKproxy.Size = new System.Drawing.Size(120, 17);
            this.CHKproxy.TabIndex = 5;
            this.CHKproxy.Text = "Server behind proxy";
            this.CHKproxy.UseVisualStyleBackColor = true;
            // 
            // CHKpublic
            // 
            this.CHKpublic.AutoSize = true;
            this.CHKpublic.Location = new System.Drawing.Point(11, 117);
            this.CHKpublic.Name = "CHKpublic";
            this.CHKpublic.Size = new System.Drawing.Size(87, 17);
            this.CHKpublic.TabIndex = 4;
            this.CHKpublic.Text = "Public server";
            this.CHKpublic.UseVisualStyleBackColor = true;
            // 
            // TXTmaxplayers
            // 
            this.TXTmaxplayers.Location = new System.Drawing.Point(11, 79);
            this.TXTmaxplayers.Name = "TXTmaxplayers";
            this.TXTmaxplayers.Size = new System.Drawing.Size(232, 20);
            this.TXTmaxplayers.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Maximum player amount";
            // 
            // TXTservername
            // 
            this.TXTservername.Location = new System.Drawing.Point(11, 29);
            this.TXTservername.Name = "TXTservername";
            this.TXTservername.Size = new System.Drawing.Size(232, 20);
            this.TXTservername.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server name";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.BTNreloadmapconfig);
            this.tabPage4.Controls.Add(this.BTNsavemapconfig);
            this.tabPage4.Controls.Add(this.groupBox3);
            this.tabPage4.Controls.Add(this.groupBox2);
            this.tabPage4.Controls.Add(this.groupBox1);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(559, 293);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Map settings";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // BTNreloadmapconfig
            // 
            this.BTNreloadmapconfig.Location = new System.Drawing.Point(332, 264);
            this.BTNreloadmapconfig.Name = "BTNreloadmapconfig";
            this.BTNreloadmapconfig.Size = new System.Drawing.Size(114, 23);
            this.BTNreloadmapconfig.TabIndex = 19;
            this.BTNreloadmapconfig.Text = "Reload from config";
            this.BTNreloadmapconfig.UseVisualStyleBackColor = true;
            this.BTNreloadmapconfig.Click += new System.EventHandler(this.button3_Click);
            // 
            // BTNsavemapconfig
            // 
            this.BTNsavemapconfig.Location = new System.Drawing.Point(452, 264);
            this.BTNsavemapconfig.Name = "BTNsavemapconfig";
            this.BTNsavemapconfig.Size = new System.Drawing.Size(99, 23);
            this.BTNsavemapconfig.TabIndex = 18;
            this.BTNsavemapconfig.Text = "Save settings";
            this.BTNsavemapconfig.UseVisualStyleBackColor = true;
            this.BTNsavemapconfig.Click += new System.EventHandler(this.BTNsavemapconfig_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.TXTtreecount);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.TXTlavafactor);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.TXTwaterfactor);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Location = new System.Drawing.Point(282, 9);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(269, 246);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Advanced map settings";
            // 
            // TXTtreecount
            // 
            this.TXTtreecount.Location = new System.Drawing.Point(6, 126);
            this.TXTtreecount.Name = "TXTtreecount";
            this.TXTtreecount.Size = new System.Drawing.Size(257, 20);
            this.TXTtreecount.TabIndex = 5;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 109);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(59, 13);
            this.label11.TabIndex = 4;
            this.label11.Text = "Tree count";
            // 
            // TXTlavafactor
            // 
            this.TXTlavafactor.Location = new System.Drawing.Point(3, 81);
            this.TXTlavafactor.Name = "TXTlavafactor";
            this.TXTlavafactor.Size = new System.Drawing.Size(257, 20);
            this.TXTlavafactor.TabIndex = 3;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 64);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(61, 13);
            this.label10.TabIndex = 2;
            this.label10.Text = "Lava factor";
            // 
            // TXTwaterfactor
            // 
            this.TXTwaterfactor.Location = new System.Drawing.Point(6, 38);
            this.TXTwaterfactor.Name = "TXTwaterfactor";
            this.TXTwaterfactor.Size = new System.Drawing.Size(257, 20);
            this.TXTwaterfactor.TabIndex = 1;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 21);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(66, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Water factor";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.CHKadmin);
            this.groupBox2.Controls.Add(this.CHKwater);
            this.groupBox2.Controls.Add(this.CHKtrees);
            this.groupBox2.Controls.Add(this.CHKlava);
            this.groupBox2.Location = new System.Drawing.Point(8, 135);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(257, 120);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Map features";
            // 
            // CHKadmin
            // 
            this.CHKadmin.AutoSize = true;
            this.CHKadmin.Location = new System.Drawing.Point(9, 19);
            this.CHKadmin.Name = "CHKadmin";
            this.CHKadmin.Size = new System.Drawing.Size(126, 17);
            this.CHKadmin.TabIndex = 0;
            this.CHKadmin.Text = "Include admin blocks";
            this.CHKadmin.UseVisualStyleBackColor = true;
            // 
            // CHKwater
            // 
            this.CHKwater.AutoSize = true;
            this.CHKwater.Location = new System.Drawing.Point(9, 42);
            this.CHKwater.Name = "CHKwater";
            this.CHKwater.Size = new System.Drawing.Size(124, 17);
            this.CHKwater.TabIndex = 1;
            this.CHKwater.Text = "Include water blocks";
            this.CHKwater.UseVisualStyleBackColor = true;
            // 
            // CHKtrees
            // 
            this.CHKtrees.AutoSize = true;
            this.CHKtrees.Location = new System.Drawing.Point(9, 88);
            this.CHKtrees.Name = "CHKtrees";
            this.CHKtrees.Size = new System.Drawing.Size(87, 17);
            this.CHKtrees.TabIndex = 3;
            this.CHKtrees.Text = "Include trees";
            this.CHKtrees.UseVisualStyleBackColor = true;
            // 
            // CHKlava
            // 
            this.CHKlava.AutoSize = true;
            this.CHKlava.Location = new System.Drawing.Point(9, 65);
            this.CHKlava.Name = "CHKlava";
            this.CHKlava.Size = new System.Drawing.Size(118, 17);
            this.CHKlava.TabIndex = 2;
            this.CHKlava.Text = "Include lava blocks";
            this.CHKlava.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.TXTorefactor);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.TXTmapsize);
            this.groupBox1.Location = new System.Drawing.Point(8, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(257, 123);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Basic map settings";
            // 
            // TXTorefactor
            // 
            this.TXTorefactor.Location = new System.Drawing.Point(6, 83);
            this.TXTorefactor.Name = "TXTorefactor";
            this.TXTorefactor.Size = new System.Drawing.Size(245, 20);
            this.TXTorefactor.TabIndex = 3;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 67);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(54, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "Ore factor";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 24);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "Map size";
            // 
            // TXTmapsize
            // 
            this.TXTmapsize.FormattingEnabled = true;
            this.TXTmapsize.Items.AddRange(new object[] {
            "(1) Small (32x32x32)",
            "(2) Normal (64x64x64)",
            "(3) Large (128x128x128)"});
            this.TXTmapsize.Location = new System.Drawing.Point(6, 40);
            this.TXTmapsize.Name = "TXTmapsize";
            this.TXTmapsize.Size = new System.Drawing.Size(245, 21);
            this.TXTmapsize.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.BTNreloadadmin);
            this.tabPage3.Controls.Add(this.BTNsaveadmin);
            this.tabPage3.Controls.Add(this.label14);
            this.tabPage3.Controls.Add(this.TXTbans);
            this.tabPage3.Controls.Add(this.label13);
            this.tabPage3.Controls.Add(this.TXTadmins);
            this.tabPage3.Controls.Add(this.label12);
            this.tabPage3.Controls.Add(this.LSTplayers);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(559, 293);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Players";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // BTNreloadadmin
            // 
            this.BTNreloadadmin.Location = new System.Drawing.Point(332, 264);
            this.BTNreloadadmin.Name = "BTNreloadadmin";
            this.BTNreloadadmin.Size = new System.Drawing.Size(114, 23);
            this.BTNreloadadmin.TabIndex = 21;
            this.BTNreloadadmin.Text = "Reload from config";
            this.BTNreloadadmin.UseVisualStyleBackColor = true;
            this.BTNreloadadmin.Click += new System.EventHandler(this.BTNreloadadmin_Click);
            // 
            // BTNsaveadmin
            // 
            this.BTNsaveadmin.Location = new System.Drawing.Point(452, 264);
            this.BTNsaveadmin.Name = "BTNsaveadmin";
            this.BTNsaveadmin.Size = new System.Drawing.Size(99, 23);
            this.BTNsaveadmin.TabIndex = 20;
            this.BTNsaveadmin.Text = "Save settings";
            this.BTNsaveadmin.UseVisualStyleBackColor = true;
            this.BTNsaveadmin.Click += new System.EventHandler(this.BTNsaveadmin_Click);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(254, 109);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(80, 13);
            this.label14.TabIndex = 5;
            this.label14.Text = "Banned players";
            // 
            // TXTbans
            // 
            this.TXTbans.Location = new System.Drawing.Point(257, 125);
            this.TXTbans.Multiline = true;
            this.TXTbans.Name = "TXTbans";
            this.TXTbans.Size = new System.Drawing.Size(296, 77);
            this.TXTbans.TabIndex = 4;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(252, 12);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(54, 13);
            this.label13.TabIndex = 3;
            this.label13.Text = "Admin ip\'s";
            // 
            // TXTadmins
            // 
            this.TXTadmins.Location = new System.Drawing.Point(255, 28);
            this.TXTadmins.Multiline = true;
            this.TXTadmins.Name = "TXTadmins";
            this.TXTadmins.Size = new System.Drawing.Size(296, 77);
            this.TXTadmins.TabIndex = 2;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(8, 12);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(87, 13);
            this.label12.TabIndex = 1;
            this.label12.Text = "Current player list";
            // 
            // LSTplayers
            // 
            this.LSTplayers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.LSTplayers.Location = new System.Drawing.Point(8, 28);
            this.LSTplayers.Name = "LSTplayers";
            this.LSTplayers.Size = new System.Drawing.Size(224, 259);
            this.LSTplayers.TabIndex = 0;
            this.LSTplayers.UseCompatibleStateImageBehavior = false;
            this.LSTplayers.View = System.Windows.Forms.View.Details;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.textBox13);
            this.tabPage2.Controls.Add(this.TXTconsole);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(559, 293);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Console";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // textBox13
            // 
            this.textBox13.Location = new System.Drawing.Point(8, 267);
            this.textBox13.Name = "textBox13";
            this.textBox13.Size = new System.Drawing.Size(545, 20);
            this.textBox13.TabIndex = 1;
            this.textBox13.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox13_KeyPress);
            // 
            // TXTconsole
            // 
            this.TXTconsole.BackColor = System.Drawing.Color.Black;
            this.TXTconsole.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TXTconsole.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TXTconsole.ForeColor = System.Drawing.Color.White;
            this.TXTconsole.Location = new System.Drawing.Point(8, 6);
            this.TXTconsole.Multiline = true;
            this.TXTconsole.Name = "TXTconsole";
            this.TXTconsole.Size = new System.Drawing.Size(545, 255);
            this.TXTconsole.TabIndex = 0;
            // 
            // ReadStream
            // 
            this.ReadStream.Enabled = true;
            this.ReadStream.Tick += new System.EventHandler(this.ReadStream_Tick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 100;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "IP";
            this.columnHeader2.Width = 70;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Team";
            this.columnHeader3.Width = 49;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(567, 390);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "MineWorld Server Manager";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripMenuItem saveLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backupWorldToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem quitManagerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem serverToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton BTNstart;
        private System.Windows.Forms.ToolStripButton BTNstop;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TXTservername;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button BTNsavebasic;
        private System.Windows.Forms.TextBox TXTlight;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox CHKlogging;
        private System.Windows.Forms.TextBox TXTautosave;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TXTmotd;
        private System.Windows.Forms.TextBox TXTlevelname;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox CHKannounce;
        private System.Windows.Forms.CheckBox CHKproxy;
        private System.Windows.Forms.CheckBox CHKpublic;
        private System.Windows.Forms.TextBox TXTmaxplayers;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox TXTorefactor;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox TXTmapsize;
        private System.Windows.Forms.CheckBox CHKtrees;
        private System.Windows.Forms.CheckBox CHKlava;
        private System.Windows.Forms.CheckBox CHKwater;
        private System.Windows.Forms.CheckBox CHKadmin;
        private System.Windows.Forms.Button BTNreloadmapconfig;
        private System.Windows.Forms.Button BTNsavemapconfig;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox TXTtreecount;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox TXTlavafactor;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox TXTwaterfactor;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button BTNreloadadmin;
        private System.Windows.Forms.Button BTNsaveadmin;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox TXTbans;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox TXTadmins;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ListView LSTplayers;
        private System.Windows.Forms.TextBox textBox13;
        private System.Windows.Forms.TextBox TXTconsole;
        private System.Windows.Forms.ToolStripStatusLabel LBLstatus;
        private System.Windows.Forms.Timer ReadStream;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
    }
}

