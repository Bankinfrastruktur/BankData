import type {Config} from '@jest/types';

// Sync object
const config: Config.InitialOptions = {
  verbose: true,
  transform: {
    '\\.[jt]sx?$': 'ts-jest'
  },
  moduleNameMapper: {
    '(.+)\\.js$': '$1'
  },
};

export default config;