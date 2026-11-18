using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using MdExplorer.Features.Services.Ocr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Ocr
{
    [TestClass]
    public class WordAttachments_Should
    {
        private string _folder;

        [TestInitialize]
        public void Setup()
        {
            _folder = Path.Combine(Path.GetTempPath(), "mde-attach-" + Path.GetRandomFileName());
            Directory.CreateDirectory(_folder);
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(_folder, true);

        [TestMethod]
        public void ReadBigAndSmallStreamsOfACompoundFile()
        {
            var big = Enumerable.Range(0, 5000).Select(i => (byte)(i % 251)).ToArray();
            var small = Encoding.ASCII.GetBytes("un flusso piccolo, che sta nel mini flusso: settanta caratteri e passa.");

            var file = new CompoundFile(Compound(("CONTENTS", big), ("piccolo", small)));

            CollectionAssert.AreEqual(big, file.Stream("contents"));
            CollectionAssert.AreEqual(small, file.Stream("piccolo"));
            Assert.IsNull(file.Stream("manca"));
            CollectionAssert.AreEquivalent(new[] { "CONTENTS", "piccolo" }, file.StreamNames.ToArray());
        }

        [TestMethod]
        public void RefuseWhatIsNotACompoundFile_AndABrokenOne()
        {
            Assert.ThrowsException<InvalidDataException>(() => new CompoundFile(new byte[600]));

            var broken = Compound(("CONTENTS", new byte[5000]));
            BitConverter.GetBytes(0xFFFFFFFFu).CopyTo(broken, 512 + 5 * 4);   // la catena del flusso si interrompe
            Assert.ThrowsException<InvalidDataException>(() => new CompoundFile(broken).Stream("CONTENTS"));
        }

        [TestMethod]
        public void TakeTheRealFileOutOfItsContainer()
        {
            var pdf = Padded("%PDF-1.7 finto");
            var excel = Office("xl/workbook.xml");

            var fromAcrobat = WordAttachments.Unwrap(Compound(("CONTENTS", pdf)), "Acrobat.Document.DC");
            var fromExcel = WordAttachments.Unwrap(Compound(("Package", excel)), "Excel.Sheet.12");
            var bare = WordAttachments.Unwrap(Office("word/document.xml", "word/vbaProject.bin"), "Word.Document.12");
            var generic = WordAttachments.Unwrap(Compound(("\u0001Ole10Native", Native("nota.msg", @"C:\Users\mario.rossi\nota.msg", Padded("contenuto")))), "Package");

            Assert.AreEqual(".pdf", fromAcrobat.Extension);
            CollectionAssert.AreEqual(pdf, fromAcrobat.Content);
            Assert.AreEqual(".xlsx", fromExcel.Extension);
            CollectionAssert.AreEqual(excel, fromExcel.Content);
            Assert.AreEqual(".docm", bare.Extension);
            Assert.AreEqual(".msg", generic.Extension);
            Assert.AreEqual("nota.msg", generic.OriginalName);
            CollectionAssert.AreEqual(Padded("contenuto"), generic.Content);
        }

        [TestMethod]
        public void SayWhatItCannotOpen()
        {
            StringAssert.Contains(Assert.ThrowsException<InvalidDataException>(
                () => WordAttachments.Unwrap(Encoding.ASCII.GetBytes("non so cosa sia, ma non è niente di noto"), "Strano.Oggetto")).Message, "Strano.Oggetto");
            StringAssert.Contains(Assert.ThrowsException<InvalidDataException>(
                () => WordAttachments.Unwrap(Compound(("Altro", new byte[5000])), "Strano.Oggetto")).Message, "Strano.Oggetto");
        }

        [TestMethod]
        public void PutALinkWhereTheIconWas_AndTheFileNextToTheImages()
        {
            var pdf = Padded("%PDF-1.7 finto");
            var docx = Document(
                ("Acrobat.Document.DC", "Allegato PDF [Contratto]", Compound(("CONTENTS", pdf))),
                ("Excel.Sheet.12", "", Compound(("Package", Office("xl/workbook.xml")))));
            var images = Path.Combine(_folder, "img");

            var result = WordToMarkdownConverter.Convert(docx, images, "/img", (bytes, extension, png) => "non serve");

            Assert.AreEqual(2, result.Attachments);
            Assert.AreEqual(0, result.Images, "l'icona di un allegato estratto non è un'immagine del documento");
            StringAssert.Contains(result.Markdown, @"Vedi [📎 Allegato PDF \[Contratto\] (PDF)](/img/attach1.pdf) e");
            Assert.IsTrue(result.Markdown.Contains("[📎 Allegato 2 (XLSX)](/img/attach2.xlsx)") || result.Markdown.Contains("[📎 Attachment 2 (XLSX)](/img/attach2.xlsx)"), result.Markdown);
            Assert.IsFalse(result.Markdown.Contains("MDEATTACH"), result.Markdown);
            CollectionAssert.AreEqual(pdf, File.ReadAllBytes(Path.Combine(images, "attach1.pdf")));
            Assert.AreEqual(0, result.Warnings.Count, string.Join(" | ", result.Warnings));
        }

        [TestMethod]
        public void LeaveTheIconAndSayWhy_WhenAnAttachmentCannotBeOpened()
        {
            var docx = Document(("Strano.Oggetto", "Un oggetto", Encoding.ASCII.GetBytes("non so cosa sia, ma non è niente di noto")));
            var images = Path.Combine(_folder, "img");

            var result = WordToMarkdownConverter.Convert(docx, images, "/img", (bytes, extension, png) => "non serve");

            Assert.AreEqual(0, result.Attachments);
            Assert.AreEqual(1, result.Images);
            StringAssert.Contains(result.Markdown, "Vedi ![](/img/img1.png) e basta.");
            Assert.AreEqual(1, result.Warnings.Count(warning => warning.Contains("Strano.Oggetto")), string.Join(" | ", result.Warnings));
        }

        [TestMethod]
        public void ReadTheFileNameWordDrawsUnderTheIcon()
        {
            Assert.AreEqual("Contratto di fornitura 2026.pdf", WordAttachments.MetafileLabel(Icon("Contratto di ", "fornitura 20", "26.pdf")));
            Assert.AreEqual(string.Empty, WordAttachments.MetafileLabel(Icon()));
            Assert.AreEqual(string.Empty, WordAttachments.MetafileLabel(new byte[] { 0x89, 0x50, 0x4E, 0x47 }));
        }

        [TestMethod]
        public void NameTheLinkAfterTheIconLabel_WithoutRepeatingTheKindOfFile()
        {
            var docx = Document(("Acrobat.Document.DC", "", Compound(("CONTENTS", Padded("%PDF-1.7 finto")))));
            using (var zip = ZipFile.Open(docx, ZipArchiveMode.Update))
            {
                zip.GetEntry("word/media/image1.png").Delete();
                using (var stream = zip.CreateEntry("word/media/image1.png").Open())
                {
                    var icon = Icon("Contratto di ", "fornitura.pdf");
                    stream.Write(icon, 0, icon.Length);
                }
            }

            var result = WordToMarkdownConverter.Convert(docx, Path.Combine(_folder, "img"), "/img", (bytes, extension, png) => "non serve");

            StringAssert.Contains(result.Markdown, "[📎 Contratto di fornitura.pdf](/img/attach1.pdf)");
        }

        // Un'icona EMF minima: intestazione, un record «scrivi testo» per riga, fine.
        private static byte[] Icon(params string[] lines)
        {
            var file = new MemoryStream();
            var header = new byte[88];
            BitConverter.GetBytes(1u).CopyTo(header, 0);
            BitConverter.GetBytes(88u).CopyTo(header, 4);
            BitConverter.GetBytes(0x464D4520u).CopyTo(header, 40);
            file.Write(header, 0, 88);
            foreach (var line in lines)
            {
                var text = Encoding.Unicode.GetBytes(line);
                var size = (76 + text.Length + 3) / 4 * 4;
                var record = new byte[size];
                BitConverter.GetBytes(84u).CopyTo(record, 0);
                BitConverter.GetBytes((uint)size).CopyTo(record, 4);
                BitConverter.GetBytes((uint)line.Length).CopyTo(record, 44);
                BitConverter.GetBytes(76u).CopyTo(record, 48);
                text.CopyTo(record, 76);
                file.Write(record, 0, size);
            }
            var end = new byte[20];
            BitConverter.GetBytes(14u).CopyTo(end, 0);
            BitConverter.GetBytes(20u).CopyTo(end, 4);
            file.Write(end, 0, 20);
            return file.ToArray();
        }

        internal static byte[] Padded(string text)
        {
            var bytes = new byte[4500];
            Encoding.ASCII.GetBytes(text).CopyTo(bytes, 0);
            return bytes;
        }

        // Un file di Office finto: uno zip con le voci che ne dicono il tipo.
        internal static byte[] Office(params string[] entries)
        {
            using (var buffer = new MemoryStream())
            {
                using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
                {
                    foreach (var entry in entries.Concat(new[] { "[Content_Types].xml" }))
                    {
                        using (var stream = zip.CreateEntry(entry, CompressionLevel.NoCompression).Open())
                        {
                            stream.Write(new byte[2000], 0, 2000);
                        }
                    }
                }
                return buffer.ToArray();
            }
        }

        private static byte[] Native(string fileName, string path, byte[] content)
        {
            using (var buffer = new MemoryStream())
            using (var writer = new BinaryWriter(buffer, Encoding.Latin1))
            {
                writer.Write(0);
                writer.Write((ushort)2);
                writer.Write(Encoding.Latin1.GetBytes(fileName + "\0"));
                writer.Write(Encoding.Latin1.GetBytes(path + "\0"));
                writer.Write(0x00030000);
                writer.Write(path.Length + 1);
                writer.Write(Encoding.Latin1.GetBytes(path + "\0"));
                writer.Write(content.Length);
                writer.Write(content);
                writer.Flush();
                return buffer.ToArray();
            }
        }

        /// <summary>
        /// Un file composto minimo, settori da 512: 0 = tabella dei settori, 1 = elenco delle voci, 2 = mini tabella,
        /// 3 = mini flusso, dal 4 i flussi grandi. I flussi sotto i 4096 byte vanno nel mini flusso (al più 512 byte in tutto).
        /// </summary>
        internal static byte[] Compound(params (string Name, byte[] Content)[] streams)
        {
            const uint end = 0xFFFFFFFE, free = 0xFFFFFFFF;
            var fat = new List<uint> { 0xFFFFFFFD, end, end, end };
            var miniFat = new List<uint>();
            var mini = new MemoryStream();
            var data = new MemoryStream();
            var entries = new List<(string Name, byte Type, uint Start, int Size)>();

            foreach (var (name, content) in streams)
            {
                if (content.Length < 4096)
                {
                    var first = (uint)miniFat.Count;
                    var sectors = (content.Length + 63) / 64;
                    for (var i = 0; i < sectors; i++) miniFat.Add(i == sectors - 1 ? end : first + (uint)i + 1);
                    mini.Write(content, 0, content.Length);
                    mini.Write(new byte[sectors * 64 - content.Length], 0, sectors * 64 - content.Length);
                    entries.Add((name, 2, first, content.Length));
                }
                else
                {
                    var first = (uint)fat.Count;
                    var sectors = (content.Length + 511) / 512;
                    for (var i = 0; i < sectors; i++) fat.Add(i == sectors - 1 ? end : first + (uint)i + 1);
                    data.Write(content, 0, content.Length);
                    data.Write(new byte[sectors * 512 - content.Length], 0, sectors * 512 - content.Length);
                    entries.Add((name, 2, first, content.Length));
                }
            }
            Assert.IsTrue(mini.Length <= 512 && fat.Count <= 128 && entries.Count <= 3, "il file di prova è troppo grande per questo costruttore");
            entries.Insert(0, ("Root Entry", 5, 3, (int)mini.Length));

            var file = new MemoryStream();
            var header = new byte[512];
            new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }.CopyTo(header, 0);
            BitConverter.GetBytes((ushort)0x3E).CopyTo(header, 0x18);
            BitConverter.GetBytes((ushort)3).CopyTo(header, 0x1A);
            BitConverter.GetBytes((ushort)0xFFFE).CopyTo(header, 0x1C);
            BitConverter.GetBytes((ushort)9).CopyTo(header, 0x1E);
            BitConverter.GetBytes((ushort)6).CopyTo(header, 0x20);
            BitConverter.GetBytes(1u).CopyTo(header, 0x2C);
            BitConverter.GetBytes(1u).CopyTo(header, 0x30);
            BitConverter.GetBytes(4096u).CopyTo(header, 0x38);
            BitConverter.GetBytes(2u).CopyTo(header, 0x3C);
            BitConverter.GetBytes(1u).CopyTo(header, 0x40);
            BitConverter.GetBytes(end).CopyTo(header, 0x44);
            for (var i = 0; i < 109; i++) BitConverter.GetBytes(i == 0 ? 0u : free).CopyTo(header, 0x4C + i * 4);
            file.Write(header, 0, 512);

            byte[] Table(List<uint> values)
            {
                var sector = new byte[512];
                for (var i = 0; i < 128; i++) BitConverter.GetBytes(i < values.Count ? values[i] : free).CopyTo(sector, i * 4);
                return sector;
            }
            file.Write(Table(fat), 0, 512);

            var directory = new byte[512];
            for (var i = 0; i < entries.Count; i++)
            {
                var name = Encoding.Unicode.GetBytes(entries[i].Name + "\0");
                name.CopyTo(directory, i * 128);
                BitConverter.GetBytes((ushort)name.Length).CopyTo(directory, i * 128 + 0x40);
                directory[i * 128 + 0x42] = entries[i].Type;
                BitConverter.GetBytes(entries[i].Start).CopyTo(directory, i * 128 + 0x74);
                BitConverter.GetBytes((uint)entries[i].Size).CopyTo(directory, i * 128 + 0x78);
            }
            file.Write(directory, 0, 512);
            file.Write(Table(miniFat), 0, 512);
            var miniSector = new byte[512];
            mini.ToArray().CopyTo(miniSector, 0);
            file.Write(miniSector, 0, 512);
            data.WriteTo(file);
            return file.ToArray();
        }

        // Un Word con un paragrafo «Vedi <allegato> e <allegato>…»: ogni allegato è un oggetto incorporato con la sua icona.
        private string Document(params (string ProgId, string Title, byte[] Container)[] attachments)
        {
            return WriteDocument(Path.Combine(_folder, "allegati.docx"), attachments);
        }

        internal static string WriteDocument(string docx, params (string ProgId, string Title, byte[] Container)[] attachments)
        {
            const string relationships = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
            using (var zip = ZipFile.Open(docx, ZipArchiveMode.Create))
            {
                void Part(string name, string xml)
                {
                    using (var writer = new StreamWriter(zip.CreateEntry(name).Open()))
                    {
                        writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + xml);
                    }
                }
                void Binary(string name, byte[] bytes)
                {
                    using (var stream = zip.CreateEntry(name).Open())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }

                Part("[Content_Types].xml", "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
                    + "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>"
                    + "<Default Extension=\"png\" ContentType=\"image/png\"/><Default Extension=\"bin\" ContentType=\"application/vnd.openxmlformats-officedocument.oleObject\"/>"
                    + "<Default Extension=\"xml\" ContentType=\"application/xml\"/>"
                    + "<Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/></Types>");
                Part("_rels/.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
                    + $"<Relationship Id=\"rId1\" Type=\"{relationships}/officeDocument\" Target=\"word/document.xml\"/></Relationships>");

                var rels = new StringBuilder();
                var runs = new StringBuilder("<w:r><w:t xml:space=\"preserve\">Vedi </w:t></w:r>");
                for (var i = 0; i < attachments.Length; i++)
                {
                    var n = i + 1;
                    rels.Append($"<Relationship Id=\"rIcon{n}\" Type=\"{relationships}/image\" Target=\"media/image{n}.png\"/>");
                    rels.Append($"<Relationship Id=\"rOle{n}\" Type=\"{relationships}/oleObject\" Target=\"embeddings/oleObject{n}.bin\"/>");
                    runs.Append($"<w:r><w:object><v:shape id=\"s{n}\" style=\"width:76pt;height:49pt\"><v:imagedata r:id=\"rIcon{n}\" o:title=\"{attachments[i].Title}\"/></v:shape>"
                        + $"<o:OLEObject Type=\"Embed\" ProgID=\"{attachments[i].ProgId}\" ShapeID=\"s{n}\" DrawAspect=\"Icon\" ObjectID=\"_{n}\" r:id=\"rOle{n}\"/></w:object></w:r>");
                    runs.Append("<w:r><w:t xml:space=\"preserve\"> e </w:t></w:r>");
                    Binary($"word/media/image{n}.png", new byte[] { 0x89, 0x50, 0x4E, 0x47 });
                    Binary($"word/embeddings/oleObject{n}.bin", attachments[i].Container);
                }
                Part("word/_rels/document.xml.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" + rels + "</Relationships>");
                Part("word/document.xml", "<w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\""
                    + $" xmlns:r=\"{relationships}\" xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\">"
                    + "<w:body><w:p>" + runs + "<w:r><w:t>basta.</w:t></w:r></w:p></w:body></w:document>");
            }
            return docx;
        }
    }
}
