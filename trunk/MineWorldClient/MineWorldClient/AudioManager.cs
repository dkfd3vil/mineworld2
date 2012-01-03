using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace MineWorld
{
    public class AudioManager
    {
        SoundEffectInstance soundinstance;
        public float volume;

        public AudioManager()
        {
        }

        public void SetVolume(float vol)
        {
            volume = vol;
        }

        public void PlaySong(Song song,bool repeat)
        {
            if(MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Stop();
            }
            MediaPlayer.Volume = volume;
            MediaPlayer.IsRepeating = repeat;
            MediaPlayer.Play(song);
        }

        public void StopPlaying()
        {
            MediaPlayer.Stop();
        }

        public void PlaySound(SoundEffect sound)
        {
            soundinstance = sound.CreateInstance();
            soundinstance.Volume = volume;
            soundinstance.Play();
        }
    }
}
