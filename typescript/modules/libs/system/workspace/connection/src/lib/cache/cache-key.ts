/**
 * What a cached view of the database belongs to: the database, the user, the workspace
 * name and the fingerprint of the meta population. A connection learns the first two from
 * the server's first response; the other two are its own. A persistence provider keeps the
 * entries of each key apart.
 */
export class CacheKey {
  constructor(
    public readonly databaseId: string,
    public readonly userId: number,
    public readonly workspaceName: string,
    public readonly metaFingerprint: string
  ) {
    if (databaseId == null) {
      throw new Error('A cache key needs the id of the database.');
    }

    if (userId == null) {
      throw new Error('A cache key needs the id of the user.');
    }

    if (workspaceName == null) {
      throw new Error('A cache key needs the name of the workspace.');
    }

    if (metaFingerprint == null) {
      throw new Error('A cache key needs the fingerprint of the meta population.');
    }
  }

  equals(other: CacheKey | null | undefined): boolean {
    return (
      other != null &&
      this.databaseId === other.databaseId &&
      this.userId === other.userId &&
      this.workspaceName === other.workspaceName &&
      this.metaFingerprint === other.metaFingerprint
    );
  }

  toString(): string {
    return `${this.databaseId}/${this.userId}/${this.workspaceName}/${this.metaFingerprint}`;
  }
}
