module.exports = {
  preset: '../../../jest.preset.js',
  globals: {
    'ts-jest': {
      tsConfig: '<rootDir>/tsconfig.spec.json',
    },
  },
  transform: {
    '^.+\\.[tj]sx?$': 'ts-jest',
  },
  moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx'],
  coverageDirectory: '../../../coverage/libs/meta/tests',
  reporters: [
    'default',
    [
      'jest-trx-results-processor',
      {
        outputFile: '../../../artifacts/tests/core.meta.trx',
      },
    ],
  ],
  displayName: 'meta-tests',
};
