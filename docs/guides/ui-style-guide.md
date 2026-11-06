# CryoLogistics 21 — Guía de estilos

Basada en las 3 pantallas de Stitch (Dashboard operativo, Telemetría y monitoreo, Bandeja QA y firma digital) y en el `DESIGN.md` «CryoLogistics Precision». Filosofía: estilo shadcn/ui sobre Tailwind, tema oscuro «slate suave» por defecto, color estrictamente semántico y datos densos en tipografía monoespaciada.

## 1. Principios

- **Restricción calibrada:** nada de negro puro; el fondo oscuro es un slate azulado que reduce fatiga en turnos largos.
- **Color semántico:** cian = activo/acción, esmeralda = nominal, ámbar = deriva/pendiente, rosa = excursión/crítico. No se usa color decorativo.
- **Densidad con aire:** tablas compactas, bordes de 1px (hairline) y profundidad por capas tonales, no por sombras pesadas.
- **Datos en mono:** IDs, lotes, hashes, timestamps UTC y temperaturas siempre en JetBrains Mono con cifras tabulares.

## 2. Color

Todo se define como variables CSS y se mapea en Tailwind (`darkMode: "class"`, clase `dark` en `<html>`).

### 2.1 Neutros y superficies

| Token | Oscuro | Claro | Uso |
| --- | --- | --- | --- |
| `--background` | `#151921` | `#f8fafc` | Fondo de la app (nivel 0) |
| `--card` / `--popover` | `#1e2430` | `#ffffff` | Paneles, tarjetas, drawers (nivel 1) |
| `--card-subtle` | `#181d26` | `#f1f5f9` | Insets, cabeceras de tabla, campos, sidebar |
| `--secondary` / `--muted` / `--accent` | `#232a38` | `#f1f5f9` | Hover, tab activa, chips neutros |
| `--border` | `#333d4f` | `#e2e8f0` | Bordes de 1px y divisores |
| `--border-subtle` | `#293240` | `#cbd5e1` | Divisores internos |
| `--foreground` | `#f8fafc` | `#0f172a` | Texto principal |
| `--muted-foreground` | `#94a3b8` | `#64748b` | Etiquetas, unidades, metadatos |
| `--primary` / `--ring` | `#38bdf8` | `#0284c7` | Acento, foco, enlaces |
| `--primary-foreground` | `#082f49` | `#ffffff` | Texto sobre primario |
| `--destructive` | `#f43f5e` | `#f43f5e` | Excursión, rechazo |

Botón primario sólido: `#0ea5e9` (sky-500) con hover `#38bdf8` (sky-400).

### 2.2 Colores de estado

Siempre en trío fondo translúcido + texto + borde translúcido.

| Estado | Fondo | Texto | Borde | Cuándo |
| --- | --- | --- | --- | --- |
| Nominal / OK | `emerald-500/10–15` | `emerald-400` (`#34d399`) | `emerald-500/25–30` | En rango, validado, entregado, sincronizado |
| Advertencia | `amber-500/15` | `amber-300` (`#fcd34d`) | `amber-500/30` | Pendiente, preparación, deriva previa |
| Crítico / Excursión | `rose-500/10–15` | `rose-400` (`#fb7185`) | `rose-500/30` | Fuera de umbral, alerta, rechazo |
| Activo / Info | `sky-500/10` | `sky-400` (`#38bdf8`) | `sky-500/25` | En tránsito, stream activo, etiquetas |
| Neutro | `--muted` | `--muted-foreground` | `--border` | Backfilled, reconciliado, sin iniciar |

Base: esmeralda `#10b981`, ámbar `#f59e0b`, rosa `#f43f5e`, cian `#0ea5e9`.

### 2.3 Gráficas

- Línea de datos: `#cbd5e1` (en vivo: blanco `#f8fafc`). Rejilla: `#2c3545`. Base/ejes: `#64748b`.
- Umbral crítico: línea discontinua rosa `#f43f5e` con etiqueta `-65.0°C LÍMITE DE ALARMA`. Objetivo: discontinua gris.
- Área bajo la curva: degradado `#38bdf8` de 18% a 0% de opacidad; el tramo en excursión se pinta en rosa.
- Último punto: círculo con halo; punto de excursión: marcador rosa con etiqueta mono.

