using MdExplorer;
using MdExplorer.Service;
using MdExplorer.Service.Controllers;
using MdExplorer.Service.Controllers.MdFiles;
using MdExplorer.Service.Controllers.MdFiles.ModelsDto;

namespace MdExplorer.Service.Controllers.MdFiles.ModelsDto
{
    public class CreateSnapshtoRequest
    {
        public string FullPath { get; set; }    
        public string SignalRConnectionId { get; set; }



    }
}