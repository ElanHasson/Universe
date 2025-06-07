# Universe - Quantum Chromodynamics Simulation

A distributed quantum chromodynamics (QCD) simulation built with Orleans 9, showcasing quark interactions, color confinement, and hadron formation with real-time 3D visualization.

## Overview

Universe simulates fundamental particle physics at the quark level, implementing:
- Strong force interactions via gluon exchange
- Color confinement and hadron formation
- Quantum chromodynamics field theory
- Real-time physics calculations with proper energy-momentum conservation
- Browser-based 3D visualization with interactive controls

## Architecture

The project uses a microservices architecture with:
- **Orleans Silo**: Distributed actor framework hosting simulation grains
- **Web API**: ASP.NET Core API with WebSocket support for real-time updates
- **React Visualization**: Three.js-based 3D particle visualization
- **Grains**: Stateful actors representing quarks, hadrons, and gluons

## Features

- **Particle Types**: Quarks (up, down, charm, strange, top, bottom), antiquarks, gluons, baryons, mesons
- **Physics Simulation**: 
  - Color charge dynamics
  - Strong force interactions
  - Confinement mechanisms
  - Energy-momentum conservation
  - Quantum field effects
- **Visualization**:
  - Real-time 3D particle rendering
  - Interactive camera controls
  - Force field visualization
  - Particle property inspection
  - Draggable information panels

## Prerequisites

- .NET 9.0 SDK
- Node.js 18+ and npm
- Git

## Quick Start

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Universe
   ```

2. **Start the Orleans Silo**
   ```bash
   dotnet run --project Universe.Silo
   ```
   The Orleans Dashboard will be available at http://localhost:8080

3. **Start the Web API** (in a new terminal)
   ```bash
   dotnet run --project Universe.Api
   ```
   The API will run on http://localhost:5000

4. **Start the Visualization** (in a new terminal)
   ```bash
   cd Universe.Visualization
   npm install
   npm run dev
   ```
   Open http://localhost:5173 in your browser

## Project Structure

```
Universe/
├── Universe.Abstractions/     # Shared interfaces and physics models
│   ├── Grains/               # Grain interfaces
│   ├── Physics/              # Physics calculations and models
│   └── Visualization/        # Visualization data structures
├── Universe.Grains/          # Grain implementations
├── Universe.Silo/            # Orleans host
├── Universe.Api/             # Web API and WebSocket server
├── Universe.Visualization/   # React/Three.js frontend
├── Universe.Client/          # CLI client (optional)
└── Universe.Tests/           # Unit tests
```

## Usage

### Visualization Controls

- **Camera**: 
  - Left click + drag: Rotate
  - Right click + drag: Pan
  - Scroll: Zoom
  - Click particle: Select and view properties
- **Particle Creation**: Use the control panel to create protons, neutrons, pions, or individual quarks
- **View Options**: Toggle interaction lines and field visualization
- **Windows**: All information panels are draggable and resizable

### API Endpoints

- `GET /api/snapshot` - Get current simulation state
- `POST /api/particle` - Create a new particle
- `PUT /api/particle/{id}` - Update particle properties
- `POST /api/simulation/{command}` - Control simulation (start/pause/reset)
- `WS /ws` - WebSocket connection for real-time updates

### WebSocket Commands

```javascript
// Get snapshot
{ "type": "getSnapshot" }

// Create particle
{ 
  "type": "createParticle",
  "payload": {
    "type": "proton",
    "position": { "x": 0, "y": 0, "z": 0 }
  }
}

// Update particle
{
  "type": "updateParticle",
  "payload": {
    "particleId": "quark-123",
    "updates": { "energy": 0.5 }
  }
}
```

## Configuration

### Orleans Configuration
- Cluster ID: `dev`
- Service ID: `Universe`
- Storage: In-memory (simulations, quarks, hadrons, gluons)

### Logging
Logs are written to:
- `silo.log` - Orleans silo logs
- `api.log` - Web API logs
- `web.log` - Visualization server logs

## Development

### Building
```bash
dotnet build
```

### Running Tests
```bash
dotnet test
```

### Adding New Features
1. Define interfaces in `Universe.Abstractions`
2. Implement grains in `Universe.Grains`
3. Update API endpoints in `Universe.Api`
4. Add visualization components in `Universe.Visualization`

## Physics Implementation

The simulation implements several key physics concepts:

- **Color Confinement**: Quarks cannot exist in isolation due to increasing strong force with distance
- **Hadron Formation**: Quarks combine into color-neutral baryons (3 quarks) and mesons (quark-antiquark pairs)
- **Gluon Exchange**: Mediates strong force interactions between quarks
- **Quantum Chromodynamics**: Full QCD calculations including running coupling constant

## Troubleshooting

- **Connection Issues**: Ensure all services are running and ports 5000, 5173, and 8080 are available
- **Performance**: Reduce particle count or disable field visualization for better performance
- **WebSocket Errors**: Check CORS configuration and ensure the API is running

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Built with [Microsoft Orleans](https://github.com/dotnet/orleans)
- Visualization powered by [Three.js](https://threejs.org/) and [React Three Fiber](https://github.com/pmndrs/react-three-fiber)
- Physics calculations based on quantum chromodynamics theory