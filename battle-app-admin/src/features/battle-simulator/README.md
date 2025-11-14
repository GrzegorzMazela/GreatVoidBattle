# Battle Simulator

Wydajny symulator bitwy kosmicznej z renderowaniem canvas i systemem rozkazów.

## 🎯 Cechy

- **Wydajne renderowanie Canvas**: Obsługuje siatki do 500×500 komórek
- **Viewport rendering**: Rysuje tylko widoczny obszar
- **System rozkazów**: Kolejkowanie rozkazów przed wykonaniem tury
- **Interaktywna mapa**: Pan, zoom, selekcja statków
- **Real-time updates**: Odświeżanie stanu bitwy

## 📦 Komponenty

### `BattleSimulator`
Główny komponent łączący wszystkie elementy.

```jsx
import { BattleSimulator } from '@/features/battle-simulator';

<Route path="/battles/:battleId/simulator" element={<BattleSimulator />} />
```

### `BattleCanvas`
Canvas do renderowania pola bitwy z wydajnym viewport rendering.

**Funkcje:**
- Rysowanie siatki (tylko widoczne komórki)
- Renderowanie statków z HP barami
- Wizualizacja rozkazów (strzałki ruchu, linie strzału)
- Pan (Ctrl + przeciągnij)
- Zoom (scroll)

### `ShipControlPanel`
Panel kontrolny dla wybranego statku.

**Wyświetla:**
- Statystyki statku (HP, pancerz, tarcze)
- Aktualny rozkaz
- Instrukcje sterowania

### `TurnController`
Kontroler tury z podsumowaniem i akcjami.

**Funkcje:**
- Wyświetlanie numeru tury
- Podsumowanie rozkazów
- Zatwierdzanie rozkazów
- Wykonywanie tury
- Status frakcji

## 🎮 Hooks

### `useBattleState`
Hook do zarządzania stanem bitwy.

```javascript
const { battleState, loading, error, refresh } = useBattleState(
  battleId,
  autoRefresh = false,
  refreshInterval = 2000
);
```

### `useOrders`
Hook do kolejkowania rozkazów lokalnie.

```javascript
const {
  orders,
  addMoveOrder,
  addLaserOrder,
  addMissileOrder,
  removeOrder,
  clearOrders,
  submit,
  getOrderForShip,
  submitting,
  error,
  hasOrders
} = useOrders(battleId, fractionId, turnNumber);
```

## 🎮 Sterowanie

### Tryb Preparation (Przygotowanie)
- **Kliknij statek** - wybierz go
- **Kliknij puste pole** - przesuń statek na nową pozycję (natychmiastowo)
- Niebieski obszar pokazuje zasięg ruchu statku

### Tryb InProgress (Rozgrywka)

#### Podstawowe
- **Kliknij statek** - wybierz go
- **Kliknij puste pole** - wydaj rozkaz ruchu
- **Kliknij wrogi statek** - zaatakuj (laser)

### Nawigacja
- **Scroll** - zoom in/out
- **Ctrl + przeciągnij** - przesuń widok
- **Środkowy przycisk myszy** - przesuń widok

## 🔧 API Endpoints

### Backend endpoints używane przez symulator:

```csharp
// Pobierz stan bitwy
GET /api/battles/{battleId}

// Wyślij rozkazy
POST /api/battles/{battleId}/fractions/{fractionId}/orders
{
  "turnNumber": 5,
  "orders": [
    {
      "shipId": "guid",
      "type": "move",
      "targetX": 100,
      "targetY": 150
    },
    {
      "shipId": "guid",
      "type": "laser",
      "targetShipId": "guid",
      "targetFractionId": "guid"
    }
  ]
}

// Wykonaj turę
POST /api/battles/{battleId}/execute-turn
```

## 🚀 Optymalizacje

### Viewport Rendering
Tylko widoczne komórki są renderowane:

```javascript
const visibleArea = {
  startX: Math.max(0, Math.floor(viewport.x)),
  endX: Math.min(width - 1, Math.ceil(viewport.x + canvasWidth / cellSize))
};
```

### Separate Layers
- Grid (statyczny)
- Ships (dynamiczny)
- Orders (dynamiczny)

### Performance Tips
- Canvas jest re-renderowany tylko przy zmianach
- Używamy `useCallback` dla funkcji renderujących
- Viewport ogranicza ilość rysowanych obiektów

## 📝 Przykład użycia

```jsx
import { BattleSimulator } from '@/features/battle-simulator';

// W routing
<Route 
  path="/battles/:battleId/simulator" 
  element={<BattleSimulator />} 
/>

// Link do symulatora
<Link to={`/battles/${battleId}/simulator`}>
  Uruchom symulator
</Link>
```

## 🎯 Przebieg gry

### Tryb Preparation (Przygotowanie)
1. **Wybór frakcji** - Gracz wybiera swoją frakcję
2. **Rozmieszczanie statków** - Klikając w statek i następnie w puste pole, gracz ustawia pozycję statku
3. **Start bitwy** - Przycisk "Rozpocznij bitwę" (trzeba dodać w UI)

### Tryb InProgress (Rozgrywka)

1. **Wybór frakcji** - Gracz wybiera swoją frakcję (jeśli nie wybrana wcześniej)
2. **Planowanie** - Gracz wybiera statki i wydaje rozkazy:
   - Ruch: kliknij w puste pole
   - Atak: kliknij we wrogi statek
3. **Zatwierdzenie** - Przycisk "Zatwierdź rozkazy"
4. **Wykonanie tury** - Przycisk "Wykonaj turę"
5. **Animacja** - Mapa pokazuje rezultaty (w przyszłości)
6. **Następna tura** - Powtórz od kroku 2

## 🔮 Przyszłe ulepszenia

- [ ] Animacje ruchów i strzałów
- [ ] Efekty dźwiękowe
- [ ] Historia tur
- [ ] Replay systemu
- [ ] Multi-player przez WebSocket
- [ ] Spatial indexing (QuadTree) dla większych map
- [ ] OffscreenCanvas dla statycznego grida
