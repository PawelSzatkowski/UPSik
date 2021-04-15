using System;

namespace UPSik.DriverClient.TimeWarper
{
    class RealTimeProvider : ITimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}
