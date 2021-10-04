#! /usr/bin/env node
import * as fs from 'fs';
import { PathResolver } from './src/helpers';
import { Project } from './src/project';

const pathResolver = new PathResolver('./apps/angular/base/app');
const project = new Project(pathResolver, 'tsconfig.app.json');

console.log();
console.log('Scaffold');
console.log('========');

console.log('-> Project');
fs.mkdirSync('./dist/base', { recursive: true } as any);
fs.writeFileSync('./dist/base/project.json', JSON.stringify(project));
