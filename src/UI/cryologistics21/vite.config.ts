import { defineConfig } from 'vitest/config';
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
  test: {
    environment: 'jsdom',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'lcov'],
      include: ['src/**/*.{ts,tsx}'],
      exclude: ['src/**/*.test.{ts,tsx}', 'src/main.tsx'],
    },
  },
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
