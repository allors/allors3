import { Response } from '../Response';

export interface PullResponse extends Response {
  namedCollections?: { [id: string]: string[] };
  namedObjects?: { [id: string]: string };
  namedValues?: { [id: string]: string };

  accessControls?: string[][];
  objects?: string[][];
}
