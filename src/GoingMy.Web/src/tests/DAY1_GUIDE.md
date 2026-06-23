# 🎯 Playwright Day 1 Practice Guide

Welcome! This guide walks you through your first Playwright tests step by step.

---

## 📋 Prerequisites

Ensure you have:
- ✅ Node.js 18+
- ✅ Playwright installed (run: `npm install` from `src/GoingMy.Web`)
- ✅ The app running (see **Start Services** below)

---

## 🚀 Quick Start

### Step 1: Start the Backend Services

Open a **new terminal** and run:

```powershell
cd src
dotnet run --project GoingMy.AppHost
```

Wait for all services to be healthy (watch the Aspire Dashboard at `http://localhost:17277`).

### Step 2: Start the Frontend Dev Server

Open **another terminal** from `src/GoingMy.Web`:

```bash
npm start
```

Wait for Angular to compile and serve at `http://localhost:4200`. You should see the app redirect to auth automatically.

### Step 3: Run Playwright Tests

From `src/GoingMy.Web`, run:

```bash
npm run e2e
```

**What happens:**
- Playwright will launch a browser automatically
- Each test will execute in sequence
- Evidence (screenshots, videos, traces) will be saved to `test-results/`
- An HTML report will be generated

### Step 4: View Results

After tests finish, view the HTML report:

```bash
npm run e2e:report
```

This opens a beautiful interactive report showing:
- ✅ Passed/failed tests
- 📸 Screenshots at each step
- 🎥 Video recordings of failures
- 🔍 Detailed trace files (Chrome DevTools protocol)

---

## 🧪 Test Breakdown

### Test 1: App Loads and Initiates OAuth

**File:** `example.spec.ts` → `should load homepage and detect unauthenticated state`

**What it tests:**
1. Navigate to app root (`/`)
2. App detects user is not authenticated
3. App auto-redirects to OAuth flow (`/signin-oidc` or auth server)

**Why it matters:**
- Tests your app's **auth guard logic** without needing credentials
- Shows **browser navigation** and **URL assertion**
- Demonstrates **screenshot capture** for evidence

**Key concepts:**
```typescript
await page.goto('/')                               // Navigate
await page.screenshot({ path: '...' })            // Capture state
await expect(page).toHaveURL(/signin-oidc/, {...}) // Assert URL
```

---

### Test 2: Dashboard Route Redirects

**File:** `example.spec.ts` → `should redirect /dashboard to auth when not logged in`

**What it tests:**
- Direct navigation to `/dashboard` (protected route)
- Auth guard redirects to auth flow

**Practice focus:**
- **Protected routes**: routes with `canActivate: [authGuard]` in `app.routes.ts`
- **Auth guards**: see `src/app/guards/auth.guard.ts`

---

### Test 3: Profile Route Protection

**File:** `example.spec.ts` → `should protect /dashboard/profile route with auth guard`

**What it tests:**
- `/dashboard/profile` is nested route with auth guard
- Must redirect to auth

**Practice focus:**
- **Nested route protection**: child routes inherit parent guards
- **Deep URL access**: testing URLs 2+ levels deep

---

### Test 4: Selector Strategies

**File:** `example.spec.ts` → `selector strategy examples (educational)`

**What it teaches:**
Common selector patterns you'll use across all tests:

```typescript
// 1. Role-based (most accessible & maintainable)
page.getByRole('button', { name: 'Submit' })
page.getByRole('link', { name: 'Profile' })
page.getByRole('heading', { name: /Welcome/ })

// 2. Text-based
page.getByText('Search for creators')
page.getByLabel('Email Address')

// 3. Placeholder-based
page.getByPlaceholder('Search people, posts...')

// 4. CSS/Data attributes
page.locator('[aria-label="Change avatar"]')
page.locator('#firstName')
```

**Your app's selectors:**
- Dashboard search: `Search for creators, inspirations, and projects`
- Discover search: `Search people, posts...`
- Profile edit modal fields: `#firstName`, `#lastName`, `#bio`

---

### Test 5: Multiple Screenshots

