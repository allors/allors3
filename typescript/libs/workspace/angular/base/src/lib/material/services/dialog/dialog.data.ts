import { PromptType, DialogConfig } from './dialog.config';

export interface DialogData {
  alert?: boolean;
  confirmation?: boolean;
  prompt?: boolean;
  promptType?: PromptType;

  config: DialogConfig;
}
