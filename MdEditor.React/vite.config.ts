import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'
import fs from 'fs'

// Helper function to ensure directory exists (can be kept as it's generally useful)
function ensureDirSync(dirPath: string) {
  if (!fs.existsSync(dirPath)) {
    fs.mkdirSync(dirPath, { recursive: true });
  }
}

// Original logic for copying main CSS files
function originalCopyCssPlugin() {
  return {
    name: 'copy-css-files', // Original name
    closeBundle() {
      console.log('[vite-plugin-copy-css] Starting copy of main CSS files...');
      const mainCssSource = 'public/css/milkdown-all.css';

      if (fs.existsSync(mainCssSource)) {
        // For dist/
        ensureDirSync('dist/css');
        fs.copyFileSync(mainCssSource, 'dist/css/milkdown-all.css');
        console.log(`[vite-plugin-copy-css] Copied ${mainCssSource} to dist/css/milkdown-all.css`);
        
        // For very-simple-test.html convenience (if served from root)
        fs.copyFileSync(mainCssSource, 'milkdown-all.css');
        console.log(`[vite-plugin-copy-css] Copied ${mainCssSource} to project root/milkdown-all.css`);
      } else {
        console.warn(`[vite-plugin-copy-css] Main CSS file ${mainCssSource} not found.`);
      }
      console.log('[vite-plugin-copy-css] Finished copying main CSS files.');
    }
  };
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    originalCopyCssPlugin() // Use the reverted plugin
  ],
  build: {
    lib: {
      entry: path.resolve(process.cwd(), 'src/integration.ts'),
      name: 'DocsPilot',
      fileName: (format) => `docspilot.${format}.js`,
      formats: ['es', 'umd'],
    },
    rollupOptions: {
      external: ['react', 'react-dom', 'react/jsx-runtime'],
      output: {
        globals: {
          react: 'React',
          'react-dom': 'ReactDOM',
          'react/jsx-runtime': 'jsxRuntime',
        },
        assetFileNames: (assetInfo) => {
          // This was in the original, keep it.
          // It handles CSS imported by JS/TS and processed by Vite.
          if (assetInfo.name && assetInfo.name.endsWith('.css')) {
            return 'css/[name][extname]';
          }
          return 'assets/[name][extname]';
        }
      }
    },
    emptyOutDir: false, 
  },
});
