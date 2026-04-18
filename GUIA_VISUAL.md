# Guía Visual — TFG_Santi (FPS Unity 6 URP)

Propuesta estética: **Neo-Samurái / Cyber-Ninja**. Base oscura con acentos carmesí (Gabimaru → HP, peligro) y cian eléctrico (MiniBot → XP, tecnología, UI tech). Contraste alto, silueta legible, "game feel" moderno tipo Ghostrunner / Cyberpunk.

---

## 1. Paleta de colores maestra

Usa estos colores de forma consistente en TODA la UI. Copia los hex directamente al color picker de Unity (el campo `Hexadecimal` acepta el código sin `#`).

| Rol | Nombre | Hex | Uso |
|---|---|---|---|
| Fondo base | Ink Black | `0A0E1A` | Paneles, fondos de inventario |
| Fondo secundario | Steel | `141824` | Slots, cajas anidadas |
| Borde / línea | Ash | `2A3040` | Bordes de paneles, separadores |
| Acento principal | Crimson | `E63946` | HP, daño, peligro, botón "Desequipar" |
| Acento principal claro | Hot Crimson | `FF5A6E` | Flash de daño, gradiente superior HP |
| Acento tech | Cyber Cyan | `00D9FF` | XP, bordes tech, botón "Equipar" |
| Acento tech claro | Electric Cyan | `7EEFFF` | Brillos, highlights |
| Texto principal | Bone White | `F1FAEE` | Casi todo el texto |
| Texto secundario | Fog Gray | `A8B2C5` | Subtítulos, stats secundarios |
| Advertencia | Amber | `FFD60A` | HP bajo (<30%), munición baja |
| Éxito | Jade | `52E3A4` | Loot raro, curación |

