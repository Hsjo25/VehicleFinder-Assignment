using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using VehicleFinder.Api.Clients;
using VehicleFinder.Api.Exceptions;
using VehicleFinder.Tests.TestDoubles;

namespace VehicleFinder.Tests.Clients;

public class NhtsaClientTests
{
    private static NhtsaClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var httpClient = new HttpClient(new FakeHttpMessageHandler(respond))
        {
            BaseAddress = new Uri("https://vpic.nhtsa.dot.gov/api/"),
        };
        return new NhtsaClient(httpClient, NullLogger<NhtsaClient>.Instance);
    }

    [Fact]
    public async Task GetAllMakesAsync_ParsesSuccessfulResponse()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"Count":1,"Message":"ok","Results":[{"Make_ID":448,"Make_Name":"TOYOTA"}]}""",
                Encoding.UTF8,
                "application/json"),
        });

        var result = await client.GetAllMakesAsync(CancellationToken.None);

        var make = Assert.Single(result);
        Assert.Equal(448, make.MakeId);
        Assert.Equal("TOYOTA", make.MakeName);
    }

    [Fact]
    public async Task GetAllMakesAsync_NonSuccessStatus_ThrowsUnavailable()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var ex = await Assert.ThrowsAsync<NhtsaApiException>(() => client.GetAllMakesAsync(CancellationToken.None));

        Assert.Equal(NhtsaFailureReason.Unavailable, ex.Reason);
    }

    [Fact]
    public async Task GetAllMakesAsync_MalformedJson_ThrowsInvalidResponse()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not valid json", Encoding.UTF8, "application/json"),
        });

        var ex = await Assert.ThrowsAsync<NhtsaApiException>(() => client.GetAllMakesAsync(CancellationToken.None));

        Assert.Equal(NhtsaFailureReason.InvalidResponse, ex.Reason);
    }

    [Fact]
    public async Task GetAllMakesAsync_HttpClientTimeout_ThrowsTimeout()
    {
        // Mirrors what HttpClient throws when its own Timeout elapses: a TaskCanceledException
        // wrapping a TimeoutException, even though the caller's token was never cancelled.
        var client = CreateClient(_ => throw new TaskCanceledException("timed out", new TimeoutException()));

        var ex = await Assert.ThrowsAsync<NhtsaApiException>(() => client.GetAllMakesAsync(CancellationToken.None));

        Assert.Equal(NhtsaFailureReason.Timeout, ex.Reason);
    }
}
