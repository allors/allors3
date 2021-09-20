module.exports = {
  displayName: 'workspace-domain-tests',
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
  coverageDirectory: '../../../../coverage/libs/workspace/domain/tests',
  // Allors: sequential with extra time
  maxWorkers: 1,
  testTimeout: 60000,
};
