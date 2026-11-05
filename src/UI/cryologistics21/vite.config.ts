import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

const apiTarget = process.env.USERS_API_HTTPS || process.env.USERS_API_HTTP;

console.log(
  'API environment variables:',
  Object.keys(process.env).filter(key => key.includes('API'))
);

if (!apiTarget) {
  throw new Error('API endpoint is not configured.');
}

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [plugin()],
  server: {
    port: 64146,
    proxy: {
      '/api': {
        target: apiTarget,
        changeOrigin: true,
        secure: false,
      },
    },
  }
})
