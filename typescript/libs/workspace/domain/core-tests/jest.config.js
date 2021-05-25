module.exports = {
  displayName: 'workspace-domain-core-tests',
  preset: '../../../../jest.preset.js',
  globals: {
    'ts-jest': {
      tsconfig: '<rootDir>/tsconfig.spec.json',
    },
  },
  transform: {
    '^.+\\.[tj]sx?$': 'ts-jest',
  },
  moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx'],
  coverageDirectory: '../../../../coverage/libs/workspace/domain/core-tests',
  // Allors
  maxWorkers: 1,
  testTimeout: 60000,
  reporters: [
    'default',
    [
      'jest-trx-results-processor',
      {
        outputFile: '../artifacts/tests/typscript.workspace.domain.core.trx',
      },
    ],
  ],
};
