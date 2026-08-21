---
skill: the-standard-reacttypescript-pages
type: example
source-section: "Pages"
demonstrates: "tsr-pages-007, tsr-pages-008 violation"
---
// ❌ WRONG: Business logic and data fetching in page
import { useState, useEffect } from 'react';

export function DashboardPage() {
  const [patients, setPatients] = useState([]);

  // ❌ Data fetching in page
  useEffect(() => {
    fetch('/api/patients').then(r => r.json()).then(setPatients);
  }, []);

  // ❌ Business logic in page
  const activePatients = patients.filter(p => p.status === 'active');

  return <div>{activePatients.map(p => <div key={p.id}>{p.name}</div>)}</div>;
}

// ✅ CORRECT: Use hook
export function DashboardPageCorrect() {
  const { patients } = useDashboardPage();
  return <PatientList patients={patients} />;
}
