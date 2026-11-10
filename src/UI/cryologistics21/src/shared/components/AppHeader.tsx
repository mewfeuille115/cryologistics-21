import { useEffect, useState } from 'react'

function getUtcTime() {
  return `${new Date().toISOString().slice(11, 19)} UTC`
}

export function AppHeader() {
  const [utcTime, setUtcTime] = useState(getUtcTime)
  const [theme, setTheme] = useState(() => {
    try {
      return localStorage.getItem('cryo-theme') === 'light' ? 'light' : 'dark'
    } catch {
      return 'dark'
    }
  })

  useEffect(() => {
    document.documentElement.dataset.theme = theme
  }, [theme])

  useEffect(() => {
    const timer = window.setInterval(() => setUtcTime(getUtcTime()), 1000)
    return () => window.clearInterval(timer)
  }, [])

  function toggleTheme() {
    const nextTheme = theme === 'dark' ? 'light' : 'dark'
    setTheme(nextTheme)
    try {
      localStorage.setItem('cryo-theme', nextTheme)
    } catch {
      // The selected theme still applies for this page view.
    }
  }

  return (
    <header className="app-header">
      <a className="brand" href="/" aria-label="CryoLogistics, inicio">
        <span className="brand-mark" aria-hidden="true">
          <span className="material-symbols">ac_unit</span>
        </span>
        <span className="brand-name">CryoLogistics</span>
        <span className="brand-standard">CFR 21</span>
      </a>
      <div className="header-tools">
        <time className="utc-clock" aria-label="Hora UTC">{utcTime}</time>
        <button
          className="icon-button"
          type="button"
          aria-label={`Cambiar a tema ${theme === 'dark' ? 'claro' : 'oscuro'}`}
          onClick={toggleTheme}
        >
          <span className="material-symbols" aria-hidden="true">
            {theme === 'dark' ? 'light_mode' : 'dark_mode'}
          </span>
        </button>
      </div>
    </header>
  )
}
