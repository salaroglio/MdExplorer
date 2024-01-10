using AutoMapper;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Service.Controllers.MdProjects.dto;

namespace MdExplorer.Service.Controllers.TabBar.Automapper
{
    public class LinkInsideMarkdownProfile : Profile
    {
        public LinkInsideMarkdownProfile()
        {
            CreateMap<LinkInsideMarkdown, LinkInsideMarkdownDto>()
                .ForMember(dest => dest.MarkdownFile, opt => opt.MapFrom(src => src.MarkdownFile));
        }
    }
}
