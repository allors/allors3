/* eslint-disable */
export default {
  displayName: 'core-workspace-derivations-custom',
  preset: '../../../../jest.preset.js',
  globals: {},
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
    '../../../../coverage/libs/core/workspace/derivations-custom',
};
