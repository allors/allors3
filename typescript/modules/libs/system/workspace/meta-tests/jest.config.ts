/* eslint-disable */
const trxFile = process.env.ALLORS_TRX_OUTPUT_FILE;

export default {
  displayName: 'system-workspace-meta-tests',
  preset: '../../../../jest.preset.js',
  globals: {
    'ts-jest': {
      tsconfig: '<rootDir>/tsconfig.spec.json',
    },
  },
  ...(trxFile
    ? {
        reporters: [
          'default',
          ['jest-trx-results-processor', { outputFile: trxFile }],
        ],
      }
    : {}),
  transform: {
    '^.+\\.[tj]sx?$': 'ts-jest',
  },
  moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx'],
  coverageDirectory: '../../../../coverage/libs/system/workspace/meta-tests',
};
