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
            var rx = new Regex("^# (.*)$",
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = rx.Matches(toCheck.DataText);
            
            // Verifica se ci sono corrispondenze (documenti senza titolo)
            if (matches.Count == 0)
            {
                // Nessun titolo trovato - il file non viola la regola
                // (non possiamo verificare la corrispondenza se non c'è titolo)
                return (false, null);
            }
            
            var firstTitle = matches[0].Groups[1].Value;

            firstTitle = GetTitle(firstTitle);

            var firstFileName = firstTitle + ".md";
            if (!string.Equals(toCheck.Name, (firstFileName),
                                StringComparison.OrdinalIgnoreCase))
            {
                return (true, firstFileName);
            }
            return (false, null);
        }

        public string GetTitle(string firstTitle)
        {
            Regex regTest = new Regex("<[^>]*>"); // clean all added html
            var firstTitleTest = regTest.Replace(firstTitle, string.Empty);
            Regex regTest2 = new Regex(":[^:^ ]*:");

            firstTitleTest = regTest2.Replace(firstTitleTest, string.Empty);

            Regex reg = new Regex("[*'\",/+:\\(\\)_&#^@]");
            firstTitle = reg.Replace(firstTitleTest, string.Empty);

            Regex reg1 = new Regex("[ ]");
            firstTitle = reg1.Replace(firstTitle.Trim(), "-");
            return firstTitle;
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
