using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MineWorldData;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace MineWorld
{
    public class MusicBlock : BaseBlock
    {
        Song currentsong;

        public MusicBlock(Vector2 top, Vector2 forward, Vector2 backward, Vector2 right, Vector2 left, Vector2 bottom, BlockModel modelIn, bool SolidIn, bool TransparentIn, bool AimSolidIn, BlockTypes typeIn,ContentManager content) : 
            base(top,forward,backward,right,left,bottom,modelIn,SolidIn,TransparentIn,AimSolidIn,typeIn)
        {
            currentsong = content.Load<Song>("testsong");
        }

        public override void OnUse()
        {
            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Stop();
            }
            else if (MediaPlayer.State == MediaState.Stopped)
            {
                MediaPlayer.Volume = 0.5f;
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(currentsong);
            }
        }
         
    }
}
