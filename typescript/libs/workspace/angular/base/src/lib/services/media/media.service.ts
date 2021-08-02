import { Injectable } from '@angular/core';
import { Media } from '@allors/workspace/domain/base';

@Injectable()
export abstract class MediaService {
  abstract url(media: Media): string;
}
