module.exports = {
  displayName: 'workspace-meta-json-system-tests',
  preset: '../../../../../jest.preset.js',
  globals: {
    'ts-jest': {
      tsconfig: '<rootDir>/tsconfig.spec.json',
    },
  },
  transform: {
    '^.+\\.[tj]sx?$': 'ts-jest',
  },
  moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx'],
  coverageDirectory: '../../../../../coverage/libs/workspace/meta/json/system-tests',
  // Allors
  reporters: [
    'default',
    [
      'jest-trx-results-processor',
      {
        outputFile: '../artifacts/tests/typscript.workspace-meta-lazy-system.trx',
      },
    ],
  ],
};
