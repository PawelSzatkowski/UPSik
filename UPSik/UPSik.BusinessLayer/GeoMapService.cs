using RestSharp;
using System;
using UPSik.DataLayer.Models;
using Newtonsoft.Json.Linq;

namespace UPSik.BusinessLayer
{
    public interface IGeoMapService
    {
        double CalculateDistanceBetweenTwoPoints(double latitudeA, double longitudeA, double latitudeB, double longitudeB);
        GeoCoords GetCoordinatesForAddress(string country, string city, string street, string building);
    }

    public class GeoMapService : IGeoMapService
    {
        private double avgEarthRadius = 6371000;

        public GeoCoords GetCoordinatesForAddress(string country, string city, string street, string building)
        {
            var client = new RestClient($"https://nominatim.openstreetmap.org/?q={street}+{building}+{city}+{country}&format=json&limit=1");
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            var jsonData = JArray.Parse(response.Content);

            if (jsonData.Count == 0)
            {
                return null;
            }

            return new GeoCoords
            {
                Latitude = (double)jsonData[0]["lat"],
                Longitude = (double)jsonData[0]["lon"]
            };
        }

        public double CalculateDistanceBetweenTwoPoints(double latitudeA, double longitudeA, double latitudeB, double longitudeB)
        {
            double fi1 = latitudeA * Math.PI / 180;
            double fi2 = latitudeB * Math.PI / 180;
            double deltaFi = (latitudeB - latitudeA) * Math.PI / 180;
            double deltaLambda = (longitudeB - longitudeA) * Math.PI / 180;

            double haversineFactorA = Math.Sin(deltaFi / 2) * Math.Sin(deltaFi / 2) + Math.Cos(fi1) * Math.Cos(fi2) * Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
            double haversineFactorB = 2 * Math.Atan2(Math.Sqrt(haversineFactorA), Math.Sqrt(1 - haversineFactorA));

            double distanceInMeters = avgEarthRadius * haversineFactorB;
            return distanceInMeters;
        }
    }
}
