import { useState, useRef, useEffect, useCallback, useMemo } from 'react';
import { Box, HStack, VStack, Text, Badge, Input, IconButton } from '@chakra-ui/react';
import { LuZoomIn, LuZoomOut, LuHouse, LuSearch, LuX } from 'react-icons/lu';
import { STAR_SYSTEMS, FACTION_COLORS, EXPLORATION_STATUS } from '../data/starSystemsData';
import './StarMap.css';

const MAP_WIDTH = 7000;
const MAP_HEIGHT = 7000;

// Pre-generowane gwiazdy tła (stałe pozycje)
const BACKGROUND_STARS = Array.from({ length: 200 }, (_, i) => ({
  id: i,
  x: (i * 7919 + 1234) % MAP_WIDTH,
  y: (i * 6271 + 5678) % MAP_HEIGHT,
  r: (i % 3) * 0.5 + 0.5,
  opacity: (i % 5) * 0.1 + 0.15
}));

/**
 * Interaktywna mapa gwiezdna z możliwością zoom i pan
 */
export const StarMap = ({ 
  explorationData = {}, 
  onSystemSelect, 
  selectedSystem,
  viewMode = 'admin',
  fractionId = null,
  highlightedSystems = [],
  filterFaction = null,
}) => {
  const containerRef = useRef(null);
  const [viewBox, setViewBox] = useState({ x: 0, y: 0, width: MAP_WIDTH, height: MAP_HEIGHT });
  const [isPanning, setIsPanning] = useState(false);
  const panStartRef = useRef({ x: 0, y: 0, vx: 0, vy: 0 });
  const [searchQuery, setSearchQuery] = useState('');
  const isDraggingRef = useRef(false);

  // Sprawdź czy układ jest znany dla frakcji
  const isSystemKnown = useCallback((system) => {
    if (viewMode !== 'fraction' || !fractionId) return true;
    
    const status = explorationData[system.id]?.status;
    return system.knownToAll || 
           system.faction === fractionId || 
           (system.knownTo && system.knownTo.includes(fractionId)) ||
           status === EXPLORATION_STATUS.DISCOVERED ||
           status === EXPLORATION_STATUS.EXPLORED;
  }, [viewMode, fractionId, explorationData]);

  // Filtrowanie układów - dla admina filtruj, dla frakcji pokaż wszystkie
  const filteredSystems = useMemo(() => {
    let systems = [...STAR_SYSTEMS];

    // Dla widoku admin - filtr frakcji
    if (viewMode === 'admin') {
      if (filterFaction && filterFaction !== 'all') {
        systems = systems.filter(s => s.faction === filterFaction);
      }
    }

    // Filtr wyszukiwania (tylko dla znanych układów w widoku frakcji)
    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      systems = systems.filter(s => {
        if (viewMode === 'fraction' && !isSystemKnown(s)) return false;
        return s.name.toLowerCase().includes(query);
      });
    }

    return systems;
  }, [filterFaction, searchQuery, viewMode, isSystemKnown]);

  // Zoom
  const handleZoom = useCallback((delta) => {
    setViewBox(prev => {
      const zoomFactor = delta > 0 ? 0.8 : 1.25;
      const newWidth = Math.max(500, Math.min(MAP_WIDTH, prev.width * zoomFactor));
      const newHeight = Math.max(500, Math.min(MAP_HEIGHT, prev.height * zoomFactor));
      
      const cx = prev.x + prev.width / 2;
      const cy = prev.y + prev.height / 2;
      
      const newX = cx - newWidth / 2;
      const newY = cy - newHeight / 2;

      return {
        x: Math.max(0, Math.min(MAP_WIDTH - newWidth, newX)),
        y: Math.max(0, Math.min(MAP_HEIGHT - newHeight, newY)),
        width: newWidth,
        height: newHeight
      };
    });
  }, []);

  // Reset widoku
  const resetView = useCallback(() => {
    setViewBox({ x: 0, y: 0, width: MAP_WIDTH, height: MAP_HEIGHT });
  }, []);

  // Centruj na układzie
  const centerOnSystem = useCallback((system) => {
    const padding = 1000;
    setViewBox({
      x: Math.max(0, system.x - padding),
      y: Math.max(0, system.y - padding),
      width: padding * 2,
      height: padding * 2
    });
  }, []);

  // Obsługa scroll (zoom)
  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    const handleWheel = (e) => {
      e.preventDefault();
      const rect = container.getBoundingClientRect();
      setViewBox(prev => {
        const mouseX = ((e.clientX - rect.left) / rect.width) * prev.width + prev.x;
        const mouseY = ((e.clientY - rect.top) / rect.height) * prev.height + prev.y;
        
        const zoomFactor = e.deltaY > 0 ? 1.1 : 0.9;
        const newWidth = Math.max(500, Math.min(MAP_WIDTH, prev.width * zoomFactor));
        const newHeight = Math.max(500, Math.min(MAP_HEIGHT, prev.height * zoomFactor));
        
        const newX = mouseX - (mouseX - prev.x) * (newWidth / prev.width);
        const newY = mouseY - (mouseY - prev.y) * (newHeight / prev.height);

        return {
          x: Math.max(0, Math.min(MAP_WIDTH - newWidth, newX)),
          y: Math.max(0, Math.min(MAP_HEIGHT - newHeight, newY)),
          width: newWidth,
          height: newHeight
        };
      });
    };

    container.addEventListener('wheel', handleWheel, { passive: false });
    return () => container.removeEventListener('wheel', handleWheel);
  }, []);

  // Przeszukaj i wycentruj
  const handleSearch = useCallback(() => {
    const found = STAR_SYSTEMS.find(s => 
      s.name.toLowerCase().includes(searchQuery.toLowerCase())
    );
    if (found) {
      centerOnSystem(found);
      onSystemSelect?.(found);
    }
  }, [searchQuery, centerOnSystem, onSystemSelect]);

  // Pobierz kolor układu
  const getSystemColor = useCallback((system, known) => {
    if (!known) return '#333333'; // Nieznane układy - ciemne
    return system.faction ? (FACTION_COLORS[system.faction] || '#888888') : '#888888';
  }, []);

  // Pobierz rozmiar układu
  const getSystemSize = useCallback((system) => {
    if (system.isCapital) return 25;
    if (system.faction) return 15;
    return 10;
  }, []);

  // Sprawdź czy układ jest zaznaczony
  const isSystemHighlighted = useCallback((system) => {
    return highlightedSystems.includes(system.id) || 
           (selectedSystem && selectedSystem.id === system.id);
  }, [highlightedSystems, selectedSystem]);

  // Kliknięcie na układ
  const handleSystemClick = useCallback((system) => {
    onSystemSelect?.(system);
  }, [onSystemSelect]);

  return (
    <Box className="star-map-container" position="relative">
      {/* Kontrolki */}
      <VStack 
        position="absolute" 
        top="4" 
        left="4" 
        zIndex="10" 
        gap="2"
        bg="rgba(10, 20, 72, 0.9)"
        p="3"
        borderRadius="lg"
        border="1px solid"
        borderColor="whiteAlpha.200"
      >
        <HStack>
          <Input
            size="sm"
            placeholder="Szukaj układu..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
            bg="whiteAlpha.100"
            color="white"
            borderColor="whiteAlpha.300"
            _placeholder={{ color: 'whiteAlpha.500' }}
            width="180px"
          />
          <IconButton
            size="sm"
            onClick={handleSearch}
            aria-label="Szukaj"
            variant="ghost"
            colorScheme="blue"
          >
            <LuSearch />
          </IconButton>
          {searchQuery && (
            <IconButton
              size="sm"
              onClick={() => setSearchQuery('')}
              aria-label="Wyczyść"
              variant="ghost"
              colorScheme="gray"
            >
              <LuX />
            </IconButton>
          )}
        </HStack>
        
        <HStack>
          <IconButton
            size="sm"
            onClick={() => handleZoom(1)}
            aria-label="Przybliż"
            variant="outline"
            colorScheme="blue"
          >
            <LuZoomIn />
          </IconButton>
          <IconButton
            size="sm"
            onClick={() => handleZoom(-1)}
            aria-label="Oddal"
            variant="outline"
            colorScheme="blue"
          >
            <LuZoomOut />
          </IconButton>
          <IconButton
            size="sm"
            onClick={resetView}
            aria-label="Reset widoku"
            variant="outline"
            colorScheme="gray"
          >
            <LuHouse />
          </IconButton>
        </HStack>

        {/* Licznik */}
        <Text fontSize="xs" color="whiteAlpha.700">
          {filteredSystems.length} układów
        </Text>
      </VStack>

      {/* Info o zaznaczonym układzie */}
      {selectedSystem && (
        <Box
          position="absolute"
          top="4"
          right="4"
          zIndex="10"
          bg="rgba(10, 20, 72, 0.95)"
          p="4"
          borderRadius="lg"
          border="1px solid"
          borderColor="whiteAlpha.300"
          minW="200px"
          maxW="280px"
        >
          <Text color="white" fontWeight="bold" fontSize="lg">
            {selectedSystem.name}
          </Text>
          {selectedSystem.faction && (
            <Badge 
              mt="1"
              colorScheme={
                selectedSystem.faction === 'hegemonia' ? 'red' :
                selectedSystem.faction === 'shimura' ? 'cyan' :
                selectedSystem.faction === 'protektorat' ? 'green' :
                'gray'
              }
            >
              {selectedSystem.faction === 'hegemonia' ? 'Hegemonia' :
               selectedSystem.faction === 'shimura' ? 'Shimura' :
               selectedSystem.faction === 'protektorat' ? 'Protektorat' :
               selectedSystem.faction === 'pirates' ? 'Piraci' :
               selectedSystem.faction === 'separatists' ? 'Separatyści' :
               selectedSystem.faction}
            </Badge>
          )}
          {selectedSystem.isCapital && (
            <Badge ml="1" colorScheme="yellow">Stolica</Badge>
          )}
          <Text color="whiteAlpha.600" fontSize="xs" mt="2">
            Współrzędne: ({selectedSystem.x}, {selectedSystem.y})
          </Text>
          {selectedSystem.description && (
            <Text color="whiteAlpha.800" fontSize="sm" mt="2">
              {selectedSystem.description}
            </Text>
          )}
        </Box>
      )}

      {/* Mapa SVG */}
      <svg
        ref={containerRef}
        className="star-map-svg"
        viewBox={`${viewBox.x} ${viewBox.y} ${viewBox.width} ${viewBox.height}`}
        onMouseDown={(e) => {
          // Tylko lewy przycisk i tylko na tle (nie na planetach)
          if (e.button !== 0) return;
          if (e.target.closest('.star-system')) return;
          
          setIsPanning(true);
          isDraggingRef.current = false;
          panStartRef.current = { 
            x: e.clientX, 
            y: e.clientY, 
            vx: viewBox.x, 
            vy: viewBox.y 
          };
        }}
        onMouseMove={(e) => {
          if (!isPanning) return;
          
          const rect = containerRef.current?.getBoundingClientRect();
          if (!rect) return;
          
          const dx = (e.clientX - panStartRef.current.x) * (viewBox.width / rect.width);
          const dy = (e.clientY - panStartRef.current.y) * (viewBox.height / rect.height);
          
          // Ustaw flagę że przeciągamy (nie klikamy)
          if (Math.abs(dx) > 5 || Math.abs(dy) > 5) {
            isDraggingRef.current = true;
          }
          
          setViewBox(prev => ({
            ...prev,
            x: Math.max(0, Math.min(MAP_WIDTH - prev.width, panStartRef.current.vx - dx)),
            y: Math.max(0, Math.min(MAP_HEIGHT - prev.height, panStartRef.current.vy - dy))
          }));
        }}
        onMouseUp={() => setIsPanning(false)}
        onMouseLeave={() => setIsPanning(false)}
        style={{ cursor: isPanning ? 'grabbing' : 'grab' }}
      >
        {/* Tło kosmiczne */}
        <defs>
          <radialGradient id="space-gradient" cx="50%" cy="50%" r="70%">
            <stop offset="0%" stopColor="#1a237e" stopOpacity="1" />
            <stop offset="50%" stopColor="#0d1442" stopOpacity="1" />
            <stop offset="100%" stopColor="#050a20" stopOpacity="1" />
          </radialGradient>
        </defs>

        {/* Tło */}
        <rect x="0" y="0" width={MAP_WIDTH} height={MAP_HEIGHT} fill="url(#space-gradient)" />

        {/* Gwiazdy tła */}
        {BACKGROUND_STARS.map(star => (
          <circle
            key={`bg-star-${star.id}`}
            cx={star.x}
            cy={star.y}
            r={star.r}
            fill="white"
            opacity={star.opacity}
          />
        ))}

        {/* Siatka nawigacyjna */}
        <g opacity="0.1" stroke="#4444ff" strokeWidth="1">
          {Array.from({ length: 8 }, (_, i) => (
            <line key={`grid-v-${i}`} x1={i * 1000} y1="0" x2={i * 1000} y2={MAP_HEIGHT} />
          ))}
          {Array.from({ length: 8 }, (_, i) => (
            <line key={`grid-h-${i}`} x1="0" y1={i * 1000} x2={MAP_WIDTH} y2={i * 1000} />
          ))}
        </g>

        {/* Centrum galaktyki (Gildia) */}
        <circle cx={3500} cy={3500} r="200" fill="none" stroke="#ffe066" strokeWidth="3" opacity="0.2" />
        <circle cx={3500} cy={3500} r="400" fill="none" stroke="#ffe066" strokeWidth="2" opacity="0.1" />

        {/* Układy gwiezdne */}
        {filteredSystems.map(system => {
          const known = isSystemKnown(system);
          const color = getSystemColor(system, known);
          const size = getSystemSize(system);
          const isHighlighted = isSystemHighlighted(system);
          const explorationStatus = explorationData[system.id]?.status;
          return (
            <g 
              key={system.id}
              className="star-system"
              onClick={() => handleSystemClick(system)}
              style={{ cursor: 'pointer' }}
            >
              {/* Pierścień dla stolic */}
              {system.isCapital && known && (
                <circle
                  cx={system.x}
                  cy={system.y}
                  r={size + 20}
                  fill="none"
                  stroke={color}
                  strokeWidth="3"
                  opacity="0.4"
                  className="capital-ring"
                />
              )}

              {/* Pierścień zaznaczenia */}
              {isHighlighted && (
                <circle
                  cx={system.x}
                  cy={system.y}
                  r={size + 15}
                  fill="none"
                  stroke="#ffffff"
                  strokeWidth="2"
                  opacity="0.8"
                  className="selection-ring"
                />
              )}

              {/* Poświata */}
              <circle
                cx={system.x}
                cy={system.y}
                r={size + 8}
                fill={color}
                opacity={known ? 0.3 : 0.1}
              />

              {/* Główna gwiazda */}
              <circle
                cx={system.x}
                cy={system.y}
                r={size}
                fill={color}
                opacity={known ? 1 : 0.4}
                stroke={isHighlighted ? '#ffffff' : 'rgba(255,255,255,0.5)'}
                strokeWidth={isHighlighted ? 3 : 1}
              />

              {/* Ikona statusu eksploracji */}
              {explorationStatus === EXPLORATION_STATUS.EXPLORED && (
                <circle
                  cx={system.x + size}
                  cy={system.y - size}
                  r="8"
                  fill="#4ade80"
                  stroke="#166534"
                  strokeWidth="2"
                />
              )}
              {explorationStatus === EXPLORATION_STATUS.DISCOVERED && (
                <circle
                  cx={system.x + size}
                  cy={system.y - size}
                  r="8"
                  fill="#fbbf24"
                  stroke="#92400e"
                  strokeWidth="2"
                />
              )}

              {/* Nazwa układu - zawsze widoczna */}
              <text
                x={system.x + size + 10}
                y={system.y + 5}
                fill={known ? (color === '#888888' ? '#ffffff' : color) : '#555555'}
                fontSize={system.isCapital ? 36 : 24}
                fontWeight={system.isCapital ? 'bold' : 'normal'}
                opacity={known ? (isHighlighted ? 1 : 0.9) : 0.5}
                style={{ pointerEvents: 'none' }}
              >
                {system.name}
              </text>
            </g>
          );
        })}
      </svg>
    </Box>
  );
};

export default StarMap;
