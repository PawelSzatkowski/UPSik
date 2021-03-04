using System;
using Topshelf;
using Unity;
using Microsoft.AspNetCore.Hosting;

namespace UPSik.WebApi
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new UnityDiContainerProvider().GetContainer();
            var appHost = container.Resolve<AppHost>();

            var rc = HostFactory.Run(x =>
            {
                x.Service<AppHost>(s =>
                {
                    s.ConstructUsing(sf => appHost);
                    s.WhenStarted(ah => ah.Start());
                    s.WhenStopped(ah => ah.Stop());
                });

                x.RunAsLocalSystem();
                x.SetDescription("UPSik_WebApi.TopShelf service");
                x.SetDisplayName("UPSik.WebApi");
                x.SetServiceName("UPSik.WebApi");
            });

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;
        }
    }
}
