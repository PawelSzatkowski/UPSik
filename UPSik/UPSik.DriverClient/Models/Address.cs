using System;
using System.Collections.Generic;
using System.Text;

namespace UPSik.DriverClient.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string Country { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
