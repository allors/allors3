import { FullConfig } from '@playwright/test';
import { ApiClient } from './api-client';

async function globalSetup(config: FullConfig) {
  const apiClient = new ApiClient();

  console.log('Waiting for server to be ready...');
  await apiClient.waitForServer(60000);
  console.log('Server is ready.');

  console.log('Setting up test database...');
  await apiClient.setup('full');
  console.log('Database setup complete.');
}

export default globalSetup;
