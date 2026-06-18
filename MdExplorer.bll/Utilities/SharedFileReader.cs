using System.IO;
using System.Text;

namespace MdExplorer.Features.Utilities
{
    /// <summary>
    /// Legge i file di progetto SENZA bloccare i writer esterni.
    ///
    /// `File.ReadAllText` apre con `FileShare.Read`: su Windows questo vieta a un
    /// processo esterno (es. un'AI che edita il .md sul filesystem) di aprire il
    /// file in scrittura mentre MDE lo legge → l'Ai ritenta in loop → hang.
    ///
    /// Qui apriamo con `FileShare.ReadWrite | FileShare.Delete`:
    ///  - ReadWrite: un altro processo può scrivere mentre MDE legge;
    ///  - Delete: un altro processo può rinominare/cancellare il file mentre MDE
    ///    lo tiene aperto — necessario per i tool che salvano in modo ATOMICO
    ///    (scrivono un temp e lo rinominano sopra l'originale).
    ///
    /// La lettura è read-only e l'handle resta aperto solo per il tempo della
    /// lettura. Una lettura "torn" (file scritto a metà) produce un fingerprint
    /// che non combacia e viene riprocessata alla run successiva: comportamento
    /// già previsto dai chiamanti, molto meno dannoso di un hang.
    /// </summary>
    public static class SharedFileReader
    {
        public static string ReadAllText(string path)
        {
            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            return reader.ReadToEnd();
        }
    }
}