Regla: **nunca uses negro puro (#000000) ni blanco puro (#FFFFFF)** en UI. Usa Ink Black y Bone White.

---

## 2. Canvas base

En tu Canvas raíz (`Screen Space - Overlay`):

- **Canvas Scaler** → `UI Scale Mode: Scale With Screen Size`
- **Reference Resolution**: `1920 x 1080`
- **Screen Match Mode**: `Match Width Or Height`
- **Match**: `0.5`

Esto hace que la UI escale proporcionalmente en cualquier resolución.

---

## 3. HUD — Barra de vida

**Anchor**: esquina inferior izquierda. **Pos**: `(40, 60, 0)` relativo al anchor.

Estructura jerárquica:

```
BarraVida_Container (RectTransform 420x56)
├── Frame_BG           (Image, color 0A0E1A, alpha 0.85)
├── Frame_Border       (Image, tipo Sliced, color E63946, borde 2px)
├── Fill_Background    (Image, color 2A3040) — el track vacío
├── Fill_Actual        (Image, Filled Horizontal, color E63946) ← el que anima
├── Fill_Brillo        (Image, Filled Horizontal, color FF5A6E alpha 0.4) — encima del fill para dar volumen
├── Icono_Corazon      (Image 32x32, color F1FAEE, a la izquierda por fuera)
└── Texto_HP           (TMP_Text, "75 / 100", color F1FAEE, TMP tamaño 22, bold)
```

Valores concretos:

- Tamaño contenedor: **420 × 56**
- `Frame_BG`: Image con color `#0A0E1A`, **Alpha: 220** (slider 0-255 → 0.85)
- `Frame_Border`: crea un sprite 9-sliced o usa `UI/Default` con `Image Type: Sliced`. Color `#E63946`. Para un borde limpio, duplica el frame e inviértelo con un offset de 2px.
- `Fill_Actual`: Image Type **Filled**, Fill Method **Horizontal**, Fill Origin **Left**, `Fill Amount` controlado por `BarraVidaUI.cs`.
- Añade un **Shadow** component a `Texto_HP`: Effect Color `#0A0E1A` alpha 255, Effect Distance `(1, -1)`.

**Feedback de daño (mejora opcional pero muy vistosa)**:

En `BarraVidaUI.cs`, añade un segundo Image `Fill_Delayed` que va por detrás y se interpola hacia el `Fill_Actual` con un delay de 0.3s. Así cuando te hacen daño ves una "barra blanca" que baja 0.3s después (efecto estilo Hollow Knight / Persona 5).

```csharp
// Pseudocódigo dentro de BarraVidaUI.cs
float fillDelayed = Mathf.Lerp(fillDelayed, vidaActual/vidaMax, Time.deltaTime * 4f);
imagenFillDelayed.fillAmount = fillDelayed;
```

**Cambio de color según HP**:

- HP > 50%: `#E63946` (crimson)
- HP 30-50%: `#FFD60A` (amber)
- HP < 30%: `#FF5A6E` con pulso (anima alpha entre 0.6 y 1 con `Mathf.PingPong`)

---

## 4. HUD — Barra de XP

**Anchor**: inferior centro. **Pos**: `(0, 24, 0)`. **Tamaño**: `960 × 14` (delgada y ancha).

Estructura:

```
BarraXP_Container (960x14)
├── Track          (Image, color 141824, alpha 0.9)
├── Fill_XP        (Image, Filled Horizontal, color 00D9FF)
├── Fill_Glow      (Image, color 7EEFFF, alpha 0.5, ligeramente más grande)
└── Texto_XP       (TMP_Text, "1240 / 2000 XP", tamaño 16, centrado sobre la barra)
```

A la izquierda del track, un círculo con el nivel:

```
Nivel_Circulo       (Image, sprite círculo, 48x48, color 0A0E1A con borde 2px 00D9FF)
└── Texto_Nivel     (TMP_Text "7", tamaño 26, bold, color 00D9FF, centrado)
```

**Efecto de subida de nivel**: cuando `OnNivelSubido` dispare, haz un tween de escala en `Nivel_Circulo` (0.8 → 1.3 → 1.0 en 0.4s) y un flash blanco sobre `Fill_XP` (alpha 1 → 0 en 0.5s). DOTween o un IEnumerator en `PanelNivelUI.cs` bastan.

---

## 5. HUD — Munición / Arma actual

Si no la tienes aún, añádela. Esquina **inferior derecha**, pos `(-60, 60, 0)`.

```
MunicionPanel (260x80)
├── Fondo      (Image, color 0A0E1A, alpha 0.7, esquinas redondeadas 8px)
├── Icono_Arma (Image 56x56, color F1FAEE)
├── Texto_Balas (TMP_Text "24", tamaño 54, bold, color F1FAEE)
└── Texto_Reserva (TMP_Text "/ 120", tamaño 22, color A8B2C5, alineado a la base de Texto_Balas)
```

Cuando `WeaponDomain` emita recargando, parpadea `Texto_Balas` con color `#FFD60A`.

---

## 6. Inventario

El inventario actual con Grid Layout Group está bien como base. Rediseño visual:

**Panel de fondo** (el contenedor grande):

- Tamaño: `900 × 640`, centrado
- Image: color `#0A0E1A`, alpha `240/255`
- Añade un **Outline** component: Color `#00D9FF`, Distance `(2, -2)`
- Encima añade una franja superior decorativa (header): Image 60px de alto, color `#141824`, con un TMP_Text "INVENTARIO" tamaño 32, bold, color `#F1FAEE`, tracking/letter-spacing `+10`

**Grid Layout Group** (configuración):

- Cell Size: `128 × 128`
- Spacing: `12 × 12`
- Padding: `24, 24, 24, 24`
- Start Corner: `Upper Left`
- Start Axis: `Horizontal`
- Constraint: `Fixed Column Count`, Column Count `6`

**Slot individual** (prefab `SlotInventario`):

```
Slot (128x128)
├── Fondo         (Image, color 141824, alpha 1)
├── Borde         (Image, color 2A3040, Image Type Sliced, 9-slice 2px)
├── Icono         (Image 88x88, centrado, preserveAspect true)
├── BordeRareza   (Image, color según rareza, alpha 0.8, outline 2px) — activar si el arma tiene rareza
├── NombreArma    (TMP_Text, abajo, tamaño 12, color F1FAEE, max 2 líneas, truncate)
└── BotonEquipar  (Button, 100x26, abajo del todo)
```

**Botón Equipar / Desequipar**:

- Fondo: `Image` con color dinámico → verde `#52E3A4` cuando dice "Equipar", rojo `#E63946` cuando dice "Desequipar"
- Texto: TMP, tamaño 14, bold, color `#0A0E1A` (contraste sobre el fondo claro)
- ColorBlock del Button: Normal `#52E3A4` / Highlighted `#7EEFFF` / Pressed `#00D9FF` / Disabled `#2A3040`
- Opcional: añade un **Shadow** al slot completo (Distance 0, 0, Color `#00D9FF` alpha 0.4) y auméntalo con `EventTrigger` `PointerEnter` para efecto hover (glow cian).

**Colores de rareza** (si implementas tiers más adelante):

| Rareza | Hex | Uso |
|---|---|---|
| Común | `A8B2C5` | Gris frío |
| Poco común | `52E3A4` | Jade |
| Raro | `00D9FF` | Cian |
| Épico | `B983FF` | Violeta |
| Legendario | `FFD60A` | Ámbar |

---

## 7. Tipografía

Descarga gratis e importa como TMP Font Asset:

- **Rajdhani** (Google Fonts) — limpia, sci-fi, perfecta para HUD. Úsala para números, stats, menús.
- **Orbitron** (Google Fonts) — más agresiva, solo para títulos grandes ("INVENTARIO", "GAME OVER", etc.).
- **Noto Sans JP** o **M PLUS 1 Code** — solo si quieres añadir kanji decorativos (ref. Gabimaru). Por ejemplo, poner 刃 (ha, "hoja") detrás del nombre de un arma, con alpha bajo.

Pipeline: `Window → TextMeshPro → Font Asset Creator`. Sampling Point Size 90, Padding 9, Atlas 2048×2048.

---

## 8. Post-processing URP

1. Instala el paquete si no está: `Window → Package Manager → Universal RP` (ya está con URP).
2. En tu escena, crea un GameObject vacío `Global Volume`, añade component **Volume**, `Is Global: true`, y crea un **Volume Profile** nuevo.
3. En tu Camera principal, activa `Post Processing` en Rendering.

Añade estos Override al profile con los valores exactos:

### Bloom
- **Threshold**: `0.95`
- **Intensity**: `0.6`
- **Scatter**: `0.75`
- **Tint**: `#E0E8FF` (bloom ligeramente frío)
- **High Quality Filtering**: ON

### Vignette
- **Color**: `#0A0E1A`
- **Center**: `(0.5, 0.5)`
- **Intensity**: `0.32`
- **Smoothness**: `0.4`
- **Rounded**: OFF

### Color Adjustments
- **Post Exposure**: `0`
- **Contrast**: `15`
- **Color Filter**: `#FFF4E0` (muy leve calidez)
- **Hue Shift**: `0`
- **Saturation**: `8`

### Channel Mixer (opcional, da carácter cinematográfico)
- Red out / Blue in: `-10`
- Blue out / Red in: `+8`

Esto mete un leve toque verde-azulado en las sombras y cálido en luces = look "teal & orange" de cine de acción.

### Split Toning
- **Shadows**: `#1A3A5C` (azul frío en sombras)
- **Highlights**: `#FFA466` (naranja cálido en luces)
- **Balance**: `0`

### Film Grain
- **Type**: `Medium 1`
- **Intensity**: `0.18`
- **Response**: `0.8`

### Chromatic Aberration
- **Intensity**: `0.12` (muy sutil; más alto marea)

### Motion Blur
- **Quality**: `Medium`
- **Intensity**: `0.3`
- **Clamp**: `0.05`

### Depth of Field — **OFF** para gameplay
Solo actívalo en menús o cutscenes. En FPS destroza la legibilidad.

### SSAO (en el Renderer, no en el Volume)
En tu `Universal Renderer Data` (Assets/Settings/...), añade **Screen Space Ambient Occlusion** como Renderer Feature:
- **Method**: `Interleaved Gradient`
- **Intensity**: `0.8`
- **Radius**: `0.35`
- **Sample Count**: `Medium`

Esto da profundidad en esquinas y debajo de objetos — mata el look "gris vacío" enseguida.

---

## 9. Iluminación de escena

Si tu escena se ve gris y vacía, el 80% es iluminación. Cambios de alto impacto:

**Directional Light** (el "sol"):
- **Color**: `#FFE4B5` (cálido, ámbar suave)
- **Intensity**: `1.3`
- **Rotation**: `X=50, Y=-30, Z=0` (luz diagonal, no cenital)
- **Shadow Type**: `Soft Shadows`
- **Shadow Strength**: `0.85`

**Environment Lighting** (`Window → Rendering → Lighting`):
- **Source**: `Gradient`
- **Sky Color**: `#3A4A6E` (azul atardecer)
- **Equator**: `#4A4438`
- **Ground**: `#1A1610`
- **Intensity Multiplier**: `1.0`

**Fog** (crítico para que no parezca vacío):
- ON, **Mode**: `Exponential Squared`
- **Color**: `#1A2A44`
- **Density**: `0.015`

Esto atenúa objetos lejanos, da profundidad atmosférica y oculta el horizonte (menos trabajo de escenario).

**Luces de acento en escena**:

Mete **Point Lights** con colores de la paleta:
- Cyan `#00D9FF` sobre paneles "tech" / terminales / el MiniBot (Intensity 2-3, Range 6)
- Crimson `#E63946` sobre zonas de peligro / spawn de enemigos (Intensity 1.5, Range 4)
- Amber `#FFD60A` en antorchas / zonas "hub" seguras

Regla: **nunca dejes una zona sin luz de color**. Mezcla siempre mínimo 2 tonos (cálido + frío) para que las formas tengan silueta.

---

## 10. Materiales

Para que los assets no parezcan grises genéricos:

**Suelos / paredes (Lit Shader URP)**:
- Smoothness: `0.3-0.5` (nada de 0, nada de 1)
- Metallic: `0` en piedra/madera, `0.8-1.0` en metales
- Normal Map: obligatorio siempre que exista; intensidad `0.7`
- Emission en detalles: zonas tech con `#00D9FF` intensity 2-3 HDR

**Enemigos**:
- Añade emissive a ojos/detalles. Un enemigo con ojos rojos brillantes `#FF2030` intensity 4 es 100× más legible que uno gris.
- Para el jefe final o enemigos raros, considera un Fresnel rim light con un shader personalizado o usa un segundo material con emission y face culling invertido.

**Armas (primera persona)**:
- Smoothness alta `0.7` → metal brillante
- Emission en partes tech/cañón: color del arma (cyan si es láser, ámbar si es balística, crimson si es demoníaca tipo Gabimaru)

---

## 11. Checklist de 30 minutos (impacto máximo / esfuerzo mínimo)

Si tienes solo media hora, haz esto en orden:

1. **Global Volume con Bloom + Vignette + Color Adjustments** (valores arriba) → 5 min, 40% del impacto visual.
2. **Fog exponencial + Directional Light cálida** → 5 min, la escena deja de parecer un test.
3. **Cambia colores de HP a `#E63946` y XP a `#00D9FF`**, añade TMP Shadow → 5 min, HUD coherente.
4. **Outline cian en panel de inventario + fondo `#0A0E1A`** → 5 min.
5. **Añade 2-3 Point Lights de color en escena** (cyan + crimson) → 5 min, profundidad inmediata.
6. **SSAO como Renderer Feature** → 5 min, oclusión en esquinas.

Con esto la primera impresión ya pasa de "prototipo" a "alpha con identidad".

---

## 12. Referencias para inspirarte

- **Ghostrunner** (UI HUD mínima, cian sobre negro, katana primera persona)
- **Cyberpunk 2077** (paleta neon sobre oscuro, glitch sutil)
- **Hell's Paradise** (el propio anime — escenas oscuras con carmesí saturado, flashes de luz dura)
- **DOOM Eternal** (HUD agresivo, número de munición gigante, feedback visual constante)
- **Persona 5** (UI con personalidad brutal — no copies el estilo pero fíjate en el uso de tipografía y formas no rectangulares)

---

Cuando apliques los cambios, si quieres puedo ayudarte a ajustar valores viendo capturas de tu escena/UI — pégalas y iteramos.
