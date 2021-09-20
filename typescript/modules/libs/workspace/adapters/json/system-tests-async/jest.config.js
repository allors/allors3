module.exports = {
  displayName: 'workspace-adapters-json-system-tests-async',
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
  coverageDirectory: '../../../../../coverage/libs/workspace/adapters/json/system-tests-async',
  // Allors
  reporters: [
    'default',
    [
      'jest-trx-results-processor',
      {
        outputFile: '../artifacts/tests/typscript.workspace-adapters-json-async-system.trx',
      },
    ],
  ],
  maxWorkers: 1,
  testTimeout: 60000 * 10,
};
