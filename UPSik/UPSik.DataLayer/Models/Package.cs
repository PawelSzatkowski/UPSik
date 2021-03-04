using System;

namespace UPSik.DataLayer.Models
{
    public class Package
    {
        public enum PackageWeight
        { 
            Small = 15,
            Medium = 50,
            Large = 150
        }

        public enum PackageState
        {
            AwaitingAddingToPackingList,
            AddedToPackingList,
            Shipping,
            AwaitingPickup,
            Delivered,
        }

        public enum PackagePriority
        {
            Normal,
            High
        }

        public int Id { get; set; }
        public Guid Number { get; set; }
        public User Receiver { get; set; }
        public User Sender { get; set; }
        public PackageWeight Weight { get; set; }
        public DateTime DateOfAdding { get; set; }
        public PackageState State { get; set; }
        public PackagePriority Priority { get; set; }
    }
}
