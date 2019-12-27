/* Audio (for KWEngine2)
 *  
 * Written by: Lutz Karau <lutz.karau@gmail.com>
 * Licence: GNU LGPL 2.1
 */

using System;
using System.Collections.Generic;
using OpenTK.Audio.OpenAL;
using OpenTK;
using System.Threading.Tasks;
using System.Threading;

namespace KWEngine2.Audio
{
    /// <summary>
    /// Standard audio engine for simple sound effects
    /// </summary>
    internal class GLAudioEngine
    {

        private static Thread mAudioInitThread;
        public string CurrentlyPlaying = null;
        public bool CurrentlyLooping { get; private set; } = false;
        internal static Dictionary<string, CachedSound> CachedSounds { get; private set; } = new Dictionary<string, CachedSound>();

        private static IntPtr mDeviceID = (IntPtr)0;
        private static ContextHandle mContext;
        private static bool mAudioOn = false;
        private static List<GLAudioSource> mSources = new List<GLAudioSource>();

        private static void TryInitAudio()
        {
            int tries = 0;
            
            while (mAudioOn == false && tries < 3)
            {
                try
                {
                    mDeviceID = Alc.OpenDevice(null);
                    Console.Write("Initializing audio engine OpenAL: ");
                    Console.WriteLine("Attempt #" + tries + "...");
                    int[] attributes = new int[0];
                    mContext = Alc.CreateContext(mDeviceID, attributes);
                    Alc.MakeContextCurrent(mContext);
                    var version = AL.Get(ALGetString.Version);
                    var vendor = AL.Get(ALGetString.Vendor);
                    
                    if (version == null)
                    {
                        //Console.WriteLine("failed. No audio devices found.");
                        throw new Exception("No Audio devices found.");
                    }

                    Console.WriteLine('\t' + version + " " + vendor);
                    Console.WriteLine("\t\tInit complete.");
                    mAudioOn = true;
                    
                }

                catch (Exception ex)
                {
                    Console.WriteLine("\tError initializing audio engine: " + ex.Message);
                    mAudioOn = false;
                }

                tries++;
                Thread.Sleep(2000);
            }


            if (mAudioOn)
            {
                for (int i = 0; i < 16; i++)
                {
                    GLAudioSource s = new GLAudioSource();
                    mSources.Add(s);
                }
            }
            else
            {
                Console.WriteLine("\t\t(Giving up on initializing audio engine. Sorry.)");
            }
        }

        public static void InitAudioEngine()
        {
            foreach (GLAudioSource s in mSources)
            {
                s.Clear();
            }
            mAudioInitThread = new Thread(new ThreadStart(TryInitAudio));
            mAudioInitThread.Start();

        }

        public static void SoundStop(string sound)
        {
            GLAudioSource source;
            for (int i = 0; i < mSources.Count; i++)
            {
                if (mSources[i] != null && mSources[i].IsPlaying && sound.Contains(mSources[i].GetFileName()))
                {
                    source = mSources[i];
                    source.Stop();
                }
            }
        }

        public static void SoundStopAll()
        {
            GLAudioSource source;
            for (int i = 0; i < mSources.Count; i++)
            {
                if (mSources[i] != null && mSources[i].IsPlaying)
                {
                    source = mSources[i];
                    source.Stop();
                }
            }
        }

        public static void SoundPlay(string sound, bool looping, float volume = 1.0f)
        {
            if (!mAudioOn)
            {
                Console.WriteLine("Error playing audio: audio device not available.");
                return;
            }
            volume = volume >= 0 && volume <= 1.0f ? volume : 1.0f;

            CachedSound soundToPlay = null;
            if (CachedSounds.ContainsKey(sound))
            {
                soundToPlay = CachedSounds[sound];
            }
            else
            {
                soundToPlay = new CachedSound(sound);
                CachedSounds.Add(sound, soundToPlay);
            }

            GLAudioSource source = null;
            for (int i = 0; i < 16; i++)
            {
                if (looping && sound.Contains(mSources[i].GetFileName()) && mSources[i].IsLooping)
                {
                    Console.WriteLine("Sound " + sound + " is already being looped. Playback aborted.");
                    return;
                }
                else if (source == null && !mSources[i].IsPlaying)
                {
                    source = mSources[i];
                    break;
                }
            }
            if (source == null)
            {
                Console.WriteLine("Error playing audio file: all 16 channels are busy.");
                return;
            }

            GLAudioPlayThread playThread = new GLAudioPlayThread(soundToPlay, source, looping, volume);
            Action a = new Action(playThread.Play);
            Task t = new Task(a);
            t.Start();
        }

        /// <summary>
        /// Entlädt alle Audioressourcen aus dem Arbeitsspeicher
        /// </summary>
        public static void Dispose()
        {
            if (mAudioOn)
            {
                foreach (GLAudioSource s in mSources)
                {
                    s.Stop();
                    s.Clear();
                }

                if (mContext != ContextHandle.Zero)
                {
                    Alc.MakeContextCurrent(ContextHandle.Zero);
                    Alc.DestroyContext(mContext);
                }
                mContext = ContextHandle.Zero;

                if (mDeviceID != IntPtr.Zero)
                {
                    Alc.CloseDevice(mDeviceID);
                }
                mDeviceID = IntPtr.Zero;
            }
        }
    }
}
