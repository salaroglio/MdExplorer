export interface SearchRequest {
  searchTerm: string;
  searchType?: SearchType;
  maxResults?: number;
}

export enum SearchType {
  All = 'All',
  Files = 'Files',
  Links = 'Links'
}

export interface SearchResult {
  files: FileSearchResult[];
  links: LinkSearchResult[];
  totalFiles: number;
  totalLinks: number;
  searchTerm: string;
  searchDurationMs: number;
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