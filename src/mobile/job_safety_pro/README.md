# Job Safety Pro — Flutter Mobile App

Feature-based Clean Architecture mobile client for the Job Safety Pro platform.

## Architecture

```
lib/
├── main.dart / app.dart
├── core/                    # Config, network, storage, theme, router
├── shared/                  # Shared models, widgets, enums
└── features/
    ├── auth/                # data → domain → presentation
    ├── dashboard/
    ├── jsa/                 # Job Safety Assessment + 6-step workflow
    ├── profile/
    ├── settings/
    ├── sync/                # Offline sync queue
    ├── notifications/
    ├── scanner/             # QR scanning
    └── camera/              # Photo capture
```

## Packages

- `flutter_riverpod` — state management
- `go_router` — navigation
- `dio` — HTTP + JWT refresh interceptor
- `hive` — offline drafts & sync queue
- `flutter_secure_storage` — token storage
- `image_picker` / `camera` — photo capture
- `signature` — digital sign-off
- `mobile_scanner` — QR codes
- `flutter_local_notifications` — push/local alerts
- `connectivity_plus` — offline detection

## Assessment Workflow

1. Observe & Stop
2. Start Conversation
3. Identify Hazards (+ photo capture)
4. Assess Risks
5. Select Controls
6. Sign Off (+ digital signature)

## Run

This is the **Flutter mobile app**. Do not run `flutter run` from `src/web/safety-manager-portal` (that folder is the React manager dashboard).

**1. Start the API** (separate terminal):

```bash
cd src/JobSafetyPro/JobSafetyPro.API
dotnet run
```

API listens on `http://localhost:5101` (and `https://localhost:7130`).

**2. Run the mobile app** (iOS Simulator):

```bash
cd src/mobile/job_safety_pro
flutter pub get
flutter run
```

Default API URL: `http://127.0.0.1:5101/api/v1` (works on iOS Simulator).

For **Android emulator**, use:

```bash
flutter run --dart-define=API_BASE_URL=http://10.0.2.2:5101/api/v1
```

Demo login: `admin@jsp.demo` / `Admin@123`

## Safety Manager Web Portal (React)

```bash
cd src/web/safety-manager-portal
npm install
npm run dev
```

Open http://localhost:5173 — this is **not** the mobile app.

## Offline First

- Assessment drafts saved to Hive immediately
- Sync queue for submissions when offline
- Cached assessment history when disconnected
- Offline banner on dashboard
