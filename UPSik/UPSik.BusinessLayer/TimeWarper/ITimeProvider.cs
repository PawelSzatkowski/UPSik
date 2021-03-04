using System;

namespace UPSik.BusinessLayer.TimeWarper
{
    public interface ITimeProvider
    {
        public DateTime Now { get; }
    }
}
