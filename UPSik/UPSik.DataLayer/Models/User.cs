using System;
using System.Collections.Generic;
using System.Text;

namespace UPSik.DataLayer.Models
{
    public class User
    {
        public enum UserType
        {
            Courier = 1,
            Customer = 2
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public Address Address { get; set; }
        public UserType Type { get; set; }
        public float CourierRating { get; set; }
    }
}
