/* eslint-disable */
export default {
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
  maxWorkers: 1,
  testTimeout: 60000 * 10,
};
