using System;
using System.Collections.Generic;
using System.Text;

namespace UPSik.DataLayer.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string RegistrationNumber { get; set; }
        public int LoadCapacity { get; set; }
        public int CurrentLoad { get; set; }
        public int AverageVelocity { get; set; }
        public User Driver { get; set; }
        public List<Package> CourierPackingList {get; set;}
        public double DistanceToCover;
    }
}
