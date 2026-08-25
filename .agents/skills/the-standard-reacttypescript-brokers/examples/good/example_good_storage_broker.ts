---
skill: the-standard-reacttypescript-brokers
type: example
source-section: "Brokers"
demonstrates: "tsr-brokers-006, tsr-brokers-009, tsr-brokers-013"
---

// ✅ CORRECT: Storage Broker implementation
// File: src/brokers/storages/LocalStorageBroker.ts

export interface ILocalStorageBroker {
  getItemAsync<T>(key: string): Promise<T | null>;
  setItemAsync<T>(key: string, value: T): Promise<void>;
  removeItemAsync(key: string): Promise<void>;
  clearAsync(): Promise<void>;
}

export class LocalStorageBroker implements ILocalStorageBroker {
  async getItemAsync<T>(key: string): Promise<T | null> {
    try {
      const item = localStorage.getItem(key);

      if (item === null) {
        return null;
      }

      return JSON.parse(item) as T;
    } catch (error) {
      throw new LocalStorageBrokerException(
        `Failed to get item with key: ${key}`,
        error as Error
      );
    }
  }

  async setItemAsync<T>(key: string, value: T): Promise<void> {
    try {
      const serialized = JSON.stringify(value);
      localStorage.setItem(key, serialized);
    } catch (error) {
      throw new LocalStorageBrokerException(
        `Failed to set item with key: ${key}`,
        error as Error
      );
    }
  }

  async removeItemAsync(key: string): Promise<void> {
    try {
      localStorage.removeItem(key);
    } catch (error) {
      throw new LocalStorageBrokerException(
        `Failed to remove item with key: ${key}`,
        error as Error
      );
    }
  }

  async clearAsync(): Promise<void> {
    try {
      localStorage.clear();
    } catch (error) {
      throw new LocalStorageBrokerException(
        'Failed to clear localStorage',
        error as Error
      );
    }
  }
}

export class LocalStorageBrokerException extends Error {
  constructor(message: string, public innerException: Error) {
    super(message);
    this.name = 'LocalStorageBrokerException';
  }
}

// Why this is correct:
// 1. Abstracts localStorage behind interface (tsr-brokers-006)
// 2. All methods async for consistency (tsr-brokers-002)
// 3. Generic type support for type safety
// 4. Exceptions wrapped properly (tsr-brokers-003, tsr-brokers-015, tsr-brokers-016)
// 5. No business logic (tsr-brokers-008)
// 6. Single responsibility - storage only (tsr-brokers-013)
// 7. Interface for testing (tsr-brokers-014)
// 8. Method naming pattern (tsr-brokers-005)
