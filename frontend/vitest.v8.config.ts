import{defineConfig}from'vitest/config';
export default defineConfig({test:{include:['src/**/*.test.ts','src/**/*.test.tsx'],environment:'jsdom',setupFiles:['./src/test-setup-v8.ts'],restoreMocks:true,clearMocks:true}});
