using MdExplorer.Features.Exports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Tests
{
    [TestClass]
    public class WordFeaturesShould
    {
        [TestMethod]
        public void Merge_two_documents()
        {
            var currentPath = AppContext.BaseDirectory; ;
            var sourceFolderPath = $"{currentPath}WordTests{Path.DirectorySeparatorChar}FilesDocx";
            string firstPageFile = Path.Combine($"{sourceFolderPath}{Path.DirectorySeparatorChar}PrimaPaginaTitolo.docx");
            string mainFile = Path.Combine($"{sourceFolderPath}{Path.DirectorySeparatorChar}testobase.docx");
            string destinationFile = Path.Combine($"{sourceFolderPath}{Path.DirectorySeparatorChar}test1.docx");

            var wordFeatures = new WordFeatures();
            wordFeatures.MergeDocuments(new string[]{ firstPageFile, mainFile}, destinationFile);


        }
    }
}
