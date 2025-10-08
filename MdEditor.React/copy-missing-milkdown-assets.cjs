const fs = require('fs');
const path = require('path');

const projectRoot = process.cwd(); // Assumes script is run from MdEditor.React directory

console.log('[postinstall-script] Starting asset copy process...');

const assetsToCopy = [
  // --- gapcursor.css ---
  {
    source: path.join(projectRoot, 'node_modules/@milkdown/prose/lib/style/gapcursor.css'),
    dest: path.join(projectRoot, 'node_modules/@milkdown/crepe/lib/theme/common/@milkdown/kit/prose/gapcursor/style/@milkdown/prose/gapcursor/style/gapcursor.css'),
    id: 'gapcursor (complex)'
  },
  // --- tables.css (original complex path from previous errors) ---
  {
    source: path.join(projectRoot, 'node_modules/@milkdown/crepe/lib/theme/common/@milkdown/kit/prose/tables/style/tables.css'),
    dest: path.join(projectRoot, 'node_modules/@milkdown/crepe/lib/theme/common/@milkdown/kit/prose/tables/style/@milkdown/prose/tables/style/tables.css'),
    id: 'tables (complex)'
  },
  // --- prosemirror.css (triple-nested path from previous errors) ---
  {
    source: path.join(projectRoot, 'node_modules/@milkdown/crepe/lib/theme/common/@milkdown/kit/prose/view/style/prosemirror.css'),
    dest: path.join(projectRoot, 'node_modules/@milkdown/crepe/lib/theme/common/@milkdown/kit/prose/view/style/@milkdown/prose/view/style/@milkdown/prose/view/style/prosemirror.css'),
    id: 'prosemirror (triple-nested)'
  },
  // --- tables.css (triple-nested path from previous errors) ---
  {
    source: path.join(projectRoot, 'node_modules/@milkdown/crepe/lib/theme/common/@milkdown/kit/prose/tables/style/tables.css'),
    dest: path.join(projectRoot, 'node_modules/@milkdown/crepe/lib/theme/common/@milkdown/kit/prose/tables/style/@milkdown/prose/tables/style/@milkdown/prose/tables/style/tables.css'),
    id: 'tables (triple-nested)'
  },
  // --- prosemirror.css (NEW quadruple-nested path) ---
  {
    source: path.join(projectRoot, 'node_modules/@milkdown/crepe/lib/theme/common/@milkdown/kit/prose/view/style/prosemirror.css'), // Assumed canonical source
    dest: path.join(projectRoot, 'node_modules/@milkdown/crepe/lib/theme/common/@milkdown/kit/prose/view/style/@milkdown/prose/view/style/@milkdown/prose/view/style/@milkdown/prose/view/style/prosemirror.css'),
    id: 'prosemirror (quadruple-nested)'
  },
  // --- tables.css (NEW quadruple-nested path) ---
  {
    source: path.join(projectRoot, 'node_modules/@milkdown/crepe/lib/theme/common/@milkdown/kit/prose/tables/style/tables.css'), // Assumed canonical source
    dest: path.join(projectRoot, 'node_modules/@milkdown/crepe/lib/theme/common/@milkdown/kit/prose/tables/style/@milkdown/prose/tables/style/@milkdown/prose/tables/style/@milkdown/prose/tables/style/tables.css'),
    id: 'tables (quadruple-nested)'
  }
];

try {
  assetsToCopy.forEach(asset => {
    const destinationDir = path.dirname(asset.dest);
    if (fs.existsSync(asset.source)) {
      if (!fs.existsSync(destinationDir)) {
        fs.mkdirSync(destinationDir, { recursive: true });
        console.log(`[postinstall-script] Created directory for ${asset.id}: ${destinationDir}`);
      }
      fs.copyFileSync(asset.source, asset.dest);
      console.log(`[postinstall-script] Successfully copied for ${asset.id}: ${asset.source} to ${asset.dest}`);
    } else {
      console.warn(`[postinstall-script] Source file not found for ${asset.id}: ${asset.source}. Cannot copy.`);
    }
  });
} catch (error) {
  console.error(`[postinstall-script] Error during copy: ${error}`);
  // process.exit(1); 
}

console.log('[postinstall-script] Finished asset copy process.');
