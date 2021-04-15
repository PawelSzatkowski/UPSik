using System;

namespace UPSik.DriverClient.TimeWarper
{
    internal class WarpedTimeProvider : ITimeProvider
    {
        private const double TimeMultiplier = 60.0d;
        private readonly DateTime DateTimeBase = new DateTime(2020, 11, 03, 12, 0, 0, 0);

        public DateTime Now
        {
            get
            {
                var timeDiffInMs = (DateTime.Now - DateTimeBase).TotalMilliseconds;
                var fastForwardTimeDiffInMs = timeDiffInMs * TimeMultiplier;
                var fastForwardTime = DateTimeBase.AddMilliseconds(fastForwardTimeDiffInMs);

                return fastForwardTime;
            }
        }
    }
}