## 3. Tipografía

- **Geist** (300–700) para interfaz y títulos. **JetBrains Mono** (400–600) para datos. **Material Symbols Outlined** para iconos (13–18 px, nunca más grandes en tablas).
- Fallbacks: `Geist, Inter, sans-serif` y `JetBrains Mono, monospace`.
- Cifras tabulares (`tabular-nums` / `font-variant-numeric: tabular-nums`) en todo valor numérico, timestamp y sensor.
- Títulos con peso 500–600 y `tracking-tight`; nunca bold en títulos de tabla.

| Rol | Clases Tailwind | Tamaño | Dónde |
| --- | --- | --- | --- |
| Valor de telemetría hero | `font-mono text-5xl font-semibold tracking-tight tabular-nums` | 48 px | Temperatura en vivo (`-71.2 °C`; unidad en `text-xl` atenuada) |
| Métrica KPI | `font-mono text-3xl font-bold tracking-tight` | 30 px | Tarjetas del dashboard |
| Título de página | `text-2xl font-semibold tracking-tight` | 24 px | «Supervisión de Envíos Críticos» |
| Título de panel | `text-base font-semibold` (15–16 px) | 16 px | «Curva de Telemetría…» |
| Cuerpo / controles | `text-sm` | 14 px | Texto, inputs, botones principales |
| Tabla, botones pequeños | `text-xs` | 12 px | Celdas, filtros, ayudas |
| Micro-etiqueta | `text-[11px] font-medium` | 11 px | Chips, cabeceras de tabla en mono, hashes |
| Micro-etiqueta mono | `font-mono text-[10px] uppercase tracking-wider` | 10 px | Metadatos de ejes, tags |
| Cabecera de tabla | `text-xs uppercase font-medium tracking-wider text-muted-foreground` | 12 px | `<th>` |

## 4. Espaciado y layout

- Todo en múltiplos de 4 px. Gaps habituales: `gap-1.5` (6), `gap-2` (8), `gap-3` (12). Agrupaciones de telemetría: 4–8 px.
- Contenedor: `max-w-7xl mx-auto px-4 sm:px-6 lg:px-8`; `<main>` con `py-6`/`py-8` y `space-y-6`/`space-y-7` entre bloques.
- Header: sticky o fixed, `h-14`, `border-b`, fondo `background/90` con `backdrop-blur-md`.
- Sidebar (solo `xl`): `fixed left-0 top-14 bottom-0 w-60`, `border-r`, fondo `--card-subtle`, `p-4`.
- Tarjeta KPI `p-5`; tarjeta interior `p-3.5`; celdas de tabla `px-4 py-3.5` (listas) o `py-2.5` (logs densos).
- Breakpoints: `sm` 640 (una columna), `md` 768 (nav visible), `lg` 1024 (12 col), `xl` 1280 (sidebar + alta densidad). Grids habituales: 4 columnas KPI, 2/3 + 1/3 para panel principal y lateral.

## 5. Forma y elevación

| Elemento | Radio |
| --- | --- |
| Chips de estado, puntos y avatares | `rounded-full` |
| Botones, inputs, selects, tabs | `rounded-md` (6 px) |
| Tarjetas, tablas, paneles | `rounded-lg` (8 px) |
| Alertas grandes, drawers, modales | `rounded-xl` (12 px) |
| Tags mono pequeños (`CFR 21`, `#SN-994`) | `rounded` (4 px) |

Elevación por capas: fondo → tarjeta con borde de 1px → popover/drawer. Sombras mínimas: `shadow-xs`/`shadow-sm` en tarjetas y botones; el drawer usa `shadow-2xl`; solo los puntos «live» llevan brillo (`0 0 8px rgba(52,211,153,.6)`). Para overlays del `DESIGN.md`: `0 8px 24px -4px rgba(0,0,0,.45)` en oscuro y `0 10px 25px -5px rgba(15,23,42,.08)` en claro.

## 6. Componentes

### 6.1 Header y navegación

