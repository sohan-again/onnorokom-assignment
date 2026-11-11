using System.Net;

namespace SchoolAdmission.Tests;

[Collection("api")]
public class AdmissionTests(ApiFixture fixture)
{
    [Fact]
    public async Task Hold_Then_Confirm_Works()
    {
        var client = fixture.CreateClient();
        var schoolClass = await Api.CreateClass(client, seats: 2);
        var student = Api.NewStudentId();

        var hold = await Api.ApplyOk(client, schoolClass.Id, student);
        Assert.Equal("HELD", hold.Status);

        var afterHold = await Api.GetClass(client, schoolClass.Id);
        Assert.Equal(1, afterHold.HeldSeats);
        Assert.Equal(1, afterHold.AvailableSeats);

        var confirmed = await Api.ConfirmOk(client, hold.HoldId, student);
        Assert.Equal("CONFIRMED", confirmed.Status);

        var afterConfirm = await Api.GetClass(client, schoolClass.Id);
        Assert.Equal(0, afterConfirm.HeldSeats);
        Assert.Equal(1, afterConfirm.ConfirmedSeats);
        Assert.Equal(1, afterConfirm.AvailableSeats);

        var mine = await Api.GetMine(client, student);
        var only = Assert.Single(mine);
        Assert.Equal(schoolClass.Id, only.ClassId);
        Assert.Equal("CONFIRMED", only.Status);
    }

    [Fact]
    public async Task Expired_Hold_Frees_The_Seat_And_Cannot_Be_Confirmed()
    {
        // One second hold, so the test does not wait long.
        var client = fixture.CreateClient(holdSeconds: 1);
        var schoolClass = await Api.CreateClass(client, seats: 1);
        var student = Api.NewStudentId();

        var hold = await Api.ApplyOk(client, schoolClass.Id, student);
        Assert.Equal(0, (await Api.GetClass(client, schoolClass.Id)).AvailableSeats);

        await Task.Delay(TimeSpan.FromMilliseconds(1500));

        var expired = await Api.GetHold(client, hold.HoldId);
        Assert.Equal("EXPIRED", expired.Status);

        var afterExpiry = await Api.GetClass(client, schoolClass.Id);
        Assert.Equal(0, afterExpiry.HeldSeats);
        Assert.Equal(1, afterExpiry.AvailableSeats);

        var confirm = await Api.Confirm(client, hold.HoldId, student);
        Assert.Equal(HttpStatusCode.Conflict, confirm.StatusCode);
    }

    [Fact]
    public async Task Same_Student_Cannot_Hold_Twice_In_One_Class()
    {
        var client = fixture.CreateClient();
        var schoolClass = await Api.CreateClass(client, seats: 10);
        var student = Api.NewStudentId();

        await Api.ApplyOk(client, schoolClass.Id, student);

        var second = await Api.Apply(client, schoolClass.Id, student);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Contains("already", await Api.ErrorMessage(second), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, (await Api.GetClass(client, schoolClass.Id)).HeldSeats);
    }

    [Fact]
    public async Task Concurrent_Requests_Never_Exceed_Total_Seats()
    {
        var client = fixture.CreateClient();
        const int seats = 5;
        const int attempts = 20;
        var schoolClass = await Api.CreateClass(client, seats);

        var requests = Enumerable.Range(0, attempts)
            .Select(_ => Api.Apply(client, schoolClass.Id, Api.NewStudentId()))
            .ToArray();

        var responses = await Task.WhenAll(requests);

        var created = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflicts = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        Assert.Equal(seats, created);
        Assert.Equal(attempts - seats, conflicts);

        var after = await Api.GetClass(client, schoolClass.Id);
        Assert.Equal(seats, after.HeldSeats);
        Assert.Equal(0, after.AvailableSeats);
    }
}
