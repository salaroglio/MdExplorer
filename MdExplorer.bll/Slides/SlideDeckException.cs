using System;

namespace MdExplorer.Features.Slides
{
    /// <summary>
    /// A slide deck that cannot be rendered as written. The message says what to change in the
    /// file: it is shown to the author instead of the presentation.
    /// </summary>
    public sealed class SlideDeckException : Exception
    {
        public SlideDeckException(string message) : base(message)
        {
        }

        public SlideDeckException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
