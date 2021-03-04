namespace UPSik.BusinessLayer
{
    public interface IPackageDeliveryConfigValues
    {
        int MaxDistanceToPickupPackage { get; }
        int WorkingHours { get; }
    }
    public class PackageDeliveryConfigValues : IPackageDeliveryConfigValues
    {
        public int MaxDistanceToPickupPackage { get; } = 50000;
        public int WorkingHours { get; } = 10;
    }
}
