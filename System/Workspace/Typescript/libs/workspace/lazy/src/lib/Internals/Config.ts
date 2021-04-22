export class Hierarchy {
  directSupertypesBySubtype: number[][];

  directSubtypesBySupertype: number[][];

  add(subtype: number, directSuptertypes: number[]) {
    this.directSupertypesBySubtype[subtype] = directSuptertypes;
    directSuptertypes.forEach((v) => {
      const directSubtypes = this.directSubtypesBySupertype[v];
      if (!directSubtypes) {
        this.directSubtypesBySupertype[v] = [subtype];
      } else {
        directSubtypes.push(v);
      }
    });
  }
}
