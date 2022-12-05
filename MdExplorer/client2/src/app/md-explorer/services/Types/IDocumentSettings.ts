
export interface IDocumentSettings {
  author: string
  documentType: string
  email: string
  title: string
  date: string
  wordSection:IWordSection
}


export interface IWordSection {
  templateSection: IWordTemplateRef
  writeToc: boolean
  documentHeader:string
}

export interface IWordTemplateRef {
  inheritFromTemplate: string // name of template inside dedicated directory
  customTemplate: string // name of template inside the /assets current directory
  templateType:string //inherit,custom
}
