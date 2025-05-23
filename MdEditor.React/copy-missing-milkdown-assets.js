const fs = require('fs');
const path = require('path');

const projectRoot = process.cwd(); // Assumes script is run from MdEditor.React directory

const sourceFilePath = path.join(projectRoot, 'node_modules/@milkdown/prose/lib/style/gapcursor.css');
const destinationFilePath = path.join(projectRoot, 'node_modules/@milkdown/crepe/lib/theme/common/@milkdown/kit/prose/gapcursor/style/@milkdown/prose/gapcursor/style/gapcursor.css');

const destinationDir = path.dirname(destinationFilePath);

try {
  if (fs.existsSync(sourceFilePath)) {
    if (!fs.existsSync(destinationDir)) {
      fs.mkdirSync(destinationDir, { recursive: true });
      console.log(`Created directory: ${destinationDir}`);
    }
    fs.copyFileSync(sourceFilePath, destinationFilePath);
    console.log(`Successfully copied ${sourceFilePath} to ${destinationFilePath}`);
  } else {
    console.warn(`Source file not found: ${sourceFilePath}. Cannot copy.`);
  }
} catch (error) {
  console.error(`Error during postinstall copy: ${error}`);
  // Exit with error code so npm knows postinstall failed, if critical
  // process.exit(1); 
}
