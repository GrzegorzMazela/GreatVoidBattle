# Szybki Przewodnik - Uruchomienie Gry z Autoryzacją

## Krok 1: Przygotowanie Gry (Admin)

### 1.1 Utwórz Bitwę
```bash
POST /api/battles
{
  "name": "Bitwa o Sektor 7",
  "width": 100,
  "height": 100
}
```
Zapisz otrzymany `battleId`.

### 1.2 Dodaj Frakcje i Graczy

Dla każdego gracza:
```bash
POST /api/battles/{battleId}/fractions
{
  "fractionName": "Imperium",
  "playerName": "Jan Kowalski",
  "fractionColor": "#FF0000"
}
```

Otrzymasz:
```json
{
  "fractionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "authToken": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
}
```

### 1.3 Wygeneruj URL dla Gracza

```
https://your-domain.com/battles/{battleId}/simulator?token={authToken}&fractionId={fractionId}
```

**Przykład:**
```
http://localhost:5173/battles/123e4567-e89b-12d3-a456-426614174000/simulator?token=7c9e6679-7425-40de-944b-e07fc1f90ae7&fractionId=3fa85f64-5717-4562-b3fc-2c963f66afa6
```

### 1.4 Wyślij Link Graczom

Każdy gracz dostaje SWÓJ unikalny link z:
- Unikalnym `authToken` - identyfikuje gracza
- `fractionId` - określa jego frakcję
- `battleId` - określa bitwę

## Krok 2: Rozgrywka (Gracz)

### 2.1 Gracz Wchodzi na Link

1. Kliknięcie w link automatycznie:
   - Zapisuje token w przeglądarce (localStorage)
   - Loguje gracza do jego frakcji
   - Przekierowuje do symulatora

2. Gracz widzi:
   - Swoje imię i nazwę frakcji w nagłówku
   - Tylko swoje statki są interaktywne
   - Może wydawać rozkazy tylko swoim jednostkom

### 2.2 Faza Przygotowania

**Status: `Preparation`**

Gracz może:
- ✅ Dodawać statki do swojej frakcji
- ✅ Ustawiać pozycje statków
- ✅ Konfigurować wyposażenie

Gracz NIE może:
- ❌ Dodawać statków do innych frakcji
- ❌ Przesuwać cudzych statków

### 2.3 Faza Bitwy

**Status: `InProgress`**

W każdej turze gracz:
1. Wybiera swój statek
2. Wydaje rozkazy (ruch, strzał)
3. Zatwierdza rozkazy przyciskiem "Zatwierdź rozkazy"
4. Czeka aż wszyscy gracze zatwierdzą
5. Admin wykonuje turę

## Przykładowy Scenariusz

### Admin Panel (Komponent FractionCreator)

```jsx
import FractionCreator from './components/FractionCreator';

function AdminPanel() {
  const battleId = "123e4567-e89b-12d3-a456-426614174000";
  
  return <FractionCreator battleId={battleId} />;
}
```

Admin wypełnia formularz:
- Nazwa Frakcji: "Imperium Galaktyczne"
- Nazwa Gracza: "Jan Kowalski"
- Kolor: #FF0000

Po kliknięciu "Utwórz Frakcję":
1. System tworzy frakcję
2. Generuje unikalny AuthToken
3. Pokazuje link do skopiowania
4. Admin wysyła link graczowi (email, chat, etc.)

### Gracz

1. **Otrzymuje link:**
   ```
   http://localhost:5173/battles/123e4567.../simulator?token=7c9e6679...&fractionId=3fa85f64...
   ```

2. **Klika w link:**
   - Strona automatycznie się ładuje
   - Token zapisany w localStorage
   - Widzi "Grasz jako: Jan Kowalski (Imperium Galaktyczne)"

3. **Dodaje statki (faza przygotowania):**
   - Przechodzi do panelu admina swojej frakcji
   - Dodaje statki przez API (z tokenem w nagłówku)

4. **Rozpoczyna bitwę:**
   - Wybiera swój statek
   - Kliknięcie = ruch
   - Wybór broni + kliknięcie w wroga = atak
   - "Zatwierdź rozkazy" = wysłanie na serwer

## Rozwiązywanie Problemów

### "Brak autoryzacji"

**Problem:** Gracz widzi ekran z błędem autoryzacji.

**Rozwiązania:**
1. Sprawdź czy link zawiera `token` i `fractionId`
2. Sprawdź czy token jest poprawny (GUID)
3. Sprawdź czy frakcja istnieje w bazie
4. Wyczyść localStorage i spróbuj ponownie

### "403 Forbidden"

**Problem:** Gracz próbuje wykonać akcję na cudzej frakcji.

**Rozwiązania:**
1. Upewnij się, że `fractionId` w URL zgadza się z tokenem
2. Sprawdź czy gracz nie próbuje modyfikować cudzych zasobów
3. Zweryfikuj czy token pasuje do frakcji w bazie

### Token zgubiony

**Problem:** Gracz zamknął przeglądarkę i nie ma już linka.

**Rozwiązanie:**
- Token jest w localStorage, więc wystarczy wejść na `/battles/{battleId}/simulator`
- Jeśli localStorage został wyczyszczony, admin musi wygenerować nowy token (dodając gracza ponownie)

## Bezpieczeństwo - Checklist

- [ ] Używaj HTTPS w produkcji
- [ ] Tokeny są Guid (bardzo trudne do odgadnięcia)
- [ ] Każdy request weryfikowany na backendzie
- [ ] Gracz nie może modyfikować cudzych zasobów
- [ ] Admin może monitorować wszystkie akcje
- [ ] Rate limiting na API (opcjonalnie)

## Przydatne Komendy

### Sprawdzenie tokenu w konsoli przeglądarki
```javascript
console.log(localStorage.getItem('authToken'));
console.log(localStorage.getItem('fractionId'));
```

### Wylogowanie (czyszczenie sesji)
```javascript
localStorage.removeItem('authToken');
localStorage.removeItem('fractionId');
localStorage.removeItem('battleId');
```

### Test API z curl
```bash
curl -X POST http://localhost:5000/api/battles/{battleId}/fractions/{fractionId}/ships \
  -H "Content-Type: application/json" \
  -H "X-Auth-Token: 7c9e6679-7425-40de-944b-e07fc1f90ae7" \
  -d '{"name":"Cruiser","type":"Cruiser",...}'
```
