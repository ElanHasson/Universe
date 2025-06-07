import { useState, useEffect, useCallback, useRef } from 'react'
import { SimulationSnapshot, ParticleType, ColorCharge, QuarkFlavor } from '../types/simulation'

export function useSimulation() {
  const [snapshot, setSnapshot] = useState<SimulationSnapshot | null>(null)
  const [connected, setConnected] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const wsRef = useRef<WebSocket | null>(null)
  const reconnectTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const reconnectAttemptsRef = useRef(0)

  const connect = useCallback(() => {
    try {
      // Connect to the WebSocket endpoint
      const ws = new WebSocket('ws://localhost:5000/ws')
      wsRef.current = ws

      ws.onopen = () => {
        console.log('WebSocket connected')
        setConnected(true)
        setError(null)
        reconnectAttemptsRef.current = 0
        
        // Request initial snapshot
        ws.send(JSON.stringify({ type: 'getSnapshot' }))
      }

      ws.onmessage = (event) => {
        try {
          const data = JSON.parse(event.data)
          
          if (data.type === 'snapshot') {
            setSnapshot(data.payload)
          } else if (data.type === 'error') {
            setError(data.message)
          }
        } catch (err) {
          console.error('Failed to parse WebSocket message:', err)
        }
      }

      ws.onerror = (event) => {
        console.error('WebSocket error:', event)
        setError('Connection error')
      }

      ws.onclose = () => {
        console.log('WebSocket disconnected')
        setConnected(false)
        wsRef.current = null
        
        // Attempt to reconnect with exponential backoff
        if (reconnectAttemptsRef.current < 5) {
          const delay = Math.min(1000 * Math.pow(2, reconnectAttemptsRef.current), 10000)
          reconnectAttemptsRef.current++
          
          reconnectTimeoutRef.current = setTimeout(() => {
            console.log(`Attempting to reconnect (${reconnectAttemptsRef.current}/5)...`)
            connect()
          }, delay)
        } else {
          setError('Failed to connect after 5 attempts')
        }
      }
    } catch (err) {
      console.error('Failed to create WebSocket:', err)
      setError('Failed to connect')
    }
  }, [])

  const sendCommand = useCallback((command: string, payload?: any) => {
    if (wsRef.current && wsRef.current.readyState === WebSocket.OPEN) {
      wsRef.current.send(JSON.stringify({ type: command, payload }))
    } else {
      console.error('WebSocket not connected')
      setError('Not connected')
    }
  }, [])

  useEffect(() => {
    connect()

    return () => {
      if (reconnectTimeoutRef.current) {
        clearTimeout(reconnectTimeoutRef.current)
      }
      if (wsRef.current) {
        wsRef.current.close()
      }
    }
  }, [connect])

  // For development: use mock data if no WebSocket connection
  useEffect(() => {
    if (!connected && !snapshot) {
      // Set mock data after a delay
      const timeout = setTimeout(() => {
        setSnapshot(getMockSnapshot())
      }, 1000)
      
      return () => clearTimeout(timeout)
    }
  }, [connected, snapshot])

  return {
    snapshot,
    connected,
    error,
    sendCommand,
  }
}

// Mock data for development
function getMockSnapshot(): SimulationSnapshot {
  return {
    timestamp: new Date().toISOString(),
    simulationTime: 0,
    particles: [
      {
        particleId: 'mock-1',
        type: ParticleType.Quark,
        position: { x: 0, y: 0, z: 0 },
        momentum: { x: 0.1, y: 0, z: 0 },
        energy: 0.002,
        color: ColorCharge.Red,
        charge: 2/3,
        spin: 0.5,
        isVirtual: false,
        boundPartners: ['mock-2', 'mock-3'],
        flavor: QuarkFlavor.Up,
      },
      {
        particleId: 'mock-2',
        type: ParticleType.Quark,
        position: { x: 0.5, y: 0.5, z: 0 },
        momentum: { x: -0.05, y: -0.05, z: 0 },
        energy: 0.002,
        color: ColorCharge.Green,
        charge: 2/3,
        spin: -0.5,
        isVirtual: false,
        boundPartners: ['mock-1', 'mock-3'],
        flavor: QuarkFlavor.Up,
      },
      {
        particleId: 'mock-3',
        type: ParticleType.Quark,
        position: { x: 0.5, y: -0.5, z: 0 },
        momentum: { x: -0.05, y: 0.05, z: 0 },
        energy: 0.005,
        color: ColorCharge.Blue,
        charge: -1/3,
        spin: 0.5,
        isVirtual: false,
        boundPartners: ['mock-1', 'mock-2'],
        flavor: QuarkFlavor.Down,
      },
    ],
    interactions: [
      {
        interactionId: 'int-1',
        type: 'Strong',
        particle1Id: 'mock-1',
        particle2Id: 'mock-2',
        forceVector: { x: 0.1, y: 0.1, z: 0 },
        strength: 0.8,
        range: 1.0,
      },
      {
        interactionId: 'int-2',
        type: 'Strong',
        particle1Id: 'mock-2',
        particle2Id: 'mock-3',
        forceVector: { x: 0, y: -0.1, z: 0 },
        strength: 0.8,
        range: 1.0,
      },
      {
        interactionId: 'int-3',
        type: 'Strong',
        particle1Id: 'mock-1',
        particle2Id: 'mock-3',
        forceVector: { x: -0.1, y: 0, z: 0 },
        strength: 0.8,
        range: 1.0,
      },
    ],
    fields: [],
    metrics: {
      totalEnergy: 0.009,
      totalMomentum: 0.001,
      totalAngularMomentum: 0.0001,
      averageBindingEnergy: -0.5,
      vacuumEnergyDensity: 0.001,
      colorSinglets: 1,
      freeQuarks: 0,
      particleCounts: {
        'Up': 2,
        'Down': 1,
        'Baryons': 1,
      },
    },
  }
}