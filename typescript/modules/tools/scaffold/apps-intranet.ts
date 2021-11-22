#! /usr/bin/env node
import * as fs from 'fs';
import { PathResolver } from './src/helpers';
import { Project } from './src/project';

const pathResolver = new PathResolver('./apps/angular/apps/intranet');
const project = new Project(pathResolver, 'tsconfig.app.json');

console.log();
console.log('Scaffold');
console.log('========');

console.log('-> Project');
fs.mkdirSync('./dist/apps-intranet', { recursive: true } as any);
fs.writeFileSync('./dist/intranet/project.json', JSON.stringify(project));
