import versionJson from '../version.json' with { type: 'json' };

// The documented version follows version.json, which Nerdbank.GitVersioning stamps on the builds: the
// label takes its major and minor numbers. Nothing built from main is a release, and the label says so.
const [release] = versionJson.version.match(/^\d+\.\d+/) ?? [];
if (!release) {
  throw new Error(`version.json holds the version '${versionJson.version}'. Start it with a major and a minor number, such as 3.2, so that the documentation can name the version it documents.`);
}

export const title = 'Allors3';
export const description = 'Documentation for developers who build applications on Allors3, and for the maintainers of the platform.';
export const version = `main · v${release} (unreleased)`;
export const repository = 'https://github.com/allors/allors3';
export const branch = 'main';
// The root of the repository, which holds this website in docs/. A link from a page to a file that is not
// a page becomes a link to that file on GitHub, by its path from this root.
export const repositoryRoot = new URL('../', import.meta.url);
// The audiences that a page can name, in the order in which the agent index lists them. The content
// schema accepts these names and no others.
export const audiences = ['Users and maintainers', 'Users', 'Maintainers'];
export const site = process.env.DOCS_SITE || 'https://docs.allors.com';
const trimmed = (process.env.DOCS_BASE || '/allors3/').split('/').filter(Boolean).join('/');
export const base = trimmed ? `/${trimmed}/` : '/';
// The documentation home is published at the root of the same origin by allors/allors.github.io. The
// sidebar links to it, and the link checker leaves that link to the home's own build.
export const home = new URL('/', site).href;
