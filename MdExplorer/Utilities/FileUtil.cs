using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace MdExplorer.Service
{
    class FileUtil
    {

        /// <summary>
        /// Extract resource files from resource files
        /// </summary>
        /// <param name="resFileName">resource file name (resource file name must include directories, separated by ".", the outermost layer is the project default namespace)</param>
        /// <param name="outputFile">output file</param>
        public static void ExtractResFile(string resFileName, string outputFile)
        {
            BufferedStream inStream = null;
            FileStream outStream = null;
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly(); //Read embedded resources
                inStream = new BufferedStream(asm.GetManifestResourceStream(resFileName));
                outStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write);

                byte[] buffer = new byte[1024];
                int length;

                while ((length = inStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outStream.Write(buffer, 0, length);
                }
                outStream.Flush();
            }
            finally
            {
                if (outStream != null)
                {
                    outStream.Close();
                }
                if (inStream != null)
                {
                    inStream.Close();
                }
            }
        }
    }

}
