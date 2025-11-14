import { useRef, useEffect, useCallback, useState } from 'react';
import PropTypes from 'prop-types';

/**
 * Wydajny komponent Canvas do renderowania pola bitwy
 * Używa viewport rendering - rysuje tylko widoczny obszar
 */
export const BattleCanvas = ({
  battleState,
  selectedShip,
  onShipClick,
  onCellClick,
  orders,
}) => {
  const canvasRef = useRef(null);
  const [viewport, setViewport] = useState({
    x: 0,
    y: 0,
    zoom: 1,
    cellSize: 20, // Rozmiar komórki w pikselach
  });
  const [isDragging, setIsDragging] = useState(false);
  const [dragStart, setDragStart] = useState({ x: 0, y: 0 });

  const { width, height, fractions } = battleState || {};

  // Funkcja do konwersji współrzędnych ekranu na współrzędne mapy
  const screenToMap = useCallback((screenX, screenY) => {
    const canvas = canvasRef.current;
    if (!canvas) return { x: 0, y: 0 };

    const rect = canvas.getBoundingClientRect();
    const x = Math.floor((screenX - rect.left) / viewport.cellSize / viewport.zoom + viewport.x);
    const y = Math.floor((screenY - rect.top) / viewport.cellSize / viewport.zoom + viewport.y);

    return { x, y };
  }, [viewport]);

  // Funkcja do konwersji współrzędnych mapy na współrzędne ekranu
  const mapToScreen = useCallback((mapX, mapY) => {
    const x = (mapX - viewport.x) * viewport.cellSize * viewport.zoom;
    const y = (mapY - viewport.y) * viewport.cellSize * viewport.zoom;
    return { x, y };
  }, [viewport]);

  // Znajdź statek w danej pozycji
  const findShipAt = useCallback((mapX, mapY) => {
    if (!fractions) return null;

    for (const fraction of fractions) {
      for (const ship of fraction.ships) {
        const shipX = Math.floor(ship.x);
        const shipY = Math.floor(ship.y);
        if (shipX === mapX && shipY === mapY) {
          return { ship, fraction };
        }
      }
    }
    return null;
  }, [fractions]);

  // Renderuj siatkę
  const drawGrid = useCallback((ctx, visibleArea) => {
    ctx.strokeStyle = '#333';
    ctx.lineWidth = 0.5;

    // Rysuj tylko widoczne linie
    for (let x = visibleArea.startX; x <= visibleArea.endX; x++) {
      const screenPos = mapToScreen(x, 0);
      ctx.beginPath();
      ctx.moveTo(screenPos.x, 0);
      ctx.lineTo(screenPos.x, ctx.canvas.height);
      ctx.stroke();
    }

    for (let y = visibleArea.startY; y <= visibleArea.endY; y++) {
      const screenPos = mapToScreen(0, y);
      ctx.beginPath();
      ctx.moveTo(0, screenPos.y);
      ctx.lineTo(ctx.canvas.width, screenPos.y);
      ctx.stroke();
    }
  }, [mapToScreen]);

  // Renderuj zasięg ruchu wybranego statku
  const drawMoveRange = useCallback((ctx, visibleArea) => {
    if (!selectedShip || !selectedShip.speed) return;

    const { cellSize, zoom } = viewport;
    const scaledCellSize = cellSize * zoom;
    const shipX = Math.floor(selectedShip.x);
    const shipY = Math.floor(selectedShip.y);
    const range = selectedShip.speed;

    ctx.fillStyle = 'rgba(100, 200, 255, 0.15)';
    ctx.strokeStyle = 'rgba(100, 200, 255, 0.3)';
    ctx.lineWidth = 1;

    // Rysuj dostępne komórki w zasięgu (Manhattan distance)
    for (let dx = -range; dx <= range; dx++) {
      for (let dy = -range; dy <= range; dy++) {
        const distance = Math.abs(dx) + Math.abs(dy);
        if (distance > range) continue;

        const targetX = shipX + dx;
        const targetY = shipY + dy;

        // Sprawdź czy w widocznym obszarze
        if (targetX < visibleArea.startX || targetX > visibleArea.endX ||
            targetY < visibleArea.startY || targetY > visibleArea.endY) {
          continue;
        }

        // Sprawdź czy to nie jest pozycja obecnego statku
        if (dx === 0 && dy === 0) continue;

        const screenX = (targetX - viewport.x) * scaledCellSize;
        const screenY = (targetY - viewport.y) * scaledCellSize;

        ctx.fillRect(screenX, screenY, scaledCellSize, scaledCellSize);
        ctx.strokeRect(screenX, screenY, scaledCellSize, scaledCellSize);
      }
    }
  }, [selectedShip, viewport]);

  // Renderuj statki
  const drawShips = useCallback((ctx, visibleArea) => {
    if (!fractions) return;

    const { cellSize, zoom } = viewport;
    const scaledCellSize = cellSize * zoom;
    const shipRadius = scaledCellSize * 0.4;

    fractions.forEach((fraction, fractionIndex) => {
      // Kolor dla każdej frakcji
      const colors = ['#4CAF50', '#F44336', '#2196F3', '#FF9800', '#9C27B0'];
      const color = colors[fractionIndex % colors.length];

      fraction.ships.forEach(ship => {
        const shipX = Math.floor(ship.x);
        const shipY = Math.floor(ship.y);

        // Sprawdź czy statek jest w widocznym obszarze
        if (shipX < visibleArea.startX || shipX > visibleArea.endX ||
            shipY < visibleArea.startY || shipY > visibleArea.endY) {
          return;
        }

        const screenPos = mapToScreen(shipX + 0.5, shipY + 0.5);

        // Podświetl wybrany statek
        if (selectedShip && selectedShip.shipId === ship.shipId) {
          ctx.fillStyle = 'rgba(255, 255, 0, 0.3)';
          ctx.fillRect(
            (shipX - viewport.x) * scaledCellSize,
            (shipY - viewport.y) * scaledCellSize,
            scaledCellSize,
            scaledCellSize
          );
        }

        // Rysuj statek jako koło
        ctx.fillStyle = color;
        ctx.beginPath();
        ctx.arc(screenPos.x, screenPos.y, shipRadius, 0, Math.PI * 2);
        ctx.fill();

        // Obramowanie
        ctx.strokeStyle = '#000';
        ctx.lineWidth = 2;
        ctx.stroke();

        // HP bar
        const hpPercent = ship.hitPoints / 100; // Zakładam max 100 HP
        const barWidth = scaledCellSize * 0.8;
        const barHeight = 4;
        const barX = (shipX - viewport.x) * scaledCellSize + (scaledCellSize - barWidth) / 2;
        const barY = (shipY - viewport.y + 0.9) * scaledCellSize;

        ctx.fillStyle = '#333';
        ctx.fillRect(barX, barY, barWidth, barHeight);
        ctx.fillStyle = hpPercent > 0.5 ? '#4CAF50' : hpPercent > 0.25 ? '#FF9800' : '#F44336';
        ctx.fillRect(barX, barY, barWidth * hpPercent, barHeight);
      });
    });
  }, [fractions, viewport, selectedShip, mapToScreen]);

  // Renderuj rozkazy
  const drawOrders = useCallback((ctx) => {
    if (!orders || orders.length === 0 || !fractions) return;

    orders.forEach(order => {
      // Znajdź statek który wydał rozkaz
      let ship = null;
      for (const fraction of fractions) {
        ship = fraction.ships.find(s => s.shipId === order.shipId);
        if (ship) break;
      }

      if (!ship) return;

      const startPos = mapToScreen(ship.x + 0.5, ship.y + 0.5);

      if (order.type === 'move') {
        // Rysuj strzałkę ruchu
        const endPos = mapToScreen(order.targetX + 0.5, order.targetY + 0.5);
        
        ctx.strokeStyle = 'rgba(100, 200, 255, 0.8)';
        ctx.lineWidth = 3;
        ctx.setLineDash([5, 5]);
        
        ctx.beginPath();
        ctx.moveTo(startPos.x, startPos.y);
        ctx.lineTo(endPos.x, endPos.y);
        ctx.stroke();
        
        ctx.setLineDash([]);

        // Strzałka
        const angle = Math.atan2(endPos.y - startPos.y, endPos.x - startPos.x);
        const arrowSize = 10;
        ctx.fillStyle = 'rgba(100, 200, 255, 0.8)';
        ctx.beginPath();
        ctx.moveTo(endPos.x, endPos.y);
        ctx.lineTo(
          endPos.x - arrowSize * Math.cos(angle - Math.PI / 6),
          endPos.y - arrowSize * Math.sin(angle - Math.PI / 6)
        );
        ctx.lineTo(
          endPos.x - arrowSize * Math.cos(angle + Math.PI / 6),
          endPos.y - arrowSize * Math.sin(angle + Math.PI / 6)
        );
        ctx.closePath();
        ctx.fill();
      } else if (order.type === 'laser' || order.type === 'missile') {
        // Znajdź cel
        let targetShip = null;
        for (const fraction of fractions) {
          targetShip = fraction.ships.find(s => s.shipId === order.targetShipId);
          if (targetShip) break;
        }

        if (targetShip) {
          const endPos = mapToScreen(targetShip.x + 0.5, targetShip.y + 0.5);
          
          const color = order.type === 'laser' ? 'rgba(255, 50, 50, 0.8)' : 'rgba(255, 150, 0, 0.8)';
          ctx.strokeStyle = color;
          ctx.lineWidth = 2;
          ctx.setLineDash([3, 3]);
          
          ctx.beginPath();
          ctx.moveTo(startPos.x, startPos.y);
          ctx.lineTo(endPos.x, endPos.y);
          ctx.stroke();
          
          ctx.setLineDash([]);

          // Cel - krzyżyk
          ctx.strokeStyle = color;
          ctx.lineWidth = 2;
          const crossSize = 8;
          ctx.beginPath();
          ctx.moveTo(endPos.x - crossSize, endPos.y - crossSize);
          ctx.lineTo(endPos.x + crossSize, endPos.y + crossSize);
          ctx.moveTo(endPos.x + crossSize, endPos.y - crossSize);
          ctx.lineTo(endPos.x - crossSize, endPos.y + crossSize);
          ctx.stroke();
        }
      }
    });
  }, [orders, fractions, mapToScreen]);

  // Główna funkcja renderująca
  const render = useCallback(() => {
    const canvas = canvasRef.current;
    if (!canvas || !battleState) return;

    const ctx = canvas.getContext('2d');
    
    // Wyczyść canvas
    ctx.fillStyle = '#1a1a1a';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    // Oblicz widoczny obszar
    const visibleArea = {
      startX: Math.max(0, Math.floor(viewport.x)),
      startY: Math.max(0, Math.floor(viewport.y)),
      endX: Math.min(width - 1, Math.ceil(viewport.x + canvas.width / (viewport.cellSize * viewport.zoom))),
      endY: Math.min(height - 1, Math.ceil(viewport.y + canvas.height / (viewport.cellSize * viewport.zoom))),
    };

    // Renderuj warstwy
    drawGrid(ctx, visibleArea);
    drawMoveRange(ctx, visibleArea);
    drawShips(ctx, visibleArea);
    drawOrders(ctx);

  }, [battleState, viewport, width, height, drawGrid, drawMoveRange, drawShips, drawOrders]);

  // Renderuj przy zmianach
  useEffect(() => {
    render();
  }, [render]);

  // Obsługa resize
  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const resizeCanvas = () => {
      const container = canvas.parentElement;
      canvas.width = container.clientWidth;
      canvas.height = container.clientHeight;
      render();
    };

    resizeCanvas();
    window.addEventListener('resize', resizeCanvas);

    return () => window.removeEventListener('resize', resizeCanvas);
  }, [render]);

  // Obsługa kliknięcia
  const handleClick = useCallback((e) => {
    const mapPos = screenToMap(e.clientX, e.clientY);
    
    // Sprawdź czy kliknięto w statek
    const shipData = findShipAt(mapPos.x, mapPos.y);
    
    if (shipData) {
      onShipClick?.(shipData.ship, shipData.fraction);
    } else {
      onCellClick?.(mapPos.x, mapPos.y);
    }
  }, [screenToMap, findShipAt, onShipClick, onCellClick]);

  // Obsługa przeciągania (pan)
  const handleMouseDown = useCallback((e) => {
    if (e.button === 1 || (e.button === 0 && e.ctrlKey)) { // Środkowy przycisk lub Ctrl+LMB
      setIsDragging(true);
      setDragStart({ x: e.clientX, y: e.clientY });
      e.preventDefault();
    }
  }, []);

  const handleMouseMove = useCallback((e) => {
    if (!isDragging) return;

    const dx = (e.clientX - dragStart.x) / (viewport.cellSize * viewport.zoom);
    const dy = (e.clientY - dragStart.y) / (viewport.cellSize * viewport.zoom);

    setViewport(prev => ({
      ...prev,
      x: Math.max(0, Math.min(width - 1, prev.x - dx)),
      y: Math.max(0, Math.min(height - 1, prev.y - dy)),
    }));

    setDragStart({ x: e.clientX, y: e.clientY });
  }, [isDragging, dragStart, viewport, width, height]);

  const handleMouseUp = useCallback(() => {
    setIsDragging(false);
  }, []);

  // Obsługa zoom (scroll)
  const handleWheel = useCallback((e) => {
    e.preventDefault();
    
    const delta = e.deltaY > 0 ? 0.9 : 1.1;
    const newZoom = Math.max(0.5, Math.min(3, viewport.zoom * delta));

    setViewport(prev => ({
      ...prev,
      zoom: newZoom,
    }));
  }, [viewport]);

  return (
    <canvas
      ref={canvasRef}
      onClick={handleClick}
      onMouseDown={handleMouseDown}
      onMouseMove={handleMouseMove}
      onMouseUp={handleMouseUp}
      onMouseLeave={handleMouseUp}
      onWheel={handleWheel}
      style={{
        width: '100%',
        height: '100%',
        cursor: isDragging ? 'grabbing' : 'crosshair',
      }}
    />
  );
};

BattleCanvas.propTypes = {
  battleState: PropTypes.shape({
    width: PropTypes.number.isRequired,
    height: PropTypes.number.isRequired,
    fractions: PropTypes.arrayOf(PropTypes.shape({
      fractionId: PropTypes.string.isRequired,
      ships: PropTypes.arrayOf(PropTypes.shape({
        shipId: PropTypes.string.isRequired,
        x: PropTypes.number.isRequired,
        y: PropTypes.number.isRequired,
        speed: PropTypes.number,
        hitPoints: PropTypes.number,
      })).isRequired,
    })).isRequired,
  }),
  selectedShip: PropTypes.object,
  onShipClick: PropTypes.func,
  onCellClick: PropTypes.func,
  orders: PropTypes.arrayOf(PropTypes.object),
};
