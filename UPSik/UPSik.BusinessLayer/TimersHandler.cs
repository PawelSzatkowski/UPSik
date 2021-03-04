using System;
using System.Collections.Generic;
using System.Timers;
using UPSik.BusinessLayer;
using UPSik.DataLayer.Models;
using UPSik.BusinessLayer.TimeWarper;

namespace UPSik
{
    public interface ITimersHandler
    {
        void SetTimers();
    }

    public class TimersHandler : ITimersHandler
    {
        private readonly IPackingListService _packingListService;
        private readonly IWorkingCouriersService _workingCouriersService;
        private readonly ITimeCalculator _timeCalculator;
        private readonly IVehicleService _vehicleService;
        private readonly IShippingPlannerService _shippingPlannerService;

        public TimersHandler(
            IPackingListService packingListService,
            IWorkingCouriersService workingCouriersService,
            ITimeCalculator timeCalculator,
            IVehicleService vehicleService,
            IShippingPlannerService shippingPlannerService)
        {
            _packingListService = packingListService;
            _workingCouriersService = workingCouriersService;
            _timeCalculator = timeCalculator;
            _vehicleService = vehicleService;
            _shippingPlannerService = shippingPlannerService;
            ;
        }


        private Timer _packingListTimer = new Timer();
        private Timer _startWorkTimer = new Timer();
        private List<Timer> _endWorkTimers = new List<Timer>();

        public const double timeWarpFactor = 60.0d;
        const int dayInMs = 24 * 60 * 60 * 1000;

        public void SetTimers()
        {


            _packingListTimer.Interval = CalculateTimeUntilMidnight();
            _packingListTimer.AutoReset = true;
            _packingListTimer.Enabled = true;
            _packingListTimer.Start();
            _packingListTimer.Elapsed += new ElapsedEventHandler(PackingListTimerElapsed);

            _startWorkTimer.Interval = CalculateTimeUntilStartOfWorkingDay();
            _startWorkTimer.AutoReset = true;
            _startWorkTimer.Enabled = true;
            _startWorkTimer.Start();
            _startWorkTimer.Elapsed += new ElapsedEventHandler(StartWorkTimerElapsed);
        }

        private double CalculateTimeUntilMidnight()
        {
            var warpedNowTime = _timeCalculator.GetCurrentWarpedTime();
            TimeSpan timerInterval = new DateTime(warpedNowTime.Year, warpedNowTime.Month, warpedNowTime.Day, 23, 59, 59, 999) - warpedNowTime;

            double timerIntervalInMiliseconds = Math.Round(timerInterval.TotalSeconds * 1000 / timeWarpFactor);
            return timerIntervalInMiliseconds;
        }

        private double CalculateTimeUntilStartOfWorkingDay()
        {
            var warpedNowTime = _timeCalculator.GetCurrentWarpedTime();
            TimeSpan timerInterval = new DateTime(warpedNowTime.Year, warpedNowTime.Month, warpedNowTime.Day, 08, 00, 00, 000) - warpedNowTime;
            var timerIntervalInMs = timerInterval.TotalMilliseconds;

            if (warpedNowTime.Hour >= 08)
            {
                timerIntervalInMs += dayInMs;
            }

            double warpedTimerIntervalInMs = Math.Round(timerIntervalInMs / timeWarpFactor);
            return warpedTimerIntervalInMs;
        }

        private double CalculateWarpedTimeUntilEndOfWorkingDay(Vehicle vehicle)
        {
            var totalDistance = _shippingPlannerService.CalculateRouteDistance(_shippingPlannerService.GetCourierRouteCoords(vehicle));
            var timeInHours = totalDistance / vehicle.AverageVelocity;
            var timeInMs = timeInHours * 60 * 60 * 1000 / timeWarpFactor;
            return timeInMs;
        }

        private void PackingListTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _packingListTimer.Interval = dayInMs / timeWarpFactor;

            _packingListService.GeneratePackingLists();
        }

        private void StartWorkTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _startWorkTimer.Interval = dayInMs / timeWarpFactor;

            _workingCouriersService.StartWorkingDay();

            var vehicles = _vehicleService.GetVehiclesList();

            foreach (var vehicle in vehicles)
            {
                if (vehicle.CourierPackingList.Count != 0)
                {
                    var endWorkTimer = new Timer();
                    endWorkTimer.Interval = CalculateWarpedTimeUntilEndOfWorkingDay(vehicle);
                    endWorkTimer.AutoReset = false;
                    endWorkTimer.Enabled = true;
                    endWorkTimer.Start();
                    endWorkTimer.Elapsed += (sender, e) => EndWorkTimerElapsed(vehicle);
                    _endWorkTimers.Add(endWorkTimer);
                }
            }
        }

        private void EndWorkTimerElapsed(Vehicle vehicle)
        {
            _workingCouriersService.EndWorkingDay(vehicle);
            _endWorkTimers.Clear();
        }
    }
}
