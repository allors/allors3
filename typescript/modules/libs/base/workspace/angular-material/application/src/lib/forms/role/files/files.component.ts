import {
  Component,
  EventEmitter,
  Input,
  Optional,
  Output,
} from '@angular/core';
import { NgForm } from '@angular/forms';
import { M } from '@allors/default/workspace/meta';
import { Media } from '@allors/default/workspace/domain';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import {
  MediaService,
  RoleField,
} from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-files',
  templateUrl: './files.component.html',
})
export class AllorsMaterialFilesComponent extends RoleField {
  @Output()
  changed: EventEmitter<RoleField> = new EventEmitter<RoleField>();

  @Input() public accept = 'image/*';

  public files: File[] | null;

  constructor(
    @Optional() form: NgForm,
    private mediaService: MediaService,
    private workspaceService: WorkspaceService
  ) {
    super(form);
  }

  get medias(): Media[] {
    return this.model;
  }

  get fieldValue(): string {
    if (this.medias && this.medias.length > 0) {
      return this.medias.length + ' file' + (this.medias.length > 1 ? 's' : '');
    }

    return '';
  }

  public src(media: Media): string | null {
    if (media.InDataUri) {
      return media.InDataUri;
    } else if (media.UniqueId) {
      return this.mediaService.url(media);
    }

    return null;
  }

  public deleteAll(): void {
    this.model = null;
    this.files = null;
  }

  public delete(media: Media): void {
    this.object.strategy.removeCompositesRole(this.roleType, media);
  }

  public onFileInput(event: Event) {
    const input = event.target as HTMLInputElement;
    const files = input.files;
    if (files?.length && files.length > 0) {
      for (let i = 0; i < files.length; i++) {
        const file = files.item(i);
        if (file != null) {
          this.addFile(file);
        }
      }
    }
  }

  // TODO: move to RxJS implementation and share with file.component
  private addFile(file: File) {
    const m = this.workspaceService.workspace.configuration.metaPopulation as M;

    const reader: FileReader = new FileReader();
    const load: () => void = () => {
      const media: Media = this.object.strategy.session.create<Media>(m.Media);
      media.InFileName = file.name;
      media.InDataUri = reader.result as string;
      this.object.strategy.addCompositesRole(this.roleType, media);
    };

    reader.addEventListener('load', load, false);
    reader.readAsDataURL(file);

    this.changed.emit(this);
  }
}
