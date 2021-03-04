using System;
using Unity;
using Unity.Injection;
using UPSik.BusinessLayer;
using UPSik.DataLayer;
using UPSik.BusinessLayer.TimeWarper;

namespace UPSik.WebApi
{
    public class UnityDiContainerProvider
    {
        public IUnityContainer GetContainer()
        {
            var container = new UnityContainer();

            container.RegisterInstance<IUnityContainer>(container);

            container.RegisterType<IDatabaseManagementService, DatabaseManagementService>();
            container.RegisterType<IGeoMapService, GeoMapService>();
            container.RegisterType<IPackageService, PackageService>();
            container.RegisterType<IPackingListService, PackingListService>();
            container.RegisterType<IUserService, UserService>();
            container.RegisterType<IVehicleService, VehicleService>();
            container.RegisterType<IWorkingCouriersService, WorkingCouriersService>();
            container.RegisterType<INotificationService, NotificationService>();
            container.RegisterType<IPackageDeliveryConfigValues, PackageDeliveryConfigValues>();
            container.RegisterType<IShippingPlannerService, ShippingPlannerService>();

            container.RegisterType<Func<IUPSikDbContext>>(
                new InjectionFactory(ctx => new Func<IUPSikDbContext>(() => new UPSikDbContext())));

            container.RegisterSingleton<ITimersHandler, TimersHandler>();
            container.RegisterType<ITimeCalculator, TimeCalculator>();

            return container;
        }
    }
}
