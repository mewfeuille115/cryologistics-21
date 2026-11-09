import { useState } from 'react'
import { LoginForm } from '../components/LoginForm'
import { AppHeader } from '../../../shared/components/AppHeader'
import { TelemetryPreview } from '../../telemetry/components/TelemetryPreview'
import './LoginPage.css'

export function LoginPage() {
  const [authenticated, setAuthenticated] = useState(false)

  return (
    <>
      <AppHeader />
      <main className="login-layout">
        <section className="auth-section" aria-labelledby="login-title">
          <div className="auth-panel">
            <div className="auth-heading">
              <p className="eyebrow">ACCESO SEGURO</p>
              <h2 id="login-title">Iniciar sesión</h2>
              <p>Usa tu cuenta corporativa de CryoLogistics.</p>
            </div>

            {authenticated ? (
              <output className="form-message form-message-success">
                <span className="material-symbols" aria-hidden="true">verified_user</span>
                <span>Acceso validado. La autenticación respondió correctamente.</span>
              </output>
            ) : (
              <LoginForm onAuthenticated={() => setAuthenticated(true)} />
            )}

            <div className="compliance-notice">
              <strong>Aviso de cumplimiento</strong>
              <p>Los accesos y acciones se registran con usuario y hora UTC (FDA 21 CFR Part 11).</p>
            </div>
          </div>
          <p className="access-help">¿Sin acceso? Pide a tu administrador que te asigne un rol.</p>
        </section>
        <TelemetryPreview />
      </main>
    </>
  )
}
