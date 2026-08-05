import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

// The dev server proxies /api/* to the Web.Api project (http://localhost:5000), so no CORS
// configuration is needed during development. The prefix is passed through rather than
// stripped: the API maps its endpoints under /api so that dev and the single-origin
// production deployment (SPA served from the API's wwwroot) address the same paths.
const apiProxy = {
  '/api': {
    target: 'http://localhost:5000',
    changeOrigin: true,
  },
  '/uploads': {
    target: 'http://localhost:5000',
    changeOrigin: true,
  },
};

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    /*
     * Force a single React instance. A leftover pnpm store (`node_modules/.pnpm`,
     * react 19.2.0) sits alongside the npm tree (19.2.7), and if a library
     * resolves the other copy its hooks read a null dispatcher — the
     * "Cannot read properties of null (reading 'useRef')" failure.
     */
    dedupe: ['react', 'react-dom'],
  },
  optimizeDeps: {
    /*
     * Pre-bundle these at startup instead of letting Vite discover them the
     * first time a lazy route imports one. Mid-session discovery forces a
     * re-optimise and full reload, and a tab caught between the two ends up
     * holding two module graphs.
     */
    include: ['react-hook-form', '@hookform/resolvers/zod', 'zod', '@tanstack/react-query'],
  },
  build: {
    rollupOptions: {
      output: {
        /*
         * Framework code changes on a dependency-upgrade cadence; app code
         * changes every deploy. Splitting them means a normal release only
         * invalidates the app chunk, and returning visitors keep the ~180 kB
         * of React and friends they already have cached.
         */
        manualChunks: {
          'vendor-react': ['react', 'react-dom', 'react-router-dom'],
          'vendor-query': ['@tanstack/react-query'],
          // Forms are only reached on the two form-bearing routes, both lazy —
          // keeping this a named chunk stops it settling into the entry bundle.
          'vendor-forms': ['react-hook-form', '@hookform/resolvers/zod', 'zod'],
        },
      },
    },
  },
  server: {
    port: 5173,
    proxy: apiProxy,
  },
  // `vite preview` needs the same proxy, otherwise the production bundle can
  // only be smoke-tested against a dead API.
  preview: {
    port: 4173,
    proxy: apiProxy,
  },
});
