using System;
using System.Collections.Generic;
using System.IO;
using MdExplorer.Features.Templates;

namespace MdExplorer.Services.Templates
{
    /// <summary>L'elenco dei template che viaggia dentro MdExplorer (<c>project-templates.json</c>, P4), letto con tolleranza zero.</summary>
    public static class TemplateCatalogResource
    {
        public static IReadOnlyList<TemplateCatalogEntry> Entries()
        {
            using var stream = typeof(TemplateCatalogResource).Assembly.GetManifestResourceStream(TemplateCatalog.ResourceName)
                ?? throw new InvalidOperationException($"MdExplorer non contiene {TemplateCatalog.ResourceName}: la build è incompleta.");
            using var reader = new StreamReader(stream);
            return TemplateCatalog.Read(reader.ReadToEnd());
        }
    }
}
