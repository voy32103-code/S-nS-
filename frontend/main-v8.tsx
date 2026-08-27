import React from'react';import{createRoot}from'react-dom/client';import{FluentProvider,webLightTheme}from'@fluentui/react-components';import AppV8 from'./src/AppV8';

const sansoTheme={...webLightTheme,colorBrandBackground:'#176b4b',colorBrandBackgroundHover:'#12583e',colorBrandBackgroundPressed:'#0d4934',colorBrandForeground1:'#12583e',colorCompoundBrandForeground1:'#12583e',fontFamilyBase:'"Segoe UI Variable", "Segoe UI", system-ui, sans-serif',borderRadiusMedium:'8px',borderRadiusLarge:'14px'};

createRoot(document.getElementById('root')!).render(<React.StrictMode><FluentProvider theme={sansoTheme}><AppV8/></FluentProvider></React.StrictMode>);