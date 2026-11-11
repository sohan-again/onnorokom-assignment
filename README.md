# School Admission System

School admission, first come first served. A student holds a seat for a few minutes,
then confirms it by paying. If the time runs out the seat goes back to the pool.

Backend: C# / .NET + PostgreSQL. Frontend: React + Bootstrap. Everything runs with Docker.

## Run it

    docker compose up -d --build

- UI:  http://localhost:5173
  - admin portal:   http://localhost:5173/admin
  - student portal: http://localhost:5173/student
- API: http://localhost:8080
- DB:  localhost:5432 (school / school / school_admission)

The backend applies migrations on startup, so the first run creates the schema by itself.

Start from a clean database:

    docker compose down -v

## Try it

There are two portals. Open http://localhost:5173 and pick one, or go straight to the URL.

1. **Admin** (http://localhost:5173/admin): create a class, for example
   "Class 6 - Morning Shift" with 120 seats.
2. **Student** (http://localhost:5173/student): the portal asks for a 17-digit birth
   registration number before showing anything. Enter something like `20152691234567890`.
3. Apply on a class, then **Pay admission fee** or **Cancel application**.
4. Keep the admin page open in another tab: held / confirmed / available move on their own,
   no refresh needed.

## API

All requests and responses are JSON. `X-Student-Id` (17-digit birth registration number)
is required on creating a hold, confirming, cancelling and `/me/holds`.

| Method | Path | Does |
|---|---|---|
| POST | `/classes` | create a class |
| GET | `/classes` | list classes with seat counts |
| GET | `/classes/{classId}` | one class |
| POST | `/classes/{classId}/holds` | hold a seat (409 if full) |
| GET | `/holds/{holdId}` | hold status |
| POST | `/holds/{holdId}/confirm` | confirm admission |
| DELETE | `/holds/{holdId}` | cancel (204) |
| GET | `/me/holds` | my applications |

## Project layout

    backend/    ASP.NET Core API + EF Core + migrations
    frontend/   React + Vite + Bootstrap
    docker-compose.yml

## Notes

Design decisions and guesses are in [ASSUMPTIONS.md](ASSUMPTIONS.md).
AI usage is written up in [AI_USAGE.md](AI_USAGE.md).
