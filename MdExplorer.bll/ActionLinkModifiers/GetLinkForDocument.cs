using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.LinkModifiers
{
    public class GetLinkForDocument: IGetModifier
    {
        public string[] GetLinks(string mardown)
        {
            var rx = new Regex(@"!\[[^\]]*\]\(([^\)]*)\)",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(mardown);
            var listToReturn = new List<string>();
            foreach (Match item in matches) 
            {
                var toStore = item.Groups[1].Value;
                listToReturn.Add(toStore);
            }
            return listToReturn.ToArray();
        }
    }
}
