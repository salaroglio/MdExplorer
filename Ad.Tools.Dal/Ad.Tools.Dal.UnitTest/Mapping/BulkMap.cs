using Ad.Tools.Dal.UnitTest;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.BellaCopia.WebAPI.DAL.Mapping
{
    public class BulkMap:ClassMap<Bulk>
    {
        public BulkMap()
        {
            Table("Bulk");
            Id(_ => _.Id).GeneratedBy.GuidComb();
            Map(_ => _.Call).Not.Nullable();
            Map(_ => _.StartDate).Nullable();
            Map(_ => _.EndDate).Nullable();
            Map(_ => _.CurrentCall).Not.Nullable();
            Map(_ => _.CurrentCallInError).Nullable();
            References(_ => _.User).Column("UserId");
        }
    }
}
