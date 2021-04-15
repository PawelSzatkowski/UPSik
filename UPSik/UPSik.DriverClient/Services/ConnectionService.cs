using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace UPSik.DriverClient.Services
{
    public interface IConnectionService
    {
        HttpResponseMessage WebApiConnect(string endpointAddress, ConnectionService.ConnectionType connectionType);
    }

    public class ConnectionService : IConnectionService
    {
        public enum ConnectionType
        {
            Get,
            Post,
            Put,
            Patch,
            Delete
        }

        public HttpResponseMessage WebApiConnect(string endpointAddress, ConnectionType connectionType)
        {
            using (var httpClient = new HttpClient())
            {
                var path = @$"http://localhost:10500/api/{endpointAddress}";
                var result = new HttpResponseMessage();

                switch (connectionType)
                {
                    case ConnectionType.Get:
                        result = httpClient.GetAsync(path).Result;
                        break;
                    case ConnectionType.Put:
                        result = httpClient.PutAsync(path, null).Result;
                        break;
                    case ConnectionType.Patch:
                        result = httpClient.PatchAsync(path, null).Result;
                        break;
                }

                return result;
            }
        }
    }
}
