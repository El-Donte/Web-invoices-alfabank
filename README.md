[//]: # (# 📄 Система автоматизации формирования счетов-фактур)

[//]: # (Enterprise-решение для автоматизации приёма, агрегации, валидации и экспорта счетов-фактур на основе данных АБС. Построено по принципам микросервисной архитектуры с асинхронным обменом через Kafka и строгой изоляцией границ контекстов.)

[//]: # ()
[//]: # (---)

[//]: # ()
[//]: # (## ✨ Ключевые возможности)

[//]: # (| Функция | Описание |)

[//]: # (|---|---|)

[//]: # (| 📥 Высоконагруженный инджест | Асинхронный приём **5000–7000 проводок/мин** через Kafka с идемпотентностью и дедупликацией |)

[//]: # (| 🔗 Агрегация операций | Группировка проводок по общим атрибутам &#40;`DRF`, дата, контрагент&#41; и формирование черновиков СФ |)

[//]: # (| 📝 Жизненный цикл документа | `Draft → Validation → Confirmation → Export` с полным аудитом изменений &#40;`invoice_field_change_history`&#41; |)

[//]: # (| 🔐 RBAC и изоляция данных | 5 ролей &#40;Бухгалтер, Админ, Эквайринг, Факторинг, Налоговая&#41; с глобальными фильтрами EF Core |)

[//]: # (| 🔄 Отказоустойчивость | Outbox-паттерн, ручное управление offset'ами, Retry/DLQ, graceful shutdown |)

[//]: # (| 📊 Наблюдаемость | OpenTelemetry → Prometheus + Grafana + Tempo. Метрики TPS, P95 latency, GC, пула Npgsql |)

[//]: # ()
[//]: # (---)

[//]: # ()
[//]: # (## 🏗️ Архитектура)

[//]: # (```mermaid)

[//]: # (flowchart TD)

[//]: # (    ABS[АБС] -->|Kafka: abs.raw_transactions| ABS_SVC[AbsIntegrationService])

[//]: # (    ABS_SVC -->|Outbox / AggregationReadyEvent| KAFKA[Apache Kafka])

[//]: # (    KAFKA -->|Kafka: abs.aggregation_ready| INV_SVC[InvoiceWebService])

[//]: # (    INV_SVC -->|Kafka: invoice.confirmed| EXP_SVC[ExportService])

[//]: # (    )
[//]: # (    subgraph "Единая БД &#40;PostgreSQL&#41;")

[//]: # (        RAW[&#40;rawtransaction&#41;])

[//]: # (        AGG[&#40;aggregation_group&#41;])

[//]: # (        DRAFT[&#40;draft_invoice / lines&#41;])

[//]: # (        INV[&#40;invoice / lines / history&#41;])

[//]: # (        ERR[&#40;processing_error&#41;])

[//]: # (        RBAC[&#40;user / dept / access&#41;])

[//]: # (    end)

[//]: # (    )
[//]: # (    ABS_SVC -.-> RAW & AGG & ERR)

[//]: # (    INV_SVC -.-> DRAFT & INV & ERR & RBAC)

[//]: # (    EXP_SVC -.-> INV & ERR)

[//]: # (    )
[//]: # (    OTEL[OpenTelemetry Collector] --> PROM[Prometheus])

[//]: # (    OTEL --> TEMP[Tempo])

[//]: # (    GRAF[Grafana] --> PROM & TEMP)

[//]: # (```)

[//]: # ()
[//]: # (> 🔹 **Примечание:** Все сервисы работают с **одной БД и одной схемой `public`**, но владеют строго определёнными таблицами. Кросс-сервисные зависимости реализованы только через Kafka-события. `virtual` навигации отключены, связи настраиваются через `Fluent API`.)

[//]: # ()
[//]: # (---)

[//]: # ()
[//]: # (## 🛠️ Технологический стек)

[//]: # (| Уровень | Технологии |)

[//]: # (|---|---|)

[//]: # (| **Backend** | .NET 10 / C# 12, ASP.NET Core, EF Core 8+, Npgsql |)

[//]: # (| **Брокер** | Apache Kafka &#40;Confluent.Kafka&#41; |)

[//]: # (| **БД** | PostgreSQL 15+ &#40;PuBouncer / Multiplexing&#41; |)

[//]: # (| **Наблюдаемость** | OpenTelemetry, Prometheus, Grafana, Tempo, cAdvisor |)

[//]: # (| **Frontend** | React 18 + TypeScript, Vite, Ant Design / TanStack Table |)

[//]: # (| **DevOps** | Docker Compose, GitHub Actions, EF Core Migrations |)

[//]: # ()
[//]: # (---)

[//]: # ()
[//]: # (## 📁 Структура решения)

[//]: # (```)

[//]: # (InvoiceSystem.sln)

[//]: # (├─ src/)

[//]: # (│  ├─ App.Domain.Shared/          # POCO-сущности, Enum, ValueObjects &#40;нулевые зависимости&#41;)

[//]: # (│  ├─ App.Contracts/              # DTO, Kafka Events, Interfaces)

[//]: # (│  ├─ App.Infrastructure.Data/    # Общие DbContext, Interceptors, Repositories)

[//]: # (│  ├─ AbsIntegrationService/      # Приём проводок, агрегация, Outbox)

[//]: # (│  ├─ InvoiceWebService/          # Черновики, валидация, RBAC, UI API)

[//]: # (│  ├─ ExportService/              # Экспорт во внешнюю УЧ, Retry/DLQ)

[//]: # (│  └─ ReportingService/           # Книги продаж, реестры, Read-модели)

[//]: # (├─ App.Migrations/                # Централизованные миграции &#40;EF Core&#41;)

[//]: # (├─ tests/                         # xUnit, Testcontainers, Moq)

[//]: # (├─ observability/                 # docker-compose для OTel/Prometheus/Grafana)

[//]: # (└─ docs/                          # ТЗ, BR, Схема БД, Runbooks)

[//]: # (```)

[//]: # ()
[//]: # (---)

[//]: # ()
[//]: # (## 🚀 Быстрый старт &#40;Local&#41;)

[//]: # ()
[//]: # (### 1. Предварительные требования)

[//]: # (- `.NET SDK 10.0+`)

[//]: # (- `Docker` & `Docker Compose`)

[//]: # (- `Git`)

[//]: # ()
[//]: # (### 2. Запуск инфраструктуры)

[//]: # (```bash)

[//]: # (# PostgreSQL + Kafka + Observability Stack)

[//]: # (cd observability)

[//]: # (docker compose up -d)

[//]: # (```)

[//]: # ()
[//]: # (### 3. Применение миграций)

[//]: # (```bash)

[//]: # (# Строго последовательно!)

[//]: # (dotnet ef database update --project App.Migrations --context IngestionDbContext)

[//]: # (dotnet ef database update --project App.Migrations --context InvoicingDbContext)

[//]: # (dotnet ef database update --project App.Migrations --context ExportDbContext)

[//]: # (```)

[//]: # ()
[//]: # (### 4. Запуск сервисов)

[//]: # (```bash)

[//]: # (dotnet run --project src/AbsIntegrationService)

[//]: # (dotnet run --project src/InvoiceWebService)

[//]: # (dotnet run --project src/ExportService)

[//]: # (```)

[//]: # (🌐 **Frontend:** `http://localhost:3000`  )

[//]: # (📊 **Grafana:** `http://localhost:3001` &#40;admin/admin&#41;  )

[//]: # (🔍 **Prometheus:** `http://localhost:9090`)

[//]: # ()
[//]: # (---)

[//]: # ()
[//]: # (## ⚙️ Конфигурация &#40;`appsettings.json`&#41;)

[//]: # (```json)

[//]: # ({)

[//]: # (  "ConnectionStrings": {)

[//]: # (    "DefaultConnection": "Host=localhost;Database=invoice_system;Username=postgres;Password=secret")

[//]: # (  },)

[//]: # (  "Kafka": {)

[//]: # (    "BootstrapServers": "localhost:9092",)

[//]: # (    "GroupId": "abs-ingestion-dev",)

[//]: # (    "Topic": "abs.raw_transactions",)

[//]: # (    "BatchSize": 500,)

[//]: # (    "FlushIntervalMs": 200)

[//]: # (  },)

[//]: # (  "AggregationWorker": {)

[//]: # (    "IntervalSeconds": 30,)

[//]: # (    "BatchSize": 1000,)

[//]: # (    "MinimumTransactionsForReady": 5,)

[//]: # (    "TimeoutMinutes": 15)

[//]: # (  })

[//]: # (})

[//]: # (```)

[//]: # ()
[//]: # (---)

[//]: # ()
[//]: # (## 📊 Мониторинг и алерты)

[//]: # (| Метрика | PromQL | Alert Threshold |)

[//]: # (|---|---|---|)

[//]: # (| **TPS &#40;проводок/сек&#41;** | `rate&#40;kafka_messages_processed_total{status="success"}[1m]&#41;` | `< 80` → P2 |)

[//]: # (| **P95 создания черновика** | `histogram_quantile&#40;0.95, rate&#40;invoice_draft_creation_duration_seconds_bucket[5m]&#41;&#41;` | `> 5s` → P2 |)

[//]: # (| **Ошибки БД/мин** | `rate&#40;ef_db_errors_total[5m]&#41; * 60` | `> 0.1` → P1 |)

[//]: # (| **Лаг Kafka консьюмера** | `kafka_consumer_lag` | `> 1000` → P1 |)

[//]: # ()
[//]: # (🔹 Метрики доступны по `GET /metrics`. Трассировка → `Tempo`. Логи → `Loki` &#40;опционально&#41;.)

[//]: # ()
[//]: # (---)

[//]: # ()
[//]: # (## 🧪 Тестирование)

[//]: # (```bash)

[//]: # (# Unit-тесты &#40;в памяти&#41;)

[//]: # (dotnet test --filter "Category=Unit")

[//]: # ()
[//]: # (# Интеграционные тесты &#40;Testcontainers: PG + Kafka&#41;)

[//]: # (dotnet test --filter "Category=Integration")

[//]: # ()
[//]: # (# Проверка миграций)

[//]: # (dotnet ef migrations list --project App.Migrations)

[//]: # (```)

[//]: # ()
[//]: # (---)

[//]: # ()
[//]: # (## 📜 Бизнес-правила &#40;из ТЗ / BR&#41;)

[//]: # (| ID | Правило | Реализация |)

[//]: # (|---|---|---|)

[//]: # (| `BR-1` | Один документ на группу операций | Идемпотентность по `AggregationGroupId` + `UNIQUE` индекс |)

[//]: # (| `BR-2` | Корректный расчёт НДС и итогов | `DraftInvoiceFactory` с точностью `DECIMAL&#40;16,4&#41;`, валидация ставок |)

[//]: # (| `BR-3` | Возможность редактирования | `DraftInvoice` до подтверждения, `InvoiceFieldChangeHistory` для аудита |)

[//]: # (| `BR-4` | Проверка данных | `IValidationService` + статусы `ValidationError`, DLQ для poison pills |)

[//]: # (| `BR-5` | Формирование отчётности | `ReportingService` + `AsNoTracking&#40;&#41;` проекции, `SplitQuery` |)

[//]: # (| `BR-6` | Хранение документов | PostgreSQL + `audit_log`, срок хранения ≥ 5 лет &#40;архивация&#41; |)

[//]: # (| `BR-7` | Поиск документов | Индексы `&#40;status, issue_date&#41;`, `&#40;counterparty_id&#41;`, фильтрация по ролям |)

[//]: # (| `BR-8` | Передача в УЧ | Outbox → Kafka → `ExportService` с `retry_policy` и подтверждением |)

[//]: # ()
[//]: # (---)

[//]: # ()
[//]: # (## 🤝 Контрибьюция)

[//]: # (1. Создайте ветку `feat/your-feature` или `fix/your-fix`)

[//]: # (2. Убедитесь, что `dotnet build` и `dotnet test` проходят)

[//]: # (3. Добавьте миграции **только** в `App.Migrations`)

[//]: # (4. Создайте Pull Request с описанием изменений и ссылками на задачи)

[//]: # (5. Код-ревью проходит по чек-листу: `virtual` absent, `AsNoTracking` for reads, manual Kafka commit, OTel metrics.)
