export interface MetaInfo {
  tag: string;
  list: string;
  overview: string;
}

export interface MenuInfo {
  tag?: string;
  link?: string;
  title?: string;
  icon?: string;
  children?: MenuInfo[];
}

export interface DialogInfo {
  create?: DialogObjectInfo[];
  edit?: DialogObjectInfo[];
}

export interface DialogObjectInfo {
  tag?: string;
  component?: string;
}
