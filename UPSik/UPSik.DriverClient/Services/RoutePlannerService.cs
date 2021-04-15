using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UPSik.DriverClient.Models;

namespace UPSik.DriverClient.Services
{
    public interface IRoutePlannerService
    {
        List<Package> CalculateEtaForPackages(List<Package> currentPackingList, int vehicleAverageSpeed, DateTime startWorkDay);
    }

    public class RoutePlannerService : IRoutePlannerService
    {
        public List<Package> CalculateEtaForPackages(List<Package> currentPackingList, int vehicleAverageSpeed, DateTime startWorkDay)
        {
            double totalDistance = 0;

            foreach (var package in currentPackingList) 
            {
                totalDistance += package.SenderDistanceFromPreviousPoint;

                package.EtaToSender = CalculateEtaFromLastPoint(totalDistance, vehicleAverageSpeed, startWorkDay);
            }

            foreach (var package in currentPackingList)
            {
                totalDistance += package.ReceiverDistanceFromPreviousPoint;

                package.EtaToReceiver = CalculateEtaFromLastPoint(totalDistance, vehicleAverageSpeed, startWorkDay);
            }

            return currentPackingList;
        }

        private DateTime CalculateEtaFromLastPoint(double distance, int speed, DateTime startWorkDay)
        {
            DateTime Eta = startWorkDay + TimeSpan.FromHours(distance / Convert.ToDouble(speed));

            return Eta;
        }
    }
}
