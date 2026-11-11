using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// L'SQL scritto nelle stringhe del codice (11/11/2026): tabelle lette e scritte, procedure chiamate, anche da un pezzo di
    /// frase; una frase per una persona non è SQL.
    /// </summary>
    [TestClass]
    public class EmbeddedSqlShould
    {
        private static string Of(string text) => string.Join(" ", EmbeddedSql.Analyze(text).Select(o => (o.Access switch { SqlAccess.Read => "r:", SqlAccess.Write => "w:", _ => "c:" }) + o.Name));
        private const string H = "\u0001";

        [TestMethod]
        public void Read_the_tables_of_a_query()
        {
            Assert.AreEqual("r:ORDINI", Of("SELECT * FROM ORDINI WHERE ID = ?"));
            Assert.AreEqual("r:ORDINI r:CLIENTI", Of("select o.id, c.nome from ORDINI o, CLIENTI c where o.cli = c.id"));
            Assert.AreEqual("r:GEST.ORDINI r:RIGHE r:ARTICOLI", Of("SELECT a.* FROM GEST.ORDINI o\n  INNER JOIN RIGHE r ON r.ord = o.id\n  LEFT OUTER JOIN \"ARTICOLI\" a ON a.id = r.art"));
            Assert.AreEqual("r:ORDINI r:STORICO", Of("SELECT x FROM (SELECT id x FROM STORICO) s, ORDINI"), "la sottoquery ha il suo FROM");
            Assert.AreEqual("r:ORDINI", Of("SELECT EXTRACT(YEAR FROM data), TRIM(' ' FROM nome) FROM ORDINI"), "FROM di una funzione non è una tabella");
            Assert.AreEqual("", Of("SELECT SYSDATE FROM DUAL"));
            Assert.AreEqual("r:ORDINI", Of("SELECT 'da FROM FINTA' FROM ORDINI -- FROM COMMENTO"), "né dentro un valore né in un commento");
            Assert.AreEqual("r:V_ORDINI r:V_STORICO", Of("WITH t AS (SELECT * FROM V_ORDINI) SELECT * FROM t UNION SELECT * FROM V_STORICO"), "il nome di WITH non è una tabella");
        }

        [TestMethod]
        public void Tell_a_write_from_a_read()
        {
            Assert.AreEqual("w:ORDINI", Of("INSERT INTO ORDINI (ID, CLI) VALUES (?, ?)"));
            Assert.AreEqual("w:STORICO r:ORDINI", Of("INSERT INTO STORICO SELECT * FROM ORDINI WHERE ANNO < ?"));
            Assert.AreEqual("w:ORDINI", Of("UPDATE ORDINI SET STATO = 'C' WHERE ID = :id"));
            Assert.AreEqual("w:ORDINI", Of("update ORDINI o set o.stato = 1"));
            Assert.AreEqual("w:ORDINI", Of("DELETE FROM ORDINI WHERE ID = ?"), "il FROM di DELETE è una scrittura");
            Assert.AreEqual("w:ORDINI r:NUOVI", Of("MERGE INTO ORDINI o USING NUOVI n ON (o.id = n.id) WHEN MATCHED THEN UPDATE SET o.x = n.x"));
            Assert.AreEqual("r:ORDINI", Of("SELECT * FROM ORDINI WHERE ID = ? FOR UPDATE NOWAIT"), "FOR UPDATE blocca, non scrive");
            Assert.AreEqual("w:TMP_ORDINI", Of("TRUNCATE TABLE TMP_ORDINI"));
        }

        [TestMethod]
        public void Find_the_procedures_called()
        {
            Assert.AreEqual("c:PKG_ORDINI.CHIUDI", Of("BEGIN PKG_ORDINI.CHIUDI(:id, :esito); END;"));
            Assert.AreEqual("c:PKG_ORDINI.TOTALE", Of("BEGIN :r := PKG_ORDINI.TOTALE(:id); END;"));
            Assert.AreEqual("c:PKG_ORDINI.ELENCO", Of("{ call PKG_ORDINI.ELENCO(?, ?) }"));
            Assert.AreEqual("c:dbo.sp_Ordini", Of("EXEC dbo.sp_Ordini @anno = 2020"));
        }

        [TestMethod]
        public void Read_a_piece_of_a_query_and_mark_a_name_that_comes_from_a_variable()
        {
            Assert.AreEqual("r:ORDINI", Of(" FROM ORDINI O "), "un pezzo aggiunto a una variabile");
            Assert.AreEqual("r:RIGHE", Of(" LEFT JOIN RIGHE R ON R.ORD = O.ID "));
            Assert.AreEqual("", Of(" WHERE O.CLIENTE = ? AND O.ANNO = ?"), "nessuna tabella: niente da dire");
            Assert.AreEqual("r:ORDINI", Of("SELECT * FROM ORDINI WHERE CLIENTE = " + H + " ORDER BY ID"), "la parte non scritta è un valore");
            var dynamic = EmbeddedSql.Analyze("SELECT * FROM " + H + " WHERE ID = ?").Single();
            Assert.AreEqual(("?", SqlAccess.Read, true), (dynamic.Name, dynamic.Access, dynamic.Dynamic), "il nome della tabella arriva da una variabile");
            Assert.AreEqual("w:?", Of("UPDATE " + H + " SET STATO = 1"));
        }

        [TestMethod]
        public void Leave_a_sentence_for_people_alone()
        {
            Assert.AreEqual("", Of("Select a file from the list."));
            Assert.AreEqual("", Of("Update available from server"));
            Assert.AreEqual("", Of("Delete the selected rows?"));
            Assert.AreEqual("", Of("Execute order now"));
            Assert.AreEqual("", Of("Errore di lettura from ORDINI"), "non comincia come una frase SQL");
            Assert.AreEqual("", Of("PKG_ORDINI.ELENCO"), "un nome solo è materia delle regole, non SQL");
            Assert.AreEqual("", Of("Insert coin to continue"));
        }
    }
}
