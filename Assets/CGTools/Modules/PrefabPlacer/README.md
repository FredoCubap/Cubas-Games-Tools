# Prefab Placer 🎨
### A CGTools Module by Cubas Games

![Prefab Placer Banner](https://raw.githubusercontent.com/FredoCubap/Cubas-Games-Tools/main/Documentation/Images/PrefabPlacer_Banner.png)

> Paint prefabs onto any mesh surface directly in the Scene View — trees, rocks, props, or anything you build.

---

## Preview

### Paint Mode
![Paint](https://raw.githubusercontent.com/FredoCubap/Cubas-Games-Tools/main/Documentation/Images/Painting-Mode.gif)

### Erase Mode
![Erase](https://raw.githubusercontent.com/FredoCubap/Cubas-Games-Tools/main/Documentation/Images/Delete-Mode.gif)

*Works on Unity Terrain, custom meshes, and any surface with a Collider.*

---

## Features

- 🖌️ **Brush painting** — click or drag in the Scene View to place prefabs
- 🧹 **Erase mode** — remove placed prefabs within the brush area
- 🔵 **Brush shapes** — Circle and Square
- 🎲 **Random rotation** — per-axis randomization with min/max range
- 📏 **Random scale** — uniform or independent per-axis variation
- 📐 **Surface alignment** — objects orient to the surface normal automatically
- ⛰️ **Slope filter** — skip placement on surfaces steeper than a defined angle
- 📦 **Multi-prefab support** — load multiple prefabs and place them randomly
- 💾 **Preset system** — save and switch between brush configurations instantly
- 📊 **Session statistics** — track how many objects you've painted or erased
- ⌨️ **Keyboard shortcuts** — full keyboard control while painting
- 🔨 **Build-safe** — all editor components are automatically stripped from builds

---

## Requirements

- CGTools Core installed
- Unity **2021.3 LTS** or newer
- Prefabs must have at least one **Collider** component for erase mode to work

---

## Installation

Prefab Placer is included in the CGTools package. No separate installation required.

If installing manually, place the `PrefabPlacer` folder inside:
```
Assets/CGTools/Modules/PrefabPlacer/
```

CGTools will detect it automatically on the next Unity startup.

---

## How to Use

### 1. Open the window
**Tools → CGTools → Prefab Placer 🎨**

Or open it from the CGTools Hub.

### 2. Add your prefabs
Click **+ Add Prefab Slot** and drag your prefabs into the slots.

> ⚠️ Objects must be saved as **Prefabs** and must have a **Collider**. Raw scene objects are not supported.

### 3. Configure your brush
Adjust brush size, quantity, shape, rotation, and scale to your needs.

### 4. Paint
Click or drag in the **Scene View** over any surface with a Collider.
- **Left click / drag** → paint or erase depending on current mode
- Switch modes with the buttons in the window or with keyboard shortcuts

---

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `P` | Switch to Paint mode |
| `E` | Switch to Erase mode |
| `[` | Decrease brush size |
| `]` | Increase brush size |

---

## Settings Reference

### Brush
| Setting | Description |
|---------|-------------|
| Brush Size | Radius of the painting area in world units |
| Quantity | Number of prefabs placed per stroke |
| Brush Shape | Circle or Square |

### Rotation
| Setting | Description |
|---------|-------------|
| Mode | Surface Aligned, Fixed, Random, or Surface Aligned + Random offset |
| Fixed Rotation | Euler angles used in Fixed mode |
| Random Rotation | Enable per-axis random rotation with Min/Max range |

### Scale
| Setting | Description |
|---------|-------------|
| Base Scale | Starting scale applied to all placed prefabs |
| Random Scale | Enable scale variation |
| Variation % | Percentage of random variation applied |
| Uniform | Apply the same random factor to all axes |

### Advanced
| Setting | Description |
|---------|-------------|
| Min Distance | Minimum distance between placed objects |
| Layer Mask | Which layers are valid painting surfaces |
| Max Slope Angle | Objects won't be placed on surfaces steeper than this |
| Surface Offset | Vertical offset from the surface hit point |
| Align to Surface | Orient objects to match the surface normal |

---

## Presets

Save your current brush configuration as a named preset for quick switching between setups.

1. Type a name in the preset field
2. Click **Save**
3. Click any preset button to restore that configuration

---

## Statistics

The Statistics section shows:
- **Total in Scene** — all prefabs currently placed by this tool
- **Painted (Session)** — placed since the window was opened
- **Erased (Session)** — removed since the window was opened

Use **Select All** to select every placed object in the scene, or **Clear All** to remove them all at once *(with Undo support)*.

---

## How It Works Internally

Each placed prefab receives a hidden `PrefabPlacerTag` component. This tag is what allows the tool to:
- Distinguish painted objects from manually placed ones
- Count, select, and erase only objects placed by this tool
- Leave all other scene objects untouched

The tag is automatically removed from builds via a custom `IProcessSceneWithReport` build processor — it never reaches the final game.

---

## Known Limitations

- Prefabs must have a Collider for erase and distance detection to work
- Painting on moving objects is not supported
- Very dense painting on large scenes may impact Editor performance

---

## Changelog

### v1.0.0
- Initial release
- Paint and erase modes
- Circle and Square brush shapes
- Surface alignment and slope filtering
- Multi-prefab support with random selection
- Preset system
- Session statistics

---

## Links

- 🏠 [CGTools Repository](https://github.com/FredoCubap/Cubas-Games-Tools)
- 🐛 [Report a Bug](https://github.com/FredoCubap/Cubas-Games-Tools/issues)
- 📧 [Contact](mailto:support@cubasgames.com)
- 🎥 [YouTube](https://youtube.com/@cubasgames)

---

*Part of the CGTools suite — Made with ☕ by Cubas Games*
