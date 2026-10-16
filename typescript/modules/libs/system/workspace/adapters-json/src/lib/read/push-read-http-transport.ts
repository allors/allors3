import { jsonBytes } from './read-message-size';
import {
  decodePushReadRequest,
  PushReadRequest,
  validatePushReadRequest,
} from '@allors/system/common/protocol-json';
import { ReadFetch } from './read-http-transport';

/** One attempt; raw JSON lets the Session validate the receipt independently of each read part. */
export class PushReadHttpTransport {
  constructor(
    private readonly pushReadUrl: string,
    private readonly fetch: ReadFetch
  ) {}

  async pushRead(
    request: PushReadRequest,
    cancellation?: AbortSignal
  ): Promise<string> {
    if (cancellation?.aborted) {
      const error = new Error('PushRead cancelled before dispatch.');
      error.name = 'AbortError';
      throw error;
    }
    validatePushReadRequest(request);
    const json = JSON.stringify(request);
    if (jsonBytes(json) > 1024 * 1024)
      throw new Error(
        'PushRead request exceeds its byte limit. Reduce the submitted changes.'
      );
    decodePushReadRequest(json);
    const response = await this.fetch(this.pushReadUrl, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: json,
      signal: cancellation,
    });
    if (!response.ok)
      throw new Error(
        `PushRead HTTP request failed with status ${response.status}. Reconcile the server outcome before another mutation.`
      );
    // Cancellation after dispatch must not discard an acknowledgement already received.
    const body = await response.text();
    if (jsonBytes(body) > 16 * 1024 * 1024)
      throw new Error(
        'PushRead response exceeds its byte limit. Reconcile the server outcome before another mutation.'
      );
    return body;
  }
}
