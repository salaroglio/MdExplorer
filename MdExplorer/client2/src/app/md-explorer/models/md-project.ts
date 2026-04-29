import { Participant } from './participant';

export class MdProject {
  id: string
  name: string
  description?: string
  path: string
  sidenavWidth: number
  lastUpdate: Date
  participants?: Participant[]
  // Custom icon flags coming from .development.yml. iconUpdatedAt is appended
  // as ?v= query param when fetching the PNG so the browser doesn't show a
  // stale cached image after the user re-saves the icon.
  hasCustomIcon?: boolean
  iconUpdatedAt?: string
}
