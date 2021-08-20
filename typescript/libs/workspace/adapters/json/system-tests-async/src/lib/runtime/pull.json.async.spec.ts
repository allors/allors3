import {
  initPull,
  andGreaterThanLessThan,
  associationMany2ManyContainedIn,
  associationMany2ManyContains,
  associationMany2ManyExists,
  associationMany2OneContainedIn,
  associationMany2OneContains,
  associationOne2ManyContainedIn,
  associationOne2ManyEquals,
  associationOne2ManyExists,
  associationOne2ManyInstanceof,
  associationOne2OneContainedIn,
  associationOne2OneEquals,
  associationOne2OneExists,
  associationOne2OneInstanceof,
  objectEquals,
  extentInterface,
  instanceof_,
  notEquals,
  orEquals,
  operatorExcept,
  operatorIntersect,
  operatorUnion,
  roleDateTimeBetweenPath,
  roleDateTimeBetweenValue,
  roleDateTimeGreaterThanPath,
  roleDateTimeGreaterThanValue,
  roleDateTimeLessThanPath,
  roleDateTimeLessThanValue,
  roleDateTimeEquals,
  roleDecimalBetweenPath,
  roleDecimalBetweenValue,
  roleDecimalGreaterThanPath,
  roleDecimalGreaterThanValue,
  roleDecimalLessThanPath,
  roleDecimalLessThanValue,
  roleDecimalEquals,
  roleDoubleBetweenPath,
  roleDoubleBetweenValue,
  roleDoubleGreaterThanPath,
  roleDoubleGreaterThanValue,
  roleDoubleLessThanPath,
  roleDoubleLessThanValue,
  roleDoubleEquals,
  roleIntegerBetweenPath,
  roleIntegerBetweenValue,
  roleIntegerGreaterThanPath,
  roleIntegerGreaterThanValue,
  roleIntegerLessThanPath,
  roleIntegerLessThanValue,
  roleIntegerEquals,
  roleIntegerExist,
  roleStringEqualsPath,
  roleStringEqualsValue,
  roleStringLike,
  roleUniqueEquals,
  roleMany2ManyContainedIn,
  roleMany2ManyContains,
  roleOne2ManyContainedIn,
  roleOne2ManyContains,
  roleMany2OneContainedIn,
  roleOne2OneContainedIn,
  withResultName,
  pullWithObjectId,
  pullWithInclude,
} from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initPull(fixture.asyncDatabaseClient, null, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('andGreaterThanLessThan', async () => {
  await andGreaterThanLessThan();
});

test('associationMany2ManyContainedIn', async () => {
  await associationMany2ManyContainedIn();
});

test('associationMany2ManyContains', async () => {
  await associationMany2ManyContains();
});

test('associationMany2ManyExists', async () => {
  await associationMany2ManyExists();
});

test('associationMany2OneContainedIn', async () => {
  await associationMany2OneContainedIn();
});

test('associationMany2OneContains', async () => {
  await associationMany2OneContains();
});

test('associationOne2ManyContainedIn', async () => {
  await associationOne2ManyContainedIn();
});

test('associationOne2ManyEquals', async () => {
  await associationOne2ManyEquals();
});

test('associationOne2ManyExists', async () => {
  await associationOne2ManyExists();
});

test('associationOne2ManyInstanceof', async () => {
  await associationOne2ManyInstanceof();
});

test('associationOne2OneContainedIn', async () => {
  await associationOne2OneContainedIn();
});

test('associationOne2OneEquals', async () => {
  await associationOne2OneEquals();
});

test('associationOne2OneExists', async () => {
  await associationOne2OneExists();
});

test('associationOne2OneInstanceof', async () => {
  await associationOne2OneInstanceof();
});

test('objectEquals', async () => {
  await objectEquals();
});

test('extentInterface', async () => {
  await extentInterface();
});

test('instanceof', async () => {
  await instanceof_();
});

test('notEquals', async () => {
  await notEquals();
});

test('orEquals', async () => {
  await orEquals();
});

test('operatorExcept', async () => {
  await operatorExcept();
});

test('operatorIntersect', async () => {
  await operatorIntersect();
});

test('operatorUnion', async () => {
  await operatorUnion();
});

test('roleDateTimeBetweenPath', async () => {
  await roleDateTimeBetweenPath();
});

test('roleDateTimeBetweenValue', async () => {
  await roleDateTimeBetweenValue();
});

test('roleDateTimeGreaterThanPath', async () => {
  await roleDateTimeGreaterThanPath();
});

test('roleDateTimeGreaterThanValue', async () => {
  await roleDateTimeGreaterThanValue();
});

test('roleDateTimeLessThanPath', async () => {
  await roleDateTimeLessThanPath();
});

test('roleDateTimeLessThanValue', async () => {
  await roleDateTimeLessThanValue();
});

test('roleDateTimeEquals', async () => {
  await roleDateTimeEquals();
});

test('roleDecimalBetweenPath', async () => {
  await roleDecimalBetweenPath();
});

test('roleDecimalBetweenValue', async () => {
  await roleDecimalBetweenValue();
});

test('roleDecimalGreaterThanPath', async () => {
  await roleDecimalGreaterThanPath();
});

test('roleDecimalGreaterThanValue', async () => {
  await roleDecimalGreaterThanValue();
});

test('roleDecimalLessThanPath', async () => {
  await roleDecimalLessThanPath();
});

test('roleDecimalLessThanValue', async () => {
  await roleDecimalLessThanValue();
});

test('roleDecimalEquals', async () => {
  await roleDecimalEquals();
});

test('roleDoubleBetweenPath', async () => {
  await roleDoubleBetweenPath();
});

test('roleDoubleBetweenValue', async () => {
  await roleDoubleBetweenValue();
});

test('roleDoubleGreaterThanPath', async () => {
  await roleDoubleGreaterThanPath();
});

test('roleDoubleGreaterThanValue', async () => {
  await roleDoubleGreaterThanValue();
});

test('roleDoubleLessThanPath', async () => {
  await roleDoubleLessThanPath();
});

test('roleDoubleLessThanValue', async () => {
  await roleDoubleLessThanValue();
});

test('roleDoubleEquals', async () => {
  await roleDoubleEquals();
});

test('roleIntegerBetweenPath', async () => {
  await roleIntegerBetweenPath();
});

test('roleIntegerBetweenValue', async () => {
  await roleIntegerBetweenValue();
});

test('roleIntegerGreaterThanPath', async () => {
  await roleIntegerGreaterThanPath();
});

test('roleIntegerGreaterThanValue', async () => {
  await roleIntegerGreaterThanValue();
});

test('roleIntegerLessThanPath', async () => {
  await roleIntegerLessThanPath();
});

test('roleIntegerLessThanValue', async () => {
  await roleIntegerLessThanValue();
});

test('roleIntegerEquals', async () => {
  await roleIntegerEquals();
});

test('roleIntegerExist', async () => {
  await roleIntegerExist();
});

test('roleStringEqualsPath', async () => {
  await roleStringEqualsPath();
});

test('roleStringEqualsValue', async () => {
  await roleStringEqualsValue();
});

test('roleStringLike', async () => {
  await roleStringLike();
});

test('roleUniqueEquals', async () => {
  await roleUniqueEquals();
});

test('roleMany2ManyContainedIn', async () => {
  await roleMany2ManyContainedIn();
});

test('roleMany2ManyContains', async () => {
  await roleMany2ManyContains();
});

test('roleOne2ManyContainedIn', async () => {
  await roleOne2ManyContainedIn();
});

test('roleOne2ManyContains', async () => {
  await roleOne2ManyContains();
});

test('roleMany2OneContainedIn', async () => {
  await roleMany2OneContainedIn();
});

test('roleOne2OneContainedIn', async () => {
  await roleOne2OneContainedIn();
});

test('withResultName', async () => {
  await withResultName();
});

test('pullWithObjectId', async () => {
  await pullWithObjectId();
});

test('pullWithInclude', async () => {
  await pullWithInclude();
});

