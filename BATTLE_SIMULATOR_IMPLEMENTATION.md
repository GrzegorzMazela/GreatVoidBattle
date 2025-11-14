# 🎮 Battle Simulator - Kompletna Implementacja

## ✅ Co zostało zaimplementowane

### 🔧 Backend (C# .NET)

#### 1. DTO dla rozkazów
- `OrderDto.cs` - reprezentacja pojedynczego rozkazu
- `SubmitOrdersDto.cs` - kontener dla zbioru rozkazów

#### 2. API Endpoints w `BattlesController.cs`

```csharp
// Wyślij rozkazy dla frakcji
POST /api/battles/{battleId}/fractions/{fractionId}/orders
Body: {
  "turnNumber": 5,
  "orders": [
    { "shipId": "guid", "type": "move", "targetX": 100, "targetY": 150 },
    { "shipId": "guid", "type": "laser", "targetShipId": "guid", "targetFractionId": "guid" },
    { "shipId": "guid", "type": "missile", "targetShipId": "guid", "targetFractionId": "guid" }
  ]
}

// Wykonaj turę
POST /api/battles/{battleId}/execute-turn
Response: BattleStateDto z nowym stanem bitwy
```

#### 3. Eventy
Eventy były już zaimplementowane:
- `AddShipMoveEvent` - rozkaz ruchu
- `AddLaserShotEvent` - rozkaz strzału laserowego
- `AddMissileShotEvent` - rozkaz strzału rakietowego
- `EndOfTurnEvent` - wykonanie tury

### 🎨 Frontend (React)

#### 1. API Service (`api.js`)
```javascript
export const submitOrders = async (battleId, fractionId, payload)
export const executeTurn = async (battleId)
```

#### 2. Custom Hooks

**`useBattleState.js`**
- Zarządza stanem bitwy
- Opcjonalne auto-refresh
- Obsługa błędów i loading states

**`useOrders.js`**
- Kolejkowanie rozkazów lokalnie
- CRUD operacje na rozkazach (add, remove, clear)
- Wysyłanie rozkazów do API
- Walidacja

#### 3. Komponenty

**`BattleCanvas.jsx`** ⭐
Najważniejszy komponent - wydajne renderowanie:
- Canvas-based rendering
- **Viewport rendering** - rysuje tylko widoczny obszar
- Pan & Zoom
- Selekcja statków i komórek
- Wizualizacja rozkazów (strzałki, linie)
- HP bary dla statków
- Różne kolory dla frakcji

**`ShipControlPanel.jsx`**
Panel informacyjny:
- Statystyki wybranego statku
- Wizualizacja HP/Shield/Armor
- Aktualny rozkaz
- Anulowanie rozkazu
- Instrukcje sterowania

**`TurnController.jsx`**
Kontroler tury:
- Numer tury i status bitwy
- Podsumowanie rozkazów
- Przycisk zatwierdzania rozkazów
- Przycisk wykonania tury
- Status wszystkich frakcji
- Obsługa błędów

**`BattleSimulator.jsx`** 🎯
Główny komponent łączący wszystko:
- Routing i parametry URL
- Wybór frakcji gracza
- Logika interakcji (klik = ruch/atak)
- Zarządzanie stanem aplikacji
- Layout responsywny

#### 4. Styling
Kompletne CSS dla wszystkich komponentów:
- Dark theme
- Responsywność
- Animacje (spinner, hover effects)
- Color coding dla frakcji
- Status badges

#### 5. Routing
Dodany route: `/battles/:battleId/simulator`

#### 6. Integracja
Przycisk "Uruchom Symulator" w `BattleDetails.jsx`

## 🎮 Jak to działa

### Przebieg gry (Turn-based)

```
1. WYBÓR FRAKCJI
   ↓
2. PLANOWANIE RUCHÓW
   - Kliknij statek → wybierz go
   - Kliknij puste pole → zaplanuj ruch
   - Kliknij wrogi statek → zaplanuj atak
   ↓
3. ZATWIERDZENIE ROZKAZÓW
   - Kliknij "Zatwierdź rozkazy"
   - Rozkazy wysyłane do API
   ↓
4. WYKONANIE TURY
   - Kliknij "Wykonaj turę"
   - Backend przetwarza wszystkie rozkazy
   - Zwraca nowy stan bitwy
   ↓
5. WIZUALIZACJA ZMIAN
   - Mapa aktualizuje się
   - Pokazuje nowe pozycje i HP statków
   ↓
6. NASTĘPNA TURA (wróć do punktu 2)
```

### Architektura danych

```
Frontend                    Backend
  │                           │
  ├─ BattleSimulator         │
  │   ├─ useBattleState  ────┼─→ GET /api/battles/{id}
  │   │   └─ battleState     │   └─ Returns: BattleStateDto
  │   │                       │
  │   ├─ useOrders           │
  │   │   ├─ orders[]        │
  │   │   └─ submit()   ─────┼─→ POST .../orders
  │   │                       │   └─ Applies: AddShipMoveEvent
  │   │                       │                AddLaserShotEvent
  │   │                       │                AddMissileShotEvent
  │   │                       │
  │   └─ executeTurn()  ─────┼─→ POST .../execute-turn
  │                           │   └─ Applies: EndOfTurnEvent
  │                           │   └─ BattleState.EndOfTurn()
  │                           │       ├─ RunLaserShots()
  │                           │       ├─ RunMissileShot()
  │                           │       └─ MoveShips()
  │                           │
  └─ BattleCanvas            │
      └─ Renders visual      │
          representation      │
```

## 🚀 Optymalizacje wydajnościowe

