using Ad.Tools.Dal.Abstractions.Interfaces;
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
        private readonly IDALFactory _dalFactory;
        private readonly RendererFactory _rendererFactory;

        public PlantumlServer(IDALFactory dalFactory, RendererFactory rendererFactory)
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

        private IPlantUmlRenderer getRenderer(ISession session)
        {
            var settingDal = session.GetDal<Setting>();
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
            return renderer;
        }
    }
}
