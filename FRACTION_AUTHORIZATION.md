# Mechanizm Autoryzacji Frakcji

## Przegląd

System implementuje mechanizm autoryzacji oparty na tokenach, który pozwala przypisać użytkowników do frakcji w grze. Każdy gracz otrzymuje unikalny AuthToken (GUID), który musi być dołączany do każdego żądania API, aby ograniczyć akcje tylko do jego frakcji.

## Zmiany w Modelu Danych

### FractionState (Core)

Dodano nowe właściwości:
- `PlayerName` - nazwa gracza przypisanego do frakcji
- `FractionColor` - kolor frakcji (dla UI)
- `AuthToken` - unikalny token autoryzacyjny (GUID) generowany automatycznie

```csharp
public class FractionState
{
    public Guid FractionId { get; set; }
    public string FractionName { get; set; }
    public string PlayerName { get; set; }
    public string FractionColor { get; set; }
    public Guid AuthToken { get; set; }
    // ... pozostałe właściwości
}
```

## API Endpoints

### 1. Tworzenie Frakcji

**POST** `/api/battles/{battleId}/fractions`

**Request Body:**
```json
{
  "fractionName": "Imperium",
  "playerName": "Jan Kowalski",
  "fractionColor": "#FF0000"
}
```

**Response:**
```json
{
  "fractionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "authToken": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
}
```

⚠️ **Ważne**: AuthToken jest zwracany tylko raz podczas tworzenia frakcji. Należy go zapisać i używać w kolejnych żądaniach.

### 2. Użycie AuthToken

Wszystkie operacje na frakcjach wymagają nagłówka HTTP:

```
X-Auth-Token: 7c9e6679-7425-40de-944b-e07fc1f90ae7
```

**Przykładowe endpointy wymagające autoryzacji:**
- `POST /api/battles/{battleId}/fractions/{fractionId}/ships` - dodanie statku
- `PUT /api/battles/{battleId}/fractions/{fractionId}/ships/{shipId}` - aktualizacja statku
- `PATCH /api/battles/{battleId}/fractions/{fractionId}/ships/{shipId}/position` - zmiana pozycji

## Generowanie URL dla Graczy

Po utworzeniu frakcji, każdy gracz powinien otrzymać URL z osadzonym AuthToken:

```
https://your-game-url.com/battles/{battleId}/simulator?token={authToken}&fractionId={fractionId}
```

**Przykład:**
```
https://localhost:5173/battles/123e4567-e89b-12d3-a456-426614174000/simulator?token=7c9e6679-7425-40de-944b-e07fc1f90ae7&fractionId=3fa85f64-5717-4562-b3fc-2c963f66afa6
```

⚠️ **Ważne**: 
- Gracz automatycznie zostanie zalogowany do swojej frakcji
- Token jest przechowywany w localStorage
- Gracz może kontrolować tylko swoje statki
- Wszystkie akcje są ograniczone do jego frakcji

## Implementacja Frontendowa

### 1. Pobranie Tokenu z URL

```javascript
const urlParams = new URLSearchParams(window.location.search);
const authToken = urlParams.get('token');
const fractionId = urlParams.get('fractionId');

// Zapisz w localStorage lub context
localStorage.setItem('authToken', authToken);
localStorage.setItem('fractionId', fractionId);
```

### 2. Dodawanie Tokenu do Requestów

#### Axios
```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://your-api-url.com/api'
});

api.interceptors.request.use(config => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers['X-Auth-Token'] = token;
  }
  return config;
});

export default api;
```

#### Fetch API
```javascript
const authToken = localStorage.getItem('authToken');

fetch(`/api/battles/${battleId}/fractions/${fractionId}/ships`, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'X-Auth-Token': authToken
  },
  body: JSON.stringify(shipData)
});
```

## Mechanizm Autoryzacji

### 1. FractionAuthAttribute

Atrybut stosowany na kontrolerach/metodach wymagających autoryzacji:

```csharp
[FractionAuth]
public class ShipsController : ControllerBase
{
    // Wszystkie metody wymagają X-Auth-Token
}
```

### 2. FractionAuthorizationMiddleware

Middleware weryfikuje:
1. Czy AuthToken w nagłówku jest poprawny
2. Czy AuthToken pasuje do frakcji z URL
3. Czy gracz ma dostęp do zasobów tej frakcji

### Kody Błędów

- **401 Unauthorized** - brak nagłówka X-Auth-Token lub niepoprawny format
- **403 Forbidden** - AuthToken nie pasuje do frakcji lub brak uprawnień

## Przykładowy Flow

1. **Admin tworzy grę:**
   - Tworzy bitwę: `POST /api/battles`
   - Otrzymuje `battleId`

2. **Admin dodaje frakcje:**
   - `POST /api/battles/{battleId}/fractions`
   ```json
   {
     "fractionName": "Rebels",
     "playerName": "Player1",
     "fractionColor": "#00FF00"
   }
   ```
   - Otrzymuje `{ fractionId, authToken }`
   - Generuje URL dla gracza

3. **Gracz wchodzi na URL:**
   - Aplikacja frontend pobiera token z URL
   - Zapisuje token w localStorage
   - Dodaje token do wszystkich requestów

4. **Gracz wykonuje akcje:**
   - Dodaje statki: `POST /api/battles/{battleId}/fractions/{fractionId}/ships`
   - Nagłówek: `X-Auth-Token: {authToken}`
   - Backend weryfikuje czy token pasuje do frakcji

## Bezpieczeństwo

⚠️ **Ważne uwagi:**

1. **Token w URL** - używany tylko do pierwszego zalogowania, potem przechowywany w localStorage
2. **HTTPS** - zawsze używaj HTTPS w produkcji
3. **Token expiration** - rozważ dodanie czasu wygaśnięcia tokenów
4. **Rate limiting** - dodaj ograniczenie liczby requestów
5. **MongoDB** - AuthToken jest przechowywany w bazie danych wraz z FractionState

## Przyszłe Rozszerzenia

Możliwe ulepszenia:
- [ ] Dodanie czasu wygaśnięcia tokenów
- [ ] Refresh tokens
- [ ] Rate limiting per token
- [ ] Audit log akcji użytkowników
- [ ] Admin panel do zarządzania tokenami
- [ ] Możliwość resetowania tokenu
