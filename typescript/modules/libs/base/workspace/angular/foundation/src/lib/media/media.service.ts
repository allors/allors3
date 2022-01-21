import { Injectable } from '@angular/core';
import { Media } from '@allors/default/workspace/domain';

@Injectable()
export abstract class MediaService {
  abstract url(media: Media): string;
}
