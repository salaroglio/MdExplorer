using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Commands.html
{
    public class FromExternalDataToPlantumlHtml : FromExternalDataToPlantuml, ICommandHtml
    {
        /// <summary>Runs on slide decks: plantuml(@json|@yaml) becomes a PlantUML diagram like any other.</summary>
        public bool WorksInSlides => true;

        public FromExternalDataToPlantumlHtml(ILogger<FromExternalDataToPlantuml> logger)
            : base(logger)
        {
        }
    }
}
