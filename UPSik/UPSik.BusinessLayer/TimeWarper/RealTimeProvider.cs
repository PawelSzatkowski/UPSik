using System;

namespace UPSik.BusinessLayer.TimeWarper
{
    class RealTimeProvider : ITimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}
