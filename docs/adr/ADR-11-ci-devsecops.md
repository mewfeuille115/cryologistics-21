# ADR-11: CI / DevSecOps con GitHub Actions y SonarCloud

- **Estado:** Aceptado
- **Fecha:** 2026-11-07

## Contexto

CryoLogistics 21 es un proyecto de portafolio público que debe demostrar prácticas de calidad y seguridad alineadas con un entorno regulado (FDA 21 CFR Part 11), sin incurrir en costos de infraestructura cloud.

## Decisión

- **Orquestador:** GitHub Actions, con workflows reutilizables (`workflow_call`) en un repositorio aparte (`reusable-workflows`), versionados por tag.
- **Calidad de código:** SonarCloud, con Quality Gate sobre código nuevo (cobertura > 80 %, ratings A, duplicación < 3 %).
- **Seguridad:** detect-secrets (secretos), Snyk (SAST y SCA), Dependency Review y Trivy (imágenes Docker).
- **Alcance:** solo CI. Sin CD.
- **Control de cambios:** `main` protegida; todo cambio entra por Pull Request con el check `CI gate` obligatorio.

## Alternativas descartadas

- **Jenkins autoalojado:** sin visibilidad pública de los status checks.
- **SonarQube Community local:** sin PR decoration, análisis de PRs ni taint analysis.
- **Despliegue continuo en cloud:** costo de infraestructura no justificado para un portafolio.

## Consecuencias

- (+) Cada PR muestra públicamente el resultado de los controles.
- (+) Los workflows se reutilizan en otros proyectos.
- (−) Dependencia de servicios externos (Snyk, SonarCloud) y de sus tokens.
- (−) Los PRs desde forks no pueden ejecutar los análisis que requieren secrets.