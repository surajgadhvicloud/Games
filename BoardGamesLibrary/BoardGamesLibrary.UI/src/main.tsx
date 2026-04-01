import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import ErrorBoundary from './presentation/components/ErrorBoundary.tsx'

// Catch any module-level or async errors before React mounts
window.addEventListener('error', (e) => {
  const root = document.getElementById('root')
  if (root && !root.hasChildNodes()) {
    root.innerHTML = `<div style="padding:20px;color:red;font-family:sans-serif"><h2>Global JS Error</h2><pre style="white-space:pre-wrap">${e.message}\n${e.filename}:${e.lineno}\n${e.error?.stack ?? ''}</pre></div>`
  }
})
window.addEventListener('unhandledrejection', (e) => {
  const root = document.getElementById('root')
  if (root) {
    root.innerHTML = `<div style="padding:20px;color:red;font-family:sans-serif"><h2>Unhandled Promise Rejection</h2><pre style="white-space:pre-wrap">${String(e.reason)}</pre></div>`
  }
})

const rootEl = document.getElementById('root')
if (!rootEl) {
  document.body.innerHTML = '<h1 style="color:red;padding:20px">ERROR: #root element not found!</h1>'
} else {
  createRoot(rootEl).render(
    <StrictMode>
      <ErrorBoundary>
        <App />
      </ErrorBoundary>
    </StrictMode>,
  )
}
