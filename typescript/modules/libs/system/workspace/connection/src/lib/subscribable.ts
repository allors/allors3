/**
 * The least an event source offers: a listener subscribes and gets a way to unsubscribe. An
 * RxJS Observable and a Subject satisfy it, so an application may pass its own.
 */
export interface Subscribable<T> {
  subscribe(listener: (value: T) => void): Subscription;
}

export interface Subscription {
  unsubscribe(): void;
}

/**
 * The event source of the connection and the cache: listeners in a set, called in the order
 * they subscribed, each on a copy of the set so that a listener may unsubscribe from within.
 */
export class Emitter<T> implements Subscribable<T> {
  private readonly listeners = new Set<(value: T) => void>();

  subscribe(listener: (value: T) => void): Subscription {
    this.listeners.add(listener);
    return {
      unsubscribe: () => {
        this.listeners.delete(listener);
      },
    };
  }

  emit(value: T): void {
    for (const listener of [...this.listeners]) {
      listener(value);
    }
  }
}
