export function TelemetryPreview() {
  return (
    <section className="telemetry-section" aria-labelledby="telemetry-title">
      <div className="intro-copy">
        <p className="eyebrow">CONTROL DE CADENA DE FRÍO</p>
        <h1 id="telemetry-title">Cadena de frío bajo control, firmada y auditada</h1>
        <p className="intro-description">
          Supervisa terapias génicas a −70 °C y conserva un registro auditable de cada envío.
        </p>
      </div>

      <div className="telemetry-panel">
        <div className="panel-heading">
          <span>Temperatura del contenedor</span>
          <span className="status-chip"><span aria-hidden="true" />En rango</span>
        </div>
        <p className="temperature-value">−71.2<span>°C</span></p>
        <div className="telemetry-metrics">
          <div className="metric">
            <span>Batería</span>
            <strong>87 %</strong>
          </div>
          <div className="metric">
            <span>Umbral ilustrativo</span>
            <strong className="metric-alert">−65.0 °C</strong>
          </div>
          <div className="metric">
            <span>Sensor</span>
            <strong>mx-09</strong>
          </div>
        </div>
        <p className="sample-note">Datos de ejemplo, no representan un envío en tiempo real.</p>
      </div>
    </section>
  )
}
