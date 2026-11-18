using System;
using System.Text;

namespace MdExplorer.Features.Services.Ocr
{
    /// <summary>
    /// Il ragionamento di un modello arriva a frammenti di poche lettere: qui diventa righe da mostrare. Una riga
    /// esce quando il modello va a capo, chiude una frase lunga o si avvicina a <see cref="MaxLine"/> caratteri; il resto esce con
    /// <see cref="Flush"/>, a domanda finita.
    /// </summary>
    public sealed class ThinkingLines
    {
        public const int MaxLine = 160;

        private readonly Action<string> _emit;
        private readonly StringBuilder _buffer = new StringBuilder();

        public ThinkingLines(Action<string> emit)
        {
            _emit = emit;
        }

        public void Add(string fragment)
        {
            foreach (var c in fragment ?? string.Empty)
            {
                if (c == '\n' || c == '\r')
                {
                    Flush();
                    continue;
                }
                _buffer.Append(c);
                // Una frase finita, o una riga troppo lunga che si può spezzare a uno spazio.
                if ((_buffer.Length >= MaxLine / 2 && (c == '.' || c == '!' || c == '?' || c == ':' || c == ';'))
                    || (_buffer.Length >= MaxLine * 3 / 4 && c == ' '))
                {
                    Flush();
                }
            }
        }

        public void Flush()
        {
            var line = _buffer.ToString().Trim().Trim('*', '#', ' ');
            _buffer.Clear();
            if (line.Length > 0)
            {
                _emit(line);
            }
        }
    }
}
