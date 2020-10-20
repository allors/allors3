export type Unit = string | Date | boolean | number;
export type Composite = string;
export type Composites = string[];
export type Association = Composite | Composites
export type Role = Unit | Composite | Composites