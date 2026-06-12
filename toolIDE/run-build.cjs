#!/usr/bin/env node
// Cross-platform build dispatcher: runs <name>.ps1 on Windows, <name>.sh elsewhere.
// Usage: node run-build.cjs <script-name>
const { spawnSync } = require('child_process');
const path = require('path');

const name = process.argv[2];
if (!name) {
  console.error('Usage: node run-build.cjs <script-name>');
  process.exit(1);
}

const isWin = process.platform === 'win32';
const script = path.join(__dirname, name + (isWin ? '.ps1' : '.sh'));

const result = isWin
  ? spawnSync('powershell', ['-ExecutionPolicy', 'Bypass', '-File', script], { stdio: 'inherit' })
  : spawnSync('bash', [script], { stdio: 'inherit' });

if (result.error) {
  console.error(result.error.message);
  process.exit(1);
}
process.exit(result.status === null ? 1 : result.status);
