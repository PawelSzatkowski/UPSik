using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UPSik.DataLayer;
using UPSik.DataLayer.Models;

namespace UPSik.BusinessLayer
{
    public interface IVehicleService
    {
        Task<bool> AddNewVehicleAsync(Vehicle newVehicle);
        bool CheckIfCourierAlreadyHaveACar(User driver);
        bool CheckIfThereIsAtLeasOneCourierVehicle();
        void ClearCouriersPackingLists();
        Vehicle GetCourierVehicle(int courierId);
        Task<List<Package>> GetLatestCourierPackingListAsync(int id);
        int GetVehicleCurrentLoad(Vehicle courierVehicle);
        List<Vehicle> GetVehiclesList();
        int GetVehicleAverageVelocity(int id);
    }

    public class VehicleService : IVehicleService
    {
        private readonly Func<IUPSikDbContext> _dbContextFactoryMethod;

        public VehicleService(
            Func<IUPSikDbContext> dbContextFactoryMethod)
        {
            _dbContextFactoryMethod = dbContextFactoryMethod;
        }

        public Vehicle GetCourierVehicle(int courierId)
        {
            using (var context = _dbContextFactoryMethod())
            {
                return context.Vehicles
                    .Include(x => x.CourierPackingList)
                    .Include(x => x.Driver)
                    .FirstOrDefault(x => x.Driver.Id == courierId);
            }
        }

        public List<Vehicle> GetVehiclesList()
        {
            using (var context = _dbContextFactoryMethod())
            {
                return context.Vehicles
                    .Include(x => x.Driver)
                      .ThenInclude(x => x.Address)
                    .Include(x => x.CourierPackingList)
                      .ThenInclude(x => x.Sender)
                          .ThenInclude(x => x.Address)
                    .Include(x => x.CourierPackingList)
                        .ThenInclude(x => x.Receiver)
                            .ThenInclude(x => x.Address)
                    .ToList();
            }
        }

        public async Task<bool> AddNewVehicleAsync(Vehicle newVehicle)
        {
            using (var context = _dbContextFactoryMethod())
            {
                newVehicle.Driver = context.Users
                    .FirstOrDefault(x => x.Email == newVehicle.Driver.Email);

                if (newVehicle.Driver != null)
                {
                    context.Vehicles.Add(newVehicle);
                    await context.SaveChangesAsync();

                    return true;
                }

                return false;
            }
        }

        public bool CheckIfCourierAlreadyHaveACar(User driver)
        {
            using (var context = _dbContextFactoryMethod())
            {
                return context.Vehicles.Any(x => x.Driver.Email == driver.Email);
            }
        }

        public bool CheckIfThereIsAtLeasOneCourierVehicle()
        {
            using (var context = _dbContextFactoryMethod())
            {
                return context.Vehicles
                    .AsQueryable()
                    .Any();
            }
        }

        public int GetVehicleCurrentLoad(Vehicle courierVehicle)
        {
            using (var context = _dbContextFactoryMethod())
            {
                var vehicle = context.Vehicles
                    .FirstOrDefault(x => x.RegistrationNumber == courierVehicle.RegistrationNumber);
                return vehicle.CurrentLoad;
            }
        }

        public void ClearCouriersPackingLists()
        {
            using (var context = _dbContextFactoryMethod())
            {
                var vehicles = context.Vehicles
                    .Include(x => x.CourierPackingList)
                    .ToList();

                foreach (var vehicle in vehicles)
                {
                    vehicle.CourierPackingList.Clear();
                    context.Vehicles.Update(vehicle);
                }

                context.SaveChanges();
            }
        }

        public async Task<List<Package>> GetLatestCourierPackingListAsync(int id)
        {
            var packingListsDirectory = new DirectoryInfo($"{Path.GetPathRoot(Environment.SystemDirectory)}shipping_lists");

            if (!packingListsDirectory.Exists)
            {
                return null;
            }

            var fileData = await packingListsDirectory.GetFiles()
                .Where(x => x.Name.StartsWith($"{id}_"))
                .OrderByDescending(f => f.LastWriteTime)
                .ToAsyncEnumerable()
                .FirstOrDefaultAsync();

            if (fileData == null)
            {
                return null;
            }

            var jsonData = File.ReadAllText(fileData.FullName);
            var packingList = JsonConvert.DeserializeObject<List<Package>>(jsonData);

            return packingList;
        }

        public int GetVehicleAverageVelocity(int id)
        {
            using (var context = _dbContextFactoryMethod())
            {
                return context.Vehicles
                    .FirstOrDefault(x => x.Driver.Id == id)
                    .AverageVelocity;
            }
        }
    }
}
