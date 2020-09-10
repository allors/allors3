import { UnitTypes, CompositeTypes, ParameterTypes, isSessionObject } from './Types';
import { SessionObject } from './SessionObject';

export function serializeAllDefined(role: UnitTypes | CompositeTypes): string {
  if (typeof role === 'string') {
    return role;
  }

  if (role instanceof Date) {
    return (role as Date).toISOString();
  }

  // TODO: better check or move to Database
  if (isSessionObject(role)) {
    return (role as SessionObject).id;
  }

  return role.toString();
}

export function serialize(role: UnitTypes | CompositeTypes | undefined): string | undefined {
  if (role == null) {
    return undefined;
  }

  return serializeAllDefined(role);
}

export function serializeObject(roles: { [name: string]: ParameterTypes } | undefined): { [name: string]: string } {
  if (roles) {
    return Object.keys(roles).reduce((obj, v) => {
      const role = roles[v];
      if (Array.isArray(role)) {
        obj[v] = role.map((w) => serialize(w)).join(',');
      } else {
        obj[v] = serialize(role);
      }
      return obj;
    }, {} as { [key: string]: any });
  }

  return {};
}
