import {
  decodeReadRequest,
  decodeReadResponse,
  ReadRequest,
  ReadResponse,
  validateReadRequest,
} from '@allors/system/common/protocol-json';

/** The raw response surface required from a standard fetch implementation. */
export interface ReadHttpResponse {
  readonly ok: boolean;
  readonly status: number;
  text(): Promise<string>;
}

/** The host supplies fetch and owns authentication and credential capture. */
export type ReadFetch = (
  url: string,
  init: RequestInit
) => Promise<ReadHttpResponse>;

/** An opt-in read transport; it neither retries nor updates a workspace or Session. */
export class ReadHttpTransport {
  constructor(
    private readonly readUrl: string,
    private readonly fetch: ReadFetch
  ) {}

  /** Capture one request and decode one response against its original cache hints. */
  async read(
    request: ReadRequest | string,
    cancellation?: AbortSignal
  ): Promise<ReadResponse> {
    this.checkCancellation(cancellation);
    if (typeof request !== 'string') validateReadRequest(request);
    const json =
      typeof request === 'string' ? request : JSON.stringify(request);
    const captured = decodeReadRequest(json);
    this.checkCancellation(cancellation);

    const response = await this.fetch(this.readUrl, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: json,
      signal: cancellation,
    });
    this.checkCancellation(cancellation);
    if (!response.ok) {
      throw new Error(
        `Read HTTP request failed with status ${response.status}. Check authentication and server availability before starting a new read.`
      );
    }

    const body = await response.text();
    this.checkCancellation(cancellation);
    const result = decodeReadResponse(body, captured);
    this.checkCancellation(cancellation);
    return result;
  }

  private checkCancellation(cancellation?: AbortSignal): void {
    if (cancellation?.aborted) {
      const error = new Error(
        'The read was cancelled. Start a new read if needed.'
      );
      error.name = 'AbortError';
      throw error;
    }
  }
}
