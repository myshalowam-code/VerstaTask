# Versta Delivery

Тестовое задание: сервис приемки заказов на доставку на ASP.NET 9 и React. 

## Чуть-чуть о выбранном решении

Я решил дать немного воли фантазии :) Возможно будет похоже на оверинжиниринг, но условимся, что заказов в будущем будет очень много, их будут постоянно читать из бд и т.д.

Решил применить такое архитектурное решение:
- ApiGateway как единая точка входа(Использовал Ocelot)
- Сервис авторизации(Примитивный) БД  Postgres 
- Сервис заказов CQRS + EventSourcing. БД Postgres + Mongo
- Сервис асинхронной синхронизации Posrgres и Mongo через kafka.
- Frontend на react
- Добавил CI


Опишу чуть детальнее:
1. Зачем тут EventSorcing(По условиям задачи выглядит избыточным). Мы храним историю изменений, тем самым сможем быстро отслеживать, что не так могло произойти с заказом и если надо, откатиться к нужной версии.
2. Зачем две БД. Из пункта выше следует, что нам надо как-то читать данные и постоянно пытаться воспроизвести все по событиям, не вариант. Для это выберем какую-нибудь NoSql бд, в нашем случае Mongo. Почему NoSql, 
а с заделом на будущее, мало ли сущность заказов будет расти, и вместо того чтобы делать кучу джойнов, лучше хранить данные в денормализованном виде. Ну и проще буде масштабировать БД, так как не факт что операций записи будет так много как чтений, 
и тогда можно масштабировать только бд которая для чтения
3. Данные решил синхронизировать асинхронно. Задержка от такого способа не сильно велика, но при большой нагрузке будет работать лучше. На стороне клиента можно обрботать по разному. 
Так как в задании про это не сказано, я решил добавит лоадер, пока заказ не появится в Mongo.
4. Для удобства рабты клиента с сервером, добавил шлюз, как единую точку входа. 


## Быстрый запуск

Понадобится только запустить команду:

```bash
docker compose up --build
```

После запуска откройте [http://localhost:5173](http://localhost:5173).

Демонстрационный пользователь:

- email: `demo@versta.local`
- пароль: `Demo123!`

Gateway доступен на `http://localhost:5080`. Для остановки выполните `docker compose down`. Чтобы также удалить локальные данные, используйте `docker compose down -v`.


## Локальная разработка

Инфраструктуру можно поднять отдельно:

```bash
docker compose up postgres mongo kafka
dotnet restore
dotnet test
```

Полный `docker compose up --build` сам применяет миграции. При запуске API вне Compose их можно выполнить отдельно:

```bash
dotnet ef database update \
  --project services/orders/src/Versta.Orders.Infrastructure \
  --startup-project services/orders/src/Versta.Orders.Api
dotnet ef database update \
  --project services/auth/src/Versta.Auth.Api
```

Backend-проекты рассчитаны на .NET SDK 9.0.300+. Frontend:

```bash
cd web
npm install
npm run dev
```

## Интеграционные тесты

Сквозные black-box тесты находятся в `tests/Versta.IntegrationTests` и запускаются одной командой:

```bash
./scripts/run-integration-tests.sh
```

Скрипт создаёт Docker Compose project и поднимает отдельные PostgreSQL, MongoDB, Kafka, migration-контейнеры, Auth API, Orders API, projection worker и Gateway. Тестовый контейнер проверяет:

- обязательность JWT для Orders API;
- отказ при неверном пароле;
- доменную валидацию команды;
- `202 Accepted` при создании заказа;
- полный асинхронный путь Event Store → Outbox → Kafka → MongoDB;
- доступность созданного заказа в detail и list query.

После успешного или неуспешного прогона `trap` выполняет `docker compose down --volumes --remove-orphans --rmi local`: контейнеры, собранные тестовые images, сеть и volumes удаляются. При ошибке перед очисткой выводятся последние логи backend-сервисов.

Integration-проект добавлен в solution-папку `Tests`. Обычный `dotnet test Versta.Delivery.sln` запускает unit-тесты, а Docker-тесты отмечает как `Skipped`, если отсутствует `GATEWAY_BASE_URL`; поэтому инфраструктура не поднимается неявно и локальная dev-база не изменяется.


## GitHub Actions

Workflow `.github/workflows/tests.yml` запускается при push, pull request и вручную. `unit-tests` и `integration-tests` выполняются параллельно на отдельных GitHub-hosted Ubuntu runners.

Unit job устанавливает версию .NET из `global.json` и запускает только доменные тесты. Integration job использует Docker и Docker Compose, предустановленные на Ubuntu runner, и вызывает тот же `scripts/run-integration-tests.sh`, что используется локально и в GitLab CI. JUnit-отчёты сохраняются как отдельные artifacts на семь дней.

При запуске всех backend-проектов вне Docker нужно заменить downstream hosts в `gateway/Versta.Gateway/ocelot.json` на `localhost` и назначить сервисам разные порты.

## API

Все запросы идут через `http://localhost:5080`:

- `POST /api/auth/login`
- `POST /api/auth/register`
- `POST /api/orders` — принять команду создания; возвращает `202 Accepted` и `orderId`, нужен Bearer token
- `GET /api/orders?limit=30&cursor=...` — страница заказов пользователя; `cursor` берётся из `nextCursor` предыдущего ответа
- `GET /api/orders/{id}` — карточка заказа

PostgreSQL, MongoDB и Kafka публикуются на стандартных портах только для удобства локальной разработки. Секрет JWT и пароли в Compose являются демонстрационными.
