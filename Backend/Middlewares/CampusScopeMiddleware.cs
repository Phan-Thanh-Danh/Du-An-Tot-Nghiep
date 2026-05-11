using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Microsoft.EntityFrameworkCore;

namespace Backend.Middlewares;

public class CampusScopeMiddleware
{
    private readonly RequestDelegate _next;

    public CampusScopeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        if (context.Items["CurrentUser"] is not CurrentUserContext currentUser)
        {
            await _next(context);
            return;
        }

        if (currentUser.Role == AuthRoles.SuperAdmin)
        {
            await _next(context);
            return;
        }

        var requestedCampusId = GetRequestedCampusId(context);
        if (requestedCampusId is null)
        {
            await _next(context);
            return;
        }

        var allowed = currentUser.Role switch
        {
            AuthRoles.CampusAdmin => await IsCampusOrChildCampusAsync(dbContext, currentUser.CampusId, requestedCampusId.Value),
            AuthRoles.SubCampusAdmin => requestedCampusId.Value == currentUser.CampusId,
            _ => requestedCampusId.Value == currentUser.CampusId
        };

        if (!allowed)
        {
            await WriteForbiddenAsync(context);
            return;
        }

        await _next(context);
    }

    private static int? GetRequestedCampusId(HttpContext context)
    {
        var campusKeys = new[] { "campusId", "maDonVi", "donViId" };

        foreach (var key in campusKeys)
        {
            if (TryParseCampusId(context.Request.RouteValues[key]?.ToString(), out var routeCampusId))
            {
                return routeCampusId;
            }
        }

        foreach (var key in campusKeys)
        {
            if (TryParseCampusId(context.Request.Query[key].FirstOrDefault(), out var queryCampusId))
            {
                return queryCampusId;
            }
        }

        if (TryParseCampusId(context.Request.Headers["X-Campus-Id"].FirstOrDefault(), out var headerCampusId))
        {
            return headerCampusId;
        }

        return null;
    }

    private static bool TryParseCampusId(string? value, out int campusId)
    {
        return int.TryParse(value, out campusId) && campusId > 0;
    }

    private static async Task<bool> IsCampusOrChildCampusAsync(ApplicationDbContext dbContext, int rootCampusId, int requestedCampusId)
    {
        if (rootCampusId == requestedCampusId)
        {
            return true;
        }

        var campuses = await dbContext.DonVis
            .Select(x => new { x.MaDonVi, x.MaDonViCha })
            .ToListAsync();

        var childCampusIds = campuses
            .Where(x => x.MaDonViCha == rootCampusId)
            .Select(x => x.MaDonVi)
            .ToList();
        var visitedCampusIds = new HashSet<int> { rootCampusId };

        for (var index = 0; index < childCampusIds.Count; index++)
        {
            var currentCampusId = childCampusIds[index];
            if (!visitedCampusIds.Add(currentCampusId))
            {
                continue;
            }

            if (currentCampusId == requestedCampusId)
            {
                return true;
            }

            childCampusIds.AddRange(campuses
                .Where(x => x.MaDonViCha == currentCampusId)
                .Select(x => x.MaDonVi));
        }

        return false;
    }

    private static async Task WriteForbiddenAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            statusCode = StatusCodes.Status403Forbidden,
            message = "Bạn không có quyền truy cập dữ liệu của cơ sở này.",
            traceId = context.TraceIdentifier
        }));
    }
}
