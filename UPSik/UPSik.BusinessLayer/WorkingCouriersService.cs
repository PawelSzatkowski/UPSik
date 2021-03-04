using UPSik.DataLayer.Models;

namespace UPSik.BusinessLayer
{
    public interface IWorkingCouriersService
    {
        void StartWorkingDay();
        void EndWorkingDay(Vehicle vehicle);
    }

    public class WorkingCouriersService : IWorkingCouriersService
    {
        private readonly IPackageService _packageService;
        private readonly INotificationService _notificationsService;

        public WorkingCouriersService(
            IPackageService packageService,
            INotificationService notificationService)
        {
            _packageService = packageService;
            _notificationsService = notificationService;
        }
        public void StartWorkingDay()
        {
            _packageService.ChangeAddedToPackingListPackagesStateToShipping();
        }

        public void EndWorkingDay(Vehicle vehicle)
        {
            _packageService.ChangeShippingPackagesStateToAwaitingPickup(vehicle.CourierPackingList);

            _notificationsService.NotifyPackageDelivery(vehicle.CourierPackingList);

            _packageService.ChangePackagesStateFromAwaitingPickupToDelivered(vehicle.CourierPackingList);
        }
    }
}
