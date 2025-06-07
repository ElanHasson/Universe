import { useState, useEffect } from 'react'
import { ParticleSnapshot, QuarkFlavor, ColorCharge } from '../types/simulation'
import { DraggableWindow } from './DraggableWindow'
import './PropertyPanel.css'

interface PropertyPanelProps {
  particle: ParticleSnapshot
  onUpdate: (particleId: string, updates: Partial<ParticleSnapshot>) => void
  onClose: () => void
}

export function PropertyPanel({ particle, onUpdate, onClose }: PropertyPanelProps) {
  const [editMode, setEditMode] = useState(false)
  const [editedValues, setEditedValues] = useState({
    position: { ...particle.position },
    momentum: { ...particle.momentum },
    energy: particle.energy,
    spin: particle.spin,
    flavor: particle.flavor,
    color: particle.color,
  })

  // Update edited values when particle changes
  useEffect(() => {
    setEditedValues({
      position: { ...particle.position },
      momentum: { ...particle.momentum },
      energy: particle.energy,
      spin: particle.spin,
      flavor: particle.flavor,
      color: particle.color,
    })
  }, [particle])

  const handleSave = () => {
    onUpdate(particle.particleId, {
      position: editedValues.position,
      momentum: editedValues.momentum,
      energy: editedValues.energy,
      spin: editedValues.spin,
      flavor: editedValues.flavor,
      color: editedValues.color,
    })
    setEditMode(false)
  }

  const handleCancel = () => {
    setEditedValues({
      position: { ...particle.position },
      momentum: { ...particle.momentum },
      energy: particle.energy,
      spin: particle.spin,
      flavor: particle.flavor,
      color: particle.color,
    })
    setEditMode(false)
  }

  return (
    <DraggableWindow
      title="Particle Properties"
      onClose={onClose}
      defaultPosition={{ x: window.innerWidth - 340, y: 20 }}
      defaultSize={{ width: 320, height: 600 }}
      minWidth={280}
      minHeight={400}
      className="property-panel"
    >
      
      <div className="property-header">
        <div className="property-type">{particle.type}</div>
        <button
          className="edit-button"
          onClick={() => setEditMode(!editMode)}
        >
          {editMode ? '📝 Editing' : '✏️ Edit'}
        </button>
      </div>

      <div className="property-grid">
        <div className="property-group">
          <h3>Identity</h3>
          <div className="property-item">
            <label>ID</label>
            <span className="mono">{particle.particleId.substring(0, 8)}...</span>
          </div>
          
          {particle.flavor && (
            <div className="property-item">
              <label>Flavor</label>
              {editMode ? (
                <select
                  value={editedValues.flavor}
                  onChange={(e) => setEditedValues({
                    ...editedValues,
                    flavor: e.target.value as QuarkFlavor
                  })}
                >
                  {Object.values(QuarkFlavor).map(f => (
                    <option key={f} value={f}>{f}</option>
                  ))}
                </select>
              ) : (
                <span>{particle.flavor}</span>
              )}
            </div>
          )}
          
          {particle.color && (
            <div className="property-item">
              <label>Color</label>
              {editMode ? (
                <select
                  value={editedValues.color}
                  onChange={(e) => setEditedValues({
                    ...editedValues,
                    color: e.target.value as ColorCharge
                  })}
                >
                  {Object.values(ColorCharge).map(c => (
                    <option key={c} value={c}>{c}</option>
                  ))}
                </select>
              ) : (
                <span className={`color-tag color-${particle.color.toLowerCase()}`}>
                  {particle.color}
                </span>
              )}
            </div>
          )}
        </div>

        <div className="property-group">
          <h3>Position</h3>
          {['x', 'y', 'z'].map(axis => (
            <div key={axis} className="property-item">
              <label>{axis.toUpperCase()}</label>
              {editMode ? (
                <input
                  type="number"
                  step="0.1"
                  value={editedValues.position[axis as keyof typeof editedValues.position]}
                  onChange={(e) => setEditedValues({
                    ...editedValues,
                    position: {
                      ...editedValues.position,
                      [axis]: parseFloat(e.target.value) || 0
                    }
                  })}
                />
              ) : (
                <span>{particle.position[axis as keyof typeof particle.position].toFixed(3)}</span>
              )}
            </div>
          ))}
        </div>

        <div className="property-group">
          <h3>Momentum</h3>
          {['x', 'y', 'z'].map(axis => (
            <div key={axis} className="property-item">
              <label>p{axis}</label>
              {editMode ? (
                <input
                  type="number"
                  step="0.01"
                  value={editedValues.momentum[axis as keyof typeof editedValues.momentum]}
                  onChange={(e) => setEditedValues({
                    ...editedValues,
                    momentum: {
                      ...editedValues.momentum,
                      [axis]: parseFloat(e.target.value) || 0
                    }
                  })}
                />
              ) : (
                <span>{particle.momentum[axis as keyof typeof particle.momentum].toFixed(3)}</span>
              )}
            </div>
          ))}
          <div className="property-item">
            <label>|p|</label>
            <span>
              {Math.sqrt(
                particle.momentum.x ** 2 +
                particle.momentum.y ** 2 +
                particle.momentum.z ** 2
              ).toFixed(3)} GeV/c
            </span>
          </div>
        </div>

        <div className="property-group">
          <h3>Physical Properties</h3>
          <div className="property-item">
            <label>Energy</label>
            {editMode ? (
              <input
                type="number"
                step="0.001"
                value={editedValues.energy}
                onChange={(e) => setEditedValues({
                  ...editedValues,
                  energy: parseFloat(e.target.value) || 0
                })}
              />
            ) : (
              <span>{particle.energy.toFixed(3)} GeV</span>
            )}
          </div>
          
          <div className="property-item">
            <label>Charge</label>
            <span>{particle.charge > 0 ? '+' : ''}{(particle.charge * 3).toFixed(0)}/3 e</span>
          </div>
          
          <div className="property-item">
            <label>Spin</label>
            {editMode ? (
              <select
                value={editedValues.spin}
                onChange={(e) => setEditedValues({
                  ...editedValues,
                  spin: parseFloat(e.target.value)
                })}
              >
                <option value="0.5">↑ (+1/2)</option>
                <option value="-0.5">↓ (-1/2)</option>
              </select>
            ) : (
              <span>{particle.spin > 0 ? '↑' : '↓'} ({particle.spin > 0 ? '+' : ''}{particle.spin})</span>
            )}
          </div>
          
          <div className="property-item">
            <label>Virtual</label>
            <span>{particle.isVirtual ? 'Yes' : 'No'}</span>
          </div>
        </div>

        {particle.boundPartners.length > 0 && (
          <div className="property-group">
            <h3>Bound State</h3>
            <div className="property-item">
              <label>Partners</label>
              <span>{particle.boundPartners.length} quarks</span>
            </div>
            <div className="bound-partners">
              {particle.boundPartners.map(id => (
                <div key={id} className="partner-id mono">
                  {id.substring(0, 8)}...
                </div>
              ))}
            </div>
          </div>
        )}
      </div>

      {editMode && (
        <div className="edit-actions">
          <button className="save-button" onClick={handleSave}>
            💾 Save Changes
          </button>
          <button className="cancel-button" onClick={handleCancel}>
            ❌ Cancel
          </button>
        </div>
      )}

      <div className="god-mode-warning">
        ⚡ God Mode: Editing particle properties may violate conservation laws
      </div>
    </DraggableWindow>
  )
}