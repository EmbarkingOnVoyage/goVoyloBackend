namespace GoVoylo.Application.Interfaces
{
    // The caller of the current HTTP request, as far as the API can tell.
    public interface IClientRequestInfo
    {
        // The customer's public IP — the client end of Azure's X-Forwarded-For,
        // not the load balancer's address. Null outside a request.
        string? IpAddress { get; }
        string? UserAgent { get; }
    }
}
