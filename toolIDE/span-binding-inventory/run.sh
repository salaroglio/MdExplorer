#!/usr/bin/env bash
# Inventario delle chiamate che C# 14 lega a System.MemoryExtensions — controllo prima di un rilascio (sprint .NET 10, D2).
#
# Uso: toolIDE/span-binding-inventory/run.sh [--lang 14|13|default] [--out <file>]
#   --lang 14       (predefinito) forza C# 14 su TUTTI i progetti: un sovrainsieme prudente, anche di quelli che oggi
#                   compilano con un C# più vecchio.
#   --lang default  la versione che ogni progetto usa davvero (da F3 in poi, net10.0 → C# 14).
# Uscita: 0 = nessuna riga «in query: sì»; 1 = ce n'è almeno una (da correggere: l'array diventa lista prima della query);
#         2 = analisi incompleta (progetto non caricato, errori di compilazione): l'elenco NON vale come «zero».
#
# Esclusi, sempre e per nome (verificato il 18/11/2026): non si compilano su Linux e non compileranno mai con C# 14,
# perché non dichiarano LangVersion e il loro framework non cambia (C# predefinito 7.3).
#   WebViewControl/WebViewControl.csproj                                                  net472, senza reference assembly su Linux
#   Ad.Tools.FluentMigrator/Ad.Tools.FluentMigrator.UnitTest/Ad.Tools.FluentMigrator.UnitTest.csproj   progetto vecchio stile, .NET Framework 4.7.2
#   Ad.Tools.Dal/Ad.Tools.Dal.UnitTest/Ad.Tools.Dal.UnitTest.csproj                       vuole il pacchetto Ad.Tools.FluentMigrator da un feed che non c'è
#   Ad.Tools.Dal.UnitTest/Ad.Tools.Dal.UnitTest.csproj                                    netcoreapp3.1, NON compila già col suo C# (5 errori: Ad.Tools.Dal.Evo, Mock<>)
# Se uno di questi un giorno dichiara LangVersion o cambia framework, va tolto da qui.
set -euo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"
REPO="$(git -C "$HERE" rev-parse --show-toplevel)"
for p in WebViewControl/WebViewControl.csproj \
         Ad.Tools.FluentMigrator/Ad.Tools.FluentMigrator.UnitTest/Ad.Tools.FluentMigrator.UnitTest.csproj \
         Ad.Tools.Dal/Ad.Tools.Dal.UnitTest/Ad.Tools.Dal.UnitTest.csproj \
         Ad.Tools.Dal.UnitTest/Ad.Tools.Dal.UnitTest.csproj; do
  if grep -q "LangVersion" "$REPO/$p"; then echo "ERRORE: $p ora dichiara LangVersion: non si può più escludere" >&2; exit 2; fi
done
dotnet build "$HERE/SpanBindingInventory.csproj" -v q -nologo > /dev/null
exec dotnet run --no-build --project "$HERE/SpanBindingInventory.csproj" -- --repo "$REPO" \
  --exclude WebViewControl/WebViewControl.csproj \
  --exclude Ad.Tools.FluentMigrator/Ad.Tools.FluentMigrator.UnitTest/Ad.Tools.FluentMigrator.UnitTest.csproj \
  --exclude Ad.Tools.Dal/Ad.Tools.Dal.UnitTest/Ad.Tools.Dal.UnitTest.csproj \
  --exclude Ad.Tools.Dal.UnitTest/Ad.Tools.Dal.UnitTest.csproj \
  "$@"
