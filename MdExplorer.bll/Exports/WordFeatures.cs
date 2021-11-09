using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Exports
{
    /// <summary>
    /// Funzioni di esportazione in Word a fronte di un MD finale, già manipolato dalle altre features
    /// </summary>
    public class WordFeatures
    {
        public void MergeDocuments(string[] sources, string destination)
        {
           
            using (WordprocessingDocument myDoc = WordprocessingDocument.Open(destination, true))
            {
                MainDocumentPart mainPart = myDoc.MainDocumentPart;
                
                foreach (var item in sources)
                {
                    string altChunkId = "AltChunkId" + DateTime.Now.Ticks.ToString();
                    AlternativeFormatImportPart chunk =
                        mainPart.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.WordprocessingML, altChunkId);
                    using (FileStream fileStream = File.Open(item, FileMode.Open))
                    {
                        chunk.FeedData(fileStream);
                        AltChunk altChunk = new AltChunk();
                        altChunk.Id = altChunkId;
                        mainPart.Document
                            .Body
                            .InsertBefore(
                                altChunk, 
                                mainPart.Document.Body.Elements<Paragraph>().Last()
                            );
                        mainPart.Document.Save();
                    }
                }
            }
        }
    }
}
