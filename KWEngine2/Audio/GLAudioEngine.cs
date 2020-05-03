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
using KWEngine2.Helper;

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

        internal static bool IsInitializing = true;
        private static IntPtr mDeviceID = (IntPtr)0;
        private static ContextHandle mContext;
        private static bool mAudioOn = false;
        private static List<GLAudioSource> mSources = new List<GLAudioSource>();

        private static int mChannels = 32;

        private static void TryInitAudio()
        {
            int tries = 0;
            
            while (mAudioOn == false && tries < 10)
            {
                try
                {
                    mDeviceID = Alc.OpenDevice(null);
                    Console.WriteLine("Initializing audio engine OpenAL (Attempt #" + tries + ")... ");
                    int[] attributes = new int[0];
                    mContext = Alc.CreateContext(mDeviceID, attributes);
                    Alc.MakeContextCurrent(mContext);
                    var version = AL.Get(ALGetString.Version);
                    var vendor = AL.Get(ALGetString.Vendor);
                    
                    if (version == null)
                    {
                        throw new Exception("No Audio devices found.");
                    }

                    Console.Write('\t' + version + " " + vendor);
                    Console.WriteLine(" Init complete.");
                    mAudioOn = true;
                    
                }

                catch (Exception)
                {
                    //Console.WriteLine("\tError initializing audio engine: " + ex.Message);
                    mAudioOn = false;
                }

                tries++;
                Thread.Sleep(500);
            }
            IsInitializing = false;

            if (mAudioOn)
            {
                for (int i = 0; i < mChannels; i++)
                {
                    GLAudioSource s = new GLAudioSource();
                    mSources.Add(s);
                }
            }
            else
            {
                Console.WriteLine("\t\t(Giving up on initializing the audio engine. Sorry.)");
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

        public static void SoundStop(int sourceId)
        {
            if (mSources[sourceId] != null && mSources[sourceId].IsPlaying)
            {
                AL.SourceStop(sourceId);
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

        public static void SoundChangeGain(int sourceId, float gain)
        {
            gain = HelperGL.Clamp(gain, 0, 8);
            if (mSources[sourceId] != null && mSources[sourceId].IsPlaying)
            {
                AL.Source(mSources[sourceId].GetSourceId(), ALSourcef.Gain, gain);
            }
        }

        public static int SoundPlay(string sound, bool looping, float volume = 1.0f)
        {
            if (!mAudioOn)
            {
                Console.WriteLine("Error playing audio: audio device not available.");
                return -1;
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
            int channelNumber = -1;
            for (int i = 0; i < mChannels; i++)
            {
                if (!mSources[i].IsPlaying)
                {
                    source = mSources[i];
                    channelNumber = i;
                    source.SetFileName(sound);
                    break;
                }
            }
            if (source == null)
            {
                Console.WriteLine("Error playing audio file: all " + mChannels + " channels are busy.");
                return -1;
            }

            AL.Source(source.GetSourceId(), ALSourcei.Buffer, soundToPlay.GetBufferPointer());
            if (looping)
            {
                AL.Source(source.GetSourceId(), ALSourceb.Looping, true);
                
            }
            else
            {
                AL.Source(source.GetSourceId(), ALSourceb.Looping, false);
            }
            AL.Source(source.GetSourceId(), ALSourcef.Gain, volume);
            AL.SourcePlay(source.GetSourceId());
            return channelNumber;
        }

        public static void SoundPreload(string sound)
        {
            if (!CachedSounds.ContainsKey(sound))
            {
                CachedSounds.Add(sound, new CachedSound(sound));
            }
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
