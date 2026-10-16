import { jsonBytes } from './read-message-size';
import {
  decodeInvokeReadRequest,
  InvokeReadRequest,
  validateInvokeReadRequest,
} from '@allors/system/common/protocol-json';
import { ReadFetch } from './read-http-transport';

/** One attempt; raw JSON lets the Session validate the receipt independently of each read part. */
export class InvokeReadHttpTransport {
  constructor(
    private readonly invokeReadUrl: string,
    private readonly fetch: ReadFetch
  ) {}

  async invokeRead(
    request: InvokeReadRequest,
    cancellation?: AbortSignal
  ): Promise<string> {
    if (cancellation?.aborted) {
      const error = new Error('InvokeRead cancelled before dispatch.');
      error.name = 'AbortError';
      throw error;
    }
    validateInvokeReadRequest(request);
    const json = JSON.stringify(request);
    if (jsonBytes(json) > 1024 * 1024)
      throw new Error(
        'InvokeRead request exceeds its byte limit. Reduce the invocation batch.'
      );
    decodeInvokeReadRequest(json);
    const response = await this.fetch(this.invokeReadUrl, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: json,
      signal: cancellation,
    });
    if (!response.ok)
      throw new Error(
        `InvokeRead HTTP request failed with status ${response.status}. Reconcile the server outcome before another mutation.`
      );
    // Cancellation after dispatch must not discard an acknowledgement already received.
    const body = await response.text();
    if (jsonBytes(body) > 16 * 1024 * 1024)
      throw new Error(
        'InvokeRead response exceeds its byte limit. Reconcile the server outcome before another mutation.'
      );
    return body;
  }
}
