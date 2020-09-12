export type Composite = string;
export type Unit = string | Date | boolean | number;
export type Association = Composite | Set<Composite>
export type Role = Unit | Composite | Set<Composite>