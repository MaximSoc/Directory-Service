# Directory Service

Directory Service - учебно-портфолио проект для управления организационной структурой компании. Система хранит справочники подразделений, должностей и локаций, позволяет работать с иерархией отделов, прикреплять медиа к подразделениям и предоставляет веб-интерфейс для ежедневной административной работы.

Проект сделан как набор сервисов: основной DirectoryService отвечает за доменную модель оргструктуры, FileService - за загрузку и хранение медиафайлов, клиентское приложение на Next.js предоставляет UI, а инфраструктура поднимается через Docker Compose.

## Возможности

- Управление подразделениями: создание, редактирование, удаление, перенос в другое место иерархии, просмотр корневых узлов и дочерних подразделений.
- Работа с должностями: CRUD, фильтрация, пагинация и привязка к подразделениям.
- Работа с локациями: CRUD, адреса, часовые пояса и привязка локаций к подразделениям.
- Медиа для подразделений: загрузка видео через отдельный FileService и сохранение ссылки на медиа в DirectoryService.
- Древовидный и списочный UI для навигации по оргструктуре.
- Единый reverse proxy через Nginx для frontend, DirectoryService, FileService и MinIO storage.
- Наблюдаемость через Serilog и Seq.

## Архитектура

```text
client/                         Next.js frontend
DirectoryService/
  src/
    DirectoryService.Domain      доменные сущности и value objects
    DirectoryService.Application use cases / handlers
    DirectoryService.Infrastructure EF Core, PostgreSQL, Redis, repositories
    DirectoryService.Contracts   DTO и request/response contracts
    DirectoryService.Presentation ASP.NET Core Web API
FileService/
  FileService.Domain             модель медиа и value objects
  FileService.Core               file use cases и minimal API endpoints
  FileService.Infrastructure.*   PostgreSQL и S3/MinIO adapters
  FileService.Communication      HTTP client для интеграции сервисов
SharedService/                   общие envelope, errors, endpoints, middleware
```

Backend разделен по слоям в стиле Clean Architecture: доменная модель не зависит от инфраструктуры, сценарии приложения оформлены отдельными handlers, а внешние зависимости подключаются через инфраструктурные проекты. Для ошибок используется единый envelope/result-подход, что упрощает обработку ответов на клиенте.

## Технологии

**Backend**

- ASP.NET Core 9
- Entity Framework Core 9
- PostgreSQL
- Redis и HybridCache
- FluentValidation
- Serilog + Seq
- Swagger / OpenAPI
- Docker

**File storage**

- S3-compatible storage
- MinIO для локального окружения
- Multipart upload flow для крупных файлов

**Frontend**

- Next.js 16
- React 19
- TypeScript
- TanStack Query
- Zustand
- Axios
- Tailwind CSS 4
- Radix UI / shadcn-style components

**Тестирование**

- xUnit
- ASP.NET Core integration testing
- Testcontainers для PostgreSQL и MinIO
- Respawn для очистки БД между интеграционными тестами

## Основные API

DirectoryService:

- `GET /api/departments` - список подразделений с фильтрацией и пагинацией.
- `GET /api/departments/roots` - корневые подразделения.
- `GET /api/departments/{parentId}/children` - дочерние подразделения.
- `POST /api/departments` - создание подразделения.
- `PUT /api/departments/{departmentId}` - обновление подразделения.
- `PUT /api/departments/{departmentId}/parent` - перенос подразделения.
- `PUT /api/departments/{departmentId}/locations` - обновление локаций подразделения.
- `PUT /api/departments/{departmentId}/video` - привязка видео.
- `GET /api/positions`, `POST /api/positions`, `PUT /api/positions/{id}`, `DELETE /api/positions/{id}` - управление должностями.
- `GET /api/locations`, `POST /api/locations`, `PUT /api/locations/{id}`, `DELETE /api/locations/{id}` - управление локациями.

FileService:

- `POST /files/upload` - загрузка файла.
- `POST /files/multipart/start` - старт multipart upload.
- `POST /files/multipart/url` - получение URL для загрузки чанка.
- `POST /files/multipart/complete` - завершение multipart upload.
- `POST /files/multipart/abort` - отмена multipart upload.
- `GET /files/{fileId}/download-url` - временная ссылка на скачивание.
- `GET /files/{mediaAssetId}` - информация о медиа.
- `POST /files/batch` - пакетное получение информации о медиа.

## Запуск

### Требования

- Docker и Docker Compose
- .NET 9 SDK
- Node.js 20+
- npm

### Инфраструктура и backend

Перед запуском заполните `.env`, если Docker build должен восстанавливать приватные NuGet-пакеты:

```env
GITHUB_USER=your-user
GITHUB_TOKEN=your-token
```

Запуск сервисов:

```bash
docker compose up --build
```

После запуска доступны:

- Nginx entrypoint: `http://localhost`
- DirectoryService: `http://localhost:8080`
- FileService: `http://localhost:9002`
- Seq: `http://localhost:8081`
- MinIO Console: `http://localhost:9001`
- PostgreSQL: `localhost:5434`

Swagger доступен в Docker/Development окружении:

- DirectoryService: `http://localhost:8080/swagger`
- FileService: `http://localhost:9002/swagger`

### Frontend

Frontend в `docker-compose.yml` сейчас закомментирован, поэтому локально его удобнее запускать отдельно:

```bash
cd client
npm install
npm run dev
```

Приложение будет доступно на `http://localhost:3000`.

## Тесты

Запуск backend-тестов:

```bash
dotnet test DirectoryService/DirectoryService.sln
dotnet test FileService/FileService.sln
```

Запуск проверки frontend:

```bash
cd client
npm run lint
```

## Что демонстрирует проект

- Проектирование backend-сервисов с разделением на Domain, Application, Infrastructure и Presentation.
- Работу с иерархическими данными и бизнес-операциями над деревом подразделений.
- Интеграцию нескольких сервисов через HTTP contracts.
- Хранение файлов в S3-compatible storage и поддержку multipart upload.
- Использование PostgreSQL, Redis, Docker Compose и observability-инструментов в локальном окружении.
- Построение frontend-приложения с современным React/Next.js стеком, кешированием запросов и управлением состоянием.
