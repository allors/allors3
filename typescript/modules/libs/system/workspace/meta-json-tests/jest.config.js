module.exports = {
  displayName: 'system-workspace-meta-json-tests',
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
    '../../../../coverage/libs/system/workspace/meta-json-tests',
  // Allors
  reporters: [
    'default',
    [
      'jest-trx-results-processor',
      {
        outputFile:
          '../../artifacts/tests/typscript.system-workspace-meta-json.trx',
      },
    ],
  ],
};
