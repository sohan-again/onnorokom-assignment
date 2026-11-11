# AI usage

## Tool

OpenCode (terminal coding agent, Claude-based) for most of the assignment.

## Where I used it

- Scaffolding: solution/projects, EF Core setup, Dockerfiles, compose file.
- Drafting the React components and the `fetch` wrapper.
- Reviewing my own code - it found a few things I had missed.

## Where it was wrong, and how I caught it

- **Wrong JSON field names.** It wrote `healdSeats` instead of `heldSeats` and
  `expiredAt` instead of `expiresAt`. Both compile fine and both break the contract.
  Caught by calling the endpoint with `curl` and comparing the output with the PDF examples.
- **No CORS.** The API worked from `curl` and failed from the browser with "Failed to fetch".
  Caught the first time the frontend called the API; fixed with a CORS policy for `:5173`.
- **Over-engineering.** Early answers suggested counter columns, a background expiry service
  and a retry loop for serializable transactions. I kept the simpler row-lock version:
  fewer moving parts, same guarantee.
