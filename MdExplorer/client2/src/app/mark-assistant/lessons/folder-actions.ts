import { MarkFolderContext, MarkLesson } from '../mark-types';

/**
 * Folder-actions lesson — shown when the user summons Mark from a folder's
 * context menu in md-tree. Mark presents a context-aware menu of things he
 * can do with that folder.
 *
 * Today there is a single action ("Riassumi documentazione"); the lesson is
 * built so more actions are just extra entries in the `actions` array.
 *
 * Built via factory: the action handler needs to call back into
 * MarkAssistantService (to start the job) and must carry the folder context,
 * so it can't be a plain constant.
 */
export function buildFolderActions(
  ctx: MarkFolderContext,
  deps: { runSummarize: () => void | Promise<void> },
): MarkLesson {
  return {
    id: 'folder-actions',
    context: 'always',
    withStatic: false,   // the user invoked Mark on purpose — no "krsshhh"
    markAsCompleted: false,
    dim: false,          // don't darken the whole tree for a context action
    steps: [
      {
        textKey: 'MARK.FOLDER.PROMPT',
        textParams: { name: ctx.folderName },
        targetSelector: null,
        actions: [
          {
            labelKey: 'MARK.FOLDER.SUMMARIZE',
            icon: '📚',
            handler: () => deps.runSummarize(),
          },
        ],
      },
    ],
  };
}
