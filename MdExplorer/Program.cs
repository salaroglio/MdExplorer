using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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
        public static void Main(string[] args)
        {
            
            CreateHostBuilder(args).Build().Run();
            
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            Startup.Args = args;
            
            var toReturn = Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   var p = System.Reflection.Assembly.GetEntryAssembly().Location;
                   p = p.Substring(0, p.LastIndexOf(@"\") + 1);
                  
                   webBuilder.UseUrls("https://127.0.0.1:0");
                   webBuilder.UseStartup<Startup>();
               });
            return toReturn;
        }
           
        

    }
}

