import { useMemo } from 'react'
import * as THREE from 'three'
import { FieldSnapshot } from '../types/simulation'

interface FieldVisualizationProps {
  fields: FieldSnapshot[]
}

export function FieldVisualization({ fields }: FieldVisualizationProps) {
  const fieldMesh = useMemo(() => {
    // Create a point cloud for field visualization
    const geometry = new THREE.BufferGeometry()
    const positions: number[] = []
    const colors: number[] = []
    const sizes: number[] = []
    
    fields.forEach(field => {
      positions.push(field.position.x, field.position.y, field.position.z)
      
      // Color based on field strength
      const strength = field.fieldStrength
      const normalizedStrength = Math.min(strength / 10, 1) // Normalize to 0-1
      
      // Gradient from blue (weak) to red (strong)
      const r = normalizedStrength
      const g = 0.2
      const b = 1 - normalizedStrength
      
      colors.push(r, g, b)
      
      // Size based on energy density
      const size = Math.min(field.energyDensity * 0.5, 0.3)
      sizes.push(size)
    })
    
    geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3))
    geometry.setAttribute('color', new THREE.Float32BufferAttribute(colors, 3))
    geometry.setAttribute('size', new THREE.Float32BufferAttribute(sizes, 1))
    
    return geometry
  }, [fields])
  
  const fieldLines = useMemo(() => {
    // Create field lines for chromoelectric fields
    const lines: React.ReactElement[] = []
    
    // Sample a subset of fields for performance
    const sampledFields = fields.filter((_, i) => i % 5 === 0).slice(0, 100)
    
    sampledFields.forEach((field, index) => {
      if (field.chromoelectricField.x === 0 && 
          field.chromoelectricField.y === 0 && 
          field.chromoelectricField.z === 0) {
        return
      }
      
      const start = new THREE.Vector3(
        field.position.x,
        field.position.y,
        field.position.z
      )
      
      const direction = new THREE.Vector3(
        field.chromoelectricField.x,
        field.chromoelectricField.y,
        field.chromoelectricField.z
      ).normalize()
      
      const length = Math.min(field.fieldStrength * 0.2, 1)
      
      lines.push(
        <arrowHelper
          key={`field-${index}`}
          args={[
            direction,
            start,
            length,
            0x4488ff,
            length * 0.3,
            length * 0.2,
          ]}
        />
      )
    })
    
    return lines
  }, [fields])
  
  return (
    <group>
      {/* Field strength visualization as point cloud */}
      <points geometry={fieldMesh}>
        <pointsMaterial
          size={0.1}
          sizeAttenuation={true}
          vertexColors={true}
          transparent={true}
          opacity={0.6}
          blending={THREE.AdditiveBlending}
        />
      </points>
      
      {/* Field direction arrows */}
      {fieldLines}
    </group>
  )
}