import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

const apiTarget =
  process.env.GATEWAY_HTTPS ||
  process.env.GATEWAY_HTTP ||
  'http://localhost:5205';

if (!apiTarget) {
  throw new Error('GATEWAY_HTTPS undefined. Run the front from Aspire AppHost.');
}

console.log(apiTarget);

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
        rewrite: (path) => path.replace(/^\/api/, ''),
      },
    },
  },
})
