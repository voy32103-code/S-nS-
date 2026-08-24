import{defineConfig}from'vite';import react from'@vitejs/plugin-react';
export default defineConfig({plugins:[{name:'sanso-v2-entry',transformIndexHtml(html){return html.replace('/src/main.tsx','/src/main-v2.tsx')}},react()]});
