using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Refactoring.Analysis.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.Refactoring.Analysis
{
    /// <summary>
    /// Check if the name of file is the same as first title in markdown
    /// </summary>
    public class GoodMdRuleFileNameShouldBeSameAsTitle : IGoodMdRule<FileInfoNode>
    {
        public (bool, string) ItBreakTheRule(FileInfoNode toCheck)
        {
            var rx = new Regex("# (.*)\r",
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = rx.Matches(toCheck.DataText);
            var firstTitle = matches[0].Groups[1].Value;
            Regex reg = new Regex("[*'\",_&#^@]");
            firstTitle = reg.Replace(firstTitle, string.Empty);

            Regex reg1 = new Regex("[ ]");
            firstTitle = reg1.Replace(firstTitle, "-");

            var firstFileName = firstTitle + ".md";
            if (!string.Equals(toCheck.Name, (firstFileName),
                                StringComparison.OrdinalIgnoreCase))
            {
                return (true, firstFileName);
            }
            return (false, null);
        }

        public FileInfoNode MakeChangesToRespectTheRule(FileInfoNode toChange)
        {
            throw new NotImplementedException();
        }

        public void PersistChanges(FileInfoNode toSave)
        {
            throw new NotImplementedException();
        }
    }
}
