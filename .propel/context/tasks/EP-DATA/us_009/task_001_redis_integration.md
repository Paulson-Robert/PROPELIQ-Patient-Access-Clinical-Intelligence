# Task - TASK_001

## Requirement Reference

- **User Story:** US_009
- **Story Location:** .propel/context/tasks/EP-DATA/us_009/us_009.md
- **Acceptance Criteria:**
  - AC-01: Redis connection established and health-checked on startup
  - AC-02: Session tokens cached in Redis with 15-minute sliding expiry
  - AC-03: Slot lock acquires and releases with 30-second TTL
  - AC-04: Rate-limiting counters enforce per-endpoint limits with HTTP 429
  - AC-05: Redis quota fallback to in-memory cache
- **Edge Cases:**
  - Redis connection failure on startup — degraded mode with in-memory fallback
  - Slot lock contention — only first SETNX succeeds
  - TTL drift — validate 30-second locks under network latency

---

## Applicable Technology Stack

| Layer   | Technology          | Version       | Justification                                     |
| ------- | ------------------- | ------------- | ------------------------------------------------- |
| Cache   | Upstash Redis       | Serverless    | DR-002 — session cache, slot locks, rate limiting |
| Backend | ASP.NET Core        | 9.0           | TR-002 — DI and middleware pipeline               |
| Library | StackExchange.Redis | Latest stable | DR-002 — .NET Redis client                        |

---

## Task Overview

Integrate Upstash Redis into the backend for session caching with sliding expiry, distributed slot locking with TTL-based optimistic concurrency, rate-limiting counters, and graceful fallback to in-memory cache when Redis is unavailable or quota is exhausted.

## Dependent Tasks

- US_002 task (Backend API Scaffold) — backend solution must exist for Redis service registration

## Impacted Components

- Infrastructure/Caching/RedisConnectionFactory.cs — Redis connection management
- Infrastructure/Caching/RedisCacheService.cs — cache abstraction implementation
- Infrastructure/Caching/InMemoryCacheService.cs — fallback implementation
- Infrastructure/Locking/RedisDistributedLockService.cs — slot lock service
- Infrastructure/RateLimiting/RedisSlidingWindowCounter.cs — rate limiting
- Application/Interfaces/ICacheService.cs — cache abstraction interface
- Application/Interfaces/IDistributedLockService.cs — lock abstraction interface
- API/Program.cs — service registration and health check configuration

## Implementation Plan

1. Create ICacheService and IDistributedLockService interfaces in Application layer
2. Implement RedisConnectionFactory with Upstash connection string and TLS
3. Implement RedisCacheService with sliding expiry (15-minute TTL reset on access)
4. Implement RedisDistributedLockService using SETNX with 30-second TTL
5. Implement RedisSlidingWindowCounter for per-endpoint rate limiting
6. Implement InMemoryCacheService as fallback with IMemoryCache
7. Configure DI with decorator pattern for automatic fallback on Redis failure
8. Add Redis health check to the health endpoint

## Current Project State

- Backend solution structure exists (US_002)
- No Redis integration exists yet

## Expected Changes

| Action | File Path                                                    | Description                              |
| ------ | ------------------------------------------------------------ | ---------------------------------------- |
| CREATE | src/Application/Interfaces/ICacheService.cs                  | Cache abstraction interface              |
| CREATE | src/Application/Interfaces/IDistributedLockService.cs        | Lock abstraction interface               |
| CREATE | src/Infrastructure/Caching/RedisConnectionFactory.cs         | Redis connection with TLS                |
| CREATE | src/Infrastructure/Caching/RedisCacheService.cs              | Redis cache with sliding expiry          |
| CREATE | src/Infrastructure/Caching/InMemoryCacheService.cs           | In-memory fallback cache                 |
| CREATE | src/Infrastructure/Locking/RedisDistributedLockService.cs    | SETNX-based distributed locks            |
| CREATE | src/Infrastructure/RateLimiting/RedisSlidingWindowCounter.cs | Sliding window rate limiter              |
| MODIFY | src/API/Program.cs                                           | Register Redis services and health check |

## External References

- [StackExchange.Redis Documentation](https://stackexchange.github.io/StackExchange.Redis/)
- [Upstash Redis REST API](https://docs.upstash.com/redis)

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — mock Redis for cache and lock service tests
- [ ] Integration tests pass — verify Redis health check, lock acquisition/release

## Implementation Checklist

- [ ] Create cache and lock abstraction interfaces in Application layer (AC-01)
- [ ] Implement Redis connection factory with Upstash TLS configuration (AC-01)
- [ ] Implement session caching with 15-minute sliding expiry using Redis SETEX and TTL refresh (AC-02)
- [ ] Implement distributed slot lock using SETNX with 30-second TTL and explicit release (AC-03)
- [ ] Implement sliding window rate-limiting counters with HTTP 429 and Retry-After header (AC-04)
- [ ] Implement in-memory fallback cache with automatic failover on Redis unavailability (AC-05)
- [ ] Add Redis connectivity health check to /health endpoint (AC-01)
- [ ] Log critical warning on Redis connection failure and quota exhaustion (Edge Cases)
