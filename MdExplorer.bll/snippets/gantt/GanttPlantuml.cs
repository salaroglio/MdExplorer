using MdExplorer.Features.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.snippets.gantt
{
    public class GanttPlantuml : ISnippet<DictionarySnippetParam>
    {
        public int Id => 4;

        public string Name => "Gantt";

        public string Group => "Plantuml";

        public string GetSnippet(DictionarySnippetParam parameters = null)
        {
            var toReturn = processTemplate();
            return toReturn;
        }

        private string processTemplate()
        {
            var text = Helper.ExtractResFileString("MdExplorer.Features.snippets.gantt.gantt.plantuml");
            text = text.Replace("__current_year__",DateTime.Now.Year.ToString());
            text = text.Replace("__day_of_week__", StartOfWeek(DayOfWeek.Monday).Day.ToString());
            return text;
        }

        public void SetAssets(string assetsPath)
        {
            // Nothing to do
        }

        private DateTime StartOfWeek(DayOfWeek startOfWeek)
        {
            var dt = DateTime.Now;
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff);
        }
    }
}
