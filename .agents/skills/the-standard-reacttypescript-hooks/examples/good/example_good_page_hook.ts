---
skill: the-standard-reacttypescript-hooks
type: example
source-section: "Hooks"
demonstrates: "tsr-hooks-001, tsr-hooks-003, tsr-hooks-004, tsr-hooks-005, tsr-hooks-014"
---
// ✅ CORRECT: Page hook calling view service
import { useState, useEffect, useCallback } from 'react';
import { PatientViewService } from '../services/views/patients/PatientViewService';
import type { PatientCardView } from '../models/views/patients/PatientCardView';

export function useDashboardPage() {
  const [patients, setPatients] = useState<PatientCardView[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const patientViewService = new PatientViewService();

  useEffect(() => {
    async function loadPatients() {
      try {
        const data = await patientViewService.getAllPatientCardViewsAsync();
        setPatients(data);
      } catch (err) {
        setError(err as Error);
      } finally {
        setLoading(false);
      }
    }
    loadPatients();
  }, []);

  const handlePatientSelect = useCallback((id: string) => {
    console.log('Selected:', id);
  }, []);

  return { patients, loading, error, handlePatientSelect };
}
