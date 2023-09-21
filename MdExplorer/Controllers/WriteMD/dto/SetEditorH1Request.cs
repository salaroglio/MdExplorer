namespace MdExplorer.Service.Controllers.WriteMD.dto
{
    public class SetEditorH1Request
    {
        public string PathFile { get; set; }
        public string OldMd {  get; set; }
        public string NewMd { get; set; }
        public int IndexStart { get; set; }
        public int IndexEnd { get; set; }
    }
}
