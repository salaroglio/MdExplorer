export interface IFileInfoNode {
    name: string;
    path: string;
    type: string;
    childrens?: IFileInfoNode[];
}
