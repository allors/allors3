import { existsSync, statSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { visit } from 'unist-util-visit';
import { repository, branch } from '../site.mjs';

const contentRoot = fileURLToPath(new URL('../src/content/docs/', import.meta.url));
const repositoryRoot = fileURLToPath(new URL('../../', import.meta.url));

export function rewriteLink(url, source, base) {
  if (!url || /^(?:[a-z][a-z\d+.-]*:|\/|#)/i.test(url)) return url;
  const [, pathname, suffix = ''] = url.match(/^([^?#]*)(.*)$/);
  const target = path.resolve(path.dirname(source), decodeURIComponent(pathname));
  if (!existsSync(target)) throw new Error(`Broken documentation link in ${source}: ${url}. Create the target or correct the link.`);
  const contentPath = path.relative(contentRoot, target);
  if (!contentPath.startsWith('..') && /\.mdx?$/.test(contentPath)) {
    const slug = contentPath.replace(/\.mdx?$/, '').replace(/(^|\/)index$/, '').replace(/\/$/, '');
    return `${base}${slug ? `${slug}/` : ''}${suffix}`;
  }
  const repoPath = path.relative(repositoryRoot, target);
  if (repoPath.startsWith('..')) throw new Error(`Documentation link leaves the repository: ${url} in ${source}.`);
  const kind = statSync(target).isDirectory() ? 'tree' : 'blob';
  return `${repository}/${kind}/${branch}/${repoPath.split(path.sep).map(encodeURIComponent).join('/')}${suffix}`;
}

export default function sourceLinks({ base }) {
  return (tree, file) => {
    visit(tree, (node) => {
      if (node.type === 'link' || node.type === 'definition') {
        node.url = rewriteLink(node.url, file.path, base);
      }
    });
  };
}
