---
skill: the-standard-reacttypescript-hooks
type: example
source-section: "Hooks"
demonstrates: "tsr-hooks-001, tsr-hooks-008 violation"
---
// ❌ WRONG: Hook calling broker directly
import { PatientApiBroker } from '../brokers/apis/PatientApiBroker';

export function useDashboardPage() {
  const broker = new PatientApiBroker(); // ❌ Direct broker access

  useEffect(() => {
    broker.getAllPatientsAsync().then(setPatients); // ❌ Broker call
  }, []);
}

// ✅ CORRECT: Call view service
export function useDashboardPageCorrect() {
  const viewService = new PatientViewService();

  useEffect(() => {
    viewService.getAllPatientCardViewsAsync().then(setPatients);
  }, []);
}
