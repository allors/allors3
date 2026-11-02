/* eslint-disable */
const trxFile = process.env.ALLORS_TRX_OUTPUT_FILE;

module.exports = {
  displayName: 'system-workspace-meta-json-tests',
  preset: '../../../../jest.preset.js',
  globals: {},
  ...(trxFile
    ? {
        reporters: [
          'default',
          ['jest-trx-results-processor', { outputFile: trxFile }],
        ],
      }
    : {}),
  transform: {
    '^.+\\.[tj]sx?$': [
      'ts-jest',
      {
        tsconfig: '<rootDir>/tsconfig.spec.json',
      },
    ],
  },
  moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx'],
  coverageDirectory:
    '../../../../coverage/libs/system/workspace/meta-json-tests',
};
