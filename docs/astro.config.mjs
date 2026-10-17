import { defineConfig } from 'astro/config';
import { unified } from '@astrojs/markdown-remark';
import starlight from '@astrojs/starlight';
import mermaid from 'astro-mermaid';
import sourceLinks from './integrations/source-links.mjs';
import scrollableContent from './integrations/scrollable-content.mjs';
import documentationExports from './integrations/exports.mjs';
import { site, base, repository } from './site.mjs';

export default defineConfig({
  site,
  base,
  trailingSlash: 'always',
  markdown: {
    processor: unified({ remarkPlugins: [[sourceLinks, { base }]], rehypePlugins: [scrollableContent] }),
  },
  integrations: [
    mermaid({ autoTheme: true, enableLog: false, mermaidConfig: { securityLevel: 'strict' } }),
    starlight({
      title: 'Allors3',
      favicon: '/favicon.ico',
      description: 'Documentation for application developers and platform maintainers.',
      social: [{ icon: 'github', label: 'GitHub', href: repository }],
      editLink: { baseUrl: `${repository}/edit/main/docs/` },
      components: { PageTitle: './src/components/PageTitle.astro' },
      customCss: ['./src/styles/documentation.css'],
      sidebar: [
        { label: 'Documentation', slug: '' },
        { label: 'For users', items: [
          { slug: 'domains' }, { slug: 'authentication' }, { slug: 'entra' },
          { slug: 'logging' }, { slug: 'terminology' },
        ] },
        { label: 'For maintainers', items: [{ autogenerate: { directory: 'internals' } }] },
      ],
    }),
    documentationExports(),
  ],
});