**File:** `example.spec.ts` → `multiple screenshots throughout navigation`

**What it teaches:**
- **Checkpoint screenshots**: capture state at each step
- **Video evidence**: Playwright auto-records on failure
- **Trace files**: full browser protocol recording for debugging

**Output structure:**
```
test-results/
├── screenshots/
│   ├── 05-step1-root.png
│   ├── 05-step2-redirected.png
│   └── ...
├── videos/
│   └── example.spec.ts-should-load-homepage...
├── traces/
│   └── ...
└── index.html (report)
```

---

## 📸 Evidence Capture Strategy

Your `playwright.config.ts` is configured to:

| Mode | When | What's Saved |
|------|------|-------------|
| **Screenshots** | Test fails | Full-page PNG |
| **Video** | Test fails | Full browser recording (MP4) |
| **Trace** | Test fails | Chrome DevTools protocol (ZIP) |

**To view failures:**
1. Run tests: `npm run e2e`
2. Open report: `npm run e2e:report`
3. Click on failed test → scroll down to see evidence

---

## 🎓 Learning Path

### Day 1 Exercises (Immediate)

Run in order to build understanding:

```bash
# Just run the auth tests
npm run e2e -- --grep "should load homepage"

# Run only dashboard tests
npm run e2e -- --grep "dashboard"

# Run with headed browser (see what's happening)
npm run e2e -- --headed

# Run in debug mode (step through code)
npm run e2e -- --debug
```

### Day 2: Add Login Flow (Next Step)

Once you're comfortable with the above, practice:

1. **Mock login**: Store auth tokens in browser storage
2. **Test authenticated routes**: access `/dashboard/home` after login
3. **Test UI interactions**: search, like, comment (requires real data)

---

## 🐛 Debugging Tips

### Run Tests in Headed Mode (See Browser)

```bash
npm run e2e -- --headed
```

### Run Single Test

```bash
npm run e2e -- --grep "should load homepage"
```

### Debug Mode (Step Through)

```bash
npm run e2e -- --debug
```

Opens Playwright Inspector with pauses on each step.

### Generate Trace for Failed Test

Already enabled in `playwright.config.ts`. View with:

```bash
npx playwright show-trace test-results/traces/example-spec-name.zip
```

### View Test Report (Always)

```bash
npm run e2e:report
```

---

## 🔗 Key Selectors in Your App

From your codebase (`src/app/`):

| Component | Selector | Notes |
|-----------|----------|-------|
| Dashboard Header | `[placeholder="Search for creators..."]` | Main search |
| Discover Page | `[placeholder="Search people, posts..."]` | Full-text search |
| Profile Modal | `#firstName`, `#lastName`, `#bio` | Edit profile fields |
| Change Password | `#currentPassword`, `#newPassword` | Security form |
| Logout Button | `getByRole('button', { name: /logout/i })` | After login (future) |

---

## ✅ Completion Checklist

- [ ] Services running on `http://localhost:5001-7007`
- [ ] Frontend running on `http://localhost:4200`
- [ ] `npm run e2e` passes all 5 tests
- [ ] `npm run e2e:report` shows passing tests + screenshots
- [ ] You understand auth redirect behavior in your app
- [ ] You've taken custom screenshots at key steps

---

## 🚀 Next Steps

Once you complete Day 1:

1. **Create `auth.spec.ts`**: Test login flow (use stored auth state)
2. **Create `navigation.spec.ts`**: Test all dashboard routes
3. **Create `search.spec.ts`**: Test Discover search interaction
4. **Create `profile.spec.ts`**: Test profile editing, avatar upload
5. **Advanced**: Mock API responses → test UI in isolation

See `PRACTICE_ADVANCED.md` for multi-file examples.

---

## 📚 Resources

- [Playwright Docs](https://playwright.dev)
- [Selectors Guide](https://playwright.dev/docs/locators)
- [Debugging Guide](https://playwright.dev/docs/debug)
- [Best Practices](https://playwright.dev/docs/best-practices)

Good luck, and have fun! 🎯
