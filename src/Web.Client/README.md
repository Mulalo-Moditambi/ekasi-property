# Ekasi Property — Web Client

React + TypeScript + Vite frontend for the Ekasi Property marketplace: backrooms and
cottages to rent, houses to buy — built for the township market.

## Run it

The dev server proxies `/api/*` to the Web.Api project at `http://localhost:5000`,
so start the backend first:

```bash
# from the repo root
docker compose up -d mssql        # SQL Server
dotnet run --project src/Web.Api  # API on http://localhost:5000

# then, in this folder
npm install
npm run dev                       # http://localhost:5173
```

## Structure

- `src/api/` — thin fetch client (JWT bearer + ProblemDetails handling) and per-resource API modules
- `src/auth/` — auth context: token storage, login/logout, current user id (from the JWT `sub` claim)
- `src/types/` — TypeScript mirrors of the backend enums and response DTOs
- `src/pages/` — Search (public), Property detail (public, with owner management actions), Create listing, Login, Register
- `src/components/` — shared layout and presentational components
