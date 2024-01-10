using AutoMapper;
using MdExplorer.Abstractions.Entities.EngineDB;

namespace MdExplorer.Service.Controllers.TabBar.Automapper
{
    public class MarkdownFileDtoProfile:Profile
    {
        public MarkdownFileDtoProfile()
        {
            CreateMap<MarkdownFile, MarkdownFileDto>();
        }
    }
}
