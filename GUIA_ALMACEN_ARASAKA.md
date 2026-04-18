# Almacén Arasaka — Plan de 5-6 horas

Una sola sala cerrada, estilo almacén corporativo Arasaka (Cyberpunk 2077). Pensada para ejecutarse en una tarde.

## Concepto en una frase

Nave industrial cerrada, paredes de metal corrugado, suelo de hormigón, **logo Arasaka rojo** en una pared dominante, 4-6 contenedores de carga, 1-2 vehículos (furgoneta / carretilla elevadora), iluminación de halógenos cenitales con haces de luz en polvo, hazard stripes amarillas/negras en el suelo.

**Paleta**:
- Rojo Arasaka: `#C41E1E`
- Amarillo hazard: `#FFC700`
- Gris industrial: `#3A3D42`
- Metal oscuro: `#1C1E22`
- Luz clave: blanco cálido `#FFD9A8` intensity 2-3

---

## Packs a descargar (GRATIS, 15 min)

Solo estos. No descargues nada más para no perderte:

1. **"Sci-Fi Warehouse"** o **"Modular Warehouse"** — busca en Unity Asset Store ordenando por "Free". Sirve cualquier pack con paredes modulares, contenedores y props industriales.
2. **"POLYGON Starter Pack (Free Sample)"** de Synty — tiene vehículos/cajas sci-fi gratis para complementar.
3. **"Unity Particle Pack"** (oficial, gratis) — para las partículas de polvo.

Si no encuentras uno específicamente Arasaka, **cualquier pack de warehouse industrial sirve** — el "look Arasaka" lo dará la iluminación roja y un par de decals con el logo, no los modelos.

**Logo Arasaka**: búscalo en Google como "Arasaka logo PNG transparent" y guárdalo. Lo pegarás como decal/textura en una pared.

---

## Dimensiones de la sala

- Suelo: **20 × 30 unidades** (rectangular, proporción warehouse)
- Altura del techo: **8 unidades** (ancho industrial)
- Puerta de entrada: pared corta, 4u alto × 3u ancho
- Puerta grande de carga (cerrada): otra pared corta, 5u × 5u

Con estas medidas entran 4 contenedores sin sentirse apretado.

---

## Cronograma 5-6 horas

### HORA 1 — Blockout + importar pack (60 min)

1. **[5 min]** Escena nueva URP. Borra todo menos Camera y Directional Light (este último lo vas a matar luego).
2. **[10 min]** `Tools → ProBuilder → New Shape → Cube`. Crea:
   - Suelo: cubo escalado `(20, 0.2, 30)` en posición `(0, 0, 0)`
   - 4 paredes: cubos `(0.3, 8, 30)` y `(20, 8, 0.3)` formando una caja cerrada
   - Techo: cubo `(20, 0.2, 30)` a Y=8
3. **[5 min]** Mete tu Player prefab dentro y pulsa Play. Camina. Confirma que la escala se siente bien (si el techo parece muy alto o bajo, ajústalo AHORA).
4. **[30 min]** Importa el pack de warehouse. Arrastra una pared modular junto a tu cubo gris. Si la escala no coincide, escala la pared hasta que encaje con 8u de alto. Sustituye **solo 2 paredes** con módulos del pack. Las otras 2 déjalas como están de momento.
5. **[10 min]** Sustituye el suelo de ProBuilder por un plano con textura de hormigón del pack (o material URP Lit, Albedo gris `#3A3D42`, Smoothness 0.4).

Al final de la hora 1: **caja cerrada jugable, 2 paredes vestidas, suelo vestido**.

### HORA 2 — Vestir paredes + techo (60 min)

1. **[20 min]** Termina las otras 2 paredes con módulos del pack.
2. **[15 min]** Techo: si el pack trae paneles de techo con tragaluces o vigas, úsalos. Si no, deja el cubo gris pero añade **vigas I** cruzando el techo cada 5u (cubos alargados de 0.3×0.5×20u, material metal oscuro).
3. **[15 min]** Puertas: **no las hagas funcionales aún**, solo estéticas. Pon una puerta grande de carga (si el pack tiene, sino un cubo con textura metálica + hazard stripes amarillas/negras) en una pared corta.
4. **[10 min]** Pared dominante (la del fondo): aquí va el **logo Arasaka grande**. Crea un plano de 4×4u pegado a la pared, material URP Lit con tu imagen del logo en Albedo + Emission color `#C41E1E` intensity 1.5. Se verá brillar sutilmente.

