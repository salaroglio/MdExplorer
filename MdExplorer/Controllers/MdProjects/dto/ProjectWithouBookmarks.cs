using AutoMapper;
using MdExplorer;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Service;
using MdExplorer.Service.Controllers;
using MdExplorer.Service.Controllers.MdFiles;
using MdExplorer.Service.Controllers.MdFiles.ModelsDto;
using MdExplorer.Service.Controllers.MdProjects.dto;
using MdExplorer.Service.Models;
using System;
using System.Collections.Generic;

namespace MdExplorer.Service.Controllers.MdProjects.dto
{
    public class ProjectWithoutBookmarks
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }  // Read from .development.yml (not persisted on UserDB)
        public string Path { get; set; }
        public DateTime LastUpdate { get; set; }

        // MdE Team participants, read from .development.yml — eager-loaded so the
        // projects grid can render gems without an extra round-trip per card.
        public IList<ProjectParticipant> Participants { get; set; } = new List<ProjectParticipant>();
    }
}
