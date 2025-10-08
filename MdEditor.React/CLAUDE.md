# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Architecture Overview

MdEditor.React is a Milkdown-based Markdown editor WebComponent that integrates with the MdExplorer ecosystem. It's built with React 19 and TypeScript, packaged as a reusable WebComponent.

### Core Components

* **WebComponent Implementation** (`src/integration.ts`) - Exports `DocsPilotElement` custom element
* **Build System** - Vite with custom CSS asset management plugins
* **Editor** - Milkdown v7.6.4+ with Crepe theme and styled-components
* **Output Formats** - ES modules and UMD bundle for flexible integration

### Integration Architecture

The editor is designed as a WebComponent (`<docs-pilot>`) that:
* Accepts markdown content via `markdown` attribute
* Emits `markdownChange` events when content changes
* Encapsulates styles using Shadow DOM
* Handles CSS path resolution for deployment within MdExplorer

## Common Development Commands

```Shell
# Install dependencies
npm install

# Development server with hot reload
npm run dev

# Production build
npm run build

# Preview production build
npm run preview

# Run ESLint
npm run lint
```

## Build Configuration

The project uses custom Vite configuration to handle Milkdown CSS bundling:
* CSS assets are copied from node_modules during build
* Deployment CSS references are adjusted for MdExplorer's structure
* Multiple output formats for broad compatibility

## Testing

Test the WebComponent integration using `very-simple-test.html` which demonstrates:
* Loading the component
* Setting markdown content
* Listening to change events

## CSS Management

Milkdown styles require special handling:
* Development: CSS loaded from `public/css/milkdown-all.css`
* Production: CSS bundled and path-adjusted in `milkdown-all-deploy.css`
* Custom build scripts (`copy-missing-milkdown-assets.js`) manage asset copying