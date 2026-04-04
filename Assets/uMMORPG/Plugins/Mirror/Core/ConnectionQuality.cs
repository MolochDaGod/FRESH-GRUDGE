using UnityEngine;

namespace Mirror
{
    public enum ConnectionQuality
    {
        ESTIMATING = 0,
        POOR = 1,
        FAIR = 2,
        GOOD = 3,
        EXCELLENT = 4
    }

    public enum ConnectionQualityMethod
    {
        Simple,
        Pragmatic
    }

    public static class ConnectionQualityHeuristics
    {
        public static ConnectionQuality Simple(double rtt, double jitter)
        {
            if (rtt <= 0)
                return ConnectionQuality.ESTIMATING;

            double rttMs = rtt * 1000.0;
            double jitterMs = jitter * 1000.0;

            if (rttMs <= 80 && jitterMs <= 15)
                return ConnectionQuality.EXCELLENT;

            if (rttMs <= 150 && jitterMs <= 30)
                return ConnectionQuality.GOOD;

            if (rttMs <= 250 && jitterMs <= 60)
                return ConnectionQuality.FAIR;

            return ConnectionQuality.POOR;
        }

        public static ConnectionQuality Pragmatic(double targetBufferTime, double currentBufferTime)
        {
            if (targetBufferTime <= 0 || currentBufferTime <= 0)
                return ConnectionQuality.ESTIMATING;

            double ratio = currentBufferTime / targetBufferTime;

            if (ratio <= 1.15)
                return ConnectionQuality.EXCELLENT;

            if (ratio <= 1.35)
                return ConnectionQuality.GOOD;

            if (ratio <= 1.75)
                return ConnectionQuality.FAIR;

            return ConnectionQuality.POOR;
        }
    }

    public static class ConnectionQualityExtensions
    {
        public static Color ColorCode(this ConnectionQuality quality)
        {
            switch (quality)
            {
                case ConnectionQuality.EXCELLENT:
                    return new Color(0.19f, 0.78f, 0.35f);
                case ConnectionQuality.GOOD:
                    return new Color(0.55f, 0.85f, 0.27f);
                case ConnectionQuality.FAIR:
                    return new Color(0.95f, 0.72f, 0.18f);
                case ConnectionQuality.POOR:
                    return new Color(0.88f, 0.28f, 0.24f);
                default:
                    return Color.gray;
            }
        }
    }
}