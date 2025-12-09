import {
  DialogConfig,
  PromptType,
} from '@allors/base/workspace/angular/foundation';

export interface AllorsMaterialDialogData {
  alert?: boolean;
  confirmation?: boolean;
  prompt?: boolean;
  promptType?: PromptType;

  config: DialogConfig;
}
