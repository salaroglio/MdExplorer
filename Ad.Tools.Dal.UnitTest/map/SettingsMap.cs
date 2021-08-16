using Ad.Tools.Dal.UnitTest.concrete;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.UnitTest.map
{
    public class SettingsMap : ClassMap<Setting>
    {
        public SettingsMap()
        {
            Table("Setting");
            Id(_ => _.Id).GeneratedBy.GuidComb();
            Map(_ => _.Name).Not.Nullable();
        }
    }
}
