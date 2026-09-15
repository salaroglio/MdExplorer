/**
 * Where a pasted screenshot goes in the markdown file: before or after a block of the document.
 *
 * Built by the backend (TriggerPasteWizard) from the block the user right-clicked, or the one
 * under the pointer at Ctrl+V, and carried unchanged through the wizard to the save — which
 * checks `expectedText` against the file again, because the wizard can stay open for minutes
 * while the file changes elsewhere. No anchor = at the end of the document.
 */
export interface PasteAnchor {
  /** 1-based lines of the block in the file (the source map's data-mde-line-start/end). */
  startLine: number;
  endLine: number;
  position: 'before' | 'after';
  /** The block's lines as they were at the right-click. */
  expectedText: string;
  /** The block's first line, shortened: what the wizard shows ("dopo «## Tabella»"). */
  label: string;
}
