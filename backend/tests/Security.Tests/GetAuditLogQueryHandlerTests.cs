using Application.Queries;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Security.Tests;

// ---------------------------------------------------------------------------
// GetAuditLogQueryHandlerTests
// AC-01: paginated query with date / actor / action filters
// AC-02: indexed columns drive server-side filtering (no client-side materialisation)
// Edge Case: large result set — efficient skip/take pagination
// ---------------------------------------------------------------------------

public sealed class GetAuditLogQueryHandlerTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static ApplicationDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"audit-log-tests-{Guid.NewGuid()}")
            .Options);

    private static GetAuditLogQueryHandler BuildHandler(ApplicationDbContext db) =>
        new(db);

    private static User MakeUser(string fullName) =>
        new() { UserId = Guid.NewGuid(), Email = $"{fullName.Replace(" ", ".")}@test.io", FullName = fullName };

    private static AuditLog MakeEntry(
        DateTime timestamp,
        string actionType,
        string resourceType,
        string resourceId,
        User? actor = null) =>
        new()
        {
            Timestamp    = timestamp,
            ActorUserId  = actor?.UserId,
            ActorUser    = actor,
            ActorRole    = actor is not null ? "staff" : "system",
            ActionType   = actionType,
            ResourceType = resourceType,
            ResourceId   = resourceId,
        };

    // Seeds 30 entries (25 on page 1, 5 on page 2) plus one user and one "System" entry.
    private static async Task<(ApplicationDbContext db, User alice, User bob)> SeedStandardDataAsync()
    {
        var db = CreateContext();

        var alice = MakeUser("Alice Johnson");
        var bob   = MakeUser("Bob Smith");
        db.Users.AddRange(alice, bob);

        var now = new DateTime(2025, 2, 1, 12, 0, 0, DateTimeKind.Utc);

        // 15 Login entries for Alice
        for (var i = 0; i < 15; i++)
            db.AuditLogs.Add(MakeEntry(now.AddDays(-i), "Login", "Session", $"SES-{100 + i}", alice));

        // 10 Update entries for Bob
        for (var i = 0; i < 10; i++)
            db.AuditLogs.Add(MakeEntry(now.AddDays(-i - 15), "Update", "Appointment", $"APT-{200 + i}", bob));

        // 5 System (no actor) Create entries
        for (var i = 0; i < 5; i++)
            db.AuditLogs.Add(MakeEntry(now.AddDays(-i - 25), "Create", "Code mapping", $"CM-{300 + i}"));

        await db.SaveChangesAsync();
        return (db, alice, bob);
    }

    // -----------------------------------------------------------------------
    // AC-01 + AC-03: pagination
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_NoFilters_ReturnsFirstPageOf25()
    {
        var (db, _, _) = await SeedStandardDataAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetAuditLogQuery(null, null, null, null, null, Page: 1, PageSize: 25),
            CancellationToken.None);

        Assert.Equal(30, result.Total);
        Assert.Equal(25, result.Entries.Count);
        Assert.Equal(1,  result.Page);
        Assert.Equal(25, result.PageSize);
    }

    [Fact]
    public async Task Handle_NoFilters_ReturnsSecondPageRemainder()
    {
        var (db, _, _) = await SeedStandardDataAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetAuditLogQuery(null, null, null, null, null, Page: 2, PageSize: 25),
            CancellationToken.None);

        Assert.Equal(30, result.Total);
        Assert.Equal(5,  result.Entries.Count);
        Assert.Equal(2,  result.Page);
    }

    [Fact]
    public async Task Handle_NoFilters_EntriesOrderedNewestFirst()
    {
        var (db, _, _) = await SeedStandardDataAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetAuditLogQuery(null, null, null, null, null, Page: 1, PageSize: 5),
            CancellationToken.None);

        var timestamps = result.Entries.Select(e => e.Timestamp).ToList();
        Assert.Equal(timestamps.OrderByDescending(t => t).ToList(), timestamps);
    }

    // -----------------------------------------------------------------------
    // AC-01: action type filter
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ActionTypeFilter_ReturnsOnlyMatchingEntries()
    {
        var (db, _, _) = await SeedStandardDataAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetAuditLogQuery(null, "Login", null, null, null),
            CancellationToken.None);

        Assert.Equal(15, result.Total);
        Assert.All(result.Entries, e => Assert.Equal("Login", e.ActionType));
    }

    [Fact]
    public async Task Handle_UnknownActionType_ReturnsEmpty()
    {
        var (db, _, _) = await SeedStandardDataAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetAuditLogQuery(null, "Export", null, null, null),
            CancellationToken.None);

        Assert.Equal(0, result.Total);
        Assert.Empty(result.Entries);
    }

    // -----------------------------------------------------------------------
    // AC-01: actor name filter (partial, case-insensitive)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ActorFilter_PartialNameMatch_ReturnsMatchingEntries()
    {
        var (db, alice, _) = await SeedStandardDataAsync();
        var handler = BuildHandler(db);

        // "alice" matches "Alice Johnson" (case-insensitive partial match)
        var result = await handler.Handle(
            new GetAuditLogQuery("alice", null, null, null, null),
            CancellationToken.None);

        Assert.Equal(15, result.Total);
        Assert.All(result.Entries, e => Assert.Equal(alice.UserId, e.ActorUserId));
    }

    [Fact]
    public async Task Handle_ActorFilter_CaseInsensitive_ReturnsMatchingEntries()
    {
        var (db, _, bob) = await SeedStandardDataAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetAuditLogQuery("BOB", null, null, null, null),
            CancellationToken.None);

        Assert.Equal(10, result.Total);
        Assert.All(result.Entries, e => Assert.Equal(bob.UserId, e.ActorUserId));
    }

    [Fact]
    public async Task Handle_ActorFilter_SystemEntries_ExcludedFromNamedFilter()
    {
        // System entries have ActorUserId = null and no ActorUser — they should be excluded
        // when an actor name filter is active.
        var (db, _, _) = await SeedStandardDataAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetAuditLogQuery("system", null, null, null, null),
            CancellationToken.None);

        Assert.Equal(0, result.Total);
    }

    // -----------------------------------------------------------------------
    // AC-01: date range filter
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_FromDateFilter_ExcludesEntriesBeforeDate()
    {
        var (db, _, _) = await SeedStandardDataAsync();
        var handler = BuildHandler(db);

        // Alice entries span 2025-01-17 to 2025-02-01; Bob entries span 2025-01-02 to 2025-01-16
        var fromDate = new DateTime(2025, 1, 17, 0, 0, 0, DateTimeKind.Utc);

        var result = await handler.Handle(
            new GetAuditLogQuery(null, null, null, fromDate, null),
            CancellationToken.None);

        Assert.All(result.Entries, e => Assert.True(e.Timestamp >= fromDate));
    }

    [Fact]
    public async Task Handle_ToDateFilter_ExcludesEntriesAfterDate()
    {
        var (db, _, _) = await SeedStandardDataAsync();
        var handler = BuildHandler(db);

        var toDate = new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc);

        var result = await handler.Handle(
            new GetAuditLogQuery(null, null, null, null, toDate),
            CancellationToken.None);

        var endOfDay = toDate.Date.AddDays(1).AddTicks(-1);
        Assert.All(result.Entries, e => Assert.True(e.Timestamp <= endOfDay));
    }

    [Fact]
    public async Task Handle_DateRangeFilter_BothBounds_ReturnsOnlyEntriesInRange()
    {
        var (db, _, _) = await SeedStandardDataAsync();
        var handler = BuildHandler(db);

        var from = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var to   = new DateTime(2025, 1, 20, 0, 0, 0, DateTimeKind.Utc);

        var result = await handler.Handle(
            new GetAuditLogQuery(null, null, null, from, to),
            CancellationToken.None);

        var endOfDay = to.Date.AddDays(1).AddTicks(-1);
        Assert.All(result.Entries, e =>
        {
            Assert.True(e.Timestamp >= from);
            Assert.True(e.Timestamp <= endOfDay);
        });
    }

    // -----------------------------------------------------------------------
    // AC-01: resource type filter
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ResourceTypeFilter_ReturnsOnlyMatchingEntries()
    {
        var (db, _, _) = await SeedStandardDataAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetAuditLogQuery(null, null, "Appointment", null, null),
            CancellationToken.None);

        Assert.Equal(10, result.Total);
        Assert.All(result.Entries, e => Assert.Equal("Appointment", e.ResourceType));
    }

    // -----------------------------------------------------------------------
    // AC-01: combined filters
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_CombinedFilters_ActionAndResourceType_IntersectsCorrectly()
    {
        var (db, _, _) = await SeedStandardDataAsync();
        var handler = BuildHandler(db);

        // "Update" + "Appointment" → exactly Bob's 10 entries
        var result = await handler.Handle(
            new GetAuditLogQuery(null, "Update", "Appointment", null, null),
            CancellationToken.None);

        Assert.Equal(10, result.Total);
        Assert.All(result.Entries, e =>
        {
            Assert.Equal("Update", e.ActionType);
            Assert.Equal("Appointment", e.ResourceType);
        });
    }

    // -----------------------------------------------------------------------
    // DTO projection
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ActorEntry_ProjectsActorNameFromNavigationProperty()
    {
        var (db, alice, _) = await SeedStandardDataAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetAuditLogQuery("Alice", null, null, null, null),
            CancellationToken.None);

        Assert.All(result.Entries, e => Assert.Equal(alice.FullName, e.ActorName));
    }

    [Fact]
    public async Task Handle_SystemEntry_ProjectsNullActorName()
    {
        var (db, _, _) = await SeedStandardDataAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetAuditLogQuery(null, "Create", "Code mapping", null, null),
            CancellationToken.None);

        Assert.Equal(5, result.Total);
        Assert.All(result.Entries, e => Assert.Null(e.ActorName));
    }
}
