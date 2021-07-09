using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Models;
using NHibernate;
using PlantUml.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public class PlantumlServer
    {
        private readonly ISession _session;
        private readonly RendererFactory _rendererFactory;

        public PlantumlServer(ISession session, RendererFactory rendererFactory)
        {
            _session = session;
            _rendererFactory = rendererFactory;
        }

        public  async Task<byte[]> GetSVGFromJar(string plantumlcode)
        {
            //var factory = new RendererFactory();
            var settingDal = _session.GetDal<Setting>();
            var plantumlSetting = settingDal.GetList().Where(_ => _.Name == "PlantumlLocalPath").FirstOrDefault()?.ValueString;
            var javaPath = settingDal.GetList().Where(_ => _.Name == "JavaPath").FirstOrDefault()?.ValueString;
            var localGraphvizDotPath = settingDal.GetList().Where(_ => _.Name == "LocalGraphvizDotPath").FirstOrDefault()?.ValueString;
            var renderer = _rendererFactory.CreateRenderer(new PlantUmlSettings()
            {
                JavaPath = javaPath,
                LocalGraphvizDotPath = localGraphvizDotPath,
                RenderingMode = RenderingMode.Local,
                LocalPlantUmlPath = plantumlSetting,//@"E:\Sviluppo\MdExplorer\InstallBinaries\plantuml.jar"
            });

            var bytes = await renderer.RenderAsync(plantumlcode, OutputFormat.Png);
            return bytes;
        }
    }
}
