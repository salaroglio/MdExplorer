using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Utilities
{
    public class ProcessUtil
    {
        private readonly IUserSettingsDB _session;
        private readonly FileSystemWatcher _fileSystemWatcher;

        public ProcessUtil(IUserSettingsDB session, 
            FileSystemWatcher fileSystemWatcher)
        {
            _session = session;
            _fileSystemWatcher = fileSystemWatcher;
        }
        public void OpenFileWithVisualStudioCode(string path)
        {
            var settingDal = _session.GetDal<Setting>();
            var editorPath = settingDal.GetList().Where(_ => _.Name == "EditorPath").FirstOrDefault()?.ValueString
                ?? @"C:\Users\Carlo\AppData\Local\Programs\Microsoft VS Code\Code.exe";

            var currentPath = path.Replace(@"\\", @"\"); // pulitura da mettere a posto            
            var dosCommand = $@"""{editorPath}"" -a ""{_fileSystemWatcher.Path}"" """ + currentPath + "\"";
            Process.Start(dosCommand);
        }
    }
}
