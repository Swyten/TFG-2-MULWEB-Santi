# Guía de Mundo — TFG_Santi (FPS Unity 6 URP)

Guía pensada **desde cero**: asume que nunca has hecho un mapa, ni importado un asset, ni configurado iluminación avanzada. Orden de lectura de arriba abajo.

Dirección artística propuesta: **Neo-Tokyo Industrial** — cyberpunk con motivos japoneses. Pasillos oscuros, neones rojos/cian, suelos mojados reflectantes, hologramas, kanji en pantallas, vallas metálicas, contenedores, conductos industriales. Mezcla lo "tech futurista" (MiniBot) con lo "ninja urbano" (Gabimaru) que ya tenías como referencia.

Ejemplos reales de este look: **Ghostrunner**, **Cyberpunk 2077**, **Ghost in the Shell**, **Blade Runner 2049**, el mod *Neon District* de Half-Life, series como **Edgerunners**.

---

## 0. Conceptos básicos antes de empezar

Tres conceptos que necesitas interiorizar:

1. **Blockout (greybox)**: construyes el nivel con cubos grises sin arte. Se juega y se prueba que las distancias, saltos y peleas funcionan. **Nunca** metes arte antes.
2. **Dressing / art pass**: cuando el blockout se siente bien, reemplazas los cubos grises por assets con arte.
3. **Lighting pass**: al final, iluminas. La iluminación puede salvar o hundir una escena entera.

Esto es el workflow estándar de la industria. Si intentas hacer arte + juego + luz a la vez, te bloqueas. Hazlo por pases.

---

## 1. Herramientas gratis que vas a usar

Abre cuenta en todas estas hoy mismo:

| Herramienta | Para qué | Precio |
|---|---|---|
| **ProBuilder** (Unity Package) | Blockout — modelar geometría simple dentro de Unity | Gratis, ya integrado |
| **Unity Asset Store** | Props, texturas, packs de arte | Hay miles gratis |
| **Kenney.nl** | Packs de arte estilizado muy limpios | Gratis, dominio público |
| **Sketchfab** | 3D models sueltos de comunidad | Muchos CC0 / CC-BY |
| **Polyhaven** | HDRIs (skyboxes) y texturas PBR | Gratis, CC0 |
| **AmbientCG** | Texturas PBR (metales, hormigón, suelos) | Gratis, CC0 |
| **Mixamo** (Adobe) | Personajes rigged + animaciones | Gratis con cuenta Adobe |
| **Blender** | Si más adelante quieres modelar tú | Gratis |
| **Affinity Photo / Photopea** | Editar texturas, decals | Photopea gratis en navegador |

Prioriza lo que es **CC0** (dominio público) — puedes usarlo sin créditos y en tu TFG sin problemas de licencia.

### Instalar ProBuilder (5 minutos)

`Window → Package Manager → Unity Registry → ProBuilder → Install`

Luego `Tools → ProBuilder → ProBuilder Window`. Esto te abre la barra con herramientas de modelado.

---

## 2. Dirección artística concreta (moodboard mental)

Antes de tocar Unity, ten claro QUÉ estás construyendo. Si no lo tienes claro, el mapa se vuelve un Frankenstein.

**El jugador está en**: distritos bajos de una megaciudad asiática futura. Callejones, azoteas, interiores de oficinas abandonadas, complejos industriales, laboratorios robóticos (ahí caben tus MiniBots).

**Elementos visuales obligatorios** (no se entiende "Neo-Tokyo" sin estos):

- Neones de colores saturados (rojo, cian, magenta, verde lima)
- Kanji / katakana en letreros y pantallas — **usa glifos reales**, no inventes
- Paraguas, cables colgantes, tendederos
- Charcos y suelos reflectantes (wet look)
- Vallas metálicas (chainlink), escaleras de incendios
- Hologramas flotantes (mujeres anunciando productos, pez koi holográfico)
- Vending machines con luz interna
- Puestos de comida (ramen stands) con vapor
- Cables, tuberías, conductos de ventilación industrial
- Pantallas grandes con glitch ocasional

**Materiales predominantes**: hormigón sucio, metal corrugado, plástico translúcido iluminado, neón, cristal reflectante, asfalto mojado.

**Paleta de luces** (tiene que coincidir con la UI que ya hiciste):

- Luz clave: cian `#00D9FF` o magenta `#FF3D9A`
- Luz de acento: crimson `#E63946` o amber `#FFAA33`
- Sombras profundas: casi negro azulado `#0A0E1A`
- Nunca blanco neutro para ambiente

