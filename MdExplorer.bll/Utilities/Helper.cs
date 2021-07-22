using MdExplorer.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
            //var correctTODelete = requestInfo.CurrentQueryRequest.Replace("\\\\", "\\");
            Dictionary<int, string> arrayToInvestigate = NomalizeArray(requestInfo.CurrentQueryRequest);

            //var pseudoPathToEvaluate = requestInfo.CurrentQueryRequest.Replace("..\\", string.Empty).Split("\\");
            var level = arrayToInvestigate.Count()-1;
            _logger.LogInformation($"level: {level}");
            _logger.LogInformation($"requestInfo.CurrentQueryRequest: {requestInfo.CurrentQueryRequest}");
            var backPath = ".";
            for (int i = 0; i < level; i++)
            {
                if (i == 0)
                {
                    backPath = ".";
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
            var arrayToInvestigate = requestInfo.Split(Path.DirectorySeparatorChar).ToDictionary(_ =>  counter++);
            var itemToCompress = arrayToInvestigate.Where(_ => _.Value.Contains("..")).ToList();
            var newCompressedPath = new Dictionary<int, string>();


            var counter1 = 0;
            while (itemToCompress.Count()>0)
            {
                foreach (var item in itemToCompress.OrderBy(_ => _.Key))
                {
                    arrayToInvestigate.Remove(item.Key);
                    var currentKey = item.Key - (2 * counter1) - 1;
                    arrayToInvestigate.Remove(currentKey);
                    itemToCompress.Remove(item);
                    break;
                }
                //ricostruisco l'indice
                counter1 ++;                 
                
            }
            

            return arrayToInvestigate;
        }
        public string GetHashString(string value, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.ASCII;
            }
            byte[] bytes = encoding.GetBytes(value);
            byte[] hash = Hash(bytes);
            string result = String(hash);
            return result;
        }

        public string String(byte[] hash)
        {
            /*https://stackoverflow.com/questions/1300890/
              md5-hash-with-salt-for-keeping-password-in-db-in-c-sharp*/
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));     /*do not make it X2*/
            }
            var result = sb.ToString();
            return result;
        }

        public byte[] Hash(byte[] value)
        {
            /*https://support.microsoft.com/en-za/help/307020/
              how-to-compute-and-compare-hash-values-by-using-visual-cs*/
            /*https://andrewlock.net/why-is-string-gethashcode-
              different-each-time-i-run-my-program-in-net-core*/
            byte[] result = MD5.Create().ComputeHash(value);
            return result;
        }
    }
}