### 1. Viewport Rendering
```javascript
// Tylko widoczny obszar jest renderowany
const visibleArea = {
  startX: Math.floor(viewport.x),
  endX: Math.ceil(viewport.x + canvasWidth / (cellSize * zoom))
};

// Dla siatki 500×500, renderujemy np. tylko 50×50 widocznych komórek
```

### 2. Canvas Layers (koncepcyjnie)
```
Layer 1: Grid (statyczny) ← rysowany raz
Layer 2: Ships (dynamiczny) ← rysowany przy zmianach
Layer 3: Orders (dynamiczny) ← rysowany przy zmianach
```

### 3. React Optimization
- `useCallback` dla funkcji renderujących
- Unikanie zbędnych re-renderów
- Memoizacja drogich obliczeń

### 4. Wydajność dla 500×500
- **DOM approach**: ~250,000 divów = BARDZO WOLNO ❌
- **Canvas approach**: Jeden element + viewport = SZYBKO ✅
- Teoretycznie może obsłużyć nawet większe mapy

## 📁 Struktura plików

```
battle-app-admin/src/
├── services/
│   └── api.js (+ submitOrders, executeTurn)
│
├── features/
│   ├── admin/
│   │   └── BattleDetails.jsx (+ przycisk symulatora)
│   │
│   └── battle-simulator/
│       ├── index.js (exports)
│       ├── README.md (dokumentacja)
│       │
│       ├── hooks/
│       │   ├── useBattleState.js
│       │   └── useOrders.js
│       │
│       └── components/
│           ├── BattleSimulator.jsx + .css
│           ├── BattleCanvas.jsx
│           ├── ShipControlPanel.jsx + .css
│           └── TurnController.jsx + .css
│
└── app/
    └── router.jsx (+ route do symulatora)

GreatVoidBattle.Application/
├── Dto/Battles/
│   ├── OrderDto.cs
│   └── SubmitOrdersDto.cs
│
├── Events/InProgress/
│   ├── AddShipMoveEvent.cs (już istniał)
│   ├── AddLaserShotEvent.cs (już istniał)
│   ├── AddMissileShotEvent.cs (już istniał)
│   └── EndOfTurnEvent.cs (już istniał)
│
└── Controllers/
    └── BattlesController.cs (+ 2 nowe endpointy)
```

## 🎯 Sterowanie

### Podstawowe
- **LMB na statek** - wybierz statek
- **LMB na puste pole** - wydaj rozkaz ruchu
- **LMB na wrogi statek** - zaatakuj laserem

### Nawigacja
- **Scroll** - zoom in/out (0.5x - 3x)
- **Ctrl + LMB przeciągnij** - przesuń viewport
- **MMB przeciągnij** - przesuń viewport

### UI
- **Przycisk ✕** - anuluj rozkaz statku
- **"Zatwierdź rozkazy"** - wyślij rozkazy do API
- **"Wykonaj turę"** - wykonaj turę
- **🔄 Odśwież** - odśwież stan bitwy

## 🔮 Możliwe rozszerzenia

### Krótkoterminowe
1. **Wybór typu ataku** - radio buttons dla laser/missile
2. **Potwierdzenia** - modals przed wykonaniem tury
3. **Tooltip** - informacje o statku przy hover
4. **Mini-mapa** - nawigacja po dużych mapach

### Średnioterminowe
5. **Animacje** - interpolacja ruchów, efekty strzałów
6. **Historia tur** - timeline z możliwością przeglądania
7. **Hotkeys** - klawiatura do szybszych akcji
8. **Zoom to selection** - automatyczne centrowanie

### Długoterminowe
9. **WebSocket** - real-time multiplayer
10. **Replay system** - nagrywanie i odtwarzanie bitew
11. **AI przeciwnik** - bot do gry solo
12. **Spatial indexing** - QuadTree dla ogromnych map
13. **WebGL** - jeszcze wydajniejszy rendering (PixiJS)
14. **Particle effects** - wybuchy, dymy

## 🐛 Znane ograniczenia

1. **Brak animacji** - zmiany są natychmiastowe
2. **Jeden typ ataku** - tylko laser przy kliknięciu
3. **Brak walidacji zasięgu** - UI nie sprawdza czy statek może strzelić
4. **Brak podglądu ścieżki** - nie ma preview ruchu
5. **Performance** - dla map 500×500 może wymagać dalszych optymalizacji

## 📝 Testowanie

### Uruchomienie

1. **Backend**:
```bash
cd GreatVoidBattle.Api
dotnet run
```

2. **Frontend**:
```bash
cd battle-app-admin
npm run dev
```

3. Otwórz: `http://localhost:5173/admin/{battleId}`
4. Kliknij: "🎮 Uruchom Symulator"

### Test scenario

1. Stwórz bitwę (np. 100×100)
2. Dodaj 2-3 frakcje
3. Dodaj kilka statków do każdej frakcji
4. Uruchom symulator
5. Wybierz frakcję
6. Zaplanuj rozkazy (ruchy i ataki)
7. Zatwierdź rozkazy
8. Wykonaj turę
9. Sprawdź czy stan się zmienił

## 🎓 Wnioski techniczne

### Co zadziałało dobrze
✅ Canvas rendering jest szybki i wydajny
✅ Viewport approach działa świetnie
✅ Hook-based architecture jest czytelna
✅ Event system w backendzie jest elastyczny

### Co można ulepszyć
⚠️ Dodać testy jednostkowe
⚠️ Rozdzielić Canvas rendering na klasy
⚠️ Dodać TypeScript dla lepszej type safety
⚠️ Implementować QuadTree dla bardzo dużych map

### Lessons learned
💡 Canvas >> DOM dla dużych siatek
💡 Viewport rendering jest kluczowy dla wydajności
💡 Separation of concerns (hooks, components) = maintainable code
💡 Backend event system + frontend state management = flexible architecture
