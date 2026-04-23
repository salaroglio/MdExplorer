import { Participant } from './participant';

export class MdProject {
  id: string
  name: string
  description?: string
  path: string
  sidenavWidth: number
  lastUpdate: Date
  participants?: Participant[]
}
