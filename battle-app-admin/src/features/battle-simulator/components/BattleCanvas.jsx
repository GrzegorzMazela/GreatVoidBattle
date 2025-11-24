import { useRef, useEffect, useCallback, useState, useImperativeHandle, forwardRef } from 'react';
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
export const BattleCanvas = forwardRef(({
  battleState,
  selectedShip,
  onShipClick,
  onCellClick,
  orders,
  weaponMode,
  playerFractionId,
}, ref) => {
  const canvasRef = useRef(null);
  const [viewport, setViewport] = useState({
    x: 0,
    y: 0,
    zoom: 1,
    cellSize: 20, // Rozmiar komórki w pikselach
  });
  const [isDragging, setIsDragging] = useState(false);
  const [dragStart, setDragStart] = useState({ x: 0, y: 0 });
  const [mouseDownPos, setMouseDownPos] = useState(null); // Pozycja początkowa dla rozróżnienia kliknięcia od przeciągania
  const [loadedImages, setLoadedImages] = useState({});
  const [missileTooltip, setMissileTooltip] = useState(null); // { x, y, missiles: [...] }
  const [shipTooltip, setShipTooltip] = useState(null); // { x, y, ship, fraction }
  const viewportInitialized = useRef(false); // Flaga czy widok był już zainicjalizowany

  const { width, height, fractions } = battleState || {};

  // Eksponuj metodę centerOnShip przez ref
  useImperativeHandle(ref, () => ({
    centerOnShip: (ship) => {
      if (!ship || !canvasRef.current) return;
      
      const canvas = canvasRef.current;
      const canvasWidth = canvas.width;
      const canvasHeight = canvas.height;
      
      if (canvasWidth === 0 || canvasHeight === 0) return;

      const { cellSize, zoom } = viewport;
      const centerX = ship.x - (canvasWidth / (cellSize * zoom)) / 2;
      const centerY = ship.y - (canvasHeight / (cellSize * zoom)) / 2;

      setViewport(prev => ({
        ...prev,
        x: Math.max(0, Math.min(width - 1, centerX)),
        y: Math.max(0, Math.min(height - 1, centerY)),
      }));
    }
  }), [viewport, width, height]);

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

  // Znajdź rakiety w danej pozycji
  const findMissilesAt = useCallback((x, y) => {
    if (!battleState?.missileMovementPaths || battleState.missileMovementPaths.length === 0) {
      return [];
    }

    return battleState.missileMovementPaths.filter(missile => {
      // Sprawdź czy rakieta jest w tej pozycji
      const mx = Math.floor(missile.startPosition.x);
      const my = Math.floor(missile.startPosition.y);
      return mx === x && my === y;
    });
  }, [battleState]);

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
    
    const MISSILE_MIN_RANGE = selectedShip.missileEffectiveRange || 35;
    const MISSILE_MAX_RANGE = selectedShip.missileMaxRange || 55;

    // Rysuj zasięg rakiet w kolorze niebieskim
    for (let x = visibleArea.startX; x <= visibleArea.endX; x++) {
      for (let y = visibleArea.startY; y <= visibleArea.endY; y++) {
        // Backend używa Manhattan distance (Path.Count)
        const distance = Math.abs(x - shipX) + Math.abs(y - shipY);

        if (distance >= MISSILE_MIN_RANGE && distance <= MISSILE_MAX_RANGE) {
          const screenX = (x - viewport.x) * scaledCellSize;
          const screenY = (y - viewport.y) * scaledCellSize;

          // Różne odcienie w zależności od efektywnego zasięgu
          if (distance <= MISSILE_MIN_RANGE) {
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
    
    const LASER_MAX_RANGE = selectedShip.laserMaxRange || 15;

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

    battleState.missileMovementPaths.forEach(missilePath => {
      if (!missilePath.path || missilePath.path.length === 0) return;

      // Sprawdź czy to rakieta gracza lub rakieta wymierzona w statek gracza
      let fractionColor = '#FF9800'; // Domyślny kolor pomarańczowy
      
      if (playerFractionId) {
        let isPlayerMissile = false;
        let isTargetingPlayer = false;

        // Sprawdź czy to rakieta statku gracza (używamy shipId, nie sourceShipId)
        for (const fraction of battleState.fractions) {
          if (fraction.fractionId === playerFractionId) {
            isPlayerMissile = fraction.ships.some(s => s.shipId === missilePath.shipId);
            if (isPlayerMissile) {
              fractionColor = fraction.fractionColor || fractionColor;
              break;
            }
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
        
        // Dla rakiet nie-gracza, znajdź kolor frakcji źródłowej
        if (!isPlayerMissile) {
          for (const fraction of battleState.fractions) {
            const sourceShip = fraction.ships.find(s => s.shipId === missilePath.shipId);
            if (sourceShip) {
              fractionColor = fraction.fractionColor || fractionColor;
              break;
            }
          }
        }
      }

      // Nie rysuj zatwierdzonych trajektorii rakiet jeśli statek źródłowy ma nowy lokalny rozkaz
      // (nowy rozkaz nadpisuje zatwierdzone trajektorie)
      if (orders && orders.some(o => o.shipId === missilePath.shipId)) {
        return;
      }

      const startPos = mapToScreen(missilePath.startPosition.x + 0.5, missilePath.startPosition.y + 0.5);

      // Rysuj linię przez 4 pierwsze elementy ścieżki
      const { cellSize, zoom } = viewport;
      const scaledCellSize = cellSize * zoom;
      
      if (missilePath.path.length > 0) {
        // Rysuj linię z gradientem od rakiety przez 4 następne komórki
        const pathLength = Math.min(4, missilePath.path.length);
        
        ctx.strokeStyle = 'rgba(255, 220, 0, 0.60)';
        ctx.lineWidth = 4;
        ctx.lineCap = 'round';
        ctx.lineJoin = 'round';
        
        ctx.beginPath();
        ctx.moveTo(startPos.x, startPos.y);
        
        let lastPos = null;
        for (let i = 0; i < pathLength; i++) {
          const pos = missilePath.path[i];
          const screenPos = mapToScreen(pos.x + 0.5, pos.y + 0.5);
          
          // Coraz mniejsza intensywność
          const alpha = 0.60 - (i * 0.12); // 0.60, 0.48, 0.36, 0.24
          ctx.strokeStyle = `rgba(255, 220, 0, ${alpha})`;
          ctx.lineTo(screenPos.x, screenPos.y);
          ctx.stroke();
          
          // Kontynuuj od tego punktu
          ctx.beginPath();
          ctx.moveTo(screenPos.x, screenPos.y);
          
          lastPos = screenPos;
        }
        
        // Rysuj strzałkę na końcu
        if (lastPos && pathLength > 0) {
          const prevPos = pathLength > 1 
            ? mapToScreen(missilePath.path[pathLength - 2].x + 0.5, missilePath.path[pathLength - 2].y + 0.5)
            : startPos;
          
          const angle = Math.atan2(lastPos.y - prevPos.y, lastPos.x - prevPos.x);
          const arrowSize = 10;
          
          ctx.fillStyle = `rgba(255, 220, 0, ${0.60 - ((pathLength - 1) * 0.12)})`;
          ctx.beginPath();
          ctx.moveTo(lastPos.x, lastPos.y);
          ctx.lineTo(
            lastPos.x - arrowSize * Math.cos(angle - Math.PI / 6),
            lastPos.y - arrowSize * Math.sin(angle - Math.PI / 6)
          );
          ctx.lineTo(
            lastPos.x - arrowSize * Math.cos(angle + Math.PI / 6),
            lastPos.y - arrowSize * Math.sin(angle + Math.PI / 6)
          );
          ctx.closePath();
          ctx.fill();
        }
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
          
          // Rysuj tło w kolorze frakcji
          ctx.fillStyle = fractionColor;
          ctx.fillRect(-iconSize / 2, -iconSize / 2, iconSize, iconSize);
          
          // Rysuj żółty zarys
          ctx.strokeStyle = 'rgba(255, 220, 0, 1)';
          ctx.lineWidth = 3;
          ctx.strokeRect(-iconSize / 2, -iconSize / 2, iconSize, iconSize);
          
          // Rysuj ikonę rakiety
          ctx.drawImage(missileIcon, -iconSize / 2, -iconSize / 2, iconSize, iconSize);
        } else {
          // Rysuj tło w kolorze frakcji
          ctx.fillStyle = fractionColor;
          ctx.fillRect(startPos.x - iconSize / 2, startPos.y - iconSize / 2, iconSize, iconSize);
          
          // Rysuj żółty zarys
          ctx.strokeStyle = 'rgba(255, 220, 0, 1)';
          ctx.lineWidth = 3;
          ctx.strokeRect(startPos.x - iconSize / 2, startPos.y - iconSize / 2, iconSize, iconSize);
          
          // Rysuj ikonę rakiety
          ctx.drawImage(missileIcon, startPos.x - iconSize / 2, startPos.y - iconSize / 2, iconSize, iconSize);
        }
        ctx.restore();
      } else {
        // Fallback - rysuj trójkąt w kolorze frakcji z żółtym zarysem
        ctx.save();
        ctx.fillStyle = fractionColor;
        ctx.strokeStyle = 'rgba(255, 220, 0, 1)';
        ctx.lineWidth = 3;
        
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
    // Jeśli użytkownik przeciągał, nie wykonuj kliknięcia
    if (isDragging) {
      return;
    }
    
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
  }, [isDragging, screenToMap, findShipAt, onShipClick, onCellClick]);

  // Obsługa prawego przycisku myszy (odznaczenie statku)
  const handleContextMenu = useCallback((e) => {
    e.preventDefault();
    onShipClick?.(null, null, 0, 0); // Odznacz statek
  }, [onShipClick]);

  // Obsługa przeciągania (pan)
  const handleMouseDown = useCallback((e) => {
    if (e.button === 0) { // Lewy przycisk myszy
      setMouseDownPos({ x: e.clientX, y: e.clientY });
      setDragStart({ x: e.clientX, y: e.clientY });
      e.preventDefault();
    }
  }, []);

  const handleMouseMove = useCallback((e) => {
    // Jeśli przycisk jest wciśnięty ale jeszcze nie uznaliśmy tego za przeciąganie
    if (mouseDownPos && !isDragging) {
      const dx = Math.abs(e.clientX - mouseDownPos.x);
      const dy = Math.abs(e.clientY - mouseDownPos.y);
      
      // Jeśli przesunięcie jest większe niż 5 pikseli, uznaj to za przeciąganie
      if (dx > 5 || dy > 5) {
        setIsDragging(true);
        setMissileTooltip(null); // Ukryj tooltip podczas przeciągania
        setShipTooltip(null); // Ukryj tooltip okrętu podczas przeciągania
      }
    }
    
    if (isDragging) {
      const dx = (e.clientX - dragStart.x) / (viewport.cellSize * viewport.zoom);
      const dy = (e.clientY - dragStart.y) / (viewport.cellSize * viewport.zoom);

      setViewport(prev => ({
        ...prev,
        x: Math.max(0, Math.min(width - 1, prev.x - dx)),
        y: Math.max(0, Math.min(height - 1, prev.y - dy)),
      }));

      setDragStart({ x: e.clientX, y: e.clientY });
    } else {
      // Jeśli nie przeciągamy, sprawdź czy mysz jest nad rakietą lub okrętem
      const mapPos = screenToMap(e.clientX, e.clientY);
      const missiles = findMissilesAt(mapPos.x, mapPos.y);
      
      if (missiles.length > 0) {
        setMissileTooltip({
          x: e.clientX,
          y: e.clientY,
          missiles: missiles
        });
        setShipTooltip(null);
      } else {
        setMissileTooltip(null);
        
        // Sprawdź czy mysz jest nad okrętem wroga
        let hoveredShip = null;
        let hoveredFraction = null;
        
        if (fractions) {
          for (const fraction of fractions) {
            // Pomijamy okręty gracza
            if (playerFractionId && fraction.fractionId === playerFractionId) continue;
            
            for (const ship of fraction.ships) {
              const shipX = Math.floor(ship.x);
              const shipY = Math.floor(ship.y);
              
              if (Math.floor(mapPos.x) === shipX && Math.floor(mapPos.y) === shipY) {
                hoveredShip = ship;
                hoveredFraction = fraction;
                break;
              }
            }
            if (hoveredShip) break;
          }
        }
        
        if (hoveredShip) {
          setShipTooltip({
            x: e.clientX,
            y: e.clientY,
            ship: hoveredShip,
            fraction: hoveredFraction
          });
        } else {
          setShipTooltip(null);
        }
      }
    }
  }, [isDragging, mouseDownPos, dragStart, viewport, width, height, screenToMap, findMissilesAt, fractions, playerFractionId]);

  const handleMouseUp = useCallback(() => {
    setIsDragging(false);
    setMouseDownPos(null);
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
        onMouseLeave={() => {
          handleMouseUp();
          setMissileTooltip(null);
          setShipTooltip(null);
        }}
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
      
      {/* Tooltip dla rakiet */}
      {missileTooltip && (
        <div 
          className="missile-tooltip"
          style={{
            position: 'absolute',
            left: missileTooltip.x + 15,
            top: missileTooltip.y - 120,
            pointerEvents: 'none',
            zIndex: 1000,
          }}
        >
          <div className="missile-tooltip-header">
            Rakiety ({missileTooltip.missiles.length})
          </div>
          {(() => {
            // Grupuj rakiety według źródła i celu
            const grouped = {};
            
            missileTooltip.missiles.forEach(missile => {
              const sourceName = missile.shipName || 'Nieznany';
              
              // Znajdź nazwę statku źródłowego i jego frakcję
              let sourceFractionName = '';
              let sourceFractionColor = '#fff';
              if (battleState?.fractions) {
                for (const fraction of battleState.fractions) {
                  const sourceShip = fraction.ships.find(s => s.shipId === missile.shipId);
                  if (sourceShip) {
                    sourceFractionName = fraction.fractionName || '';
                    sourceFractionColor = fraction.fractionColor || '#fff';
                    break;
                  }
                }
              }
              
              // Znajdź nazwę statku docelowego i jego frakcję
              let targetName = 'Nieznany';
              let targetFractionName = '';
              let targetFractionColor = '#fff';
              if (battleState?.fractions) {
                for (const fraction of battleState.fractions) {
                  const targetShip = fraction.ships.find(s => s.shipId === missile.targetId);
                  if (targetShip) {
                    targetName = targetShip.name;
                    targetFractionName = fraction.fractionName || '';
                    targetFractionColor = fraction.fractionColor || '#fff';
                    break;
                  }
                }
              }
              
              // Liczba tur do dolecenia
              const turnsToImpact = missile.path ? Math.ceil(missile.path.length / (missile.speed || 10)) : '?';
              
              // Klucz grupowania: źródło + cel + tury
              const key = `${sourceName}->${targetName}`;
              
              if (!grouped[key]) {
                grouped[key] = {
                  sourceName,
                  sourceFractionName,
                  sourceFractionColor,
                  targetName,
                  targetFractionName,
                  targetFractionColor,
                  turnsToImpact,
                  count: 0
                };
              }
              grouped[key].count++;
            });
            
            // Renderuj zgrupowane rakiety
            return Object.values(grouped).map((group, index) => (
              <div key={index} className="missile-tooltip-item">
                {group.count > 1 && (
                  <div className="missile-info">
                    <span className="missile-label">✕</span>
                    <span className="missile-value">{group.count}</span>
                  </div>
                )}
                <div className="missile-info">
                  <span className="missile-label">➤ Od:</span>
                  <span className="missile-value">
                    {group.sourceName}
                    {group.sourceFractionName && (
                      <span className="missile-fraction" style={{ color: group.sourceFractionColor }}>
                        {' '}({group.sourceFractionName})
                      </span>
                    )}
                  </span>
                </div>
                <div className="missile-info">
                  <span className="missile-label">◉ Do:</span>
                  <span className="missile-value">
                    {group.targetName}
                    {group.targetFractionName && (
                      <span className="missile-fraction" style={{ color: group.targetFractionColor }}>
                        {' '}({group.targetFractionName})
                      </span>
                    )}
                  </span>
                </div>
                <div className="missile-info">
                  <span className="missile-label">⏱ Tur:</span>
                  <span className="missile-value">{group.turnsToImpact}</span>
                </div>
              </div>
            ));
          })()}
        </div>
      )}
      
      {/* Tooltip dla okrętów wroga */}
      {shipTooltip && ((() => {
        const maxHp = SHIP_MAX_HP[shipTooltip.ship.type] || 100;
        const hpPercent = Math.round((shipTooltip.ship.hitPoints / maxHp) * 100);
        const damagePercent = 100 - hpPercent;
        
        return (
          <div 
            className="ship-tooltip"
            style={{
              position: 'absolute',
              left: shipTooltip.x + 15,
              top: shipTooltip.y - 80,
              pointerEvents: 'none',
              zIndex: 1000,
            }}
          >
            <div className="ship-tooltip-name" style={{ color: shipTooltip.fraction.fractionColor }}>
              {shipTooltip.ship.name}
            </div>
            <div className="ship-tooltip-damage">
              Uszkodzenia: {damagePercent}%
            </div>
          </div>
        );
      })())}
    </div>
  );
});

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
