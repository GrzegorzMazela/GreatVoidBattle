# Mapowanie ról Discord - Instrukcja

## Struktura ról

### Hierarchia dostępu:
1. **Admin** - pełny dostęp do panelu administracyjnego, widzi wszystko
2. **Gracz** - podstawowa rola autoryzacyjna, dostęp do gry
3. **Role frakcji** (Hegemonia Titanum, Shimura Incorporated, Protektorat Pogranicza) - widzi tylko treści swojej frakcji + treści ogólne

### Zasady autoryzacji:
- **Panel Admina** - wymaga roli Admin
- **Symulator bitew** - wymaga roli Gracz (lub Admin)
- **Treści specyficzne dla frakcji** - widoczne tylko dla członków danej frakcji (+ Admin)
- **Admin widzi wszystko** bez ograniczeń

## Krok 1: Pobierz ID ról z Discord

### Włącz Tryb Dewelopera:
1. Otwórz Discord
2. Przejdź do **Ustawienia użytkownika** (⚙️)
3. **Zaawansowane** → włącz **Tryb dewelopera** ✅

### Pobierz ID każdej roli:
1. Przejdź do **Ustawienia Serwera** (kliknij prawym na nazwę serwera)
2. Wybierz **Role** z menu po lewej
3. Dla każdej roli (Admin, Gracz, Hegemonia Titanum, Shimura Incorporated, Protektorat Pogranicza):
   - Kliknij na rolę aby ją edytować
   - **Kliknij prawym przyciskiem** na nazwę roli u góry
   - Wybierz **Kopiuj ID**
   - Zapisz to ID

**Przykład:**
```
Admin → 1351222990276268195
Gracz → 1382607945522741248
Hegemonia Titanum → 1367771541152071773
Shimura Incorporated → 1367771982812287026
Protektorat Pogranicza → 1367772143227633706
```

## Krok 2: Zaktualizuj appsettings.json

Otwórz plik `GreatVoidBattle.Api/appsettings.json` i uzupełnij sekcję Discord:

```json
{
  "Discord": {
    "ClientId": "TWÓJ_CLIENT_ID",
    "ClientSecret": "TWÓJ_CLIENT_SECRET",
    "GuildId": "TWÓJ_SERVER_ID",
    "RedirectUri": "https://localhost:7295/api/auth/discord/callback",
    "AdminRoleId": "WKLEJ_ID_ROLI_ADMIN",
    "AdminRoleName": "Admin",
    "PlayerRoleId": "WKLEJ_ID_ROLI_GRACZ",
    "PlayerRoleName": "gracz",
    "RoleMapping": {
      "WKLEJ_ID_ROLI_ADMIN": "Admin",
      "WKLEJ_ID_ROLI_GRACZ": "gracz",
      "WKLEJ_ID_HEGEMONIA": "Hegemonia Titanum",
      "WKLEJ_ID_SHIMURA": "Shimura Incorporated",
      "WKLEJ_ID_PROTEKTORAT": "Protektorat Pogranicza"
    },
    "FractionRoles": {
      "HegemoniaTitanum": "Hegemonia Titanum",
      "ShimuraIncorporated": "Shimura Incorporated",
      "ProtektoratPogranicza": "Protektorat Pogranicza"
    }
  }
}
```

**Zamień:**
- `WKLEJ_ID_ROLI_ADMIN` → ID roli Admin (np. `1351222990276268195`)
- `WKLEJ_ID_ROLI_GRACZ` → ID roli Gracz (np. `1382607945522741248`)
- `WKLEJ_ID_HEGEMONIA` → ID roli Hegemonia Titanum
- `WKLEJ_ID_SHIMURA` → ID roli Shimura Incorporated
- `WKLEJ_ID_PROTEKTORAT` → ID roli Protektorat Pogranicza

## Krok 3: Alternatywny sposób - sprawdź logi backendu

Jeśli nie możesz skopiować ID ról, możesz je zobaczyć w logach:

1. Zrestartuj backend
2. Zaloguj się przez Discord
3. W konsoli backendu zobaczysz logi:
   ```
   User 123456789 has role IDs: 1234567890123456789, 1234567890123456790
   ```
4. Te liczby to ID Twoich ról - użyj ich w konfiguracji

## Krok 4: Testowanie

Po zaktualizowaniu konfiguracji:

1. **Zrestartuj backend** (Ctrl+C i ponownie uruchom)
2. Odśwież przeglądarkę i zaloguj się ponownie
3. Sprawdź logi backendu - powinny pokazać:
   ```
   User 123456789 has role IDs: ...
   Admin Role ID: 1234567890123456789
   Role Mapping: 1234567890123456789=Admin, ...
   Mapped role names: Admin, Hegemonia Titanum
   User 123456789 isAdmin: True
   ```

## Krok 5: Weryfikacja w aplikacji

Po zalogowaniu:
- W prawym górnym rogu powinien pojawić się Twój avatar i nazwa użytkownika
- Kliknij na avatar aby zobaczyć dropdown z rolami
- Powinieneś mieć dostęp do panelu administracyjnego

## Troubleshooting

### "Brak dostępu - Ta strona wymaga uprawnień administratora"
- Sprawdź czy ID roli Admin w konfiguracji zgadza się z ID w logach
- Sprawdź czy masz przypisaną rolę Admin na serwerze Discord
- Zrestartuj backend po zmianie konfiguracji

### "isAdmin: False" w logach
- Upewnij się że `AdminRoleId` w konfiguracji jest prawidłowe
- Sprawdź czy w logach "has role IDs" zawiera ID roli Admin
- Porównaj ID z konfiguracji z ID w logach - muszą być identyczne

### Role nie są widoczne w dropdown
- Sprawdź czy `RoleMapping` zawiera wszystkie ID ról
- Sprawdź czy nazwy w `RoleMapping` zgadzają się z nazwami w `FractionRoles`
