using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodeLens.Web.Models.Common;
using CodeLens.Web.Models.Reviews;

namespace CodeLens.Web.Services;

public interface IReviewService
{
    Task<ServiceResult<CodeReviewDto>> SubmitAsync(SubmitReviewRequest request, CancellationToken ct = default);
    Task<ServiceResult<PagedResult<CodeReviewDto>>> GetHistoryAsync(int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ServiceResult<CodeReviewDetailDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<CodeReviewDetailDto>> ReRunAsync(Guid id, CancellationToken ct = default);
}

internal sealed class ReviewService : IReviewService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;

    public ReviewService(HttpClient http) => _http = http;

    public Task<ServiceResult<CodeReviewDto>> SubmitAsync(
        SubmitReviewRequest request, CancellationToken ct = default) =>
        SafeCallAsync(async () =>
        {
            var response = await _http.PostAsJsonAsync("/api/reviews", request, ct);
            return await ReadAsync<CodeReviewDto>(response, ct);
        });

    public Task<ServiceResult<PagedResult<CodeReviewDto>>> GetHistoryAsync(
        int page = 1, int pageSize = 10, CancellationToken ct = default) =>
        SafeCallAsync(async () =>
        {
            var response = await _http.GetAsync($"/api/reviews?page={page}&pageSize={pageSize}", ct);
            return await ReadAsync<PagedResult<CodeReviewDto>>(response, ct);
        });

    public Task<ServiceResult<CodeReviewDetailDto>> GetByIdAsync(
        Guid id, CancellationToken ct = default) =>
        SafeCallAsync(async () =>
        {
            var response = await _http.GetAsync($"/api/reviews/{id}", ct);
            return await ReadAsync<CodeReviewDetailDto>(response, ct);
        });

    public Task<ServiceResult<CodeReviewDetailDto>> ReRunAsync(
        Guid id, CancellationToken ct = default) =>
        SafeCallAsync(async () =>
        {
            var response = await _http.PostAsync($"/api/reviews/{id}/rerun", null, ct);
            return await ReadAsync<CodeReviewDetailDto>(response, ct);
        });

    private static async Task<ServiceResult<T>> ReadAsync<T>(
        HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<T>(JsonOpts, ct);
            return ServiceResult<T>.Success(data!);
        }

        try
        {
            using var doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

            var detail = doc.RootElement.TryGetProperty("detail", out var d) ? d.GetString() : null;
            var title  = doc.RootElement.TryGetProperty("title",  out var t) ? t.GetString() : null;
            return ServiceResult<T>.Failure(detail ?? title ?? $"Error {(int)response.StatusCode}");
        }
        catch
        {
            return ServiceResult<T>.Failure($"Request failed ({(int)response.StatusCode}).");
        }
    }

    private static async Task<ServiceResult<T>> SafeCallAsync<T>(Func<Task<ServiceResult<T>>> call)
    {
        try { return await call(); }
        catch (Exception ex) { return ServiceResult<T>.Failure($"Could not reach the server: {ex.Message}"); }
    }
}
