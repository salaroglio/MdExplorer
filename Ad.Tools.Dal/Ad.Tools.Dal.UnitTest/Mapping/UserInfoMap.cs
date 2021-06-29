using Ad.Tools.Dal.UnitTest;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.BellaCopia.WebAPI.DAL.Mapping
{
    public class UserInfoMap:ClassMap<UserInfo>
    {
        public UserInfoMap()
        {
            Table("UserInfo");
            Id(_ => _.Id).GeneratedBy.GuidComb();
            Map(_ => _.UserName).Not.Nullable();
            Map(_ => _.Password).Not.Nullable();
        }
    }
}
