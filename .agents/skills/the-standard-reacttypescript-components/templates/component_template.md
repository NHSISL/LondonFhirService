---
skill: the-standard-reacttypescript-components
type: template
source-section: "Components"
---

# Component Template

```typescript
import type { {Component}Props } from '../../models/components/{domain}/{Component}Props';

export function {Component}({ 
  // Destructure all props
}: {Component}Props) {
  return (
    <div className="{component}">
      {/* Pure rendering only */}
    </div>
  );
}
```

## Rules
- Pure function (no side effects)
- Data via props only
- TypeScript props
- Named export
- Props destructured
- No data fetching
- No business logic
