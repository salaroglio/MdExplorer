using AutoMapper;
using MdExplorer;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Service;
using MdExplorer.Service.Controllers;
using MdExplorer.Service.Controllers.MdFiles;
using MdExplorer.Service.Controllers.MdFiles.ModelsDto;
using MdExplorer.Service.Controllers.MdProjects.dto;
using System;

namespace MdExplorer.Service.Controllers.MdProjects.dto
{
    public class ProjectWithoutBookmarks
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime LastUpdate { get; set; }

    }
}
