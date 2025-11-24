# Jak naprawić problem z pustymi rolami Discord

## Problem
Użytkownik loguje się przez Discord, ale `roles` i `fractionRoles` są puste w aplikacji.

## Przyczyna
Użytkownik autoryzował aplikację Discord **ZANIM** dodaliśmy scope `guilds.members.read`. Discord cache'uje uprawnienia OAuth, więc aplikacja nie ma dostępu do ról użytkownika na serwerze.

## Rozwiązanie

### Krok 1: Odwołaj dostęp aplikacji w Discord
1. Otwórz Discord (aplikację lub przeglądarkę)
2. Kliknij **User Settings** (ikona zębatki obok nazwy użytkownika)
3. W lewym menu wybierz **Authorized Apps**
4. Znajdź aplikację "Wielka Pustka" lub "Great Void Battle" (nazwa z Discord Developer Portal)
5. Kliknij **Deauthorize** (Odwołaj dostęp)

### Krok 2: Wyczyść cache w aplikacji
1. W aplikacji wyloguj się (przycisk logout)
2. Lub otwórz DevTools (F12) → Console → wpisz:
   ```javascript
   localStorage.clear();
   location.reload();
   ```

### Krok 3: Zaloguj się ponownie
1. Kliknij "Zaloguj się przez Discord"
2. Discord zapyta o **NOWE** uprawnienia, w tym dostęp do informacji o członkostwie w serwerach
3. Zatwierdź uprawnienia
4. Aplikacja powinna teraz widzieć Twoje role

## Weryfikacja
Po zalogowaniu otwórz DevTools (F12) → Console i sprawdź:
```javascript
JSON.parse(localStorage.getItem('discord_user'))
```

Powinieneś zobaczyć:
- `roles` - lista nazw ról (np. ["gracz", "Hegemonia Titanum"])
- `fractionRoles` - lista ról frakcyjnych (np. ["Hegemonia Titanum"])
- `isAdmin` - true/false

## Dodatkowa weryfikacja w Discord Developer Portal
1. Idź do https://discord.com/developers/applications
2. Wybierz swoją aplikację
3. OAuth2 → General
4. Sprawdź czy **Scopes** zawierają:
   - `identify`
   - `email`
   - `guilds`
   - `guilds.members.read` ← TO JEST KLUCZOWE

## Jeśli nadal nie działa
Sprawdź logi backendu podczas logowania. Powinny pokazać:
```
Discord API Response Status: 200
Discord API Response Body: {"roles":["1382607945522741248","1367771541152071773"],...}
Deserialized member roles count: 2
```

Jeśli Status to 401 lub 403 - problem z uprawnieniami OAuth
Jeśli Status to 404 - użytkownik nie jest członkiem serwera Discord
