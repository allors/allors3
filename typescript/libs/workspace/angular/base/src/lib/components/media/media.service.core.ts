import { Injectable } from '@angular/core';
import { Media } from '@allors/workspace/domain/base';
import { MediaConfig } from './media.config';

import { MediaService } from '../../services/media/media.service';

@Injectable()
export class MediaServiceCore extends MediaService {

  constructor(private config: MediaConfig) {
    super();
  }

  public url(media: Media): string {
    if (media.FileName) {
      const fileName = encodeURI(media.FileName);
      return `${this.config.url}media/${media.UniqueId}/${media.Revision}/${fileName}`;
    } else {
      return `${this.config.url}media/${media.UniqueId}/${media.Revision}`;
    }
  }
}