### HORA 3 — Props y relleno (60 min)

Principio clave: **agrupa los props en "clusters"**, no los desparrames uniformes.

Reparto recomendado:

```
        [Pared fondo con logo Arasaka]
             │
  [Contenedor]  [Contenedor]
     rojo         gris
                                [Vehículo]
                                (furgoneta
                                 o carretilla)
  [Contenedor]      [Caja × 3]
     azul           apiladas
             
        [Barriles × 2]
             │
   [Puerta de entrada — Player spawnea aquí]
```

1. **[20 min]** Coloca 4 contenedores de carga del pack. Píntalos con materiales distintos: uno rojo Arasaka `#C41E1E`, uno gris, uno amarillo hazard, uno con el logo. Si el pack no te deja cambiar colores fácil, duplica el material y cambia el color base.
2. **[15 min]** Coloca 1 vehículo. Si el pack no trae, usa una carretilla elevadora de Synty Starter o una furgoneta. Ponla entre dos contenedores, ligeramente girada (no perpendicular a la pared — queda más natural).
3. **[15 min]** Props secundarios:
   - 3-4 cajas de madera o metal apiladas (ladeadas 5-10° para que no parezcan clones)
   - 2 barriles industriales
   - Un "pallet" de madera en el suelo
   - Una mesa de trabajo con herramientas si el pack las trae
4. **[10 min]** Hazard stripes en el suelo: crea un plano delgado con textura de bandas amarillas/negras en zonas clave (delante de la puerta de carga, perímetro de un contenedor "peligroso"). Lo importante: **que rompa la monotonía del suelo de hormigón**.

### HORA 4 — Iluminación (60 min) ⭐ HORA CRÍTICA

Aquí es donde el mapa pasa de "demo de Asset Store" a "Arasaka warehouse".

1. **[5 min]** Mata el Directional Light: Intensity `0.1`, color azul frío `#3A4A6E`. Queremos oscuridad, no luz solar.
2. **[20 min]** **Halógenos cenitales**: crea 4-6 **Spot Lights** colgando del techo, apuntando hacia abajo.
   - Color: `#FFD9A8` (blanco cálido, halógeno)
   - Intensity: `3`
   - Range: `10`
   - Spot Angle: `60°`
   - Inner Spot Angle: `30°`
   - Shadows: Soft Shadows
   - Colócalos cada ~8u en una línea sobre el pasillo central
3. **[10 min]** **Luz roja de alarma / Arasaka**: 1-2 Point Lights con color `#C41E1E`, Intensity 2, Range 5, en zonas específicas:
   - Una cerca del logo de la pared del fondo (lo resalta)
   - Otra en una esquina para dramatismo
4. **[10 min]** **Luz de emergencia amarilla**: 1 Point Light amber `#FFC700`, Intensity 1.5, Range 4, cerca de la puerta de carga. Evoca "zona de trabajo".
5. **[10 min]** **Reflection Probe**: un solo Reflection Probe en el centro de la sala, Box Size 20×8×30. Esto hace que los metales reflejen el entorno.
6. **[5 min]** Baja el **Ambient Light** en `Window → Rendering → Lighting → Environment Lighting → Source: Color`, color `#0A0E1A`, Intensity Multiplier `0.2`. Esto asegura que las sombras sean **realmente oscuras**.

Al final de la hora 4: haces de luz cayendo del techo, logo Arasaka brillando rojo, esquinas en penumbra. **Ya se ve bien**.

### HORA 5 — VFX y atmósfera (60 min)

Lo que convierte "bien" en "pulido".

1. **[20 min]** **Partículas de polvo en los haces de luz**: crea un `Particle System` debajo de cada Spot Light.
   - Duration: `5`, Looping ON
   - Start Lifetime: `8`
   - Start Size: `0.05`
   - Start Speed: `0.1` (casi flotando)
   - Max Particles: `30`
   - Emission rate: `3`
   - Shape: Cone, radius 0.5, angle 15° (caen desde arriba)
   - Material: el "DustParticle" del Unity Particle Pack, o un material Unlit con textura de círculo suave y alpha 0.3
   - Color over Lifetime: blanco → transparent
