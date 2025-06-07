import { PhysicsMetrics } from '../types/simulation'
import { DraggableWindow } from './DraggableWindow'
import './StatsPanel.css'

interface StatsPanelProps {
  metrics: PhysicsMetrics
  particleCount: number
  interactionCount: number
  simulationTime: number
  onClose: () => void
}

export function StatsPanel({
  metrics,
  particleCount,
  interactionCount,
  simulationTime,
  onClose,
}: StatsPanelProps) {
  return (
    <DraggableWindow
      title="Simulation Statistics"
      onClose={onClose}
      defaultPosition={{ x: 20, y: 80 }}
      defaultSize={{ width: 320, height: 500 }}
      minWidth={280}
      minHeight={400}
      className="stats-panel"
    >
      
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-label">Simulation Time</div>
          <div className="stat-value">{simulationTime.toFixed(6)} s</div>
        </div>
        
        <div className="stat-card">
          <div className="stat-label">Total Particles</div>
          <div className="stat-value">{particleCount}</div>
        </div>
        
        <div className="stat-card">
          <div className="stat-label">Active Interactions</div>
          <div className="stat-value">{interactionCount}</div>
        </div>
        
        <div className="stat-card">
          <div className="stat-label">Free Quarks</div>
          <div className="stat-value warning">
            {metrics.freeQuarks}
            {metrics.freeQuarks > 0 && ' ⚠️'}
          </div>
        </div>
      </div>
      
      <div className="stats-section">
        <h3>Energy & Momentum</h3>
        <div className="stat-row">
          <span>Total Energy</span>
          <span>{metrics.totalEnergy.toFixed(3)} GeV</span>
        </div>
        <div className="stat-row">
          <span>Total Momentum</span>
          <span>{metrics.totalMomentum.toFixed(3)} GeV/c</span>
        </div>
        <div className="stat-row">
          <span>Angular Momentum</span>
          <span>{metrics.totalAngularMomentum.toFixed(6)} ℏ</span>
        </div>
        <div className="stat-row">
          <span>Binding Energy</span>
          <span>{metrics.averageBindingEnergy.toFixed(3)} GeV</span>
        </div>
        <div className="stat-row">
          <span>Vacuum Energy</span>
          <span>{metrics.vacuumEnergyDensity.toExponential(2)} GeV⁴</span>
        </div>
      </div>
      
      <div className="stats-section">
        <h3>Particle Counts</h3>
        {Object.entries(metrics.particleCounts).map(([type, count]) => (
          <div key={type} className="stat-row">
            <span>{type}</span>
            <span>{count}</span>
          </div>
        ))}
      </div>
      
      <div className="stats-section">
        <h3>Color Confinement</h3>
        <div className="stat-row">
          <span>Color Singlets</span>
          <span className="success">{metrics.colorSinglets}</span>
        </div>
        <div className="confinement-status">
          {metrics.freeQuarks === 0 ? (
            <div className="status-good">✓ All quarks confined</div>
          ) : (
            <div className="status-bad">✗ Confinement violated!</div>
          )}
        </div>
      </div>
    </DraggableWindow>
  )
}