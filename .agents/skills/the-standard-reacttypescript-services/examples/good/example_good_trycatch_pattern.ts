---
skill: the-standard-reacttypescript-services
type: example
source-section: "Foundation Services"
demonstrates: "tsr-services-007, tsr-services-017, tsr-services-018, tsr-services-019"
---

// ✅ CORRECT: TryCatch pattern with complete exception mapping
// File: src/services/foundations/patients/PatientService.TryCatch.ts

export class PatientService {
  // ... constructor and public methods ...

  // TryCatch wrapper - the heart of exception mapping
  private async tryCatch<T>(returningFunction: () => Promise<T>): Promise<T> {
    try {
      return await returningFunction();
    } catch (error) {
      // Pass through validation exceptions (already service-layer)
      if (this.isValidationException(error)) {
        throw error;
      }

      // Wrap broker exceptions in DependencyException
      if (this.isBrokerException(error)) {
        throw this.createDependencyException(error as Error);
      }

      // Wrap unexpected errors in ServiceException
      throw this.createServiceException(error as Error);
    }
  }

  private isValidationException(error: unknown): boolean {
    return error instanceof PatientValidationException;
  }

  private isBrokerException(error: unknown): boolean {
    if (!(error instanceof Error)) {
      return false;
    }

    const brokerExceptionNames = [
      'PatientApiBrokerException',
      'BrokerException',
      'ApiBrokerException'
    ];

    return brokerExceptionNames.some(name => 
      error.name === name || error.name.includes('BrokerException')
    );
  }

  private createDependencyException(error: Error): PatientDependencyException {
    return new PatientDependencyException(
      'Patient dependency error occurred, contact support',
      error
    );
  }

  private createServiceException(error: Error): PatientServiceException {
    return new PatientServiceException(
      'Patient service error occurred, contact support',
      error
    );
  }
}

// Exception hierarchy
export class PatientValidationException extends Error {
  constructor(message: string, public field?: string) {
    super(message);
    this.name = 'PatientValidationException';
  }
}

export class PatientDependencyException extends Error {
  constructor(
    message: string,
    public innerException: Error
  ) {
    super(message);
    this.name = 'PatientDependencyException';
  }
}

export class PatientServiceException extends Error {
  constructor(
    message: string,
    public innerException: Error
  ) {
    super(message);
    this.name = 'PatientServiceException';
  }
}

// Why this is correct:
// 1. TryCatch wraps all broker calls (tsr-services-007)
// 2. Validation exceptions pass through (tsr-services-018)
// 3. Broker exceptions wrapped in DependencyException (tsr-services-017)
// 4. Unexpected errors wrapped in ServiceException (tsr-services-019)
// 5. All exceptions preserve inner exception (tsr-services-020)
// 6. Messages are user-facing (tsr-services-021)
// 7. Clear exception type detection logic
// 8. Reusable across all service methods
