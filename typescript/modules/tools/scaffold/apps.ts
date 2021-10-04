#! /usr/bin/env node
import * as fs from 'fs';
import { PathResolver } from './src/helpers';
import { Project } from './src/project';

const pathResolver = new PathResolver('./apps/angular/apps/app');
const project = new Project(pathResolver, 'tsconfig.app.json');

console.log();
console.log('Scaffold');
console.log('========');

console.log('-> Project');
fs.mkdirSync('./dist/apps', { recursive: true } as any);
fs.writeFileSync('./dist/apps/project.json', JSON.stringify(project));