- Logo: cuadro `h-7 w-7 rounded-md` con icono `ac_unit` en `text-sky-400`, junto a «CryoLogistics» (`text-sm font-semibold`) y un tag mono (`CFR 21`).
- Tab: `px-3 py-1.5 rounded-md text-sm font-medium`. Inactiva: `text-muted-foreground hover:text-foreground hover:bg-secondary/60`. Activa: `bg-secondary border border-border text-foreground shadow-sm`.
- A la derecha: píldora de estado SSE (punto esmeralda pulsante + `font-mono text-xs`), reloj UTC, toggle de tema y avatar circular con iniciales y rol.

### 6.2 Botones

| Variante | Clases |
| --- | --- |
| Primario | `px-3.5 py-2 rounded-md bg-sky-500 hover:bg-sky-400 text-slate-950 text-sm font-medium shadow-sm` |
| Secundario | `px-3 py-1.5 rounded-md border border-border bg-card hover:bg-secondary text-xs font-medium` |
| Inverso (exportar) | `px-3.5 py-1.5 rounded-lg bg-slate-50 hover:bg-slate-200 text-slate-900 text-xs font-medium` |
| Destructivo (suave) | `border border-rose-500/30 bg-rose-500/10 text-rose-400 hover:bg-rose-500/20` |
| Icono / ghost | `p-1 text-muted-foreground hover:text-foreground` |

Todos con `inline-flex items-center gap-1.5` y `transition-colors`. El icono va a la izquierda a 15–18 px.

### 6.3 Chips de estado

`inline-flex items-center gap-1.5 px-2 py-0.5 rounded-full text-[11px] font-medium border` + el trío de color de la sección 2.2. Pueden llevar un punto de 6 px (`h-1.5 w-1.5 rounded-full`) a la izquierda. En excursión crítica se admite `animate-pulse`. Para IDs y valores límite usar tag rectangular `rounded px-1.5 py-0.5 font-mono`.

### 6.4 Tarjeta de métrica (KPI)

`bg-card border border-border rounded-lg p-5 shadow-xs`. Estructura: fila superior con etiqueta `text-xs text-muted-foreground` y chip de estado; valor en `font-mono text-3xl font-bold` con unidad a la derecha en `text-xs text-muted-foreground`; pie con icono + frase de estado. En alarma, el valor se pinta `text-rose-400` y el chip pasa a crítico. Tarjetas de nodo (batería, vacío, presión, MKT): inset `bg-card-subtle border rounded-lg p-3.5` con valor mono `text-lg`.

### 6.5 Tablas

- Contenedor: `border border-border rounded-lg overflow-hidden`, con pie de paginación (`Anterior` / `Siguiente`) separado por borde.
- Cabecera: `bg-card-subtle/80 border-b border-border text-xs uppercase font-medium tracking-wider text-muted-foreground`, `py-3 px-4`.
- Filas: `border-b border-border`, `hover:bg-card-subtle/60 transition-colors`. IDs, temperaturas, timestamps y hashes en `font-mono tabular-nums`.
- Fila en alarma: `bg-rose-500/[0.04]` (o `/5`) con texto rosa en la secuencia y la temperatura. Alinear números a la derecha.
- Identificador de dos líneas: ID mono en `foreground` y lote en `text-[11px] text-muted-foreground`.

### 6.6 Formularios

- Input: `w-full rounded-md border border-border bg-input px-3 py-2 text-sm placeholder:text-muted-foreground/60 focus:outline-none focus:border-sky-500 focus:ring-1 focus:ring-sky-500`.
- Variante mono (`font-mono`) para lote, UID, RFID y credencial de firma.
- Buscador: icono a la izquierda con `pl-9`. Select: igual que input, `appearance-none` con chevron propio. Textarea: `leading-relaxed`, `rows=3`.
- Etiqueta: `text-xs font-medium uppercase tracking-wider`, con asterisco rosa en obligatorios y ayuda a la derecha en `text-[11px] text-muted-foreground font-mono`.
- Decisión binaria (Liberar / Rechazar): dos tarjetas-radio `rounded-md border`; seleccionada = tinte semántico (esmeralda o rosa) con icono.

