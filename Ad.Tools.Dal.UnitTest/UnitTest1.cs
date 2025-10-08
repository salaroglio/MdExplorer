using Ad.Tools.Dal.Concrete;
using Ad.Tools.Dal.Extensions;
using Ad.Tools.Dal.UnitTest.concrete;
using Ad.Tools.Dal.UnitTest.interfaces;
using Ad.Tools.Dal.UnitTest.map;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace Ad.Tools.Dal.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        IServiceCollection serviceCollection;
        [TestInitialize]
        public void InitAll()
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var databasePath = $"Data Source = {Path.Combine(appdata, "MdExplorer.db")}";
            var databasePathEngine = $"Data Source = {Path.Combine(appdata, "MdEngine.db")}";
            serviceCollection = new ServiceCollection();

            serviceCollection.AddDalFeatures(typeof(SettingsMap).Assembly,
                                    new DatabaseSQLite(), typeof(IUserSettingsDB),
                                    databasePath);

            serviceCollection.AddDalFeatures(typeof(MarkdownFileMap).Assembly,
                                   new DatabaseSQLite(), typeof(IEngineDB),
                                   databasePathEngine);
        }

        [TestMethod]
        public void TestMethod1()
        {
            var provider = serviceCollection.BuildServiceProvider();
            var userSettingSession = provider.GetService<IUserSettingsDB>();
            var engineSession = provider.GetService<IEngineDB>();

            var markdownFileDal = engineSession.GetDal<MarkdownFile>();
            var mdList = markdownFileDal.GetList().ToList();

            var settingDal = userSettingSession.GetDal<Setting>();
            var list = settingDal.GetList().ToList();
        }

        [TestMethod]
        public void TestMethod2()
        {
            var provider = serviceCollection.BuildServiceProvider();
            var userSettingSession = provider.GetService<IUserSettingsDB>();
            var settingDal = userSettingSession.GetDal<Setting>();
            var list = settingDal.GetList().ToList();
        }
    }
}
