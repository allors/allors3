/* eslint-disable */
const trxFile = process.env.ALLORS_TRX_OUTPUT_FILE;

export default {
  displayName: 'base-workspace-angular-foundation-tests',
  preset: '../../../../../jest.preset.js',
  setupFilesAfterEnv: ['<rootDir>/src/test-setup.ts'],
  globals: {
    'ts-jest': {
      tsconfig: '<rootDir>/tsconfig.spec.json',
      stringifyContentPathRegex: '\\.(html|svg)$',
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
  coverageDirectory:
    '../../../../../coverage/libs/base/workspace/angular/foundation-tests',
  // Angular ships its packages as ESM (.mjs), which plain ts-jest cannot load — the same
  // jest-preset-angular transform the Angular apps use is required here.
  transform: {
    '^.+\\.(ts|mjs|js|html)$': 'jest-preset-angular',
  },
  transformIgnorePatterns: ['node_modules/(?!.*\\.mjs$)'],
  snapshotSerializers: [
    'jest-preset-angular/build/serializers/no-ng-attributes',
    'jest-preset-angular/build/serializers/ng-snapshot',
    'jest-preset-angular/build/serializers/html-comment',
  ],
};
