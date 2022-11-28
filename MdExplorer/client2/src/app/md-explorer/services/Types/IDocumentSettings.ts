
export interface IDocumentSettings {
  author: string
  documentType: string
  email: string
  title: string
  date: string
  wordSection:IWordSection
}


export interface IWordSection {
  writeToc: boolean
  documentHeader:string
}
