import { useState } from 'react'
import { Canvas } from '@react-three/fiber'
import { SimulationScene } from './components/SimulationScene'
import { TestScene } from './components/TestScene'
import { ControlPanel } from './components/ControlPanel'
import { PropertyPanel } from './components/PropertyPanel'
import { StatsPanel } from './components/StatsPanel'
import { useSimulation } from './hooks/useSimulation'
import { ParticleSnapshot } from './types/simulation'
import './App.css'

function App() {
  const [selectedParticle, setSelectedParticle] = useState<ParticleSnapshot | null>(null)
  const [showFields, setShowFields] = useState(false)
  const [showInteractions, setShowInteractions] = useState(true)
  const [showStats, setShowStats] = useState(true)
  const [isPaused, setIsPaused] = useState(false)
  const [debugMode, setDebugMode] = useState(false)
  
  const { snapshot, connected, error, sendCommand } = useSimulation()

  const handleParticleSelect = (particle: ParticleSnapshot | null) => {
    setSelectedParticle(particle)
  }

  const handleParticleUpdate = (particleId: string, updates: Partial<ParticleSnapshot>) => {
    sendCommand('updateParticle', { particleId, updates })
  }

  const handleCreateParticle = (type: string, position: { x: number; y: number; z: number }) => {
    sendCommand('createParticle', { type, position })
  }

  const handleTogglePause = () => {
    sendCommand(isPaused ? 'resume' : 'pause')
    setIsPaused(!isPaused)
  }

  return (
    <div className="app">
      <div className="viewport">
        <Canvas
          camera={{
            position: [10, 10, 10],
            fov: 60,
            near: 0.01,
            far: 1000,
          }}
          gl={{
            antialias: true,
            alpha: true,
          }}
        >
          {debugMode ? (
            <TestScene />
          ) : (
            <SimulationScene
              snapshot={snapshot}
              selectedParticleId={selectedParticle?.particleId}
              onParticleSelect={handleParticleSelect}
              showFields={showFields}
              showInteractions={showInteractions}
            />
          )}
        </Canvas>
        
        {/* Debug toggle */}
        <button
          onClick={() => setDebugMode(!debugMode)}
          style={{
            position: 'absolute',
            top: '10px',
            right: '10px',
            padding: '8px 16px',
            background: debugMode ? '#ff0000' : '#0066cc',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
            cursor: 'pointer',
            zIndex: 1000,
          }}
        >
          {debugMode ? 'Exit Debug' : 'Debug Mode'}
        </button>
      </div>

      <ControlPanel
        connected={connected}
        isPaused={isPaused}
        showFields={showFields}
        showInteractions={showInteractions}
        onTogglePause={handleTogglePause}
        onToggleFields={() => setShowFields(!showFields)}
        onToggleInteractions={() => setShowInteractions(!showInteractions)}
        onCreateParticle={handleCreateParticle}
        onReset={() => sendCommand('reset')}
      />

      {selectedParticle && (
        <PropertyPanel
          particle={selectedParticle}
          onUpdate={handleParticleUpdate}
          onClose={() => setSelectedParticle(null)}
        />
      )}

      {showStats && snapshot && (
        <StatsPanel
          metrics={snapshot.metrics}
          particleCount={snapshot.particles.length}
          interactionCount={snapshot.interactions.length}
          simulationTime={snapshot.simulationTime}
          onClose={() => setShowStats(false)}
        />
      )}

      {error && (
        <div className="error-message">
          Connection Error: {error}
        </div>
      )}
    </div>
  )
}

export default App