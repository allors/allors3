export const version = 'main · v3.2 (unreleased)';
export const repository = 'https://github.com/allors/allors3';
export const branch = 'main';
export const site = process.env.DOCS_SITE || 'https://docs.allors.com';
export const base = `/${(process.env.DOCS_BASE || '/allors3/').replace(/^\/+|\/+$/g, '')}/`.replace('//', '/');
// The documentation home is published at the root of the same origin by allors/allors.github.io. The
// sidebar links to it, and the link checker leaves that link to the home's own build.
export const home = new URL('/', site).href;
