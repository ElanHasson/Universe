import { useRef, useMemo } from 'react'
import { useFrame } from '@react-three/fiber'
import * as THREE from 'three'
import { InteractionSnapshot, Vector3 } from '../types/simulation'

interface InteractionProps {
  interaction: InteractionSnapshot
  position1: Vector3
  position2: Vector3
}

const INTERACTION_COLORS = {
  Strong: '#ff6600',
  Electromagnetic: '#00ffff',
  Weak: '#ff00ff',
  Confinement: '#ff0000',
}

export function Interaction({ interaction, position1, position2 }: InteractionProps) {
  const lineRef = useRef<THREE.Line>(null as any)
  const tubeRef = useRef<THREE.Mesh>(null)
  
  const color = INTERACTION_COLORS[interaction.type] || '#ffffff'
  
  // Create curved path for interaction
  const curve = useMemo(() => {
    const start = new THREE.Vector3(position1.x, position1.y, position1.z)
    const end = new THREE.Vector3(position2.x, position2.y, position2.z)
    const middle = new THREE.Vector3()
      .addVectors(start, end)
      .multiplyScalar(0.5)
    
    // Add some curvature for visual appeal
    const distance = start.distanceTo(end)
    const offset = new THREE.Vector3(
      (Math.random() - 0.5) * distance * 0.1,
      (Math.random() - 0.5) * distance * 0.1,
      (Math.random() - 0.5) * distance * 0.1
    )
    middle.add(offset)
    
    return new THREE.QuadraticBezierCurve3(start, middle, end)
  }, [position1, position2])
  
  const points = curve.getPoints(50)
  const geometry = new THREE.BufferGeometry().setFromPoints(points)
  
  // Animate interaction
  useFrame((state) => {
    if (lineRef.current && lineRef.current.material) {
      const material = lineRef.current.material as THREE.LineBasicMaterial
      
      // Pulse effect based on strength
      material.opacity = 0.3 + Math.abs(Math.sin(state.clock.elapsedTime * 2)) * 0.4 * interaction.strength
      
      // Animate dash offset for flow effect
      if ('dashSize' in material) {
        (material as any).dashSize = 0.1;
        (material as any).gapSize = 0.1;
        (material as any).dashOffset = -state.clock.elapsedTime * 0.5
      }
    }
    
    // Animate confinement tube
    if (tubeRef.current && interaction.type === 'Confinement') {
      const material = tubeRef.current.material as THREE.MeshBasicMaterial
      material.opacity = 0.2 + Math.abs(Math.sin(state.clock.elapsedTime * 3)) * 0.2
    }
  })
  
  return (
    <group>
      {/* Main interaction line */}
      <primitive object={new THREE.Line(geometry, new THREE.LineBasicMaterial({ 
        color,
        transparent: true,
        opacity: 0.6
      }))} ref={lineRef} />
      
      {/* Confinement tube for strong force */}
      {interaction.type === 'Confinement' && (
        <mesh ref={tubeRef}>
          <tubeGeometry args={[curve, 20, 0.05, 8, false]} />
          <meshBasicMaterial
            color={color}
            transparent
            opacity={0.3}
            side={THREE.DoubleSide}
          />
        </mesh>
      )}
      
      {/* Force vector arrow */}
      {interaction.forceVector && (
        <arrowHelper
          args={[
            new THREE.Vector3(
              interaction.forceVector.x,
              interaction.forceVector.y,
              interaction.forceVector.z
            ).normalize(),
            new THREE.Vector3(position1.x, position1.y, position1.z),
            interaction.strength * 0.5,
            color,
            0.2,
            0.1,
          ]}
        />
      )}
    </group>
  )
}