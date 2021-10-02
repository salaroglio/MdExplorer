using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public class CommandBase
    {
        protected (string, int) ManageReplaceOnMD(string stringToReturn, int currentIncrement, Match item, string raplaceWith)
        {
            var currentIndex = item.Index + currentIncrement;
            stringToReturn = stringToReturn.Remove(currentIndex, item.Groups[0].Value.Length).Insert(currentIndex, raplaceWith);
            currentIncrement += raplaceWith.Length - item.Groups[0].Value.Length;
            return (stringToReturn, currentIncrement);
        }
        
    }
}
