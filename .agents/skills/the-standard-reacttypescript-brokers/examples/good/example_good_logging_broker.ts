---
skill: the-standard-reacttypescript-brokers
type: example
source-section: "Brokers"
demonstrates: "tsr-brokers-007, tsr-brokers-013"
---

// ✅ CORRECT: Logging Broker implementation
// File: src/brokers/loggings/LoggingBroker.ts

export interface ILoggingBroker {
  logInfo(message: string): void;
  logWarning(message: string): void;
  logError(error: Error): void;
  logDebug(message: string): void;
}

export class LoggingBroker implements ILoggingBroker {
  logInfo(message: string): void {
    console.info(`[INFO] ${new Date().toISOString()} - ${message}`);
  }

  logWarning(message: string): void {
    console.warn(`[WARN] ${new Date().toISOString()} - ${message}`);
  }

  logError(error: Error): void {
    console.error(
      `[ERROR] ${new Date().toISOString()} - ${error.name}: ${error.message}`,
      error.stack
    );
  }

  logDebug(message: string): void {
    if (import.meta.env.DEV) {
      console.debug(`[DEBUG] ${new Date().toISOString()} - ${message}`);
    }
  }
}

// Alternative: External logging service broker
export class ApplicationInsightsBroker implements ILoggingBroker {
  private appInsights: any; // Replace with actual ApplicationInsights type

  constructor(instrumentationKey: string) {
    // Initialize Application Insights SDK
    this.appInsights = this.initializeAppInsights(instrumentationKey);
  }

  private initializeAppInsights(key: string): any {
    // Actual initialization logic
    return {};
  }

  logInfo(message: string): void {
    this.appInsights.trackTrace({ message, severityLevel: 1 });
  }

  logWarning(message: string): void {
    this.appInsights.trackTrace({ message, severityLevel: 2 });
  }

  logError(error: Error): void {
    this.appInsights.trackException({ exception: error });
  }

  logDebug(message: string): void {
    if (import.meta.env.DEV) {
      this.appInsights.trackTrace({ message, severityLevel: 0 });
    }
  }
}

// Why this is correct:
// 1. Abstracts console logging (tsr-brokers-007)
// 2. Interface allows swapping implementations (console vs external service)
// 3. Single responsibility - logging only (tsr-brokers-013)
// 4. No business logic (tsr-brokers-008)
// 5. Can inject different implementations for testing
// 6. Synchronous methods acceptable for logging (fire-and-forget)
