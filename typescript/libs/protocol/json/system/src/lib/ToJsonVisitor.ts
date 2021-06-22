import { IVisitor, And, Between, ContainedIn, Contains, Equals, Except, Exists, GreaterThan, Instanceof, Intersect, LessThan, Like, Not, Or, Union } from '@allors/workspace/domain/system';
import { Extent as DataExtent, Select as DataSelect, Node as DataNode, Pull as DataPull, Result as DataResult, Sort as DataSort, Step as DataStep, Procedure as DataProcedure } from '@allors/workspace/domain/system';
import { AssociationType, RoleType } from '@allors/workspace/meta/system';
import { Extent } from './data/Extent';
import { ExtentKind } from './data/ExtentKind';
import { Node } from './data/Node';
import { Predicate } from './data/Predicate';
import { PredicateKind } from './data/PredicateKind';
import { Procedure } from './data/Procedure';
import { Pull } from './data/Pull';
import { Result } from './data/Result';
import { Select } from './data/Select';
import { Sort } from './data/Sort';
import { Step } from './data/Step';
import { UnitConvert } from './UnitConvert';

export class ToJsonVisitor implements IVisitor {
  private extents: Extent[];

  private predicates: Predicate[];

  private results: Result[];

  private selects: Select[];

  private steps: Step[];

  private nodes: Node[];

  private sorts: Sort[];

  Pull: Pull;

  Procedure: Procedure;

  get Extent(): Extent {
    return this.extents.pop();
  }

  get Select(): Select {
    return this.selects.pop();
  }

  public constructor(private unitConvert: UnitConvert) {
    this.extents = [];
    this.predicates = [];
    this.results = [];
    this.selects = [];
    this.steps = [];
    this.nodes = [];
    this.sorts = [];
  }

  public VisitAnd(visited: And) {
    const predicate: Predicate = {
      k: PredicateKind.And,
      d: visited.dependencies,
    };

    this.predicates.push(predicate);

    if (visited.operands != null) {
      const length = visited.operands.length;
      predicate.ops = [];
      for (let i = 0; i < length; i++) {
        const operand = visited.operands[i];
        operand.accept(this);
        predicate.ops[i] = this.predicates.pop();
      }
    }
  }

  public VisitBetween(visited: Between) {
    const predicate: Predicate = {
      k: PredicateKind.Between,
      d: visited.dependencies,
      r: visited.roleType?.relationType.tag,
      vs: visited.values?.map((v) => this.unitConvert.ToJson(v)),
      pas: visited.paths?.map((v) => v.relationType.tag),
      p: visited.parameter,
    };

    this.predicates.push(predicate);
  }

  public VisitContainedIn(visited: ContainedIn) {
    const predicate: Predicate = {
      k: PredicateKind.ContainedIn,
      d: visited.dependencies,
      a: (visited.propertyType as AssociationType)?.relationType.tag,
      r: (visited.propertyType as RoleType)?.relationType.tag,
      vs: visited.objects?.map((v) => v.id),
      p: visited.parameter,
    };

    this.predicates.push(predicate);

    if (visited.extent != null) {
      visited.extent.accept(this);
      predicate.e = this.extents.pop();
    }
  }

  public VisitContains(visited: Contains) {
    const predicate: Predicate = {
      k: PredicateKind.Contains,
      d: visited.dependencies,
      a: visited.propertyType.isAssociationType ? (visited.propertyType as AssociationType)?.relationType.tag : undefined,
      r: visited.propertyType.isRoleType ? (visited.propertyType as RoleType)?.relationType.tag : undefined,
      ob: visited.object?.id,
      p: visited.parameter,
    };

    this.predicates.push(predicate);
  }

  public VisitEquals(visited: Equals) {
    const predicate: Predicate = {
      k: PredicateKind.Equals,
      d: visited.dependencies,
      a: visited.propertyType.isAssociationType ? (visited.propertyType as AssociationType)?.relationType.tag : undefined,
      r: visited.propertyType.isRoleType ? (visited.propertyType as RoleType)?.relationType.tag : undefined,
      ob: visited.object?.id,
      v: this.unitConvert.ToJson(visited.value),
      pa: visited.path?.relationType.tag,
      p: visited.parameter,
    };

    this.predicates.push(predicate);
  }

