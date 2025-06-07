import { useRef, useState } from 'react'
import { useFrame } from '@react-three/fiber'
import { Html } from '@react-three/drei'
import * as THREE from 'three'
import { ParticleSnapshot, ColorCharge, ParticleType } from '../types/simulation'

interface ParticleProps {
  particle: ParticleSnapshot
  isSelected: boolean
  onClick: () => void
}

const COLOR_MAP: Record<ColorCharge, string> = {
  [ColorCharge.Red]: '#ff0000',
  [ColorCharge.Green]: '#00ff00',
  [ColorCharge.Blue]: '#0080ff',
  [ColorCharge.AntiRed]: '#ff8080',
  [ColorCharge.AntiGreen]: '#80ff80',
  [ColorCharge.AntiBlue]: '#80c0ff',
}

const PARTICLE_SIZES: Record<ParticleType, number> = {
  [ParticleType.Quark]: 0.2,
  [ParticleType.Antiquark]: 0.2,
  [ParticleType.Gluon]: 0.15,
  [ParticleType.Photon]: 0.1,
  [ParticleType.WBoson]: 0.3,
  [ParticleType.ZBoson]: 0.3,
  [ParticleType.Meson]: 0.4,
  [ParticleType.Baryon]: 0.5,
}

export function Particle({ particle, isSelected, onClick }: ParticleProps) {
  const meshRef = useRef<THREE.Mesh>(null)
  const [hovered, setHovered] = useState(false)
  
  // Get particle color
  const getParticleColor = () => {
    if (particle.color) {
      return COLOR_MAP[particle.color] || '#ffffff'
    }
    
    // Default colors for non-colored particles
    switch (particle.type) {
      case ParticleType.Photon:
        return '#ffff00'
      case ParticleType.WBoson:
        return '#ff00ff'
      case ParticleType.ZBoson:
        return '#00ffff'
      case ParticleType.Gluon:
        return '#ff8800'
      default:
        return '#ffffff'
    }
  }
  
  const color = getParticleColor()
  const size = PARTICLE_SIZES[particle.type] || 0.2
  
  // Animate selection
  useFrame((state) => {
    if (meshRef.current) {
      // Pulse effect for selected particle
      if (isSelected) {
        meshRef.current.scale.setScalar(size * (1 + Math.sin(state.clock.elapsedTime * 3) * 0.1))
      } else {
        meshRef.current.scale.setScalar(size * (hovered ? 1.2 : 1))
      }
      
      // Spin for bosons
      if (particle.type === ParticleType.Gluon || 
          particle.type === ParticleType.Photon ||
          particle.type === ParticleType.WBoson ||
          particle.type === ParticleType.ZBoson) {
        meshRef.current.rotation.y += 0.01
      }
      
      // Virtual particle effect
      if (particle.isVirtual && meshRef.current.material) {
        ;(meshRef.current.material as THREE.MeshStandardMaterial).opacity = 
          0.5 + Math.sin(state.clock.elapsedTime * 5) * 0.3
      }
    }
  })
  
  return (
    <group position={[particle.position.x, particle.position.y, particle.position.z]}>
      <mesh
        ref={meshRef}
        onClick={(e) => {
          e.stopPropagation()
          onClick()
        }}
        onPointerOver={(e) => {
          e.stopPropagation()
          setHovered(true)
          document.body.style.cursor = 'pointer'
        }}
        onPointerOut={() => {
          setHovered(false)
          document.body.style.cursor = 'auto'
        }}
      >
        {particle.type === ParticleType.Quark || particle.type === ParticleType.Antiquark ? (
          <sphereGeometry args={[1, 16, 16]} />
        ) : particle.type === ParticleType.Gluon ? (
          <octahedronGeometry args={[1]} />
        ) : particle.type === ParticleType.Photon ? (
          <icosahedronGeometry args={[1]} />
        ) : (
          <dodecahedronGeometry args={[1]} />
        )}
        
        <meshStandardMaterial
          color={color}
          emissive={color}
          emissiveIntensity={isSelected ? 0.5 : (hovered ? 0.3 : 0.1)}
          metalness={0.3}
          roughness={0.4}
          transparent={particle.isVirtual}
          opacity={particle.isVirtual ? 0.7 : 1}
        />
      </mesh>
      
      {/* Selection ring */}
      {isSelected && (
        <mesh rotation={[Math.PI / 2, 0, 0]}>
          <ringGeometry args={[size * 1.5, size * 1.7, 32]} />
          <meshBasicMaterial color="#ffffff" side={THREE.DoubleSide} />
        </mesh>
      )}
      
      {/* Bound state connections */}
      {particle.boundPartners.length > 0 && (
        <mesh>
          <sphereGeometry args={[size * 2, 16, 16]} />
          <meshBasicMaterial
            color="#4488ff"
            transparent
            opacity={0.1}
            side={THREE.BackSide}
          />
        </mesh>
      )}
      
      {/* Label on hover */}
      {hovered && (
        <Html
          position={[0, size * 2, 0]}
          center
          style={{
            transition: 'all 0.2s',
            opacity: 1,
            transform: 'scale(1)',
          }}
        >
          <div
            style={{
              background: 'rgba(0, 0, 0, 0.8)',
              color: 'white',
              padding: '4px 8px',
              borderRadius: '4px',
              fontSize: '12px',
              whiteSpace: 'nowrap',
            }}
          >
            {particle.type} {particle.flavor && `(${particle.flavor})`}
            {particle.isVirtual && ' [Virtual]'}
          </div>
        </Html>
      )}
    </group>
  )
}