using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Configuration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public interface ICommand
    {
        //string ServerAddress { get; set; }
        public int Priority { get; set; }
        public bool Enabled { get; set; }

        /// <summary>
        /// List of compatibility modes this command supports.
        /// If null or empty, command is assumed to support all modes.
        /// </summary>
        List<CompatibilityMode> SupportedModes { get; }

        /// <summary>
        /// Whether the command runs on a slide deck (<see cref="RequestInfo.SlideDeck"/>): its output
        /// must work in a reveal.js page, without the document view's scripts. Off unless a command
        /// says otherwise, so a new command does not reach the slides untried.
        /// </summary>
        bool WorksInSlides => false;

        string TransformInNewMDFromMD(string markdown,RequestInfo requestInfo);
        string TransformAfterConversion(string html, RequestInfo requestInfo);
        string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo);

        MatchCollection GetMatches(string markdown);
        string Name { get; set; }
    }
}
