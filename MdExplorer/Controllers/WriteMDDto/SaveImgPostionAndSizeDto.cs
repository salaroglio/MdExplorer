using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Controllers.WriteMDDto
{
    public class SaveImgPostionAndSizeDto
    {        
        public string PathFile { get; set; }
        public string LinkHash { get; set; }
        public string CSSHash { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }        
        public int ClientX { get; set; } 
        public int ClientY { get; set; } 
        public string Position { get; set; }
    }
}
