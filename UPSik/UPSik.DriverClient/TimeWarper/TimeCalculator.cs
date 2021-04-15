using System;

namespace UPSik.DriverClient.TimeWarper
{
    public interface ITimeCalculator
    {
        DateTime GetCurrentRealTime();
        DateTime GetCurrentWarpedTime();
    }

    public class TimeCalculator : ITimeCalculator
    {
        ITimeProvider realTimeProvider = new RealTimeProvider();
        ITimeProvider warpedTimeProvider = new WarpedTimeProvider();

        public DateTime GetCurrentRealTime()
        {
            return realTimeProvider.Now;
        }

        public DateTime GetCurrentWarpedTime()
        {
            return warpedTimeProvider.Now;
        }
    }
}
