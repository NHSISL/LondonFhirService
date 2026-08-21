---
skill: the-standard-reacttypescript-files
type: example
source-section: "1. Project Structure"
demonstrates: "tsr-files-014 violation"
---

// ❌ WRONG: Generic utility file with mixed concerns
// File: src/utils/helpers.ts

// This file violates tsr-files-014 and tsr-files-001

export const fetchData = async (url: string) => {
  const response = await fetch(url);
  return await response.json();
};

export const formatDate = (date: Date): string => {
  return date.toLocaleDateString();
};

export const PatientHelper = {
  getFullName: (firstName: string, lastName: string) => {
    return `${firstName} ${lastName}`;
  },

  calculateAge: (dateOfBirth: Date) => {
    const today = new Date();
    const birthDate = new Date(dateOfBirth);
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();

    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }

    return age;
  }
};

export const capitalize = (str: string): string => {
  return str.charAt(0).toUpperCase() + str.slice(1);
};

export const debounce = (func: Function, delay: number) => {
  let timeoutId: NodeJS.Timeout;

  return (...args: any[]) => {
    clearTimeout(timeoutId);
    timeoutId = setTimeout(() => func(...args), delay);
  };
};

// Why this is wrong:
// 1. Generic "helpers" name provides no architectural meaning
// 2. Mixes multiple unrelated concerns (API, dates, names, strings, timing)
// 3. Becomes a dumping ground for miscellaneous code
// 4. No clear owner or responsibility
// 5. Hard to test, maintain, and understand
//
// Fix: Split into:
// - API utilities → patientApiBroker.ts
// - Date formatting → dateTimeBroker.ts
// - Patient business logic → patientService.ts
// - String utilities → stringUtilities.ts (if truly generic) or specific domain service
// - Timing utilities → create a specific timing utility with clear purpose
