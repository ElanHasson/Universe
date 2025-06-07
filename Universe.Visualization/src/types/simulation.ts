export interface Vector3 {
  x: number
  y: number
  z: number
}

export enum QuarkFlavor {
  Up = 'Up',
  Down = 'Down',
  Charm = 'Charm',
  Strange = 'Strange',
  Top = 'Top',
  Bottom = 'Bottom',
}

export enum ColorCharge {
  Red = 'Red',
  Green = 'Green',
  Blue = 'Blue',
  AntiRed = 'AntiRed',
  AntiGreen = 'AntiGreen',
  AntiBlue = 'AntiBlue',
}

export enum ParticleType {
  Quark = 'Quark',
  Antiquark = 'Antiquark',
  Gluon = 'Gluon',
  Photon = 'Photon',
  WBoson = 'WBoson',
  ZBoson = 'ZBoson',
  Meson = 'Meson',
  Baryon = 'Baryon',
}

export interface ParticleSnapshot {
  particleId: string
  type: ParticleType
  position: Vector3
  momentum: Vector3
  energy: number
  color?: ColorCharge
  charge: number
  spin: number
  isVirtual: boolean
  boundPartners: string[]
  flavor?: QuarkFlavor
}

export interface InteractionSnapshot {
  interactionId: string
  type: 'Strong' | 'Electromagnetic' | 'Weak' | 'Confinement'
  particle1Id: string
  particle2Id: string
  forceVector: Vector3
  strength: number
  range: number
}

export interface FieldSnapshot {
  position: Vector3
  chromoelectricField: Vector3
  chromomagneticField: Vector3
  fieldStrength: number
  energyDensity: number
}

export interface PhysicsMetrics {
  totalEnergy: number
  totalMomentum: number
  totalAngularMomentum: number
  averageBindingEnergy: number
  vacuumEnergyDensity: number
  colorSinglets: number
  freeQuarks: number
  particleCounts: Record<string, number>
}

export interface SimulationSnapshot {
  timestamp: string
  simulationTime: number
  particles: ParticleSnapshot[]
  interactions: InteractionSnapshot[]
  fields: FieldSnapshot[]
  metrics: PhysicsMetrics
}