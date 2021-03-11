import { ObjectType } from '@allors/meta/core';
import { DatabaseObject, CompositeTypes, ParameterTypes, serializeObject } from '@allors/workspace/core';

import { IExtent } from './IExtent';
import { Result } from './Result';
import { FlatPull } from './FlatPull';
import { Extent } from './Extent';
import { Sort } from './Sort';
import { Select } from './Select';
import { Tree } from './Tree';

export type PullArgs = Pick<Pull, 'objectType' | 'extentRef' | 'extent' | 'object' | 'results' | 'parameters'>;

export class Pull {
  public objectType?: ObjectType;

  public extentRef?: string;

  public extent?: IExtent;

  public object?: CompositeTypes;

  public results?: Result[];

  public parameters?: { [name: string]: ParameterTypes };

  constructor(args: PullArgs);
  constructor(objectType: ObjectType, flat?: FlatPull);
  constructor(args: PullArgs | ObjectType, flat?: FlatPull) {
    if (args instanceof ObjectType) {
      this.objectType = args as ObjectType;

      if (!flat) {
        this.extent = new Extent({ objectType: this.objectType });
      } else {
        this.extentRef = flat.extentRef;
        this.extent = flat.extent;
        this.object = flat.object;
        this.parameters = flat.parameters;

        const sort = flat.sort instanceof Sort ? [flat.sort] : flat.sort;

        if (flat.predicate) {
          if (this.object || this.extent || this.extentRef) {
            throw new Error('predicate conflicts with object/extent/extentRef');
          }

          this.extent = new Extent({
            objectType: this.objectType,
            predicate: flat.predicate,
            sort,
          });
        }

        if (!this.object && !this.extent && !this.extentRef) {
          this.extent = new Extent({ objectType: this.objectType, sort });
        }

        if (flat.selectRef || flat.select || flat.include || flat.name || flat.skip || flat.take) {
          const result = new Result({
            selectRef: flat.selectRef,
            select: flat.select ? (flat.select instanceof Select ? flat.select : new Select(this.objectType, flat.select)) : undefined,
            name: flat.name,
            skip: flat.skip,
            take: flat.take,
          });

          if (flat.include) {
            const include = flat.include instanceof Tree ? flat.include : new Tree(this.objectType, flat.include);

            if (result.select) {
              if (result.select.step) {
                throw new Error('include conflicts with select step');
              }

              result.select.include = include;
            } else {
              result.select = new Select({ include });
            }
          }

          this.results = this.results || [];
          this.results.push(result);
        }
      }
    } else {
      Object.assign(this, args);
      this.objectType = args.objectType;
    }
  }

  public toJSON(): any {
    const sessionObject = this.object as DatabaseObject;

    return {
      extentRef: this.extentRef,
      extent: this.extent,
      objectType: this.objectType && this.objectType.id,
      object: sessionObject && sessionObject.id ? sessionObject.id : this.object,
      results: this.results,
      parameters: serializeObject(this.parameters),
    };
  }
}
