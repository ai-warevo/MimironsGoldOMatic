Пользовательский запрос:

"да, давай, НО, прежде чем что-либо выполнять и принимать решения - спроси меня: сначала я дам ответы и выберу решения которые меня устроят"

После уточнений пользователь утвердил решения:

- ES-first в MVP: Marten/Event Store как source of truth.
- EF оставляем только для read-моделей.
- Главный архитектурный приоритет: DDD + CQRS.
- POST /api/payouts/claim: 201 для нового создания, 200 для идемпотентного дубля.
- GET /api/payouts/my-last при отсутствии данных: 404.
- Confirm semantics: переход в Sent при подтверждении фактической отправки (send_confirm).
- Desktop injection: PostMessage + fallback SendInput.
- Frontend state stack: Zustand обязателен.
- Объем правок: SPEC + выравнивание component docs под SPEC.
