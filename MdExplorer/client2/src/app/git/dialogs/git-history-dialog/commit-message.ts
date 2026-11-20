/**
 * Un messaggio di commit diviso come lo scrive la finestra del commit (19/11/2026): la testata, poi il corpo. Il corpo è
 * fatto di punti «- …» (una riga per attività; un punto lungo va a capo rientrando di due spazi) e, nei commit scritti a
 * mano o più vecchi, di paragrafi andati a capo a 72 colonne.
 */
/** Un pezzo del corpo, nell'ordine in cui è scritto: un paragrafo oppure un elenco di punti. */
export interface CommitMessageBlock {
  /** Il paragrafo, con le sue righe riunite. */
  text?: string;
  /** I punti, con le righe di continuazione riunite al loro punto. */
  points?: string[];
}

export interface CommitMessageParts {
  /** La prima riga: quella che si legge nell'elenco. */
  title: string;
  /** Il corpo, paragrafi ed elenchi nell'ordine del messaggio. */
  blocks: CommitMessageBlock[];
}

/** Le righe finali che git aggiunge per chi ha firmato o collaborato: non sono il racconto del commit. */
const TRAILER = /^(co-authored-by|signed-off-by|reviewed-by|claude-session|change-id):\s/i;
const BULLET = /^\s*[-*•]\s+(.*)$/;

export function splitCommitMessage(message: string | null | undefined): CommitMessageParts {
  const lines = (message || '').replace(/\r\n?/g, '\n').split('\n');
  let i = 0;
  while (i < lines.length && !lines[i].trim()) i++;
  const title = (lines[i] || '').trim();

  const blocks: CommitMessageBlock[] = [];
  let paragraph: string[] = [];
  let list: string[] | null = null;
  const closeParagraph = () => { if (paragraph.length) { blocks.push({ text: paragraph.join(' ') }); paragraph = []; } };
  const closeList = () => { if (list) { blocks.push({ points: list }); list = null; } };

  for (const raw of lines.slice(i + 1)) {
    const line = raw.trimEnd();
    if (!line.trim()) { closeParagraph(); continue; } // una riga vuota fra due punti non chiude l'elenco
    if (TRAILER.test(line.trim())) continue;
    const bullet = BULLET.exec(line);
    if (bullet) {
      closeParagraph();
      (list ??= []).push(bullet[1].trim());
    } else if (list && /^\s/.test(line)) {
      // la continuazione di un punto lungo, rientrata sotto il suo testo
      list[list.length - 1] += ' ' + line.trim();
    } else {
      closeList();
      paragraph.push(line.trim());
    }
  }
  closeParagraph();
  closeList();
  return { title, blocks };
}
