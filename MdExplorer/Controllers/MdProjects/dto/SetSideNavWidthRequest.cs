using System;

namespace MdExplorer.Service.Controllers.MdProjects.dto
{
    public class SetSideNavWidthRequest
    {
        public Guid Id { get; set; }
        public int? SidenavWidth { get; set; }
    }
}
