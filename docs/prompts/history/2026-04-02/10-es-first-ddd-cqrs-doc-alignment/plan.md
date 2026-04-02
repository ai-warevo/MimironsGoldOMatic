## Plan

1. Обновить `docs/SPEC.md`:
   - зафиксировать ES-first MVP модель хранения;
   - зафиксировать HTTP semantics: `POST /claim` (201/200), `GET /my-last` (404);
   - зафиксировать `Sent` как подтверждение фактической отправки;
   - зафиксировать стратегию Desktop injection: PostMessage + SendInput fallback.

2. Выровнять документацию компонентов и общий docs entrypoint:
   - `docs/ReadME.md`
   - `README.md`
   - `docs/MimironsGoldOMatic.Backend/ReadME.md`
   - `docs/MimironsGoldOMatic.Desktop/ReadME.md`
   - `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`
   - `docs/MimironsGoldOMatic.Shared/ReadME.md`
   - `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`

3. Убрать формулировки, противоречащие спецификации:
   - EF как source of truth;
   - нефиксированные API-вилки;
   - конфликтный формат подтверждения;
   - устаревшие payload-примеры.

4. Заполнить `checks.md` по мере выполнения и финальный `report.md`.
5. Провести финальный consistency sweep и оформить матрицу готовности к реализации.

## Риски

- Возможны будущие расхождения с `docs/ROADMAP.md`, если там останутся старые формулировки (в рамках текущего scope не меняется).