### 6.7 Filtros segmentados

Contenedor `border border-border rounded-md p-0.5 bg-card`; botón activo `bg-secondary text-foreground shadow-xs rounded px-2.5 py-1 text-xs font-medium`; inactivo `text-muted-foreground`. Incluyen el conteo entre paréntesis.

### 6.8 Alertas y avisos

- Banner crítico: `rounded-xl border border-rose-500/30 bg-rose-500/10 px-4 py-3 text-xs text-rose-200 backdrop-blur-sm`, icono a la izquierda, tags mono incrustados (`03m 40s`) y botón de cierre.
- Aviso legal / informativo: caja `rounded-md border border-border bg-card-subtle p-3 text-[11px]` con título en mayúsculas.
- Conectividad: punto `h-1.5 w-1.5 rounded-full bg-emerald-400` con `animate-pulse` y brillo, más texto mono.

### 6.9 Paneles, gráficas y drawer

- Panel de gráfica: tarjeta `rounded-lg p-5`, título + chip (`DESVIACIÓN`, `PT100`), leyenda a la derecha con puntos de color, lienzo interior `rounded-lg border bg-card-subtle` y pie con acción.
- Etiquetas de ejes en mono 10–11 px; umbral con píldora de fondo translúcido.
- Drawer lateral: `fixed right-0 inset-y-0 max-w-md bg-card border-l border-border shadow-2xl p-6`, entra con `translate-x` en 200 ms.
- Audit trail: tabla densa con secuencia `#10842`, timestamp ISO UTC, evento, actor y hash truncado (`3f2e1...d98a`) con icono de copiar en `text-sky-400`.

## 7. Tokens listos para usar

### 7.1 Variables CSS

<!-- markdownlint-disable MD010 -->
```css
:root {
	--background: #f8fafc;
	--foreground: #0f172a;
	--card: #ffffff;
	--card-foreground: #0f172a;
	--card-subtle: #f1f5f9;
	--popover: #ffffff;
	--popover-foreground: #0f172a;
	--primary: #0284c7;
	--primary-foreground: #ffffff;
	--secondary: #e2e8f0;
	--secondary-foreground: #1e293b;
	--muted: #f1f5f9;
	--muted-foreground: #64748b;
	--accent: #f1f5f9;
	--accent-foreground: #0f172a;
	--destructive: #f43f5e;
	--destructive-foreground: #ffffff;
	--success: #10b981;
	--warning: #f59e0b;
	--border: #e2e8f0;
	--border-subtle: #cbd5e1;
	--input: #e2e8f0;
	--ring: #0284c7;
	--chart-grid: #e2e8f0;
	--chart-line: #0284c7;
	--chart-base: #94a3b8;
	--radius: 0.5rem;
}
.dark {
	--background: #151921;
	--foreground: #f8fafc;
	--card: #1e2430;
	--card-foreground: #f8fafc;
	--card-subtle: #181d26;
	--popover: #1e2430;
	--popover-foreground: #f8fafc;
	--primary: #38bdf8;
	--primary-foreground: #082f49;
	--secondary: #232a38;
	--secondary-foreground: #f8fafc;
	--muted: #232a38;
	--muted-foreground: #94a3b8;
	--accent: #232a38;
	--accent-foreground: #f8fafc;
	--destructive: #f43f5e;
	--destructive-foreground: #ffffff;
	--success: #10b981;
	--warning: #f59e0b;
	--border: #333d4f;
	--border-subtle: #293240;
	--input: #181d26;
	--ring: #38bdf8;
	--chart-grid: #2c3545;
	--chart-line: #cbd5e1;
	--chart-base: #64748b;
}
body {
	background-color: var(--background);
	color: var(--foreground);
	font-family: 'Geist', 'Inter', sans-serif;
	font-variant-numeric: tabular-nums;
	transition: background-color 0.25s ease, color 0.25s ease;
}
```

### 7.2 Configuración de Tailwind

