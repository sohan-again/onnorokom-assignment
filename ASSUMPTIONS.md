# Assumptions

The brief leaves a few things open. Here is what I picked and why.

## Domain

- **Each shift is its own class.** "Class 6 - Morning Shift" and "Class 6 - Day Shift"
  are two rows with their own seat counts, as the brief requires.
- **No login.** The 17-digit birth registration number is the identity. It is sent as
  `X-Student-Id` and kept in the browser's localStorage.
- **Payment is fake.** Pressing "Pay admission fee" confirms the hold. No gateway.
- **Cancelling an expired hold is allowed** and just marks it CANCELLED. Nothing changes
  for other students either way.

## Seats

- **Counts are calculated, not stored.** `heldSeats` = holds with status HELD and
  `expiresAt > now`. `confirmedSeats` = holds with status CONFIRMED.
  One less thing that can drift out of sync.
- **Atomic hold creation.** The class row is locked with `SELECT ... FOR UPDATE` inside
  one transaction, then seats are counted and the hold is inserted. Two requests for the
  same class queue up; different classes run in parallel. Check-then-insert is safe because
  nothing can change between the two steps. The lock lives in Postgres, so it would also
  hold up with two backend processes running.
- **One active application per student per class** is enforced by a partial unique index
  (`WHERE "Status" = 'HELD'`), not just by a code check.
- **Expiry needs no cron job.** Every query compares `expiresAt` with `now`, so an expired
  hold stops taking a seat immediately. The API also reports it as EXPIRED on read.
  Stale rows for the same student are cleaned inside the next hold transaction, so the
  unique index does not block a new application.
- **409 when there is no seat**, and also when the student already has an active
  application. I did not invent a different code for the two cases; the message says which.

## API details

- IDs look like `c_ab12cd34ef56` / `h_ab12cd34ef56`. The brief's `c_123` / `h_789`
  are examples and the format was left to me.
- The create-class response also includes the seat counts. Added fields are allowed,
  removed or renamed ones are not.
- Times are UTC, stored as `timestamptz`, returned as `...Z`.
- A missing `X-Student-Id` is a 400. A hold that belongs to another student is a 404,
  so the response does not confirm that the hold exists.

## Frontend

- **Two portals, two URLs.** `/admin` is the school authority: create classes and watch the
  seat counts. `/student` is gated - the 17-digit number must be entered before the class
  list is visible at all. `/` is just a page linking to both.
- **`/admin` has no authentication.** The brief says there is no login, so anyone with the
  URL can create a class. For a real deployment you would put an admin login in front of it.
- **Polling every 2 seconds** instead of websockets. The brief asks for "within a few
  seconds"; polling is a lot less code.
- The countdown is drawn from the server's `expiresAt` using the local clock. Even with a
  wrong clock the backend still refuses to confirm an expired hold.
- Screens are tabs in one page. No router, no state library, no custom CSS - Bootstrap only.