  public VisitExcept(visited: Except) {
    const extent: Extent = {
      k: ExtentKind.Except,
    };

    this.extents.push(extent);
    if (visited.operands != null) {
      const length = visited.operands.length;
      extent.o = [];
      for (let i = 0; i < length; i++) {
        const operand = visited.operands[i];
        operand.accept(this);
        extent.o[i] = this.extents.pop();
      }
    }

    if (visited.sorting?.length > 0) {
      const length = visited.sorting.length;
      extent.s = [];
      for (let i = 0; i < length; i++) {
        const sorting = visited.sorting[i];
        sorting.accept(this);
        extent.s[i] = this.sorts.pop();
      }
    }
  }

  public VisitExists(visited: Exists) {
    const predicate: Predicate = {
      k: PredicateKind.Exists,
      d: visited.dependencies,
      a: visited.propertyType.isAssociationType ? (visited.propertyType as AssociationType)?.relationType.tag : undefined,
      r: visited.propertyType.isRoleType ? (visited.propertyType as RoleType)?.relationType.tag : undefined,
      p: visited.parameter,
    };

    this.predicates.push(predicate);
  }

  public VisitExtent(visited: DataExtent) {
    const extent: Extent = {
      k: ExtentKind.Extent,
      t: visited.objectType?.tag,
      s: visited.sorting?.map((v) => ({
        d: v.sortDirection,
        r: v.roleType?.relationType.tag,
      })),
    };

    this.extents.push(extent);
    if (visited.predicate != null) {
      visited.predicate.accept(this);
      extent.p = this.predicates.pop();
    }
  }

  public VisitSelect(visited: DataSelect) {
    const select: Select = {} as any;

    this.selects.push(select);

    if (visited.step != null) {
      visited.step.accept(this);
      select.s = this.steps.pop();
    }

    if (visited.include?.length > 0) {
      const includes = visited.include;
      const length = includes.length;
      select.i = [];
      for (let i = 0; i < length; i++) {
        const node = includes[i];
        node.accept(this);
        select.i[i] = this.nodes.pop();
      }
    }
  }

  public VisitGreaterThan(visited: GreaterThan) {
    const predicate: Predicate = {
      k: PredicateKind.GreaterThan,
      d: visited.dependencies,
      r: visited.roleType?.relationType.tag,
      v: this.unitConvert.ToJson(visited.value),
      pa: visited.path?.relationType.tag,
      p: visited.parameter,
    };

    this.predicates.push(predicate);
  }

  public VisitInstanceOf(visited: Instanceof) {
    const predicate: Predicate = {
      k: PredicateKind.InstanceOf,
      d: visited.dependencies,
      o: visited.objectType?.tag,
      a: visited.propertyType.isAssociationType ? (visited.propertyType as AssociationType)?.relationType.tag : undefined,
      r: visited.propertyType.isRoleType ? (visited.propertyType as RoleType)?.relationType.tag : undefined,
    };

    this.predicates.push(predicate);
  }

  public VisitIntersect(visited: Intersect) {
    const extent: Extent = {
      k: ExtentKind.Intersect,
    };

    this.extents.push(extent);

    if (visited.operands != null) {
      const length = visited.operands.length;
      extent.o = [];
      for (let i = 0; i < length; i++) {
        const operand = visited.operands[i];
        operand.accept(this);
        extent.o[i] = this.extents.pop();
      }
    }

    if (visited.sorting?.length > 0) {
      const length = visited.sorting.length;
      extent.s = [];
      for (let i = 0; i < length; i++) {
        const sorting = visited.sorting[i];
        sorting.accept(this);
        extent.s[i] = this.sorts.pop();
      }
    }
  }

  public VisitLessThan(visited: LessThan) {
    const predicate: Predicate = {
      k: PredicateKind.LessThan,
      d: visited.dependencies,
      r: visited.roleType?.relationType.tag,
      v: this.unitConvert.ToJson(visited.value),
      pa: visited.path?.relationType.tag,
      p: visited.parameter,
    };

    this.predicates.push(predicate);
  }

  public VisitLike(visited: Like) {
    const predicate: Predicate = {
      k: PredicateKind.Like,
      d: visited.dependencies,
      r: visited.roleType?.relationType.tag,
      v: this.unitConvert.ToJson(visited.value),
      p: visited.parameter,
    };

    this.predicates.push(predicate);
  }