2. **[10 min]** **Niebla general**: `Window → Rendering → Lighting → Environment → Other Settings → Fog`:
   - Fog ON
   - Color: `#1C1E22`
   - Mode: Exponential
   - Density: `0.03`
   Hace que el fondo se desvanezca en oscuridad. Profundidad instantánea.
3. **[15 min]** **Decals** en paredes y suelo:
   - Activa Decal Renderer Feature en tu URP Renderer (si no está).
   - Mete 3-4 decals: "DANGER", flechas amarillas, pegatinas de mantenimiento, logo Arasaka pequeño. Muchos packs traen decals listos; si no, crea planos finos con imágenes PNG transparentes.
4. **[10 min]** **Detalles finos**:
   - Cable colgante del techo (LineRenderer o prefab del pack)
   - Una caja volcada con "contenido" desparramado (cajas pequeñas, herramientas)
   - Manchas de óxido o aceite en el suelo (decal oscuro)
5. **[5 min]** Comprueba tu **Post-Processing Volume** (el de la guía anterior). Si no estaba activo, añádelo: Bloom, Vignette, Color Adjustments. Los spots halógenos + bloom = magia inmediata.

### HORA 6 — Gameplay pass + pulido final (60 min) — buffer

1. **[15 min]** Mete 3-4 spawn points de enemigos en el mapa. Escóndelos detrás de contenedores para que el jugador tenga encuentros interesantes en lugar de verlos todos desde el spawn.
2. **[10 min]** Coloca 2-3 ítems / drops en zonas exploratorias (detrás de una caja, encima de un contenedor que se alcance con grappling hook).
3. **[10 min]** **NavMesh**: `Window → AI → Navigation (Obsolete)` o el nuevo "AI Navigation" del package. Marca el suelo como Navigation Static, bake. Comprueba que los enemigos con NavMeshAgent se mueven.
4. **[15 min]** Playtest completo: entra, combate, loot, sal. Apunta cosas que no funcionan.
5. **[10 min]** Fixes rápidos: una luz que no se ve bien, un prop que flota, una pared sin colisión.

---

## Checklist visual final

Cuando termines, tu sala debe tener:

- [ ] Logo Arasaka visible desde el spawn, en rojo brillante
- [ ] Al menos 3 haces de luz cayendo del techo con polvo flotando
- [ ] Contraste fuerte: zonas iluminadas vs zonas en penumbra negra
- [ ] 4 contenedores bien distribuidos (no alineados, ligeramente rotados)
- [ ] 1 vehículo visible desde el spawn
- [ ] Suelo con hazard stripes en al menos una zona
- [ ] Decals/pegatinas en paredes y suelo
- [ ] Fog sutil que oscurece el fondo
- [ ] 1-2 luces rojas de alarma de acento
- [ ] Reflection Probe haciendo que los metales reflejen

Si tachas 8 de 10 → has terminado.

---

## Trucos de último minuto si ves que el tiempo se acaba

- **Sin packs de pago**: una pared lisa con textura metálica + decals = 80% del trabajo hecho.
- **Sin vehículo**: 2 contenedores formando "L" con una caja grande encima sustituye visualmente a un vehículo.
- **Iluminación barata que salva todo**: si solo tienes 10 min de luz, mete UN spotlight halógeno grande en el centro + bajas el ambient a 0.1 + pones el logo Arasaka emissive. Con eso la habitación ya grita "industrial cyberpunk".
- **No te obsesiones con los decals**: un logo Arasaka grande en la pared vale más que 20 pegatinas pequeñas.

---

## Qué NO hacer (te ahorras horas)

- No modeles nada en Blender. No tienes tiempo.
- No intentes que las puertas se abran automáticas. Déjalas decorativas.
- No metas lluvia / partículas complejas fuera de la sala — es interior.
- No pongas más de 2 packs distintos. La inconsistencia de estilo se nota.
- No bakees lighting (tiempo perdido para una escena pequeña). Déjalo en Realtime con Auto Generate OFF.
- No añadas un segundo piso / altillo. Una planta única se resuelve, dos se te van las horas.

---

Cuando tengas la sala, aunque sea a medias, mándame captura y te digo qué ajustar antes del playtest final. Si un paso se te atasca, corta por lo sano y pasa al siguiente — en 5-6 horas lo que importa es tener algo **terminado y presentable**, no perfecto.
