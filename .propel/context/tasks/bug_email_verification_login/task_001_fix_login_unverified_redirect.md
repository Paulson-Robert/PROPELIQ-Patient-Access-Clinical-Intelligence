# Bug Fix Task - BUG_EMAIL_VERIFICATION_LOGIN

## Bug Report Reference

- Bug ID: BUG_EMAIL_VERIFICATION_LOGIN
- Source: User report — "email verification on login is not working properly"

## Bug Summary

### Issue Classification

- **Priority**: High [SOURCE:INFERRED] Basis: Core login feature broken for all unverified users — no actionable recovery path
- **Severity**: Major UX degradation on primary authentication path [SOURCE:INFERRED] Basis: Users hit a dead-end error; backend functions correctly
- **Affected Version**: Commit `129588b` (origin/us_013_task_auth) [SOURCE:INPUT] Basis: Single commit introduced all affected auth files
- **Environment**: All browsers; frontend React app (Vite + React Router); both mock and real API modes

### Steps to Reproduce

1. Register a new account via `/auth/login` → "Create account" tab with a valid email and password
2. Do NOT click the email verification link
3. Switch to "Log in" tab and submit login with the registered email and password
4. **Expected**: User is redirected to `/auth/verify?status=pending&email={email}` with an option to resend the verification email
5. **Actual**: User sees a red error banner "Please verify your email before logging in." with no link, no redirect, and no way to resend the verification email

**Error Output**:

```text
HTTP 423 Locked
{
  "code": "account_not_verified",
  "message": "Please verify your email before logging in."
}

Frontend treats this identically to HTTP 401/500 — displays message as generic error text.
No navigation or actionable UI is presented.
```

### Root Cause Analysis

- **File**: `frontend/src/services/authApi.ts:190-207`
- **Component**: `authApi.login()` function
- **Function**: `login` — real API path
- **Cause**: The `authApi.login()` function calls `postJson('/api/auth/login', payload)`. The shared `postJson` helper (line 64-80) treats ALL non-OK HTTP responses identically: extract `message` from JSON body, throw `ApiError(message, status)`. There is no special handling for HTTP 423, so it is caught by the generic error handler in `useAuth.ts:loginWithPassword` (line 94-107), which dispatches `{ type: 'failure' }` and re-throws. `LoginPage.tsx:onLoginSubmit` (line 53-56) does not catch this error at all — it propagates to the `useAuth` error state and is displayed as a red error banner. The user is never redirected to the verification page. [SOURCE:INPUT] Basis: Code trace of the complete login error flow

Secondary contributing factor: `mockAuthApi.login()` (authApi.ts:128-142) always returns success regardless of email — there is no mock simulation of the unverified email scenario, making it impossible to catch this bug during frontend-only development. [SOURCE:INFERRED] Basis: Mock bypasses all error paths

### Impact Assessment

- **Affected Features**: Login flow for locally-registered unverified users; user onboarding/activation journey (register → verify → first login); re-login after session expiry for users who registered but haven't verified [SOURCE:INFERRED] Basis: All paths through `authApi.login()` with an unverified account
- **User Impact**: 100% of new locally-registered users during the window between registration and email verification. Users see a dead-end error with no actionable next step — no link to resend, no redirect to verification page. Likely causes user abandonment. [SOURCE:INFERRED] Basis: No alternative path exists in current UI
- **Data Integrity Risk**: No [SOURCE:INPUT] Basis: Backend correctly prevents unverified logins; no data mutation occurs
- **Security Implications**: None [SOURCE:INPUT] Basis: Backend enforcement is correct; issue is UX-only on the frontend

## Fix Overview

Export `ApiError` from `authApi.ts` to allow consumers to inspect HTTP status codes. In `LoginPage.tsx`, catch the login error in `onLoginSubmit`, detect `ApiError` with status 423, and navigate to `/auth/verify?status=pending&email={loginEmail}` instead of displaying a generic error. Add mock simulation for unverified email login to enable frontend-only testing. [SOURCE:INFERRED] Basis: Minimal change that handles the 423 response specifically and redirects to the existing verification page

