using Ad.Tools.Dal.UnitTest;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.BellaCopia.WebAPI.DAL.Mapping
{
    public class InvoiceMap:ClassMap<Invoice>
    {
        public InvoiceMap()
        {
            Table("Invoice");
            Id(_ => _.Id).GeneratedBy.GuidComb();
            Map(_ => _.IP).Not.Nullable();
            Map(_ => _.CustomerName).Nullable();
            References(_ => _.User).Column("UserId");
            Map(_ => _.StartDate).Not.Nullable();
            Map(_ => _.EndDate).Not.Nullable();
            Map(_ => _.FileName).Not.Nullable();
            Map(_ => _.Message).Nullable();
        }
    }
}
