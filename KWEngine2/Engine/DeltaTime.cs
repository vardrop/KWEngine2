using System;
using System.Diagnostics;

namespace KWEngine2.Engine
{
    public static class DeltaTime
    {
        private static float smoothedDeltaRealTime_ms = 16.666666f; // initial value, Optionally you can save the new computed value (will change with each hardware) in Preferences to optimize the first drawing frames 
        private static float movAverageDeltaTime_ms = 16.6666666f; // mov Average start with default value
        private static float lastRealTimeMeasurement_ms = 0; // temporal storage for last time measurement
        private const float movAveragePeriod = 5f; // #frames involved in average calc (suggested values 5-100)
        private const float smoothFactor = 0.1f; // adjusting ratio (suggested values 0.01-0.5)
        private const float TargetFrameTime = 1f / 60f * 1000f;

        private static float deltaTimeFactor = 1;
        
        public static float GetDeltaTimeFactor()
        {
            return deltaTimeFactor;
        }

        internal static void UpdateDeltaTime()
        {
            float currTimePick_ms = Stopwatch.GetTimestamp() / (float)TimeSpan.TicksPerMillisecond;
            float realTimeElapsed_ms;
            if (lastRealTimeMeasurement_ms > 0)
            {
                realTimeElapsed_ms = (currTimePick_ms - lastRealTimeMeasurement_ms);
            }
            else
            {
                realTimeElapsed_ms = smoothedDeltaRealTime_ms; // just the first time
            }
            movAverageDeltaTime_ms = (realTimeElapsed_ms + movAverageDeltaTime_ms * (movAveragePeriod - 1)) / movAveragePeriod;
            // Calc a better aproximation for smooth stepTime
            smoothedDeltaRealTime_ms = smoothedDeltaRealTime_ms + (movAverageDeltaTime_ms - smoothedDeltaRealTime_ms) * smoothFactor;

            deltaTimeFactor = smoothedDeltaRealTime_ms / TargetFrameTime;

            lastRealTimeMeasurement_ms = currTimePick_ms;
        }
    }
}
