import { useState } from 'react'
import './ControlPanel.css'

interface ControlPanelProps {
  connected: boolean
  isPaused: boolean
  showFields: boolean
  showInteractions: boolean
  onTogglePause: () => void
  onToggleFields: () => void
  onToggleInteractions: () => void
  onCreateParticle: (type: string, position: { x: number; y: number; z: number }) => void
  onReset: () => void
}

export function ControlPanel({
  connected,
  isPaused,
  showFields,
  showInteractions,
  onTogglePause,
  onToggleFields,
  onToggleInteractions,
  onCreateParticle,
  onReset,
}: ControlPanelProps) {
  const [showCreateMenu, setShowCreateMenu] = useState(false)

  const handleCreateProton = () => {
    onCreateParticle('proton', { x: 0, y: 0, z: 0 })
    setShowCreateMenu(false)
  }

  const handleCreateNeutron = () => {
    onCreateParticle('neutron', { x: 0, y: 0, z: 0 })
    setShowCreateMenu(false)
  }

  const handleCreatePion = () => {
    onCreateParticle('pion', { x: 0, y: 0, z: 0 })
    setShowCreateMenu(false)
  }

  return (
    <div className="control-panel panel">
      <h2>Simulation Controls</h2>
      
      <div className="control-status">
        <div className={`status-indicator ${connected ? 'connected' : 'disconnected'}`} />
        <span>{connected ? 'Connected' : 'Disconnected'}</span>
      </div>

      <div className="control-section">
        <h3>Playback</h3>
        <div className="control-buttons">
          <button
            className="control-button"
            onClick={onTogglePause}
            disabled={!connected}
          >
            {isPaused ? '▶ Play' : '⏸ Pause'}
          </button>
          <button
            className="control-button danger"
            onClick={onReset}
            disabled={!connected}
          >
            🔄 Reset
          </button>
        </div>
      </div>

      <div className="control-section">
        <h3>Visualization</h3>
        <label className="control-checkbox">
          <input
            type="checkbox"
            checked={showInteractions}
            onChange={onToggleInteractions}
          />
          <span>Show Interactions</span>
        </label>
        <label className="control-checkbox">
          <input
            type="checkbox"
            checked={showFields}
            onChange={onToggleFields}
          />
          <span>Show Fields</span>
        </label>
      </div>

      <div className="control-section">
        <h3>Create Particles</h3>
        <button
          className="control-button primary"
          onClick={() => setShowCreateMenu(!showCreateMenu)}
          disabled={!connected}
        >
          + Add Particle
        </button>
        
        {showCreateMenu && (
          <div className="create-menu">
            <button onClick={handleCreateProton}>Proton (uud)</button>
            <button onClick={handleCreateNeutron}>Neutron (udd)</button>
            <button onClick={handleCreatePion}>Pion (ud̄)</button>
            <button onClick={() => onCreateParticle('quark', { x: 0, y: 0, z: 0 })}>
              Single Quark
            </button>
            <button onClick={() => onCreateParticle('gluon', { x: 0, y: 0, z: 0 })}>
              Gluon
            </button>
          </div>
        )}
      </div>

      <div className="control-section">
        <h3>Camera</h3>
        <div className="control-info">
          <p>🖱 Left click + drag: Rotate</p>
          <p>🖱 Right click + drag: Pan</p>
          <p>🖱 Scroll: Zoom</p>
          <p>🖱 Click particle: Select</p>
        </div>
      </div>
    </div>
  )
}