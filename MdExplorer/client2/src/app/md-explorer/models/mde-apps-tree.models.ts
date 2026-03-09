import { MdeAppDefinition } from '../services/external-apps.service';

export interface MdeTreeNode {
  type: 'category' | 'app';
  id?: string;
  appId?: string;
  name?: string;
  icon?: string;
  children?: MdeTreeNode[];
}

export interface MdeAppsConfig {
  version: string;
  apps: MdeAppDefinition[];
  tree: MdeTreeNode[];
}
