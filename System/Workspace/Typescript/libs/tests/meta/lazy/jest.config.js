module.exports = {
  displayName: 'tests-meta-lazy',
  preset: '../../../../jest.preset.js',
  globals: {
    'ts-jest': {
      tsConfig: '<rootDir>/tsconfig.spec.json',
    },
  },
  transform: {
    '^.+\\.[tj]sx?$': 'ts-jest',
  },
  moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx'],
  coverageDirectory: '../../../../coverage/libs/tests/meta/lazy',
  reporters: [
    'default',
    [
      'jest-trx-results-processor',
      {
        outputFile: '../../../artifacts/tests/system.workspace.json.trx',
      },
    ],
  ],
};
