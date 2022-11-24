import {
  Component,
  EventEmitter,
  HostBinding,
  Input,
  Optional,
  Output,
} from '@angular/core';
import { NgForm } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { IObject } from '@allors/system/workspace/domain';
import { Media } from '@allors/default/workspace/domain';
import {
  AllorsComponent,
  MediaService,
} from '@allors/base/workspace/angular/foundation';
import { AllorMediaPreviewComponent } from './preview/media-preview.component';
import { isImage } from './media';

@Component({
  selector: 'a-mat-media',
  templateUrl: './media.component.html',
  styleUrls: ['./media.component.scss'],
})
export class AllorsMaterialMediaComponent extends AllorsComponent {
  @Input() media: Media;

  @Output() public delete: EventEmitter<IObject> = new EventEmitter();

  constructor(
    @Optional() form: NgForm,
    private dialog: MatDialog,
    private mediaService: MediaService
  ) {
    super();
  }

  @HostBinding('attr.data-allors-id')
  get dataAllorsId(): number {
    return this.media?.id;
  }

  get src(): string | null {
    if (this.media.InDataUri) {
      return this.media.InDataUri;
    } else if (this.media.UniqueId) {
      return this.mediaService.url(this.media);
    }

    return null;
  }

  get icon(): string {
    switch (this.media.Type) {
      case 'application/pdf':
        return 'picture_as_pdf';
    }

    return 'attachment';
  }

  public preview(): void {
    const dialogRef = this.dialog.open(AllorMediaPreviewComponent, {
      maxHeight: '80vh',
      maxWidth: '60vw',
      data: { media: this.media },
    });

    dialogRef.afterClosed().subscribe((result) => {});
  }

  public download(): void {
    if (this.media.InDataUri) {
      const link = document.createElement('a');
      if (this.media.FileName) {
        link.download = this.media.FileName;
      }
      link.href = this.media.InDataUri;

      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    } else if (this.media.UniqueId) {
      const url = this.mediaService.url(this.media);
      window.open(url, '_blank', 'noopener');
    }
  }

  get isImage(): boolean {
    return isImage(this.media);
  }
}