  public VisitNode(visited: DataNode) {
    const node: Node = {
      a: visited.propertyType.isAssociationType ? (visited.propertyType as AssociationType)?.relationType.tag : undefined,
      r: visited.propertyType.isRoleType ? (visited.propertyType as RoleType)?.relationType.tag : undefined,
    };

    this.nodes.push(node);
    if (visited.nodes?.length > 0) {
      const length = visited.nodes.length;
      node.n = [];
      for (let i = 0; i < length; i++) {
        visited.nodes[i].accept(this);
        node.n[i] = this.nodes.pop();
      }
    }
  }

  public VisitNot(visited: Not) {
    const predicate: Predicate = {
      k: PredicateKind.Not,
      d: visited.dependencies,
    };
    this.predicates.push(predicate);
    if (visited.operand != null) {
      visited.operand.accept(this);
      predicate.op = this.predicates.pop();
    }
  }

  public VisitOr(visited: Or) {
    const predicate: Predicate = {
      k: PredicateKind.Or,
      d: visited.dependencies,
    };

    this.predicates.push(predicate);
    if (visited.operands != null) {
      const length = visited.operands.length;
      predicate.ops = [];
      for (let i = 0; i < length; i++) {
        const operand = visited.operands[i];
        operand.accept(this);
        predicate.ops[i] = this.predicates.pop();
      }
    }
  }

  public VisitPull(visited: DataPull) {
    const pull: Pull = {
      er: visited.extentRef,
      t: visited.objectType?.tag,
      o: visited.objectId ?? visited.object?.id,
    };

    if (visited.extent != null) {
      visited.extent.accept(this);
      pull.e = this.extents.pop();
    }

    if (visited.results?.length > 0) {
      const length = visited.results.length;
      pull.r = [];
      for (let i = 0; i < length; i++) {
        const result = visited.results[i];
        result.accept(this);
        pull.r[i] = this.results.pop();
      }
    }

    this.Pull = pull;
  }

  public VisitResult(visited: DataResult) {
    const result: Result = {
      r: visited.selectRef,
      n: visited.name,
      k: visited.skip,
      t: visited.take,
    };

    this.results.push(result);
    if (visited.select != null) {
      visited.select.accept(this);
      result.s = this.selects.pop();
    }
  }

  public VisitSort(visited: DataSort) {
    const sort: Sort = {
      d: visited.sortDirection,
      r: visited.roleType?.relationType.tag,
    };

    this.sorts.push(sort);
  }

  public VisitStep(visited: DataStep) {
    const step: Step = {
      a: visited.propertyType.isAssociationType ? (visited.propertyType as AssociationType)?.relationType.tag : undefined,
      r: visited.propertyType.isRoleType ? (visited.propertyType as RoleType)?.relationType.tag : undefined,
    };

    this.steps.push(step);
    if (visited.include?.length > 0) {
      const length = visited.include.length;
      step.i = [];
      for (let i = 0; i < length; i++) {
        const node = visited.include[i];
        node.accept(this);
        step.i[i] = this.nodes.pop();
      }
    }

    if (visited.next != null) {
      visited.next.accept(this);
      step.n = this.steps.pop();
    }
  }

  public VisitUnion(visited: Union) {
    const extent: Extent = {
      k: ExtentKind.Union,
    };

    this.extents.push(extent);

    if (visited.operands != null) {
      const length = visited.operands.length;
      extent.o = [];
      for (let i = 0; i < length; i++) {
        const operand = visited.operands[i];
        operand.accept(this);
        extent.o[i] = this.extents.pop();
      }
    }

    if (visited.sorting?.length > 0) {
      const length = visited.sorting.length;
      extent.s = [];
      for (let i = 0; i < length; i++) {
        const sorting = visited.sorting[i];
        sorting.accept(this);
        extent.s[i] = this.sorts.pop();
      }
    }
  }

  public VisitProcedure(procedure: DataProcedure) {
    const c = {};
    for (const [name, objects] of procedure.collections) {
      c[name] = objects;
    }

    const o = {};
    for (const [name, object] of procedure.objects) {
      o[name] = object;
    }

    const v = {};
    for (const [name, value] of procedure.values) {
      v[name] = value;
    }

    const p = [];
    for (const [object, version] of procedure.pool) {
      p.push([object.id, version]);
    }

    this.Procedure = {
      n: procedure.name,
      c,
      o,
      v,
      p,
    };
  }
}
