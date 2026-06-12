export interface SearchRequest {
  searchTerm: string;
  searchType?: SearchType;
  maxResults?: number;
}

export enum SearchType {
  All = 'All',
  Files = 'Files',
  Links = 'Links',
  Content = 'Content'
}

export interface SearchResult {
  files: FileSearchResult[];
  links: LinkSearchResult[];
  contents: ContentSearchResult[];
  totalFiles: number;
  totalLinks: number;
  totalContents: number;
  searchTerm: string;
  searchDurationMs: number;
}

export interface ContentSearchResult {
  markdownFileId: string;
  fileName: string;
  path: string;
  /** HTML-escaped excerpt with <mark>…</mark> around matches */
  snippet: string;
  /** bm25 score: lower is more relevant */
  score: number;
}

export interface FileSearchResult {
  id: string;
  fileName: string;
  path: string;
  fileType: string;
  matchedField: string;
  highlightedText: string;
}

export interface LinkSearchResult {
  id: string;
  path: string;
  fullPath: string;
  mdTitle: string;
  htmlTitle: string;
  mdContext: string;
  source: string;
  linkedCommand: string;
  markdownFileName: string;
  markdownFilePath: string;
  matchedField: string;
  highlightedText: string;
}