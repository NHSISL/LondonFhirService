---
skill: the-standard-reacttypescript-services
type: template
source-section: "Foundation Services"
---

# Foundation Service Template

Use this template to create a new foundation service.

```typescript
// File: src/services/foundations/{domain}/{Domain}Service.ts

import type { {Entity} } from '../../../models/foundations/{entities}/{Entity}';
import type { I{Entity}ApiBroker } from '../../../brokers/apis/I{Entity}ApiBroker';

export interface I{Entity}Service {
  add{Entity}Async(entity: {Entity}): Promise<{Entity}>;
  retrieveAll{Entity}sAsync(): Promise<{Entity}[]>;
  retrieve{Entity}ByIdAsync(id: string): Promise<{Entity}>;
  modify{Entity}Async(entity: {Entity}): Promise<{Entity}>;
  remove{Entity}ByIdAsync(id: string): Promise<void>;
}

export class {Entity}Service implements I{Entity}Service {
  constructor(private {entity}ApiBroker: I{Entity}ApiBroker) {}

  async add{Entity}Async(entity: {Entity}): Promise<{Entity}> {
    this.validate{Entity}OnAdd(entity);

    return await this.tryCatch(async () => {
      return await this.{entity}ApiBroker.post{Entity}Async(entity);
    });
  }

  async retrieveAll{Entity}sAsync(): Promise<{Entity}[]> {
    return await this.tryCatch(async () => {
      return await this.{entity}ApiBroker.getAll{Entity}sAsync();
    });
  }

  async retrieve{Entity}ByIdAsync(id: string): Promise<{Entity}> {
    this.validate{Entity}Id(id);

    return await this.tryCatch(async () => {
      return await this.{entity}ApiBroker.get{Entity}ByIdAsync(id);
    });
  }

  async modify{Entity}Async(entity: {Entity}): Promise<{Entity}> {
    this.validate{Entity}OnModify(entity);

    return await this.tryCatch(async () => {
      return await this.{entity}ApiBroker.put{Entity}Async(entity);
    });
  }

  async remove{Entity}ByIdAsync(id: string): Promise<void> {
    this.validate{Entity}Id(id);

    return await this.tryCatch(async () => {
      return await this.{entity}ApiBroker.delete{Entity}ByIdAsync(id);
    });
  }

  // Validation methods
  private validate{Entity}OnAdd(entity: {Entity}): void {
    this.validate{Entity}IsNotNull(entity);
    this.validate{Entity}RequiredFields(entity);
  }

  private validate{Entity}OnModify(entity: {Entity}): void {
    this.validate{Entity}IsNotNull(entity);
    this.validate{Entity}Id(entity.id);
    this.validate{Entity}RequiredFields(entity);
  }

  private validate{Entity}IsNotNull(entity: {Entity}): void {
    if (!entity) {
      throw new {Entity}ValidationException('{Entity} is required');
    }
  }

  private validate{Entity}RequiredFields(entity: {Entity}): void {
    // Add field-specific validation
    // Example:
    // if (!entity.name?.trim()) {
    //   throw new {Entity}ValidationException('{Entity} name is required', 'name');
    // }
  }

  private validate{Entity}Id(id: string): void {
    if (!id?.trim()) {
      throw new {Entity}ValidationException('{Entity} ID is required', 'id');
    }
  }

  // TryCatch wrapper for exception mapping
  private async tryCatch<T>(returningFunction: () => Promise<T>): Promise<T> {
    try {
      return await returningFunction();
    } catch (error) {
      if (error instanceof {Entity}ValidationException) {
        throw error; // Validation exceptions pass through
      }

      if (this.isBrokerException(error)) {
        throw new {Entity}DependencyException(
          '{Entity} dependency error occurred, contact support',
          error as Error
        );
      }

      throw new {Entity}ServiceException(
        '{Entity} service error occurred, contact support',
        error as Error
      );
    }
  }

  private isBrokerException(error: unknown): boolean {
    return error instanceof Error && 
           error.name.includes('BrokerException');
  }
}

// Exception types
export class {Entity}ValidationException extends Error {
  constructor(message: string, public field?: string) {
    super(message);
    this.name = '{Entity}ValidationException';
  }
}

export class {Entity}DependencyException extends Error {
  constructor(message: string, public innerException: Error) {
    super(message);
    this.name = '{Entity}DependencyException';
  }
}

export class {Entity}ServiceException extends Error {
  constructor(message: string, public innerException: Error) {
    super(message);
    this.name = '{Entity}ServiceException';
  }
}
```

## Replacements

- `{Entity}`: PascalCase singular (e.g., `Patient`, `Appointment`)
- `{entity}`: camelCase singular (e.g., `patient`, `appointment`)
- `{entities}`: lowercase plural (e.g., `patients`, `appointments`)
- `{domain}`: lowercase domain (e.g., `patients`, `appointments`)

## Usage

1. Copy template
2. Replace placeholders
3. Add domain-specific validation in `validate{Entity}RequiredFields`
4. Adjust method signatures to match broker interface
5. Keep business logic minimal - this is a thin wrapper over the broker

## Checklist

- [ ] Wraps exactly one broker
- [ ] All inputs validated
- [ ] TryCatch wrapper implemented
- [ ] Exception types defined
- [ ] Interface exported for testing
- [ ] One service per file
