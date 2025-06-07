import { useState, useRef, useEffect, ReactNode } from 'react'
import './DraggableWindow.css'

interface DraggableWindowProps {
  children: ReactNode
  title: string
  onClose?: () => void
  defaultPosition?: { x: number; y: number }
  defaultSize?: { width: number; height: number }
  minWidth?: number
  minHeight?: number
  className?: string
}

export function DraggableWindow({
  children,
  title,
  onClose,
  defaultPosition = { x: 20, y: 20 },
  defaultSize = { width: 320, height: 400 },
  minWidth = 200,
  minHeight = 100,
  className = '',
}: DraggableWindowProps) {
  const [position, setPosition] = useState(defaultPosition)
  const [size, setSize] = useState(defaultSize)
  const [isDragging, setIsDragging] = useState(false)
  const [isResizing, setIsResizing] = useState(false)
  const [dragStart, setDragStart] = useState({ x: 0, y: 0 })
  const windowRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const handleMouseMove = (e: MouseEvent) => {
      if (isDragging && dragStart) {
        setPosition({
          x: e.clientX - dragStart.x,
          y: e.clientY - dragStart.y,
        })
      } else if (isResizing) {
        const newWidth = Math.max(minWidth, e.clientX - position.x)
        const newHeight = Math.max(minHeight, e.clientY - position.y)
        setSize({ width: newWidth, height: newHeight })
      }
    }

    const handleMouseUp = () => {
      setIsDragging(false)
      setIsResizing(false)
    }

    if (isDragging || isResizing) {
      document.addEventListener('mousemove', handleMouseMove)
      document.addEventListener('mouseup', handleMouseUp)
      
      return () => {
        document.removeEventListener('mousemove', handleMouseMove)
        document.removeEventListener('mouseup', handleMouseUp)
      }
    }
  }, [isDragging, isResizing, dragStart, position, minWidth, minHeight])

  const handleDragStart = (e: React.MouseEvent) => {
    const rect = windowRef.current?.getBoundingClientRect()
    if (rect) {
      setDragStart({
        x: e.clientX - rect.left,
        y: e.clientY - rect.top,
      })
      setIsDragging(true)
    }
  }

  const handleResizeStart = (e: React.MouseEvent) => {
    e.stopPropagation()
    setIsResizing(true)
  }

  return (
    <div
      ref={windowRef}
      className={`draggable-window ${className}`}
      style={{
        left: `${position.x}px`,
        top: `${position.y}px`,
        width: `${size.width}px`,
        height: `${size.height}px`,
      }}
    >
      <div
        className="window-header"
        onMouseDown={handleDragStart}
      >
        <h3>{title}</h3>
        {onClose && (
          <button className="window-close" onClick={onClose}>
            ✕
          </button>
        )}
      </div>
      
      <div className="window-content">
        {children}
      </div>
      
      <div
        className="resize-handle"
        onMouseDown={handleResizeStart}
      />
    </div>
  )
}