export interface Request {
  /** A tracing string the server appends to its events */
  x?: string;

  /**
   * The name of the workspace the client is built for; the server refuses a name that
   * differs from the one it serves. Absent when the client does not say.
   */
  _w?: string;

  /**
   * The fingerprint of the client's workspace meta; the server refuses a fingerprint that
   * differs from its own. Absent when the client does not say.
   */
  _f?: string;
}
