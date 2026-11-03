import { Node, nodeLeafs } from '@allors/system/workspace/domain';

// Regression for resolveLeafs (pointer/node.ts): it is a standalone function, so it must add the
// leaf `node` to the result set — not `this` (undefined). resolveLeafs only reads `node.nodes`, so
// the tree can be built from minimal Node literals. Leaves use `nodes: []`.
describe('nodeLeafs', () => {
  it('returns the leaf nodes of a tree (not undefined)', () => {
    const leafA = { nodes: [] } as unknown as Node;
    const leafB = { nodes: [] } as unknown as Node;
    const root = { nodes: [leafA, leafB] } as unknown as Node;

    const leafs = nodeLeafs(root);

    expect(leafs.has(leafA)).toBe(true);
    expect(leafs.has(leafB)).toBe(true);
    expect(leafs.size).toBe(2);
    expect(leafs.has(undefined as unknown as Node)).toBe(false);
  });

  it('returns a childless root as its own single leaf', () => {
    const root = { nodes: [] } as unknown as Node;

    const leafs = nodeLeafs(root);

    expect([...leafs]).toEqual([root]);
  });

  it('descends nested branches to the deepest leaves', () => {
    const deepLeaf = { nodes: [] } as unknown as Node;
    const branch = { nodes: [deepLeaf] } as unknown as Node;
    const root = { nodes: [branch] } as unknown as Node;

    const leafs = nodeLeafs(root);

    expect([...leafs]).toEqual([deepLeaf]);
  });
});
