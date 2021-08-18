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
} from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initPull(null, fixture.reactiveDatabaseClient, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
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