---
skill: the-standard-reacttypescript-brokers
type: template
source-section: "Brokers"
---

# API Broker Template

Use this template to create a new API broker for an entity.

```typescript
// File: src/brokers/apis/{Entity}ApiBroker.ts

import type { {Entity} } from '../../models/foundations/{entities}/{Entity}';

export interface I{Entity}ApiBroker {
  getAll{Entity}sAsync(signal?: AbortSignal): Promise<{Entity}[]>;
  get{Entity}ByIdAsync(id: string, signal?: AbortSignal): Promise<{Entity}>;
  post{Entity}Async(entity: {Entity}, signal?: AbortSignal): Promise<{Entity}>;
  put{Entity}Async(entity: {Entity}, signal?: AbortSignal): Promise<{Entity}>;
  delete{Entity}ByIdAsync(id: string, signal?: AbortSignal): Promise<void>;
}

export class {Entity}ApiBroker implements I{Entity}ApiBroker {
  private baseUrl: string;

  constructor() {
    this.baseUrl = import.meta.env.VITE_API_BASE_URL;
  }

  async getAll{Entity}sAsync(signal?: AbortSignal): Promise<{Entity}[]> {
    try {
      const response = await fetch(`${this.baseUrl}/api/{entities}`, { signal });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      return await response.json();
    } catch (error) {
      throw new {Entity}ApiBrokerException(
        'Failed to retrieve {entities}',
        error as Error
      );
    }
  }

  async get{Entity}ByIdAsync(id: string, signal?: AbortSignal): Promise<{Entity}> {
    try {
      const response = await fetch(`${this.baseUrl}/api/{entities}/${id}`, { signal });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      return await response.json();
    } catch (error) {
      throw new {Entity}ApiBrokerException(
        `Failed to retrieve {entity} ${id}`,
        error as Error
      );
    }
  }

  async post{Entity}Async(entity: {Entity}, signal?: AbortSignal): Promise<{Entity}> {
    try {
      const response = await fetch(`${this.baseUrl}/api/{entities}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(entity),
        signal
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      return await response.json();
    } catch (error) {
      throw new {Entity}ApiBrokerException(
        'Failed to create {entity}',
        error as Error
      );
    }
  }

  async put{Entity}Async(entity: {Entity}, signal?: AbortSignal): Promise<{Entity}> {
    try {
      const response = await fetch(`${this.baseUrl}/api/{entities}/${entity.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(entity),
        signal
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      return await response.json();
    } catch (error) {
      throw new {Entity}ApiBrokerException(
        `Failed to update {entity} ${entity.id}`,
        error as Error
      );
    }
  }

  async delete{Entity}ByIdAsync(id: string, signal?: AbortSignal): Promise<void> {
    try {
      const response = await fetch(`${this.baseUrl}/api/{entities}/${id}`, {
        method: 'DELETE',
        signal
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }
    } catch (error) {
      throw new {Entity}ApiBrokerException(
        `Failed to delete {entity} ${id}`,
        error as Error
      );
    }
  }
}

export class {Entity}ApiBrokerException extends Error {
  constructor(message: string, public innerException: Error) {
    super(message);
    this.name = '{Entity}ApiBrokerException';
  }
}
```

## Replacements

- `{Entity}`: PascalCase singular (e.g., `Patient`, `Appointment`)
- `{entity}`: lowercase singular (e.g., `patient`, `appointment`)
- `{entities}`: lowercase plural (e.g., `patients`, `appointments`)

## Usage

1. Copy template
2. Replace `{Entity}`, `{entity}`, `{entities}` placeholders
3. Adjust API endpoint paths to match backend
4. Add additional methods as needed (search, query, etc.)
5. Keep I/O only — no business logic