---

## 3. Packs de assets concretos que te recomiendo bajar

Prioriza estos (busca exactamente así en el Asset Store):

### Para entorno urbano/cyberpunk
- **"POLYGON - Sci-Fi Worlds"** (Synty Studios) — de pago pero el estándar. Si tienes presupuesto, es la base perfecta.
- **"Sci-Fi Industrial"** y **"Sci-Fi Styled Modular Pack"** — versiones gratis en Asset Store (busca "sci-fi modular free")
- **"Cyberpunk City Pack"** (varios gratis disponibles) — el look exacto Neo-Tokyo
- **Kenney City Kit (Urban)** — estilizado low-poly, gratis
- **"Hologram Ultimate"** (Asset Store, suele haber versiones gratuitas)

### Para personajes/enemigos
- **Mixamo.com** — busca "robot", "cyberpunk", "soldier", "ninja". Descarga en FBX con skin. Dentro del paquete elige animaciones (Idle, Walking, Running, Attack, Death).
- **"Sci-Fi Enemies and Vehicles"** (Unity Asset Store, gratuito)
- **Synty POLYGON Cyber** para personajes estilizados consistentes

### Para armas
- **"Sci-Fi Weapons Pack"** (hay varios gratis)
- **"Gun Pack - Modern"** para lo balístico
- **"Energy Weapons VFX"** para los efectos de disparo

### Para VFX (partículas, humo, impactos)
- **Unity Particle Pack** (gratis, oficial de Unity)
- **"Realistic Effects Pack"** (búscalo en Asset Store, hay versiones gratis)

### Para audio (no lo olvides, el audio es 50% de la ambientación)
- **freesound.org** — efectos CC0
- **"Sci-Fi Sound FX"** packs del Asset Store

**Regla importante**: elige UN pack de entorno principal y cíñete a él. Mezclar 5 packs distintos hace que tu mapa parezca un collage. Puedes complementar con props sueltos pero la base debe ser un solo estilo.

---

## 4. Diseño del mapa — paso a paso

### 4.1 Papel y lápiz (15 minutos)

Antes de Unity, dibuja tu mapa desde arriba en un folio. No necesita ser bonito. Indica:

- Punto de spawn del jugador (círculo con "P")
- Objetivo / salida
- 3-5 zonas conectadas con rutas (pasillos, puertas)
- Dónde spawneean enemigos
- Dónde están los drops / cajas / coleccionables
- Cambios de altura (una azotea, un sótano, una escalera)

Principio de diseño: **el jugador debe tener siempre al menos 2 rutas hacia delante**. Un pasillo único es aburrido. Tres rutas paralelas le deja elegir.

Estructura recomendada para tu primer mapa (una sola "misión"):

```
[Spawn/Callejón] → [Plaza con enemigos] → [Interior oficina/almacén]
                       ↓ (ruta alt: azotea)
                   [Laboratorio/Boss] → [Salida]
```

### 4.2 Blockout en ProBuilder (1-2 horas)

Dentro de Unity:

1. Crea una escena nueva: `File → New Scene → Basic (URP)`.
2. Abre `Tools → ProBuilder → ProBuilder Window`.
3. Crea un plano grande (floor) con el icono "New Shape" → Plane, tamaño 50×50.
4. Crea cubos (`New Shape → Cube`) de tamaños **múltiplos de la escala del jugador** (tu jugador mide ~2 Unity units).
   - Pared estándar: 4u de alto, 4-8u de largo, 0.3u de grosor
   - Cubierta baja (cover): 1.2u de alto — justo lo que un jugador puede saltar
   - Plataforma: 3u de alto — dash/grapple la alcanzan, salto no
5. Construye TODO gris. No pongas texturas aún.
6. Mete tu Player prefab y **pruébalo jugando**. Ajusta hasta que las peleas funcionen.

