# Safety Manager Portal

Enterprise React portal for Job Safety Pro safety managers.

## Stack

- React 18 + Vite
- Material UI + MUI DataGrid
- React Query
- Axios (JWT + refresh token)
- React Router

## Setup

```bash
cd src/web/safety-manager-portal
npm install
npm run dev
```

Ensure the API is running at `http://localhost:5101`.

Optional `.env`:

```
VITE_API_URL=http://localhost:5101/api/v1
```

## Login

| Account | Password | Role | Best for |
|---------|----------|------|----------|
| `safety.manager@jsp.demo` | `Admin@123` | Safety Manager | Injury capture & register |
| `safety.officer@jsp.demo` | `Admin@123` | Safety Officer | Injury capture & register |
| `hse.manager@jsp.demo` | `Admin@123` | HSE Manager | JSAs, near miss investigations |
| `admin@jsp.demo` | `Admin@123` | Administrator | Full access, add employees |

Requires **Safety Lead** role (Administrator, HSE Manager, Safety Manager, or Safety Officer).

## Features

- JWT login with Remember Me, show/hide password, route guards
- Enterprise sidebar layout (responsive)
- Dashboard with Injury Free Days and safety KPIs
- Employee Management with search, department/occupation filters, DataGrid
- Employee detail tabs (Profile, Assessments, Near Misses, Corrective Actions, Timeline)
- Injury Register (list, capture, detail)

## API Endpoints Used

- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh`
- `GET /api/v1/employees/search`
- `GET /api/v1/employees/{id}`
- `GET /api/v1/employees/stats`
- `GET /api/v1/safety-kpis/manager`
- `GET /api/v1/dashboard/injury-free-days`
- `GET /api/v1/injuries`
