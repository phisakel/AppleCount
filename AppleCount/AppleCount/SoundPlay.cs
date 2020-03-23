using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AppleCount
{
    public static class SoundPlay
    {
        public static Task PlayAsync(this Sounds sound)
        {
            
            string file = null;
            switch (sound)
            {
                case Sounds.DropSound:
                    file = "drop_by_naotuki.mp3";
                    break;
                case Sounds.RustleSound:
                    file = "rustle by Eliot Lash.mp3";
                    break;
                case Sounds.WhooshSound:
                    file = "whoosh by michel marchant roman.mp3";
                    break;
                case Sounds.AwaySound:
                    file = "meep meep by benboncan.mp3";
                    break;
            }

            var player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
            player.Load(file);
            return Task.Delay(1000);
        }
    }

    public enum Sounds
    {
        DropSound, RustleSound, WhooshSound, AwaySound
    }

  
}
