import { useRef, useEffect, useCallback, useState } from 'react';
import PropTypes from 'prop-types';
import './BattleCanvas.css';

// Import ship icons
import CorvetteIcon from '../../../assets/Corvette_64.png';
import DestroyerIcon from '../../../assets/Destroyer_64.png';
import CruiserIcon from '../../../assets/Cruiser_64.png';
import BattleshipIcon from '../../../assets/Battleship_64.png';
import SuperBattleshipIcon from '../../../assets/SuperBattleship_64.png';
import OrbitalFortIcon from '../../../assets/OrbitalFort_64.png';
import MissileIcon from '../../../assets/Missile_64.png';

// Map ship types to icons
const SHIP_ICONS = {
  Corvette: CorvetteIcon,
  Destroyer: DestroyerIcon,
  Cruiser: CruiserIcon,
  Battleship: BattleshipIcon,
  SuperBattleship: SuperBattleshipIcon,
  OrbitalFort: OrbitalFortIcon,
};

// Map ship types to max HP (from ShipFactory.cs)
const SHIP_MAX_HP = {
  Corvette: 50,
  Destroyer: 100,
  Cruiser: 200,
  Battleship: 400,
  SuperBattleship: 600,
  OrbitalFort: 100,
};

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
  weaponMode,
  playerFractionId,
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
  const [loadedImages, setLoadedImages] = useState({});
  const viewportInitialized = useRef(false); // Flaga czy widok był już zainicjalizowany

  const { width, height, fractions } = battleState || {};

  // Load ship icons
  useEffect(() => {
    const images = {};
    const loadPromises = [];

    // Load ship icons
    Object.entries(SHIP_ICONS).forEach(([type, src]) => {
      const img = new Image();
      const promise = new Promise((resolve) => {
        img.onload = () => {
          images[type] = img;
          resolve();
        };
        img.onerror = () => {
          console.error(`Failed to load ship icon: ${type}`);
          resolve();
        };
      });
      img.src = src;
      loadPromises.push(promise);
    });

    // Load missile icon
    const missileImg = new Image();
    const missilePromise = new Promise((resolve) => {
      missileImg.onload = () => {
        images.Missile = missileImg;
        resolve();
      };
      missileImg.onerror = () => {
        console.error('Failed to load missile icon');
        resolve();
      };
    });
    missileImg.src = MissileIcon;
    loadPromises.push(missilePromise);

    Promise.all(loadPromises).then(() => {
      setLoadedImages(images);
    });
  }, []);

  // Wycentruj widok na pierwszym statku gracza przy inicjalizacji
  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas || !battleState || !playerFractionId || viewportInitialized.current) {
      return;
    }

    // Znajdź frakcję gracza
    const playerFraction = battleState.fractions?.find(f => f.fractionId === playerFractionId);
    if (!playerFraction || !playerFraction.ships || playerFraction.ships.length === 0) {
      return;
    }

    // Pobierz pierwszy statek gracza
    const firstShip = playerFraction.ships[0];
    if (!firstShip) return;

    // Wycentruj widok na statku - czekamy aż canvas będzie miał rozmiar
    setTimeout(() => {
      const canvasWidth = canvas.width;
      const canvasHeight = canvas.height;
      
      if (canvasWidth === 0 || canvasHeight === 0) return;

      const { cellSize, zoom } = viewport;

      const centerX = firstShip.x - (canvasWidth / (cellSize * zoom)) / 2;
      const centerY = firstShip.y - (canvasHeight / (cellSize * zoom)) / 2;

      setViewport(prev => ({
        ...prev,
        x: Math.max(0, Math.min(battleState.width - 1, centerX)),
        y: Math.max(0, Math.min(battleState.height - 1, centerY)),
      }));

      viewportInitialized.current = true;
      console.log(`Widok wycentrowany na pierwszym statku gracza: ${firstShip.name || firstShip.shipId} (${firstShip.x}, ${firstShip.y})`);
    }, 100);
  }, [battleState, playerFractionId, viewport.cellSize, viewport.zoom]);

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

  // Renderuj zasięg rakiet wybranego statku
  const drawMissileRange = useCallback((ctx, visibleArea) => {
    if (!selectedShip || weaponMode !== 'missile') return;

    const { cellSize, zoom } = viewport;
    const scaledCellSize = cellSize * zoom;
    const shipX = Math.floor(selectedShip.x);
    const shipY = Math.floor(selectedShip.y);
    
    const MISSILE_MIN_RANGE = 35;
    const MISSILE_MAX_RANGE = 55;

    // Rysuj zasięg rakiet w kolorze niebieskim
    for (let x = visibleArea.startX; x <= visibleArea.endX; x++) {
      for (let y = visibleArea.startY; y <= visibleArea.endY; y++) {
        // Backend używa Manhattan distance (Path.Count)
        const distance = Math.abs(x - shipX) + Math.abs(y - shipY);

        if (distance >= MISSILE_MIN_RANGE && distance <= MISSILE_MAX_RANGE) {
          const screenX = (x - viewport.x) * scaledCellSize;
          const screenY = (y - viewport.y) * scaledCellSize;

          // Różne odcienie w zależności od efektywnego zasięgu
          if (distance <= 35) {
            // Efektywny zasięg - intensywniejszy niebieski
            ctx.fillStyle = 'rgba(50, 150, 255, 0.25)';
          } else {
            // Daleki zasięg - słabszy niebieski
            ctx.fillStyle = 'rgba(50, 150, 255, 0.12)';
          }
          
          ctx.fillRect(screenX, screenY, scaledCellSize, scaledCellSize);
          
          // Subtelne obramowanie
          ctx.strokeStyle = 'rgba(50, 150, 255, 0.3)';
          ctx.lineWidth = 0.5;
          ctx.strokeRect(screenX, screenY, scaledCellSize, scaledCellSize);
        }
      }
    }

    // Rysuj romby zasięgu (Manhattan distance)
    const centerPos = mapToScreen(shipX + 0.5, shipY + 0.5);
    
    // Romb minimalnego zasięgu
    ctx.strokeStyle = 'rgba(50, 150, 255, 0.5)';
    ctx.lineWidth = 2;
    ctx.setLineDash([5, 5]);
    ctx.beginPath();
    ctx.moveTo(centerPos.x, centerPos.y - MISSILE_MIN_RANGE * scaledCellSize);
    ctx.lineTo(centerPos.x + MISSILE_MIN_RANGE * scaledCellSize, centerPos.y);
    ctx.lineTo(centerPos.x, centerPos.y + MISSILE_MIN_RANGE * scaledCellSize);
    ctx.lineTo(centerPos.x - MISSILE_MIN_RANGE * scaledCellSize, centerPos.y);
    ctx.closePath();
    ctx.stroke();
    
    // Romb maksymalnego zasięgu
    ctx.strokeStyle = 'rgba(50, 150, 255, 0.4)';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.moveTo(centerPos.x, centerPos.y - MISSILE_MAX_RANGE * scaledCellSize);
    ctx.lineTo(centerPos.x + MISSILE_MAX_RANGE * scaledCellSize, centerPos.y);
    ctx.lineTo(centerPos.x, centerPos.y + MISSILE_MAX_RANGE * scaledCellSize);
    ctx.lineTo(centerPos.x - MISSILE_MAX_RANGE * scaledCellSize, centerPos.y);
    ctx.closePath();
    ctx.stroke();
    
    ctx.setLineDash([]);
  }, [selectedShip, viewport, weaponMode, mapToScreen]);

  // Renderuj zasięg laserów wybranego statku
  const drawLaserRange = useCallback((ctx, visibleArea) => {
    if (!selectedShip || weaponMode !== 'laser') return;

    const { cellSize, zoom } = viewport;
    const scaledCellSize = cellSize * zoom;
    const shipX = selectedShip.x;
    const shipY = selectedShip.y;
    
    const LASER_MAX_RANGE = 20;

    // Rysuj zasięg laserów w kolorze czerwonym
    for (let x = visibleArea.startX; x <= visibleArea.endX; x++) {
      for (let y = visibleArea.startY; y <= visibleArea.endY; y++) {
        const distance = Math.sqrt(
          Math.pow(x - shipX, 2) + Math.pow(y - shipY, 2)
        );

        if (distance <= LASER_MAX_RANGE) {
          const screenX = (x - viewport.x) * scaledCellSize;
          const screenY = (y - viewport.y) * scaledCellSize;

          ctx.fillStyle = 'rgba(255, 50, 50, 0.2)';
          ctx.fillRect(screenX, screenY, scaledCellSize, scaledCellSize);
          
          // Subtelne obramowanie
          ctx.strokeStyle = 'rgba(255, 50, 50, 0.3)';
          ctx.lineWidth = 0.5;
          ctx.strokeRect(screenX, screenY, scaledCellSize, scaledCellSize);
        }
      }
    }

    // Rysuj okrąg maksymalnego zasięgu
    const centerPos = mapToScreen(shipX, shipY);
    
    ctx.strokeStyle = 'rgba(255, 50, 50, 0.6)';
    ctx.lineWidth = 2;
    ctx.setLineDash([3, 3]);
    ctx.beginPath();
    ctx.arc(centerPos.x, centerPos.y, LASER_MAX_RANGE * scaledCellSize, 0, Math.PI * 2);
    ctx.stroke();
    
    ctx.setLineDash([]);
  }, [selectedShip, viewport, weaponMode, mapToScreen]);

  // Renderuj statki
  const drawShips = useCallback((ctx, visibleArea) => {
    if (!fractions) return;

    const { cellSize, zoom } = viewport;
    const scaledCellSize = cellSize * zoom;

    fractions.forEach((fraction, fractionIndex) => {
      // Użyj koloru przypisanego do frakcji, fallback do domyślnych kolorów
      const defaultColors = ['#4CAF50', '#F44336', '#2196F3', '#FF9800', '#9C27B0'];
      const color = fraction.fractionColor || defaultColors[fractionIndex % defaultColors.length];
      const isPlayerFraction = playerFractionId && fraction.fractionId === playerFractionId;

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

        // Rysuj ikonę statku jeśli dostępna
        const shipIcon = loadedImages[ship.type];
        if (shipIcon) {
          const iconSize = scaledCellSize * 0.9;
          
          // Rysuj kolorowe tło dla frakcji
          ctx.fillStyle = color;
          ctx.fillRect(
            screenPos.x - iconSize / 2,
            screenPos.y - iconSize / 2,
            iconSize,
            iconSize
          );
          
          // Rysuj ikonę statku na kolorowym tle
          ctx.drawImage(
            shipIcon,
            screenPos.x - iconSize / 2,
            screenPos.y - iconSize / 2,
            iconSize,
            iconSize
          );
        } else {
          // Fallback - rysuj statek jako koło
          const shipRadius = scaledCellSize * 0.4;
          ctx.fillStyle = color;
          ctx.beginPath();
          ctx.arc(screenPos.x, screenPos.y, shipRadius, 0, Math.PI * 2);
          ctx.fill();

          // Obramowanie
          ctx.strokeStyle = '#000';
          ctx.lineWidth = 2;
          ctx.stroke();
        }

        // HP bar - tylko dla statków gracza
        if (isPlayerFraction) {
          const maxHp = SHIP_MAX_HP[ship.type] || 100;
          const hpPercent = ship.hitPoints / maxHp;
          const barWidth = scaledCellSize * 0.8;
          const barHeight = 4;
          const barX = (shipX - viewport.x) * scaledCellSize + (scaledCellSize - barWidth) / 2;
          const barY = (shipY - viewport.y + 0.9) * scaledCellSize;

          ctx.fillStyle = '#333';
          ctx.fillRect(barX, barY, barWidth, barHeight);
          ctx.fillStyle = hpPercent > 0.5 ? '#4CAF50' : hpPercent > 0.25 ? '#FF9800' : '#F44336';
          ctx.fillRect(barX, barY, barWidth * hpPercent, barHeight);
        }
      });
    });
  }, [fractions, viewport, selectedShip, mapToScreen, loadedImages, playerFractionId]);

  // Renderuj rozkazy
  const drawOrders = useCallback((ctx) => {
    if (!orders || orders.length === 0 || !fractions) return;

    orders.forEach(order => {
      // Znajdź statek który wydał rozkaz
      let ship = null;
      let shipFraction = null;
      for (const fraction of fractions) {
        ship = fraction.ships.find(s => s.shipId === order.shipId);
        if (ship) {
          shipFraction = fraction;
          break;
        }
      }

      if (!ship) return;

      // Pokazuj tylko rozkazy gracza (ruchy i ataki)
      const isPlayerOrder = playerFractionId && shipFraction.fractionId === playerFractionId;
      if (!isPlayerOrder) return;

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
  }, [orders, fractions, mapToScreen, playerFractionId]);

  // Renderuj zatwierdzone ścieżki ruchu statków
  const drawMovementPaths = useCallback((ctx) => {
    if (!battleState?.shipMovementPaths || battleState.shipMovementPaths.length === 0) return;

    const { cellSize, zoom } = viewport;

    battleState.shipMovementPaths.forEach(movementPath => {
      if (!movementPath.path || movementPath.path.length === 0) return;

      // Sprawdź czy to ścieżka statku gracza
      if (playerFractionId) {
        let isPlayerShip = false;
        for (const fraction of battleState.fractions) {
          if (fraction.fractionId === playerFractionId) {
            isPlayerShip = fraction.ships.some(s => s.shipId === movementPath.shipId);
            if (isPlayerShip) break;
          }
        }
        if (!isPlayerShip) return;
      }

      // Nie rysuj zatwierdzonych ścieżek jeśli statek ma nowy lokalny rozkaz
      // (nowy rozkaz nadpisuje zatwierdzoną ścieżkę)
      if (orders && orders.some(o => o.shipId === movementPath.shipId)) {
        return;
      }

      // Rysuj ścieżkę jako połączone segmenty
      ctx.strokeStyle = 'rgba(100, 255, 100, 0.7)';
      ctx.lineWidth = 4;
      ctx.setLineDash([]);

      // Podświetl komórki przez które przechodzi ścieżka
      ctx.fillStyle = 'rgba(100, 255, 100, 0.2)';
      movementPath.path.forEach(pos => {
        const screenX = (Math.floor(pos.x) - viewport.x) * cellSize * zoom;
        const screenY = (Math.floor(pos.y) - viewport.y) * cellSize * zoom;
        ctx.fillRect(screenX, screenY, cellSize * zoom, cellSize * zoom);
      });

      // Rysuj linię ścieżki
      ctx.beginPath();
      const startPos = mapToScreen(movementPath.startPosition.x + 0.5, movementPath.startPosition.y + 0.5);
      ctx.moveTo(startPos.x, startPos.y);

      movementPath.path.forEach(pos => {
        const screenPos = mapToScreen(pos.x + 0.5, pos.y + 0.5);
        ctx.lineTo(screenPos.x, screenPos.y);
      });

      ctx.stroke();

      // Rysuj strzałkę na końcu
      if (movementPath.path.length > 0) {
        const lastPos = movementPath.path[movementPath.path.length - 1];
        const secondLastPos = movementPath.path.length > 1 
          ? movementPath.path[movementPath.path.length - 2] 
          : movementPath.startPosition;
        
        const endScreenPos = mapToScreen(lastPos.x + 0.5, lastPos.y + 0.5);
        const prevScreenPos = mapToScreen(secondLastPos.x + 0.5, secondLastPos.y + 0.5);
        
        const angle = Math.atan2(endScreenPos.y - prevScreenPos.y, endScreenPos.x - prevScreenPos.x);
        const arrowSize = 12;
        
        ctx.fillStyle = 'rgba(100, 255, 100, 0.7)';
        ctx.beginPath();
        ctx.moveTo(endScreenPos.x, endScreenPos.y);
        ctx.lineTo(
          endScreenPos.x - arrowSize * Math.cos(angle - Math.PI / 6),
          endScreenPos.y - arrowSize * Math.sin(angle - Math.PI / 6)
        );
        ctx.lineTo(
          endScreenPos.x - arrowSize * Math.cos(angle + Math.PI / 6),
          endScreenPos.y - arrowSize * Math.sin(angle + Math.PI / 6)
        );
        ctx.closePath();
        ctx.fill();
      }
    });
  }, [battleState, viewport, mapToScreen, playerFractionId, orders]);

  // Renderuj zatwierdzone ścieżki pocisków
  const drawMissilePaths = useCallback((ctx) => {
    if (!battleState?.missileMovementPaths || battleState.missileMovementPaths.length === 0) return;

    const { cellSize, zoom } = viewport;

    battleState.missileMovementPaths.forEach(missilePath => {
      if (!missilePath.path || missilePath.path.length === 0) return;

      // Sprawdź czy to rakieta gracza lub rakieta wymierzona w statek gracza
      if (playerFractionId) {
        let isPlayerMissile = false;
        let isTargetingPlayer = false;

        // Sprawdź czy to rakieta statku gracza (używamy shipId, nie sourceShipId)
        for (const fraction of battleState.fractions) {
          if (fraction.fractionId === playerFractionId) {
            isPlayerMissile = fraction.ships.some(s => s.shipId === missilePath.shipId);
            if (isPlayerMissile) break;
          }
        }

        // Sprawdź czy cel to statek gracza (używamy targetId, nie targetShipId)
        if (!isPlayerMissile) {
          for (const fraction of battleState.fractions) {
            if (fraction.fractionId === playerFractionId) {
              isTargetingPlayer = fraction.ships.some(s => s.shipId === missilePath.targetId);
              if (isTargetingPlayer) break;
            }
          }
        }

        // Pokazuj tylko rakiety gracza lub rakiety wymierzone w gracza
        if (!isPlayerMissile && !isTargetingPlayer) return;
      }

      // Nie rysuj zatwierdzonych trajektorii rakiet jeśli statek źródłowy ma nowy lokalny rozkaz
      // (nowy rozkaz nadpisuje zatwierdzone trajektorie)
      if (orders && orders.some(o => o.shipId === missilePath.shipId)) {
        return;
      }

      // Rysuj całą planowaną ścieżkę jako półprzezroczystą linię
      ctx.strokeStyle = 'rgba(255, 150, 0, 0.3)';
      ctx.lineWidth = 2;
      ctx.setLineDash([5, 3]);

      // Podświetl komórki przez które przechodzi pocisk (całą przyszłą ścieżkę)
      ctx.fillStyle = 'rgba(255, 150, 0, 0.1)';
      missilePath.path.forEach(pos => {
        const screenX = (Math.floor(pos.x) - viewport.x) * cellSize * zoom;
        const screenY = (Math.floor(pos.y) - viewport.y) * cellSize * zoom;
        ctx.fillRect(screenX, screenY, cellSize * zoom, cellSize * zoom);
      });

      // Rysuj linię całej trajektorii
      ctx.beginPath();
      const startPos = mapToScreen(missilePath.startPosition.x + 0.5, missilePath.startPosition.y + 0.5);
      ctx.moveTo(startPos.x, startPos.y);

      missilePath.path.forEach(pos => {
        const screenPos = mapToScreen(pos.x + 0.5, pos.y + 0.5);
        ctx.lineTo(screenPos.x, screenPos.y);
      });

      ctx.stroke();
      ctx.setLineDash([]);

      // Podświetl komórkę, w której znajduje się rakieta (aktualną pozycję)
      const currentCellScreenX = (Math.floor(missilePath.startPosition.x) - viewport.x) * cellSize * zoom;
      const currentCellScreenY = (Math.floor(missilePath.startPosition.y) - viewport.y) * cellSize * zoom;
      
      // Pulsujące podświetlenie komórki z rakietą
      ctx.fillStyle = 'rgba(255, 200, 50, 0.4)';
      ctx.fillRect(currentCellScreenX, currentCellScreenY, cellSize * zoom, cellSize * zoom);
      
      // Dodatkowa ramka wokół komórki z rakietą
      ctx.strokeStyle = 'rgba(255, 220, 100, 0.8)';
      ctx.lineWidth = 3;
      ctx.setLineDash([]);
      ctx.strokeRect(currentCellScreenX, currentCellScreenY, cellSize * zoom, cellSize * zoom);

      // Rysuj ślad rakiety (trail) - ostatnie kilka pozycji
      const trailLength = Math.min(5, missilePath.speed || 2); // Długość śladu zależna od prędkości
      ctx.strokeStyle = 'rgba(255, 200, 50, 0.9)';
      ctx.lineWidth = 4;
      ctx.lineCap = 'round';
      ctx.lineJoin = 'round';

      // Gradient śladu - od aktualnej pozycji do przeszłości
      ctx.beginPath();
      ctx.moveTo(startPos.x, startPos.y);
      
      // Rysuj ślad z malejącą intensywnością
      for (let i = 0; i < trailLength && i < missilePath.path.length; i++) {
        const pos = missilePath.path[i];
        const screenPos = mapToScreen(pos.x + 0.5, pos.y + 0.5);
        const alpha = 1 - (i / trailLength) * 0.7; // Od 1.0 do 0.3
        ctx.strokeStyle = `rgba(255, 200, 50, ${alpha})`;
        ctx.lineTo(screenPos.x, screenPos.y);
        ctx.stroke();
        ctx.beginPath();
        ctx.moveTo(screenPos.x, screenPos.y);
      }

      // Rysuj jaskrawą linię pokazującą bezpośredni ruch rakiety
      if (missilePath.path.length > 0) {
        const nextPos = missilePath.path[0];
        const nextScreenPos = mapToScreen(nextPos.x + 0.5, nextPos.y + 0.5);
        
        // Główna linia śladu
        ctx.strokeStyle = 'rgba(255, 220, 100, 1)';
        ctx.lineWidth = 5;
        ctx.beginPath();
        ctx.moveTo(startPos.x, startPos.y);
        ctx.lineTo(nextScreenPos.x, nextScreenPos.y);
        ctx.stroke();

        // Dodatkowy efekt świecenia
        ctx.strokeStyle = 'rgba(255, 255, 200, 0.6)';
        ctx.lineWidth = 3;
        ctx.beginPath();
        ctx.moveTo(startPos.x, startPos.y);
        ctx.lineTo(nextScreenPos.x, nextScreenPos.y);
        ctx.stroke();
      }

      // Rysuj ikonę rakiety na aktualnej pozycji
      const missileIcon = loadedImages.Missile;
      if (missileIcon) {
        const { cellSize, zoom } = viewport;
        const scaledCellSize = cellSize * zoom;
        const iconSize = scaledCellSize * 0.7;
        
        ctx.save();
        // Obróć ikonę w kierunku ruchu (jeśli ścieżka ma punkty)
        if (missilePath.path.length > 0) {
          const firstPos = missilePath.path[0];
          const angle = Math.atan2(
            firstPos.y - missilePath.startPosition.y,
            firstPos.x - missilePath.startPosition.x
          );
          ctx.translate(startPos.x, startPos.y);
          ctx.rotate(angle);
          
          // Dodaj efekt świecenia wokół rakiety
          ctx.shadowColor = 'rgba(255, 200, 50, 0.8)';
          ctx.shadowBlur = 15;
          ctx.drawImage(missileIcon, -iconSize / 2, -iconSize / 2, iconSize, iconSize);
          ctx.shadowBlur = 0;
        } else {
          ctx.shadowColor = 'rgba(255, 200, 50, 0.8)';
          ctx.shadowBlur = 15;
          ctx.drawImage(missileIcon, startPos.x - iconSize / 2, startPos.y - iconSize / 2, iconSize, iconSize);
          ctx.shadowBlur = 0;
        }
        ctx.restore();
      } else {
        // Fallback - rysuj trójkąt ze świeceniem
        ctx.save();
        ctx.shadowColor = 'rgba(255, 200, 50, 0.8)';
        ctx.shadowBlur = 10;
        ctx.fillStyle = 'rgba(255, 180, 0, 1)';
        ctx.strokeStyle = 'rgba(255, 255, 200, 0.9)';
        ctx.lineWidth = 2;
        
        const missileSize = 8;
        ctx.beginPath();
        
        // Obróć trójkąt w kierunku ruchu
        if (missilePath.path.length > 0) {
          const firstPos = missilePath.path[0];
          const angle = Math.atan2(
            firstPos.y - missilePath.startPosition.y,
            firstPos.x - missilePath.startPosition.x
          );
          
          ctx.translate(startPos.x, startPos.y);
          ctx.rotate(angle - Math.PI / 2); // -90 stopni bo trójkąt domyślnie skierowany w górę
          
          ctx.moveTo(0, -missileSize);
          ctx.lineTo(missileSize, missileSize);
          ctx.lineTo(-missileSize, missileSize);
        } else {
          ctx.moveTo(startPos.x, startPos.y - missileSize);
          ctx.lineTo(startPos.x + missileSize, startPos.y + missileSize);
          ctx.lineTo(startPos.x - missileSize, startPos.y + missileSize);
        }
        
        ctx.closePath();
        ctx.fill();
        ctx.stroke();
        ctx.restore();
      }
    });
  }, [battleState, viewport, mapToScreen, loadedImages, playerFractionId, orders]);

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
    drawMissileRange(ctx, visibleArea);
    drawLaserRange(ctx, visibleArea);
    drawMovementPaths(ctx); // Zatwierdzone ścieżki ruchu statków
    drawMissilePaths(ctx); // Zatwierdzone ścieżki pocisków
    drawShips(ctx, visibleArea);
    drawOrders(ctx); // Planowane rozkazy (jeszcze niezatwierdzone)

  }, [battleState, viewport, width, height, drawGrid, drawMoveRange, drawMissileRange, drawLaserRange, drawMovementPaths, drawMissilePaths, drawShips, drawOrders]);

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
    // Ignoruj prawy przycisk - obsługiwany przez contextmenu
    if (e.button !== 0) return;
    
    const mapPos = screenToMap(e.clientX, e.clientY);
    
    // Sprawdź czy kliknięto w statek
    const shipData = findShipAt(mapPos.x, mapPos.y);
    
    // Najpierw wywołaj onCellClick (obsługuje logikę ataku)
    // Jeśli to zatrzyma propagację, onShipClick nie zostanie wywołany
    if (onCellClick) {
      const result = onCellClick(mapPos.x, mapPos.y);
      // Jeśli onCellClick zwrócił false, zatrzymaj dalsze przetwarzanie
      if (result === false) {
        return;
      }
    }
    
    // Jeśli był statek i nie zatrzymano propagacji, wywołaj onShipClick
    if (shipData && onShipClick) {
      onShipClick(shipData.ship, shipData.fraction, e.clientX, e.clientY);
    }
  }, [screenToMap, findShipAt, onShipClick, onCellClick]);

  // Obsługa prawego przycisku myszy (odznaczenie statku)
  const handleContextMenu = useCallback((e) => {
    e.preventDefault();
    onShipClick?.(null, null, 0, 0); // Odznacz statek
  }, [onShipClick]);

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

  // Zoom In
  const handleZoomIn = useCallback(() => {
    setViewport(prev => ({
      ...prev,
      zoom: Math.min(prev.zoom + 0.2, 3),
    }));
  }, []);

  // Zoom Out
  const handleZoomOut = useCallback(() => {
    setViewport(prev => ({
      ...prev,
      zoom: Math.max(prev.zoom - 0.2, 0.2),
    }));
  }, []);

  return (
    <div style={{ position: 'relative', width: '100%', height: '100%' }}>
      <canvas
        ref={canvasRef}
        onClick={handleClick}
        onContextMenu={handleContextMenu}
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
      {/* Przyciski zoom */}
      <div className="zoom-controls">
        <button className="zoom-btn" onClick={handleZoomIn} title="Przybliż (+)">
          +
        </button>
        <button className="zoom-btn" onClick={handleZoomOut} title="Oddal (-)">
          −
        </button>
      </div>
    </div>
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
  weaponMode: PropTypes.oneOf(['missile', 'laser', null]),
  playerFractionId: PropTypes.string,
};
