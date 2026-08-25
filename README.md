# Vehicle Finder

A web application for looking up vehicle models by make, model year, and vehicle type, backed by the NHTSA vPIC API.

## Project Overview

Vehicle Finder lets a user pick a car make, a model year, and a vehicle type, and see the matching vehicle models. Make and vehicle-type data come from the [NHTSA vPIC API](https://vpic.nhtsa.dot.gov/api/); the application adds validation, error handling, deduplication, and a clean UI on top of it.

## Features

- Browse all vehicle makes (12,000+), with a fast client-side searchable dropdown
- Vehicle types load automatically once a make is selected
- Search vehicle models by make + model year + vehicle type
- Server-side validation (make ID, year range, vehicle type) with clear 400 responses
- Friendly loading, empty-result, and error states throughout the UI, with retry
- Responsive layout (desktop/tablet/mobile)
- Swagger/OpenAPI docs for the backend

## Architecture

```
React (Vite)  →  ASP.NET Core Web API  →  NHTSA vPIC API
```

The frontend never calls NHTSA directly — it only talks to our own backend. The backend acts as an integration layer:

- **Stable contract for the frontend.** NHTSA's raw responses are inconsistent (e.g. `Make_ID`/`Make_Name` for makes vs. `VehicleTypeId`/`VehicleTypeName` for vehicle types, results wrapped in a `Count`/`Message`/`Results` envelope, fields that are only present for some queries). The backend maps all of this into a small set of clean DTOs (`MakeDto`, `VehicleTypeDto`, `VehicleModelDto`) so the frontend only ever deals with `{ id, name }`-shaped data.
- **A single place for validation and error handling.** Invalid input (bad make ID, out-of-range year, empty vehicle type) is rejected with a clean `400` before any call to NHTSA. NHTSA failures (timeout, non-2xx, malformed JSON) are translated into distinct `502`/`503`/`504` responses instead of leaking raw exceptions to the browser.
- **No CORS entanglement with a third-party domain.** The browser only ever talks to our own origin; the backend is the one making outbound calls to NHTSA.

## Technology Stack

**Backend:** ASP.NET Core 8 Web API (C#), Controllers, `IHttpClientFactory` typed client, DTOs, Swagger/Swashbuckle, `ILogger`, `ProblemDetails`, xUnit.

**Frontend:** React 19, TypeScript, Vite, plain CSS (no UI framework), native `fetch` in a small API service layer.

**Infra:** Docker (multi-stage builds), Docker Compose, nginx (serves the frontend and reverse-proxies `/api` to the backend).

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) and npm
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (only needed for the Docker workflow)

## Running Locally

### Backend

```bash
cd backend/VehicleFinder.Api
dotnet restore
dotnet run
```

The API starts at **http://localhost:5024** (see `Properties/launchSettings.json`). Swagger UI is at `http://localhost:5024/swagger`.

### Frontend

In a separate terminal:

```bash
cd frontend
npm install
npm run dev
```

The app starts at **http://localhost:5173**. The Vite dev server proxies `/api/*` requests to `http://localhost:5024`, so the frontend never needs to know the backend's URL directly, and no CORS setup is required for local dev.

> If you run the backend on a different port, update the `server.proxy` target in `frontend/vite.config.ts` to match.

## Running With Docker

From the repository root:

```bash
docker compose up --build
```

- Frontend: **http://localhost:3000**
- Backend / Swagger: **http://localhost:5080/swagger**

The frontend container (nginx) proxies `/api/*` to the backend container over the Docker Compose network (`http://backend:8080/api/`), exactly like the Vite dev proxy does locally — the frontend code is identical in both environments; only the proxy target changes.

To stop: `docker compose down`.

## API Endpoints

All endpoints are under `/api/vehicles`.

| Method | Route                                                             | Description                                               |
| ------ | ----------------------------------------------------------------- | --------------------------------------------------------- |
| GET    | `/api/vehicles/makes`                                             | All vehicle makes, deduplicated and sorted alphabetically |
| GET    | `/api/vehicles/makes/{makeId}/types`                              | Vehicle types available for a given make                  |
| GET    | `/api/vehicles/models?makeId={id}&year={year}&vehicleType={type}` | Vehicle models matching make + year + vehicle type        |

Full interactive documentation is available via Swagger UI when running the backend (see above).

### Error responses

Errors use [RFC 9457 ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457). Invalid input returns `400`; upstream NHTSA failures return `502` (malformed response), `503` (unavailable), or `504` (timeout) — never a raw exception message.

## Configuration

Backend configuration (`appsettings.json`, overridable via environment variables using the standard ASP.NET Core convention, e.g. `Nhtsa__BaseUrl`):

| Key                    | Default                           | Purpose                                  |
| ---------------------- | --------------------------------- | ---------------------------------------- |
| `Nhtsa:BaseUrl`        | `https://vpic.nhtsa.dot.gov/api/` | Base URL for the NHTSA vPIC API          |
| `Nhtsa:TimeoutSeconds` | `15`                              | Timeout for outbound NHTSA requests      |
| `Cors:AllowedOrigins`  | `["http://localhost:5173"]`       | Origins allowed to call the API directly |

## Testing

```bash
cd backend
dotnet test
```

20 xUnit tests covering:

- Mapping, deduplication, sorting, and caching for makes, vehicle types, and models (`VehicleServiceTests`)
- NHTSA HTTP failure translation — non-success status, malformed JSON, timeout (`NhtsaClientTests`, using a fake `HttpMessageHandler`, no real network calls)
- Input validation and NHTSA-failure-to-HTTP-status mapping at the controller level (`VehiclesControllerTests`)

## AWS Deployment

**Recommendation: a single EC2 instance (Free Tier eligible) running Docker Compose.**

| Option            | Cost                                                                            | Setup effort | Notes                                                                                                                                                                                           |
| ----------------- | ------------------------------------------------------------------------------- | ------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **EC2 (chosen)**  | $0 on Free Tier (t2/t3.micro, 750 hrs/month for 12 months)                      | Low–medium   | Runs our existing `docker-compose.yml` unchanged — no code changes needed                                                                                                                       |
| Elastic Beanstalk | Small (EB itself is free, but provisions an EC2 + load balancer under the hood) | Medium       | More moving parts than we need for one small app                                                                                                                                                |
| App Runner        | ~$5–15/month, always billed while running (no scale-to-zero)                    | Low          | Very easy, but each service needs its own image/URL, and our nginx proxy would need to target a public backend URL instead of a Docker-network hostname — extra wiring for no real benefit here |

Given the assignment's priority on minimal/free cost first and Docker compatibility, EC2 running our existing Compose file as-is is the simplest fit: no changes to `docker-compose.yml`, `Dockerfile`s, or `nginx.conf` are needed.

### Steps

1. **Launch an instance**: EC2 console → Launch Instance → Amazon Linux 2023, `t3.micro` (or `t2.micro`, whichever is Free Tier eligible in your region) → create/select a key pair.
2. **Security group**: allow inbound TCP `22` (SSH, restrict to your IP), `3000` (frontend), and optionally `5080` (direct API/Swagger access).
3. **Connect and install Docker**:
   ```bash
   ssh -i your-key.pem ec2-user@<public-ip>
   sudo dnf install -y docker
   sudo systemctl enable --now docker
   sudo usermod -aG docker ec2-user
   # log out and back in for the group change to apply, then:
   docker compose version || sudo dnf install -y docker-compose-plugin
   ```
4. **Get the code onto the instance**:
   ```bash
   git clone <your-repo-url>
   cd <repo-folder>
   ```
5. **Run it**:
   ```bash
   docker compose up -d --build
   ```
6. **Access it**: `http://<ec2-public-ip>:3000`.
7. **(Optional) Elastic IP**: attach one so the public address doesn't change on instance restart.
8. **(Optional) HTTPS/custom domain**: not set up by default — for a production deployment, put the instance behind a reverse proxy with a Let's Encrypt certificate, or in front of CloudFront/an Application Load Balancer with an ACM certificate.

No AWS resources have been provisioned as part of this work — the steps above are ready to run when you choose to deploy.

## Live Application

Live Application: TBD
