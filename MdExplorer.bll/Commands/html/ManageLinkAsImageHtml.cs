using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands.html
{
    public class ManageLinkAsImageHtml : ManageLinkAsImages, ICommandHtml
    {
        public ManageLinkAsImageHtml(ILogger<ManageLinkAsImages> logger, IHelper helper) : base(logger, helper)
        {
        }

        public override string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            if (requestInfo.RootQueryRequest == requestInfo.CurrentQueryRequest)
            {
                // Nel caso non ci sia ricorsività (MdShowMd)
                return markdown;
            }
            var matches = GetMatches(markdown);

            foreach (Match item in matches)
            {
                // here you should compose the path adding missing part
                // the missing part is the distance from the root folder and the current file
                // you can build this using requestInfo.currentqueryrequest

                var listOfItemCurrent = requestInfo.CurrentQueryRequest.Split(Path.DirectorySeparatorChar, options: StringSplitOptions.RemoveEmptyEntries).ToList();
                listOfItemCurrent.RemoveAt(listOfItemCurrent.Count - 1);

                var listOfItemRoot = requestInfo.RootQueryRequest.Split(Path.DirectorySeparatorChar, options: StringSplitOptions.RemoveEmptyEntries).ToList();
                listOfItemRoot.RemoveAt(listOfItemRoot.Count - 1);

                var currentWebFolder = string.Empty;
                var rootWebFolder = string.Empty;

                
                currentWebFolder = string.Join(Path.DirectorySeparatorChar, listOfItemCurrent.ToArray());
                rootWebFolder = string.Join(Path.DirectorySeparatorChar, listOfItemRoot.ToArray());
                var fileName = item.Groups[2].Value;

                if (currentWebFolder != rootWebFolder && listOfItemCurrent.Count > listOfItemRoot.Count) // Si trova in un sottofolder
                {
                    var step = listOfItemCurrent.Count - listOfItemRoot.Count;
                    var calculatedFolderFromRoot = string.Empty;
                    for (int i = listOfItemCurrent.Count; i > listOfItemCurrent.Count-step; i--)
                    {
                        calculatedFolderFromRoot = listOfItemCurrent[i-1];
                    }
                    fileName = calculatedFolderFromRoot + "/" + fileName.Replace("\\","/");
                }
                 
                var allElementToReplace = item.Groups[0].Value.Replace(item.Groups[2].Value, fileName);
                markdown = markdown.Replace(item.Groups[0].Value, allElementToReplace);
            }

            return markdown;

        }
    }
}
