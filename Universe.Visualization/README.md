# Universe Quantum Simulator - 3D Visualization

A real-time 3D visualization for the Universe quantum chromodynamics simulation, built with React and Three.js.

## Features

- **3D Particle Rendering**: Visualize quarks, antiquarks, gluons, and composite particles
- **Color Charge Visualization**: Color-coded particles based on their quantum chromodynamic properties
- **Real-time Updates**: WebSocket connection for live simulation data
- **Interactive Camera**: Orbit controls for pan, zoom, and rotate
- **Particle Selection**: Click on particles to view and edit properties
- **God Mode**: Edit particle properties in real-time
- **Field Visualization**: Optional chromoelectric and chromomagnetic field overlays
- **Statistics Panel**: Real-time physics metrics and conservation law monitoring

## Getting Started

### Prerequisites

- Node.js 18+ and npm
- The Universe simulation backend running (Silo + API)

### Installation

```bash
# Install dependencies
npm install

# Start development server
npm run dev
```

The visualization will be available at `http://localhost:3000`

### Backend Setup

1. Start the Orleans Silo:
   ```bash
   dotnet run --project ../Universe.Silo
   ```

2. Start the API server:
   ```bash
   dotnet run --project ../Universe.Api
   ```

## Controls

- **Left Mouse + Drag**: Rotate camera
- **Right Mouse + Drag**: Pan camera
- **Scroll Wheel**: Zoom in/out
- **Click Particle**: Select and view properties
- **Edit Mode**: Modify particle properties (God Mode)

## Particle Types

### Quarks & Antiquarks
- **Shapes**: Spheres
- **Colors**: Based on color charge (Red, Green, Blue, Anti-colors)
- **Sizes**: 0.2 units

### Gluons
- **Shape**: Octahedron
- **Color**: Orange (#ff8800)
- **Animation**: Continuous rotation
- **Size**: 0.15 units

### Bosons
- **Photon**: Yellow icosahedron
- **W Boson**: Magenta dodecahedron
- **Z Boson**: Cyan dodecahedron

### Composite Particles
- **Meson**: 0.4 units
- **Baryon**: 0.5 units
- **Bound State Indicator**: Blue transparent sphere

## Visualization Elements

### Interactions
- **Strong Force**: Orange lines with confinement tubes
- **Electromagnetic**: Cyan lines
- **Weak Force**: Magenta lines
- **Confinement**: Red tubes

### Field Visualization
- Point cloud representation of field strength
- Color gradient from blue (weak) to red (strong)
- Directional arrows for chromoelectric fields

## Architecture

```
src/
├── components/
│   ├── SimulationScene.tsx    # Main 3D scene
│   ├── Particle.tsx           # Particle rendering
│   ├── Interaction.tsx        # Force visualization
│   ├── FieldVisualization.tsx # Field rendering
│   ├── ControlPanel.tsx       # Simulation controls
│   ├── PropertyPanel.tsx      # Particle properties
│   └── StatsPanel.tsx         # Physics metrics
├── hooks/
│   └── useSimulation.ts       # WebSocket connection
├── types/
│   └── simulation.ts          # TypeScript interfaces
└── App.tsx                    # Main application
```

## Development

### Building for Production

```bash
npm run build
```

### Type Checking

```bash
npm run typecheck
```

## Performance Considerations

- Particle count: Optimized for up to 1000 particles
- Field visualization: Sampled for performance
- Update rate: 10 FPS via WebSocket
- Three.js optimizations: Instanced rendering for similar particles

## Troubleshooting

### Connection Issues
- Ensure the API server is running on port 5000
- Check CORS settings if running on different ports
- Verify WebSocket support in your environment

### Performance Issues
- Disable field visualization for better performance
- Reduce particle count in simulation
- Use Chrome/Edge for best WebGL performance

## Future Enhancements

- [ ] Particle trails visualization
- [ ] Energy density volume rendering
- [ ] VR/AR support
- [ ] Recorded simulation playback
- [ ] Advanced shader effects
- [ ] Multi-user collaboration