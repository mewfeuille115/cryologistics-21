import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

const apiTarget =
  process.env.GATEWAY_HTTPS ||
  process.env.GATEWAY_HTTP;

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
