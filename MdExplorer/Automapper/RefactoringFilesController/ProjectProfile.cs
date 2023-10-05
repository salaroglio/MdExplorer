using AutoMapper;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Service.Controllers.MdProjects.dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Automapper.RefactoringFilesController
{
    public class ProjectProfile: Profile
    {
        public ProjectProfile()
        {
            CreateMap<Project, ProjectWithoutBookmarks>();
        }
    }
}
