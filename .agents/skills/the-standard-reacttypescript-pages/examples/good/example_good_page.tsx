---
skill: the-standard-reacttypescript-pages
type: example
source-section: "Pages"
demonstrates: "tsr-pages-001, tsr-pages-004, tsr-pages-006, tsr-pages-010, tsr-pages-013"
---
// ✅ CORRECT: Page using hook and composing components
import { useDashboardPage } from '../hooks/useDashboardPage';
import { PatientList } from '../components/patients/PatientList';
import { LoadingSpinner } from '../components/common/LoadingSpinner';
import { ErrorDisplay } from '../components/common/ErrorDisplay';

export function DashboardPage() {
  const { patients, loading, error, handlePatientSelect } = useDashboardPage();

  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorDisplay error={error} />;

  return (
    <div className="dashboard-page">
      <h1>Patient Dashboard</h1>
      <PatientList patients={patients} onSelect={handlePatientSelect} />
    </div>
  );
}
