# Konfiguracja Discord OAuth

## 1. Utworzenie aplikacji Discord

1. Przejdź do [Discord Developer Portal](https://discord.com/developers/applications)
2. Kliknij **"New Application"**
3. Nadaj nazwę aplikacji (np. "Great Void Battle")
4. Zaakceptuj warunki i kliknij **"Create"**

## 2. Konfiguracja OAuth2

1. W lewym menu wybierz **"OAuth2"**
2. W sekcji **"Redirects"** dodaj:
   - Development: `http://localhost:5000/api/auth/discord/callback`
   - Production: `https://your-domain.com/api/auth/discord/callback`
3. Zapisz zmiany

## 3. Pobranie danych uwierzytelniających

### Client ID i Client Secret
1. W zakładce **"OAuth2" → "General"** znajdziesz:
   - **CLIENT ID** - skopiuj ten identyfikator
   - **CLIENT SECRET** - kliknij "Reset Secret" aby wygenerować nowy (UWAGA: zapisz go, nie będzie już widoczny!)

### Guild ID (Server ID)
1. W Discord włącz **Developer Mode**:
   - Ustawienia → Zaawansowane → Tryb dewelopera
2. Kliknij prawym przyciskiem na nazwę serwera Discord
3. Wybierz **"Kopiuj ID serwera"**

## 4. Konfiguracja ról Discord

Utwórz następujące role na swoim serwerze Discord:
- `Admin` - pełen dostęp do panelu administracyjnego
- `Hegemonia Titanum` - dostęp do treści frakcji Hegemonia Titanum
- `Shimura Incorporated` - dostęp do treści frakcji Shimura Incorporated
- `Protektorat Pogranicza` - dostęp do treści frakcji Protektorat Pogranicza

## 5. Aktualizacja appsettings.json

Otwórz plik `appsettings.Development.json` i uzupełnij:

```json
{
  "FrontendUrl": "http://localhost:5173",
  "Discord": {
    "ClientId": "TWÓJ_CLIENT_ID",
    "ClientSecret": "TWÓJ_CLIENT_SECRET",
    "GuildId": "TWÓJ_SERVER_ID",
    "RedirectUri": "http://localhost:5000/api/auth/discord/callback",
    "AdminRoleName": "Admin",
    "FractionRoles": {
      "HegemoniaTitanum": "Hegemonia Titanum",
      "ShimuraIncorporated": "Shimura Incorporated",
      "ProtektoratPogranicza": "Protektorat Pogranicza"
    }
  }
}
```

W produkcji zaktualizuj `appsettings.json`:
```json
{
  "FrontendUrl": "https://your-domain.com",
  "Discord": {
    "ClientId": "TWÓJ_CLIENT_ID",
    "ClientSecret": "TWÓJ_CLIENT_SECRET",
    "GuildId": "TWÓJ_SERVER_ID",
    "RedirectUri": "https://your-domain.com/api/auth/discord/callback",
    ...
  }
}
```

## 6. Scope OAuth2

Aplikacja wymaga następujących scope:
- `identify` - podstawowe informacje o użytkowniku
- `email` - adres email użytkownika
- `guilds.members.read` - role użytkownika na serwerze

Te scope są automatycznie konfigurowane w `DiscordService.cs`.

## 7. Testowanie

1. Uruchom backend: `dotnet run` w folderze `GreatVoidBattle.Api`
2. Uruchom frontend: `npm run dev` w folderze `battle-app-admin`
3. Otwórz przeglądarkę na `http://localhost:5173`
4. Kliknij "Zaloguj przez Discord"
5. Zaloguj się na Discord i autoryzuj aplikację
6. Powinieneś zostać przekierowany z powrotem do aplikacji

## 8. Mapowanie ról

### Backend (C#)
Role są automatycznie mapowane przez `DiscordAuthMiddleware`:
- Dodawane jako Claims do `HttpContext.User`
- Sprawdzane przez `DiscordAuthAttribute`

### Frontend (React)
Role są przechowywane w `localStorage`:
```javascript
const session = getDiscordSession();
console.log(session.user.roles); // Array ról
console.log(session.user.isAdmin); // boolean
console.log(session.user.fractionRole); // string | null
```

## 9. Zabezpieczanie endpointów

### Backend
```csharp
[DiscordAuth(requireAdmin: true)]
public async Task<IActionResult> AdminOnlyEndpoint()
{
    // Tylko dla adminów
}

[DiscordAuth(allowedRoles: new[] { "Hegemonia Titanum" })]
public async Task<IActionResult> FractionSpecificEndpoint()
{
    // Tylko dla określonej frakcji (lub adminów)
}
```

### Frontend
```jsx
<ProtectedRoute requireAdmin={true}>
  <AdminPanel />
</ProtectedRoute>

<ProtectedRoute allowedRoles={["Hegemonia Titanum"]}>
  <FractionContent />
</ProtectedRoute>
```

## 10. Wylogowanie

```javascript
import { clearDiscordSession } from './services/discordAuthApi';

const handleLogout = () => {
  clearDiscordSession();
  navigate('/login');
};
```

## 11. Troubleshooting

### "Invalid redirect_uri"
- Sprawdź czy URL callback w appsettings zgadza się z konfiguracją w Discord Developer Portal
- Upewnij się że dodałeś redirect URI w OAuth2 → Redirects

### "Invalid client credentials"
- Sprawdź Client ID i Client Secret
- Upewnij się że nie ma spacji ani dodatkowych znaków

### "User has no roles"
- Sprawdź czy bot ma uprawnienia `guilds.members.read`
- Sprawdź czy GuildId jest prawidłowe
- Upewnij się że użytkownik ma przypisane role na serwerze

### "CORS errors"
- Sprawdź konfigurację CORS w `Program.cs`
- Upewnij się że FrontendUrl jest poprawnie ustawiony
