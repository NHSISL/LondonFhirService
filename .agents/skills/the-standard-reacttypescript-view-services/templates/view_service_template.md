---
skill: the-standard-reacttypescript-view-services
type: template
source-section: "View Services"
---

# View Service Template

```typescript
import type { {Entity} } from '../../../models/foundations/{entities}/{Entity}';
import type { {Entity}CardView } from '../../../models/views/{entities}/{Entity}CardView';
import type { I{Entity}Service } from '../../foundations/{entities}/I{Entity}Service';

export interface I{Entity}ViewService {
  get{Entity}CardViewAsync(id: string): Promise<{Entity}CardView>;
  getAll{Entity}CardViewsAsync(): Promise<{Entity}CardView[]>;
}

export class {Entity}ViewService implements I{Entity}ViewService {
  constructor(private {entity}Service: I{Entity}Service) {}

  async get{Entity}CardViewAsync(id: string): Promise<{Entity}CardView> {
    const {entity} = await this.{entity}Service.retrieve{Entity}ByIdAsync(id);

    return {
      id: {entity}.id,
      // Add UI transformations here
    };
  }

  async getAll{Entity}CardViewsAsync(): Promise<{Entity}CardView[]> {
    const {entities} = await this.{entity}Service.retrieveAll{Entity}sAsync();

    return {entities}.map({entity} => this.transform{Entity}To{Entity}CardView({entity}));
  }

  private transform{Entity}To{Entity}CardView({entity}: {Entity}): {Entity}CardView {
    return {
      id: {entity}.id,
      // Add transformations
    };
  }
}
```