## Fix Dependencies

- No new packages required [SOURCE:INPUT] Basis: All needed components exist (React Router `navigate`, `ApiError` class, verification page)
- Existing `EmailVerificationPage` at `/auth/verify` handles `?status=pending&email=xxx` query params and provides "Resend verification email" functionality

## Impacted Components

### Frontend — Authentication Layer

- `frontend/src/services/authApi.ts` — Export `ApiError` class; add unverified email mock scenario (updated)
- `frontend/src/pages/auth/LoginPage.tsx` — Handle 423 error with redirect to verification page (updated)

## Expected Changes

| Action | File Path                               | Description                                                                                                                                 |
| ------ | --------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| MODIFY | `frontend/src/services/authApi.ts`      | Export `ApiError` class for external consumers; add unverified email simulation in `mockAuthApi.login()` for emails containing "unverified" |
| MODIFY | `frontend/src/pages/auth/LoginPage.tsx` | Import `ApiError`; wrap `onLoginSubmit` to catch 423 errors and `navigate` to `/auth/verify?status=pending&email={loginEmail}`              |

> Only list concrete, verifiable file operations. No speculative directory trees.

## Implementation Plan

1. **Export `ApiError` from `authApi.ts`**: Add `export` keyword to the existing `ApiError` class declaration (line 44). No structural change needed — the class is already well-defined with `status` and `message` properties.

2. **Add mock unverified email simulation**: In `mockAuthApi.login()` (lines 128-142), add a check: if the email contains "unverified", throw an `ApiError` with status 423 and the same message the backend returns. This enables frontend-only testing of the fix.

3. **Handle 423 in `LoginPage.tsx`**: In the `onLoginSubmit` handler (line 53-56), wrap the `loginWithPassword` call in a try/catch. In the catch block, check if the error is an `ApiError` with status 423. If so, call `navigate('/auth/verify?status=pending&email={encodeURIComponent(loginEmail)}')` and return. Otherwise, let the existing error handling in `useAuth` take effect (it already dispatches the error to state).

4. **Verify existing verification page**: Confirm that `EmailVerificationPage` at `/auth/verify` correctly renders the "pending" state with the resend button when `?status=pending&email=xxx` is in the URL. (Already verified during analysis — it does.)

## Regression Prevention Strategy

- [ ] Unit test: Login with unverified email (mock mode with "unverified" email) triggers navigation to `/auth/verify` with correct query params
- [ ] Unit test: Login with valid credentials still navigates to dashboard (no regression on happy path)
- [ ] Unit test: Login with wrong password still shows generic error (no false redirect to verification page)

## Rollback Procedure

1. Revert changes to `authApi.ts` and `LoginPage.tsx` — no backend changes are involved
2. Verify login flow returns to showing the generic error message for unverified accounts

## External References

- React Router `useNavigate` — used for programmatic navigation on 423 detection
- Backend `AuthController.Login` — returns HTTP 423 with `code: "account_not_verified"` for unverified users (no changes needed)

## Build Commands

```bash
cd frontend
pnpm run build
pnpm run test
```

## Implementation Validation Strategy

- [ ] Bug no longer reproducible — login with unverified email redirects to verification page with resend option
- [ ] All existing tests pass — `pnpm run test` exits with 0
- [ ] New regression tests pass — unverified login redirect, valid login happy path, wrong password error

## Implementation Checklist

- [x] Export `ApiError` class from `frontend/src/services/authApi.ts`
- [x] Add unverified email mock scenario in `mockAuthApi.login()`
- [x] Import `ApiError` in `LoginPage.tsx`
- [x] Add 423 detection and redirect in `onLoginSubmit` handler
- [x] Run `pnpm run build` — confirm no compilation errors
- [x] Run `pnpm run test` — confirm all tests pass
- [ ] Manual verification: register → login without verifying → confirm redirect to verification page
