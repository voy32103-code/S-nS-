# SànSổ — Docker Compose V14

Docker artifacts canonical:

- `docker-compose.yml` / `docker-compose.v14.yml`.
- API multi-stage Dockerfile V14.
- Migrator V8 one-shot Dockerfile.
- Frontend V8 Node build → Nginx runtime.
- PostgreSQL 16, Redis 7, healthchecks và dependency ordering.
- `.env.example` là local-only template; production secrets phải đến từ secret manager.

Chạy dự kiến:

```powershell
Copy-Item .env.example .env
# thay local passwords; không dùng các giá trị này ở production
docker compose config
docker compose up --build
```

Static `docker compose config --quiet` đã pass. Runtime chưa pass trên máy hiện tại vì Docker Desktop daemon trả `Docker Desktop is unable to start`; không có container project nào được tạo.

