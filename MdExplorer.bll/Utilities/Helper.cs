using MdExplorer.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Utilities
{
    public class Helper : IHelper
    {
        private readonly ILogger<Helper> _logger;

        public Helper(ILogger<Helper> logger)
        {
            _logger = logger;
        }

        public string NormalizePath(string requestInfo)
        {
            var dictionary = NomalizeArray(requestInfo);            
            return string.Join("\\", dictionary.Select(_ => _.Value));
        }


        public virtual string GetBackPath(RequestInfo requestInfo)
        {
            var correctTODelete = requestInfo.CurrentQueryRequest.Replace("\\\\", "\\");
            Dictionary<int, string> arrayToInvestigate = NomalizeArray(correctTODelete);

            //var pseudoPathToEvaluate = requestInfo.CurrentQueryRequest.Replace("..\\", string.Empty).Split("\\");
            var level = arrayToInvestigate.Count() - 1;
            _logger.LogInformation($"level: {level}");
            _logger.LogInformation($"requestInfo.CurrentQueryRequest: {requestInfo.CurrentQueryRequest}");
            var backPath = string.Empty;
            for (int i = 0; i < level; i++)
            {
                if (i == 0)
                {
                    backPath += "..";
                }
                else
                {
                    backPath += "\\..";
                }

            }
            backPath += "\\.md";
            return backPath;
        }

        private static Dictionary<int, string> NomalizeArray(string requestInfo)
        {
            var counter = 0;
            var arrayToInvestigate = requestInfo.Split("\\").ToDictionary(_ => counter++);
            var itemToCompress = arrayToInvestigate.Where(_ => _.Value.Contains(".."));
            var newCompressedPath = new Dictionary<int, string>();

            foreach (var item in itemToCompress.OrderByDescending(_ => _.Key))
            {
                arrayToInvestigate.Remove(item.Key);
                arrayToInvestigate.Remove(item.Key - 1);
            }

            return arrayToInvestigate;
        }
    }
}
