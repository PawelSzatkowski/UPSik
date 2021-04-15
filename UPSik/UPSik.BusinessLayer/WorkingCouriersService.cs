using UPSik.DataLayer.Models;

namespace UPSik.BusinessLayer
{
    public interface IWorkingCouriersService
    {
        void StartWorkingDay();
        void EndWorkingDay(Vehicle vehicle);
        void FinishWorkManually(int courierId);
    }

    public class WorkingCouriersService : IWorkingCouriersService
    {
        private readonly IPackageService _packageService;
        private readonly INotificationService _notificationsService;
        private readonly IPackingListService _packingListService;
        private readonly IVehicleService _vehicleService;

        public WorkingCouriersService(
            IPackageService packageService,
            INotificationService notificationService,
            IPackingListService packingListService,
            IVehicleService vehicleService)
        {
            _packageService = packageService;
            _notificationsService = notificationService;
            _packingListService = packingListService;
            _vehicleService = vehicleService;
        }
        public void StartWorkingDay()
        {
            _packageService.ChangeAddedToPackingListPackagesStateToShipping();
        }

        public void EndWorkingDay(Vehicle vehicle)
        {
            _packageService.ChangeShippingPackagesStateToAwaitingPickup(vehicle.CourierPackingList);

            _notificationsService.NotifyPackageDelivery(vehicle.CourierPackingList);

            _packageService.ChangePackagesState(vehicle.CourierPackingList, Package.PackageState.Delivered);

            _packingListService.CalculateCourierRatingForPackingList(vehicle);
        }

        public void FinishWorkManually(int courierId)
        {
            var courierVehicle = _vehicleService.GetCourierVehicle(courierId);

            _packingListService.CalculateCourierRatingForPackingList(courierVehicle);

            _packageService.ManageUndeliveredPackages(courierVehicle.CourierPackingList);
        }
    }
}
