module.exports = {
  displayName: 'system-workspace-adapters-json-tests',
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
  coverageDirectory:
    '../../../../coverage/libs/system/workspace/adapters-json-tests',
  // Allors
  reporters: [
    'default',
    [
      'jest-trx-results-processor',
      {
        outputFile:
          '../../artifacts/tests/typscript.workspace-adapters-json-system.trx',
      },
    ],
  ],
  maxWorkers: 1,
  testTimeout: 60000 * 10,
};
