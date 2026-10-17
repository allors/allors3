export const version = 'main · v3.2 (unreleased)';
export const repository = 'https://github.com/allors/allors3';
export const branch = 'main';
export const site = process.env.DOCS_SITE || 'https://allors.github.io';
export const base = `/${(process.env.DOCS_BASE || '/allors3/').replace(/^\/+|\/+$/g, '')}/`.replace('//', '/');
