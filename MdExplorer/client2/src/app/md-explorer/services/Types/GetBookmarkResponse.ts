export class GetBookmarkResponseDto {
  id: string
  fullPath: string
  name: string
  // Label resolved server-side: document title (front matter/H1) or file name,
  // with the parent folder appended when two bookmarks would look identical.
  displayName: string
  projectId: string
}