**Escalas de referencia** (muy importante, el error nº1 de novatos):
- Jugador capsule: altura 2u, radio 0.5u
- Puerta: 2.2u de alto, 1.2u de ancho
- Pasillo: mínimo 3u de ancho para que no se sienta claustrofóbico
- Habitación estándar: 8-12u por lado
- Arena de combate (para Hell's Paradise vibes): 20×20u mínimo

### 4.3 Art pass (meter assets)

Cuando el blockout funcione:

1. Importa tu pack principal (ejemplo: "Sci-Fi Modular").
2. Coloca los módulos del pack **encima** de tus cubos de ProBuilder, como si vistieras un maniquí. Los cubos grises siguen ahí por debajo como colisiones.
3. Alternativa más limpia: ve borrando cada cubo y sustituyéndolo por la pieza modular equivalente (pared sci-fi, suelo corrugado, techo con tuberías).
4. Añade props: contenedores, cajas, máquinas expendedoras, carteles de neón, cables colgantes, conductos de ventilación.

**Regla del 70-20-10**:
- 70% del espacio = estructura principal (paredes, suelos) — un solo material dominante
- 20% = elementos secundarios (cover, máquinas, mobiliario)
- 10% = puntos focales (neones, hologramas, carteles importantes)

Si el 10% de puntos focales no destaca, el mapa se ve plano.

### 4.4 Detalles que convierten "sci-fi genérico" en "Neo-Tokyo"

- **Carteles en kanji**: descarga texturas de letreros en Polyhaven o crea uno en Photopea con estos caracteres: 刃 (hoja), 地獄 (infierno, Hell's Paradise), 東京 (Tokio), 二〇九九 (2099). Ponles Emission color cian o rosa.
- **Charcos reflectantes**: crea un plano fino (0.01u de grosor) en el suelo, material URP Lit con Smoothness 0.95 y color casi negro. Reflejos inmediatos.
- **Cables colgantes**: usa el componente `LineRenderer` con una curva colgante manual o descarga un prefab de cables del Asset Store.
- **Niebla volumétrica**: `Volume → Add Override → Fog` en URP + partículas de humo lentas para simular niebla pesada en callejones.
- **Lluvia ligera**: un Particle System con textura de gota alargada, emitiendo desde arriba hacia abajo a velocidad 30-40, cantidad 300.

---

## 5. Ambiente — iluminación y atmósfera avanzada

La iluminación es lo que hace que un pack genérico parezca AAA. Invierte tiempo aquí.

### 5.1 Setup inicial de iluminación

1. **Borra el Directional Light por defecto** o déjalo muy débil (Intensity 0.2, color azul noche `#1A2A44`). En Neo-Tokyo apenas hay sol.
2. Crea un **Skybox Material** con el shader `Skybox/Cubemap` o usa el panorámico nocturno de Polyhaven (búscalo como "city night HDRI" en polyhaven.com).
3. **Environment Lighting** → Source: `Skybox`, Intensity Multiplier: `0.4` (queremos oscuro).

### 5.2 Iluminación de neón (la clave del look)

Esto es lo que diferencia un mapa aburrido de uno cyberpunk:

Para cada cartel de neón:

1. Crea un plano delgado con el texto (puede ser una imagen en Emission).
2. Material URP Lit → Emission ON, color cian `#00D9FF` HDR intensity **3-5**, color magenta `#FF3D9A` intensity 4 para variedad.
3. **Point Light** justo delante del cartel, mismo color, Range 4-6, Intensity 4-8.
4. En URP, activa **Light Cookies** (en el Renderer Feature) para que las luces proyecten patrones — el clásico "sombra de ventana veneciana" sobre una pared.

**Ratio cromático del mapa**: 60% cian, 30% magenta/rojo, 10% amber. NO metas luz de TODOS los colores del arcoíris — mata el look.

### 5.3 Volumen / niebla / rayos de luz

En URP 17+ (Unity 6), tienes **Volumetric Fog** si activas HDRP... pero HDRP es otro pipeline. Para URP hay truco fácil:

1. Mete partículas lentas (`Particle System`) con textura de humo, alpha 0.05, cantidad 50-100, escala 5-10u.
2. Colócalas en pasillos y cerca de neones — al estar iluminadas por los point lights, se leen como niebla volumétrica.
3. Bonus: crea un **Light Shaft** fake con un plano semitransparente con shader Unlit/Transparent color cian alpha 0.15, inclinado 30° desde una ventana. Queda "rayo de luz que entra".

### 5.4 Reflection Probes

En el Hierarchy: `GameObject → Light → Reflection Probe`. Pon uno en cada habitación (radio 8-15u). Esto hace que los metales reflejen el entorno real → suelos mojados brillando con los neones. Baja calidad, impacto visual alto.

---

## 6. Diseño de armas

Tienes 3 caminos según tu tiempo/habilidad:

### Camino A — Lo más práctico (recomendado para TFG)
Descarga un pack de armas sci-fi del Asset Store (hay muchos gratis: busca "sci-fi weapons free"). Úsalo tal cual.

Estandariza 3-4 arquetipos que ya encajan con tu `WeaponDomain`:

| Arma | Rol gameplay | Look | Color Emission |
|---|---|---|---|
| Pistola plasma | DPS medio, sin recarga manual | Compacta, cañón grueso | Cian `#00D9FF` |
| Rifle de asalto balístico | DPS alto, recarga frecuente | Angular, "Kalashnikov" futurista | Amber `#FFAA33` (munición real) |
| Katana (melee C) | Cuerpo a cuerpo, daño alto | Hoja curva con runas | Crimson `#FF2030` |
| Escopeta térmica | Boss killer, lento | Robusta, bobinas expuestas | Rojo `#FF5A6E` |

### Camino B — Kitbashing (60% personalización, 40% asset)
Descargas 2-3 armas de diferentes packs, las abres en Blender, combinas piezas (la empuñadura de una, el cañón de otra, mira de una tercera) y obtienes armas únicas. Tutorial clave: busca en YouTube "Blender weapon kitbashing for games".

### Camino C — Modelado desde cero
Solo si tienes tiempo y te interesa. Curva de aprendizaje alta. Blender + Substance Painter (Painter tiene trial gratuito de 30 días).

### Consejos visuales para que las armas se vean "tuyas"

- **Pinta TODAS las armas con la misma paleta del mundo**. Si tu mapa es cian/magenta, tus armas también. Si las descargas con colores dispares, entra al material y cámbialos.
- Añade **Emission en 2-3 puntos** de cada arma: el cargador, la mira, el cañón. Intensity 2-4. Dale vida.
- **Decals / stickers**: pega pegatinas (kanji, logos falsos de corporaciones) en los laterales. Unity 6 soporta **URP Decal Projector** → Renderer Feature → Decal.
- **Primera persona**: el arma ocupa ~40% del encuadre inferior derecho. Mueve el prefab hasta que se sienta así. Añade un leve `Animator` con sway (movimiento al girar la cámara) — muy fácil, cualquier tutorial "Unity FPS weapon sway" de 10 min.

---

## 7. Diseño de personajes

### 7.1 Enemigos

Propuesta de 3 tipos (encajan con el lore Neo-Tokyo):

1. **Drón Rastreador (MiniBot ligero)** — enemigo básico, rápido, poco HP. Flota. Modelo: cualquier robot pequeño de Mixamo o Asset Store. Emission cian en ojo central.
2. **Sicario cibernético** — humanoide con katana o pistola. HP medio. Mixamo tiene muchos "Mutant Samurai" o "Cyborg Soldier". Emission crimson en ojos.
3. **MiniBot Pesado** (boss/mini-boss) — robot grande, lento, mucho HP. Búscalo como "mech" o "heavy robot". Emission amarilla en pecho.

Para cada enemigo:

1. Descarga el modelo de **Mixamo** (gratis, necesitas cuenta Adobe).
2. Selecciona animaciones: **Idle**, **Walking**, **Running**, **Attack** (melee o shooting), **Hit Reaction**, **Death**. Descárgalas todas **sobre el mismo personaje** en FBX, checkbox "Without Skin" en las adicionales para que no dupliquen el mesh.
3. En Unity, crea un **Animator Controller** con estados: Idle → Walk → Attack → Die. Transiciones con parámetros (`isMoving`, `isAttacking`, `isDead`).
4. Ajusta tu IA NavMesh para que cambie `Animator.SetBool` según el estado.

**Consistencia visual**: todos los enemigos deben compartir algo. Propuesta: **todos tienen un "ojo rojo brillante"** (emission `#FF2030` intensity 4). Así el jugador los identifica al instante incluso en la penumbra. Es la regla de Valve para Half-Life.

### 7.2 Jugador (primera persona)

En FPS no ves tu cuerpo, pero sí:

- **Las manos y el arma** → parte crítica. Descarga un "FPS Hands" pack del Asset Store (hay gratis).
- Opcional pero molón: **mirar abajo** y ver las piernas / cuerpo → implica más trabajo, skipeable para TFG.

Si quieres opcional: mete un **cameo** de personaje estilo Gabimaru como skin visible en cutscenes/menú principal, no en gameplay.

### 7.3 Animación de enemigos (consejo clave)

No uses TODAS las animaciones de Mixamo sin ajustar. Mixamo las escala mal a veces. En el importer del FBX:

- **Rig**: Humanoid (si es bípedo) — así compartes animaciones entre modelos.
- **Animation → Bake Into Pose**: Root Transform Position Y, XZ según convenga.
- **Loop Time**: activado en Idle/Walk/Run.

Si una animación patina en el suelo, el problema es el Root Motion. Desactívalo y mueve al enemigo con NavMeshAgent.

---

## 8. Flujo completo — roadmap de 2-3 semanas

Si vas de cero, planifica algo así (cada "día" = 2-3 horas reales):

**Semana 1 — Exploración y pipeline**
- Día 1: Instalar ProBuilder, explorar Asset Store, descargar 2 packs que te gusten.
- Día 2: Tutorial "ProBuilder basics" (YouTube, 30 min). Crear una habitación test.
- Día 3: Importar un pack sci-fi. Arrastrar módulos a la escena. Familiarizarte.
- Día 4: Tutorial Mixamo → Unity (busca "Mixamo Unity import tutorial 2024"). Meter un enemigo andando.
- Día 5: Tutorial "URP Lighting basics" + probar neones con point lights.

**Semana 2 — Primer nivel**
- Día 6-7: Blockout completo de tu primer nivel en ProBuilder. Solo cubos grises. Jugabilidad funcionando (spawn → combate → salida).
- Día 8-9: Art pass — reemplazar cubos por módulos del pack.
- Día 10: Props secundarios (cajas, máquinas, cables, carteles).
- Día 11-12: Iluminación — point lights de neón, reflection probes, fog.

**Semana 3 — Pulido**
- Día 13: Post-processing (ya lo tienes con la guía anterior).
- Día 14: VFX — partículas de humo, lluvia, impactos de bala.
- Día 15: Audio ambiente (loops de ciudad, pisadas, neón zumbando).
- Día 16: Playtest + ajustes.

---

## 9. Errores comunes de primer mapa (evítalos)

1. **Todo a la misma escala de gris**. Si abres gameplay y todo son paredes beige/grises → tienes que meter al menos 2 materiales con emissive por zona.
2. **Iluminación plana** (solo un directional light). Mínimo 3-4 point lights por habitación, con colores distintos.
3. **Habitaciones vacías**. Aunque no uses cada prop, el espacio debe tener mobiliario. Regla: si puedes caminar 3 segundos en línea recta sin ver un objeto interesante, añade algo.
4. **Mezclar estilos art**. Un pack realista + un pack low-poly cartoon = desastre. Elige uno y fuerza consistencia.
5. **No testear escala**. Entrar a una habitación donde tu cabeza choca con el techo o donde la puerta es de 5 metros → haz el mapa con el jugador dentro siempre.
6. **Olvidar el skybox**. Una escena con skybox por defecto (azul cielo) aunque sea nocturna urbana → ridículo. Pon un skybox oscuro nocturno cuanto antes.
7. **Performance desde el final**: cuando detectes lag, activa el **Frame Debugger** (`Window → Analysis → Frame Debugger`) y **Profiler**. Culpables típicos: demasiados real-time lights (pasa algunos a Baked), sombras en todas las luces (solo la principal), materiales con resolución de textura 4K innecesaria.

---

## 10. Qué hacer esta semana (tu próximo paso concreto)

Si hoy mismo quieres avanzar:

1. **Crea cuenta en Mixamo** (5 min).
2. **Descarga "Sci-Fi Modular Environment" gratis del Asset Store** — busca literalmente eso.
3. **Instala ProBuilder** (2 min).
4. **Mira el tutorial oficial "Unity ProBuilder Tutorial" en YouTube** (30 min).
5. **Crea UNA habitación**: suelo, 4 paredes, un techo, una puerta. 12×8u. Métele el Player y camina dentro.

Con eso ya tienes el workflow completo probado en una habitación. Escalar a un mapa entero es repetir el proceso.

---

Cuando tengas el primer blockout o un par de capturas, pégamelas y te digo qué ajustar antes del art pass. También puedo ayudarte a:

- Escribir scripts para puertas automáticas, ascensores, paneles interactivos.
- Configurar un **Shader Graph** de hologramas o suelos mojados.
- Afinar el Animator Controller de un enemigo con transiciones limpias.
- Dar feedback sobre layout del mapa.

Avísame por dónde quieres empezar.
