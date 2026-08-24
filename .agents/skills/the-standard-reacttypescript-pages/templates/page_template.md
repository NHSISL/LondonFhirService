---
skill: the-standard-reacttypescript-pages
type: template
source-section: "Pages"
---
# Page Template
```typescript
import { use{Domain}Page } from '../hooks/use{Domain}Page';

export function {Domain}Page() {
  const { data, loading, error, handlers } = use{Domain}Page();

  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorDisplay error={error} />;

  return (
    <div className="{domain}-page">
      {/* Compose components */}
    </div>
  );
}
```
