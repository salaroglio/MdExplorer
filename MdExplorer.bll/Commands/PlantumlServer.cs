using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using NHibernate;
using PlantUml.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public class PlantumlServer
    {
        private readonly IDALFactory<IUserSettingsDB> _dalFactory;
        private readonly RendererFactory _rendererFactory;

        public PlantumlServer(IDALFactory<IUserSettingsDB> dalFactory, RendererFactory rendererFactory)
        {
            _dalFactory = dalFactory;
            _rendererFactory = rendererFactory;
        }

        public  async Task<byte[]> GetSvgFromJar(string plantumlcode)
        {
            //var factory = new RendererFactory();
            using (var session = _dalFactory.OpenSession())
            {
                IPlantUmlRenderer renderer = getRenderer(session);

                var bytes = await renderer.RenderAsync(plantumlcode, OutputFormat.Svg);
                return bytes;
            }
        }

        public async Task<byte[]> GetPngFromJar(string plantumlcode)
        {
            //var factory = new RendererFactory();
            using (var session = _dalFactory.OpenSession())
            {
                IPlantUmlRenderer renderer = getRenderer(session);

                var bytes = await renderer.RenderAsync(plantumlcode, OutputFormat.Png);
                return bytes;
            }
        }

        private IPlantUmlRenderer getRenderer(ISessionDB session)
        {
            var settingDal = session.GetDal<Setting>();
            var currentApplicationPath = AppContext.BaseDirectory;
            
            // Get settings from database
            var plantumlPathSetting = settingDal.GetList().Where(_ => _.Name == "PlantumlLocalPath").FirstOrDefault()?.ValueString;
            var javaPath = settingDal.GetList().Where(_ => _.Name == "JavaPath").FirstOrDefault()?.ValueString;
            var graphvizPathSetting = settingDal.GetList().Where(_ => _.Name == "LocalGraphvizDotPath").FirstOrDefault()?.ValueString;
            
            // Build full paths - handle both relative and absolute paths
            string plantumlPath = null;
            if (!string.IsNullOrEmpty(plantumlPathSetting))
            {
                if (Path.IsPathRooted(plantumlPathSetting))
                {
                    plantumlPath = plantumlPathSetting;
                }
                else
                {
                    plantumlPath = Path.Combine(currentApplicationPath, plantumlPathSetting);
                }
            }
            
            // Handle Graphviz path based on OS
            string localGraphvizDotPath = null;
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                // Windows: use the configured path or default to Graphviz\windows\dot.exe
                if (!string.IsNullOrEmpty(graphvizPathSetting))
                {
                    if (Path.IsPathRooted(graphvizPathSetting))
                    {
                        localGraphvizDotPath = graphvizPathSetting;
                    }
                    else
                    {
                        localGraphvizDotPath = Path.Combine(currentApplicationPath, graphvizPathSetting);
                    }
                }
                else
                {
                    localGraphvizDotPath = Path.Combine(currentApplicationPath, "Binaries", "Graphviz", "windows", "dot.exe");
                }
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                // Linux: use the bundled Linux binaries
                localGraphvizDotPath = Path.Combine(currentApplicationPath, "Binaries", "Graphviz", "linux", "dot");
            }
            
            // Auto-discover Java if the configured path doesn't exist or is empty
            if (string.IsNullOrEmpty(javaPath) || !File.Exists(javaPath))
            {
                var discoveredJavaPath = Utilities.JavaDiscovery.DiscoverJavaPath();
                if (!string.IsNullOrEmpty(discoveredJavaPath))
                {
                    javaPath = discoveredJavaPath;
                    
                    // Optionally update the database with the discovered path
                    var javaSetting = settingDal.GetList().Where(_ => _.Name == "JavaPath").FirstOrDefault();
                    if (javaSetting != null)
                    {
                        javaSetting.ValueString = discoveredJavaPath;
                        settingDal.Save(javaSetting);
                        session.Flush();
                    }
                }
            }
            
            // If PlantUML path is not set or doesn't exist, try to find it
            if (string.IsNullOrEmpty(plantumlPath) || !File.Exists(plantumlPath))
            {
                // Try common locations
                var possiblePaths = new[]
                {
                    Path.Combine(currentApplicationPath, "Binaries", "plantuml.jar"),
                    Path.Combine(currentApplicationPath, "Binaries", "plantuml-1.jar"),
                    Path.Combine(currentApplicationPath, "..", "..", "..", "..", "Binaries", "plantuml.jar")
                };
                
                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        plantumlPath = path;
                        break;
                    }
                }
            }
            
            // Make the Linux dot executable if needed
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux) && 
                !string.IsNullOrEmpty(localGraphvizDotPath) && File.Exists(localGraphvizDotPath))
            {
                try
                {
                    // Make sure the dot file is executable
                    var chmod = System.Diagnostics.Process.Start("chmod", $"+x {localGraphvizDotPath}");
                    chmod?.WaitForExit();
                }
                catch { }
            }
            
            // Log the paths for debugging
            Console.WriteLine($"[PlantumlServer] JavaPath: {javaPath}");
            Console.WriteLine($"[PlantumlServer] PlantUmlPath: {plantumlPath}");
            Console.WriteLine($"[PlantumlServer] GraphvizPath: {localGraphvizDotPath}");
            Console.WriteLine($"[PlantumlServer] PlantUML exists: {File.Exists(plantumlPath)}");
            Console.WriteLine($"[PlantumlServer] Graphviz exists: {File.Exists(localGraphvizDotPath)}");
            
            var renderer = _rendererFactory.CreateRenderer(new PlantUmlSettings()
            {
                JavaPath = javaPath,
                LocalGraphvizDotPath = localGraphvizDotPath,
                RenderingMode = RenderingMode.Local,
                LocalPlantUmlPath = plantumlPath,
            });
            return renderer;
        }
    }
}
