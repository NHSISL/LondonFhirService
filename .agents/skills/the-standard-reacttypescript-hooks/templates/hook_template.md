---
skill: the-standard-reacttypescript-hooks
type: template
source-section: "Hooks"
---
# Hook Template
```typescript
import { useState, useEffect, useCallback } from 'react';

export function use{Purpose}() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    async function load() {
      try {
        const result = await viewService.getDataAsync();
        setData(result);
      } catch (err) {
        setError(err as Error);
      } finally {
        setLoading(false);
      }
    }
    load();
  }, []);

  const handleAction = useCallback(() => {}, []);

  return { data, loading, error, handleAction };
}
```
