using System.Net;
using System.Net.Http.Json;

namespace SchoolAdmission.Tests;

// Small helpers so the tests read like the API contract.

internal record ClassDto(
    string Id, string Name, int TotalSeats, int HeldSeats, int ConfirmedSeats, int AvailableSeats);

internal record HoldDto(
    string HoldId, string ClassId, string StudentId, string Status, DateTime ExpiresAt);

internal record MyHoldDto(
    string HoldId, string ClassId, string ClassName, string Status, DateTime ExpiresAt);

internal record ConfirmDto(string HoldId, string Status);

internal record ErrorDto(string Message);

internal static class Api
{
    /// <summary>Creates a random 17-digit birth registration number.</summary>
    public static string NewStudentId() =>
        "2015" + Random.Shared.NextInt64(1_000_000_000_000, 9_999_999_999_999);

    public static async Task<ClassDto> CreateClass(HttpClient client, int seats)
    {
        var response = await client.PostAsJsonAsync("/classes", new
        {
            name = $"Class {Guid.NewGuid():N}",
            totalSeats = seats,
        });

        if (response.StatusCode != HttpStatusCode.Created)
            throw new Exception($"Creating a class failed: {response.StatusCode}");

        return (await response.Content.ReadFromJsonAsync<ClassDto>())!;
    }

    public static async Task<HttpResponseMessage> Apply(HttpClient client, string classId, string studentId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/classes/{classId}/holds");
        request.Headers.Add("X-Student-Id", studentId);
        return await client.SendAsync(request);
    }

    public static async Task<HoldDto> ApplyOk(HttpClient client, string classId, string studentId)
    {
        var response = await Api.Apply(client, classId, studentId);
        if (response.StatusCode != HttpStatusCode.Created)
            throw new Exception($"Applying failed: {response.StatusCode}");

        return (await response.Content.ReadFromJsonAsync<HoldDto>())!;
    }

    public static async Task<ClassDto> GetClass(HttpClient client, string classId) =>
        (await client.GetFromJsonAsync<ClassDto>($"/classes/{classId}"))!;

    public static async Task<HoldDto> GetHold(HttpClient client, string holdId) =>
        (await client.GetFromJsonAsync<HoldDto>($"/holds/{holdId}"))!;

    public static async Task<MyHoldDto[]> GetMine(HttpClient client, string studentId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/me/holds");
        request.Headers.Add("X-Student-Id", studentId);
        var response = await client.SendAsync(request);

        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"/me/holds failed: {response.StatusCode}");

        return (await response.Content.ReadFromJsonAsync<MyHoldDto[]>())!;
    }

    public static async Task<HttpResponseMessage> Confirm(HttpClient client, string holdId, string studentId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/holds/{holdId}/confirm");
        request.Headers.Add("X-Student-Id", studentId);
        return await client.SendAsync(request);
    }

    public static async Task<ConfirmDto> ConfirmOk(HttpClient client, string holdId, string studentId)
    {
        var response = await Api.Confirm(client, holdId, studentId);
        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Confirm failed: {response.StatusCode}");

        return (await response.Content.ReadFromJsonAsync<ConfirmDto>())!;
    }

    public static async Task<string> ErrorMessage(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<ErrorDto>())!.Message;
}
