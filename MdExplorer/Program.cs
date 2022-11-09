using MdExplorer.Features.Utilities;
using MdExplorer.Service;
using MdExplorer.Service.HostedServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MdExplorer
{
    public class Program
    {

        public static Task<int> _uiTask;

        public static void Main(string[] args)
        {
            ExtractServer();
            CreateHostBuilder(args).Build().Run();
        }

        private static void ExtractServer()
        {
            //var plantumlPath = AppContext.BaseDirectory + "plantuml.jar";
            //if (!File.Exists(plantumlPath))
            //{
            //    FileUtil.ExtractResFile("MdExplorer.Service.Binaries.plantuml.jar", plantumlPath);
            //}
            //var dotExePath = AppContext.BaseDirectory + "dot.exe";
            //if (!File.Exists(dotExePath))
            //{

            //    FileUtil.ExtractResFile("MdExplorer.Service.Binaries.GraphWiz.binaries.cdt.dll", dotExePath);
            //    FileUtil.ExtractResFile("MdExplorer.Service.Binaries.GraphWiz.binaries.cgraph.dll", dotExePath);
            //    FileUtil.ExtractResFile("MdExplorer.Service.Binaries.GraphWiz.binaries.config6", dotExePath);
            //    FileUtil.ExtractResFile("MdExplorer.Service.Binaries.GraphWiz.binaries.dot.exe", dotExePath);
            //    FileUtil.ExtractResFile("MdExplorer.Service.Binaries.GraphWiz.binaries.expat.dll", dotExePath);
            //    FileUtil.ExtractResFile("MdExplorer.Service.Binaries.GraphWiz.binaries.gvc.dll", dotExePath);
            //    FileUtil.ExtractResFile("MdExplorer.Service.Binaries.GraphWiz.binaries.gvplugin_core.dll", dotExePath);
            //    FileUtil.ExtractResFile("MdExplorer.Service.Binaries.GraphWiz.binaries.gvplugin_dot_layout.dll", dotExePath);
            //    FileUtil.ExtractResFile("MdExplorer.Service.Binaries.GraphWiz.binaries.pathplan.dll", dotExePath);
            //    FileUtil.ExtractResFile("MdExplorer.Service.Binaries.GraphWiz.binaries.vcruntime140.dll", dotExePath);
            //    FileUtil.ExtractResFile("MdExplorer.Service.Binaries.GraphWiz.binaries.xdot.dll", dotExePath);

            //}

            //var plantumlSequenceDiagramPath = AppContext.BaseDirectory + "sequence-diagram.plantuml";
            //if (!File.Exists(plantumlSequenceDiagramPath))
            //{
            //    FileUtil.ExtractResFile("MdExplorer.Service.Binaries.plantuml_templates.sequence_diagram.plantuml", plantumlSequenceDiagramPath);
            //}

        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            Startup.Args = args;

            var toReturn = Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseUrls("http://127.0.0.1:0");
                   webBuilder.UseStartup<Startup>();
               })
               .ConfigureServices(services =>
               {
                   services.AddHostedService<MonitorMDHostedService>();
               });
            ;
            return toReturn;
        }



    }
}

