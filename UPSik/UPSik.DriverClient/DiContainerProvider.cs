using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using UPSik.DriverClient.Services;
using UPSik.DriverClient.TimeWarper;

namespace UPSik.DriverClient
{
    public class DiContainerProvider
    {
        public IUnityContainer GetContainer()
        {
            var container = new UnityContainer();

            container.RegisterType<IMenu, Menu>();
            container.RegisterType<IIoHelper, IoHelper>();
            container.RegisterType<IMenuOptions, MenuOptions>();

            container.RegisterType<ITimeCalculator, TimeCalculator>();
            container.RegisterType<IConnectionService, ConnectionService>();
            container.RegisterType<IPackingListService, PackingListService>();
            container.RegisterType<IVehicleService, VehicleService>();
            container.RegisterType<IRoutePlannerService, RoutePlannerService>();
            container.RegisterType<IPackagesService, PackagesService>();

            return container;
        }
    }
}
