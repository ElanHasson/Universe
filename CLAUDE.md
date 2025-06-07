# Universe Quark Simulator

A world simulator built with Microsoft Orleans 9 that accurately models quarks and their interactions based on quantum chromodynamics (QCD), quantum field theory, and the Standard Model of particle physics.

## Project Structure

- **Universe.Abstractions**: Grain interfaces and shared types
- **Universe.Grains**: Grain implementations for quarks and physics simulation
- **Universe.Silo**: Orleans silo host for running the simulation
- **Universe.Client**: CLI interface for controlling the simulation
- **Universe.Tests**: Unit and integration tests for physics accuracy

## Key Components

### Quark Model
- Six flavors: up, down, charm, strange, top, bottom
- Three color charges: red, green, blue (plus anti-colors)
- Spin: 1/2 (fermions)
- Electric charge: +2/3 or -1/3
- Mass values based on current physics data

### Physics Implementation
- **Quantum Chromodynamics (QCD)**
  - Strong force interactions via gluon exchange
  - Running coupling constant with asymptotic freedom
  - Color confinement using Cornell potential
  - Gluon self-interactions
  - Non-perturbative regime handling
  
- **Quantum Field Theory**
  - Feynman propagators for virtual particles
  - Path integral formulation
  - Vacuum fluctuations and zero-point energy
  - Renormalization group equations
  - Quantum corrections and loop calculations
  
- **Quantum Mechanics**
  - Wave function evolution
  - Quantum measurement and collapse
  - Spin states and Pauli matrices
  - Entanglement and Bell inequalities
  - Quantum tunneling
  
- **Electroweak Interactions**
  - Electromagnetic forces between charged quarks
  - Weak interactions and quark flavor changes
  - W and Z boson exchanges
  - CP violation
  - Electroweak unification at high energies

### Advanced Features
- Dynamic gluon creation for non-perturbative interactions
- Proper color charge conservation
- Quantum field operators
- Vacuum energy calculations
- Coherence length and quantum Zeno effect

## Running the Simulator

1. Start the silo: `dotnet run --project Universe.Silo`
2. Run the client: `dotnet run --project Universe.Client -- [command]`

## Client Commands

### Particle Creation
- `create-proton` - Create a proton (uud quarks)
- `create-neutron` - Create a neutron (udd quarks)
- `create-pion --type [positive|negative|neutral]` - Create a pion meson

All creation commands support `--x`, `--y`, `--z` position options.

### Simulation Control
- `start` - Start the simulation
- `pause` - Pause the simulation
- `reset` - Reset the simulation

### Monitoring & Visualization
- `stats` - Show simulation statistics
- `list-quarks` - List all quarks in the simulation
- `visualize` - Display ASCII visualization of current state
- `monitor [--interval N]` - Real-time monitoring (press Q to quit)
- `save-snapshot [--file filename]` - Save simulation state to JSON file

## 3D Web Visualization

The project includes a React/Three.js visualization for real-time monitoring and interaction:

### Starting the Visualization

1. Start the Orleans Silo: `dotnet run --project Universe.Silo`
2. Start the API server: `dotnet run --project Universe.Api`
3. Start the web app: `cd Universe.Visualization && npm run dev`
4. Open http://localhost:3000

### Visualization Features
- **3D Rendering**: Real-time particle positions and interactions
- **Color Coding**: Visual representation of color charges
- **God Mode**: Click particles to edit their properties (position, momentum, energy, spin)
- **Field Visualization**: Toggle chromodynamic fields
- **Statistics**: Live physics metrics and conservation monitoring with real-time updates
- **Movable Windows**: All panels can be dragged and resized

### Controls
- Left click + drag: Rotate view
- Right click + drag: Pan camera
- Scroll: Zoom in/out
- Click particle: Select and edit
- Window headers: Drag to move
- Window corners: Drag to resize

## Development Notes

- Orleans 9 grains represent individual quarks
- Simulation orchestrator grain manages the overall system
- State persistence for time evolution
- WebSocket API for real-time visualization updates
- React + Three.js for 3D rendering