```js
tailwind.config = {
	darkMode: "class",
	theme: {
		extend: {
			fontFamily: {
				sans: ["Geist", "Inter", "sans-serif"],
				mono: ["JetBrains Mono", "monospace"],
			},
			colors: {
				background: "var(--background)",
				foreground: "var(--foreground)",
				border: "var(--border)",
				input: "var(--input)",
				ring: "var(--ring)",
				card: { DEFAULT: "var(--card)", foreground: "var(--card-foreground)", subtle: "var(--card-subtle)" },
				popover: { DEFAULT: "var(--popover)", foreground: "var(--popover-foreground)" },
				primary: { DEFAULT: "var(--primary)", foreground: "var(--primary-foreground)" },
				secondary: { DEFAULT: "var(--secondary)", foreground: "var(--secondary-foreground)" },
				muted: { DEFAULT: "var(--muted)", foreground: "var(--muted-foreground)" },
				accent: { DEFAULT: "var(--accent)", foreground: "var(--accent-foreground)" },
				destructive: { DEFAULT: "var(--destructive)", foreground: "var(--destructive-foreground)" },
			},
			borderRadius: { lg: "var(--radius)", md: "calc(var(--radius) - 2px)" },
		},
	},
};
```

### 7.3 Fuentes e iconos

```html
<link href="https://fonts.googleapis.com/css2?family=Geist:wght@300;400;500;600;700&family=JetBrains+Mono:wght@400;500;600&display=swap" rel="stylesheet"/>
<link href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:wght,FILL@100..700,0..1&display=swap" rel="stylesheet"/>
```
<!-- markdownlint-enable MD010 -->

## 8. Inconsistencias entre pantallas y `DESIGN.md`

Las pantallas no comparten exactamente los mismos valores. En esta guía se unificó así; conviene confirmarlo antes de implementar.

| Punto | Lo que hay en el código | Decisión en esta guía |
| --- | --- | --- |
| Fondo oscuro | Dashboard `#161b24`; Telemetría y QA `#151921`; `DESIGN.md` `#181b20` | `#151921` (2 de 3 pantallas) |
| Tarjeta oscura | Dashboard `#1f2633`; Telemetría y QA `#1e2430`; `DESIGN.md` `#242933` | `#1e2430` |
| Borde oscuro | `#323b4b` (Dashboard); `#333d4f` (resto); `DESIGN.md` `#333a46` | `#333d4f` |
| Primario oscuro | `#38bdf8` (Dashboard, Telemetría); `#0ea5e9` (QA y `DESIGN.md`) | `#38bdf8` como acento; `#0ea5e9` como fondo de botón sólido |
| Primario claro | `#0284c7` (Dashboard, Telemetría); `#0ea5e9` (QA) | `#0284c7` (mejor contraste) |
| Destructivo claro | `#ef4444` en las pantallas; `#f43f5e` en `DESIGN.md` | `#f43f5e` en ambos temas |
| Fuente de interfaz | Geist en Dashboard y Telemetría; Inter en QA | Geist (también en `DESIGN.md`); Inter solo como fallback |
| Radio de chips | `rounded-full` en pantallas; `DESIGN.md` también lo usa en chips pero prohíbe píldoras en tarjetas de datos | `rounded-full` solo en chips y puntos |
| Escala tipográfica | Pantallas usan 24/16/14/12/11/10 px; `DESIGN.md` define 32/24/18/16/14/13 px | Se documentó la escala real de las pantallas |

El modo claro solo existe como variables (las capturas son oscuras), así que sus valores salen del código y no están validados visualmente.

## 9. Reglas rápidas

- Usar solo los tokens; evitar hex sueltos salvo en los tintes de estado de la sección 2.2.
- Todo dato numérico, ID, hash o timestamp va en mono con cifras tabulares.
- El color solo comunica estado; no usar rosa ni esmeralda como decoración.
- Bordes de 1px en lugar de sombras para separar; sombras solo en overlays.
- Mantener `rounded-lg` en tarjetas y `rounded-md` en controles; evitar radios mayores en superficies de datos.
- Cada elemento interactivo lleva `transition-colors` y foco visible (`ring-1` cian).
- Los estados críticos combinan color, icono y texto (nunca solo color) para accesibilidad.
