import { useRef, useMemo, useState } from 'react'
import { useFrame, useThree } from '@react-three/fiber'
import { OrbitControls, Grid, Stars } from '@react-three/drei'
import * as THREE from 'three'
import { Particle } from './Particle'
import { Interaction } from './Interaction'
import { FieldVisualization } from './FieldVisualization'
import { SimulationSnapshot, ParticleSnapshot } from '../types/simulation'

interface SimulationSceneProps {
  snapshot: SimulationSnapshot | null
  selectedParticleId?: string
  onParticleSelect: (particle: ParticleSnapshot | null) => void
  showFields: boolean
  showInteractions: boolean
}

export function SimulationScene({
  snapshot,
  selectedParticleId,
  onParticleSelect,
  showFields,
  showInteractions,
}: SimulationSceneProps) {
  const { camera } = useThree()
  const sceneRef = useRef<THREE.Group>(null)

  // Auto-center camera only once when particles first appear
  const [cameraInitialized, setCameraInitialized] = useState(false)
  
  useMemo(() => {
    if (!cameraInitialized && snapshot && snapshot.particles.length > 0) {
      const positions = snapshot.particles.map(p => p.position)
      const center = positions.reduce(
        (acc, pos) => ({
          x: acc.x + pos.x / positions.length,
          y: acc.y + pos.y / positions.length,
          z: acc.z + pos.z / positions.length,
        }),
        { x: 0, y: 0, z: 0 }
      )
      
      // Find max distance from center
      const maxDist = Math.max(...positions.map(pos => 
        Math.sqrt(
          Math.pow(pos.x - center.x, 2) +
          Math.pow(pos.y - center.y, 2) +
          Math.pow(pos.z - center.z, 2)
        )
      ))
      
      // Set camera to view all particles
      if (maxDist > 0.1) {
        camera.position.set(
          center.x + maxDist * 5,
          center.y + maxDist * 3,
          center.z + maxDist * 5
        )
        camera.lookAt(center.x, center.y, center.z)
        setCameraInitialized(true)
      }
    }
  }, [snapshot, camera, cameraInitialized])

  useFrame(() => {
    // Animation updates can go here
  })

  if (!snapshot) {
    return (
      <>
        <ambientLight intensity={0.5} />
        <pointLight position={[10, 10, 10]} />
        <Stars radius={100} depth={50} count={5000} factor={4} saturation={0} fade speed={1} />
        <Grid args={[20, 20]} cellSize={1} cellThickness={0.5} cellColor="#1a1a2e" sectionColor="#16213e" />
      </>
    )
  }

  return (
    <>
      <ambientLight intensity={0.3} />
      <pointLight position={[10, 10, 10]} intensity={0.5} />
      <pointLight position={[-10, -10, -10]} intensity={0.3} color="#4488ff" />
      
      <Stars radius={100} depth={50} count={5000} factor={4} saturation={0} fade speed={1} />
      
      <group ref={sceneRef}>
        {/* Particles */}
        {snapshot.particles.map((particle) => (
          <Particle
            key={particle.particleId}
            particle={particle}
            isSelected={particle.particleId === selectedParticleId}
            onClick={() => onParticleSelect(particle)}
          />
        ))}

        {/* Interactions */}
        {showInteractions && snapshot.interactions.map((interaction) => {
          const particle1 = snapshot.particles.find(p => p.particleId === interaction.particle1Id)
          const particle2 = snapshot.particles.find(p => p.particleId === interaction.particle2Id)
          
          if (!particle1 || !particle2) return null
          
          return (
            <Interaction
              key={interaction.interactionId}
              interaction={interaction}
              position1={particle1.position}
              position2={particle2.position}
            />
          )
        })}

        {/* Field Visualization */}
        {showFields && snapshot.fields.length > 0 && (
          <FieldVisualization fields={snapshot.fields} />
        )}
      </group>

      <OrbitControls
        enablePan={true}
        enableZoom={true}
        enableRotate={true}
        zoomSpeed={0.6}
        panSpeed={0.5}
        rotateSpeed={0.4}
        minDistance={0.1}
        maxDistance={100}
      />
      
      <Grid 
        args={[50, 50]} 
        cellSize={1} 
        cellThickness={0.5} 
        cellColor="#1a1a2e" 
        sectionColor="#16213e"
        fadeDistance={50}
        infiniteGrid
      />
    </>
  )
}