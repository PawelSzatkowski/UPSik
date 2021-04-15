using System;

namespace UPSik.DriverClient.TimeWarper
{
    public interface ITimeProvider
    {
        public DateTime Now { get; }
    }
}
