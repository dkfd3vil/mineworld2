using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MineWorld;
using System.IO;

namespace MineWorld
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            fillGrid();
        }

        public void fillGrid()
        {
            for (int p = 0; p < 4096; p++)
            {
                Button ChangeButton = new Button();
                ChangeButton.BackColor = System.Drawing.Color.White;
                ChangeButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
                ChangeButton.FlatAppearance.BorderSize = 0;
                ChangeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                ChangeButton.Margin = new System.Windows.Forms.Padding(0);
                //ChangeButton.Name = "ChangeButton";
                ChangeButton.Size = new System.Drawing.Size(10, 10);
                ChangeButton.UseVisualStyleBackColor = false;
                grid.Controls.Add(ChangeButton);
            }
        }
        public BlockType[] world=new BlockType[262144];
        public Dictionary<String, Image> smalltexture = new Dictionary<string, Image>();

        public Image loadTexture(String texture)
        {
            if (!smalltexture.ContainsKey(texture))
            {
                Image temp = Image.FromFile("textures/" + texture + ".png");

                smalltexture[texture] = temp.GetThumbnailImage(10,10,null,IntPtr.Zero);
            }
            return smalltexture[texture];
        }

        public void setGridData(int offset)
        {
            for (int p = 0; p < 4096; p++)
            {
                String fileName=BlockInformation.GetTopTextureFile(world[p+offset]);
                if (fileName != "")
                {
                    grid.Controls[4095 - p].BackgroundImage = loadTexture(fileName);
                }
                else
                {
                    grid.Controls[4095 - p].BackgroundImage = null;
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DLGopenMap.ShowDialog() == DialogResult.OK)
            {
                String[] file=File.ReadAllLines(DLGopenMap.FileName);
                for (int i = 1; i < file.Length; i++)
                {
                    String[] part = file[i].Split(",".ToCharArray());
                    world[i-1] = (BlockType)Int32.Parse(part[0]);
                }
            }
            
        }

        private void BTNgoto_Click(object sender, EventArgs e)
        {
            int layerID = Int32.Parse(TXTgoto.Text);
            int offset = layerID * 4096;
            setGridData(offset);
        }
    }
}
