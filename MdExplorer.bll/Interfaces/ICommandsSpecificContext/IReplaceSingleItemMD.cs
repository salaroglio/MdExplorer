using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Interfaces.ICommandsSpecificContext
{
    public interface IReplaceSingleItemMD<in T>
    {
        string ReplaceSingleItem(string markdown, RequestInfo requestinfo, T emojiPriorityOrderInfo);
    }
}
