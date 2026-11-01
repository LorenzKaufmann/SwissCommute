using System.Net;
using System.Text;

namespace SwissCommute.Tests.Fakes;

public sealed class StubHttpMessageHandler(HttpStatusCode statusCode, string jsonContent) : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        return Task.FromResult(new HttpResponseMessage(statusCode) { Content = content });
    }
}
