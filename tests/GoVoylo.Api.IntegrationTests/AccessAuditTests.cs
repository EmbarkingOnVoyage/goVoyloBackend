using FluentAssertions;
using GoVoylo.Api.Services;
using GoVoylo.Application.Features.Payments.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.ExternalServices.ProxyCheck;
using GoVoylo.Infrastructure.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace GoVoylo.Api.IntegrationTests
{
    public class AccessAuditTests
    {
        private static ClientRequestInfo ClientWith(string? forwardedFor, string remote = "10.0.0.5")
        {
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(remote);
            if (forwardedFor != null) context.Request.Headers["X-Forwarded-For"] = forwardedFor;
            context.Request.Headers.UserAgent = "GoVoylo/1.0";
            return new ClientRequestInfo(new HttpContextAccessor { HttpContext = context });
        }

        [Theory]
        [InlineData("49.36.10.20:51234", "49.36.10.20")]
        [InlineData("1.1.1.1, 49.36.10.20:51234", "49.36.10.20")]
        [InlineData("[2401:4900:1c::5]:443", "2401:4900:1c::5")]
        [InlineData("2401:4900:1c::5", "2401:4900:1c::5")]
        public void TheClientIp_IsTheLastForwardedAddressWithoutItsPort(string header, string expected)
        {
            ClientWith(header).IpAddress.Should().Be(expected);
        }

        [Fact]
        public void WithoutForwarding_TheConnectionAddressIsUsed()
        {
            var client = ClientWith(null, "127.0.0.1");
            client.IpAddress.Should().Be("127.0.0.1");
            client.UserAgent.Should().Be("GoVoylo/1.0");
        }

        [Theory]
        [InlineData("49.36.10.20", true)]
        [InlineData("2401:4900:1c::5", true)]
        [InlineData("127.0.0.1", false)]
        [InlineData("10.1.2.3", false)]
        [InlineData("172.20.0.1", false)]
        [InlineData("192.168.1.10", false)]
        [InlineData("100.64.0.1", false)]
        [InlineData("::1", false)]
        [InlineData("fd00::1", false)]
        [InlineData("not-an-ip", false)]
        public void OnlyPublicAddresses_AreLookedUp(string ip, bool expected)
        {
            AccessAuditBackgroundService.IsPublic(ip).Should().Be(expected);
        }

        private static IpReputation? ParseProxyCheck(string json, string ip) =>
            ProxyCheckClient.Parse(System.Text.Json.JsonDocument.Parse(json).RootElement, ip, 67);

        [Fact]
        public void AJioMobileAddress_IsSecure()
        {
            var r = ParseProxyCheck("""
                { "status": "ok", "49.36.10.20": { "asn": "AS55836", "provider": "Reliance Jio Infocomm Limited",
                  "isocode": "IN", "region": "Maharashtra", "city": "Navi Mumbai (Ghansoli)",
                  "latitude": 19.1258, "longitude": 73.0004, "vpn": "no", "proxy": "no", "type": "Wireless", "risk": 0 } }
                """, "49.36.10.20")!;

            r.City.Should().Be("Navi Mumbai (Ghansoli)");
            r.Region.Should().Be("Maharashtra");
            r.CountryCode.Should().Be("IN");
            r.Isp.Should().Be("Reliance Jio Infocomm Limited");
            r.Latitude.Should().Be(19.1258m);
            r.ConnectionType.Should().Be("Wireless");
            r.IsSecure.Should().BeTrue();
        }

        [Fact]
        public void ATorExit_IsNotSecure()
        {
            var r = ParseProxyCheck("""
                { "status": "ok", "185.220.101.1": { "isocode": "DE", "city": "Hamburg", "proxy": "yes", "type": "TOR", "risk": 100 } }
                """, "185.220.101.1")!;

            r.IsTor.Should().BeTrue();
            r.IsProxy.Should().BeTrue();
            r.RiskScore.Should().Be(100);
            r.IsSecure.Should().BeFalse();
        }

        [Fact]
        public void AVpn_IsNotSecure()
        {
            var r = ParseProxyCheck("""
                { "status": "ok", "2001:db8::1": { "isocode": "NL", "vpn": "yes", "proxy": "no", "type": "VPN", "risk": 33 } }
                """, "2001:0db8::1")!;

            r.IsVpn.Should().BeTrue();
            r.IsSecure.Should().BeFalse();
        }

        [Fact]
        public void ADeniedLookup_GivesNothing()
        {
            ParseProxyCheck("""{ "status": "denied", "message": "1,000 free queries exhausted." }""", "49.36.10.20")
                .Should().BeNull();
        }

        [Fact]
        public async Task APayment_IsRecordedWithLocationSafetyAndHowItWasPaid()
        {
            var (worker, services, repository, ipClient) = Build();

            await worker.ProcessAsync(new PaymentStartedAuditEntry(
                Guid.NewGuid(), "FLYB1", "order_1", 7370m, "INR", "49.36.10.20", "GoVoylo/1.0"), services, default);
            await worker.ProcessAsync(new PaymentCompletedAuditEntry(
                "order_1", "pay_1", "Succeeded", "49.36.10.20", "GoVoylo/1.0"), services, default);

            var log = repository.Rows.Single();
            log.EventType.Should().Be(AccessAuditLog.EventPayment);
            log.PaymentStatus.Should().Be("Succeeded");
            log.PaymentMethod.Should().Be("card");
            log.PaymentMethodDetail.Should().Be("Visa credit card, HDFC");
            log.City.Should().Be("Pune");
            log.CountryCode.Should().Be("IN");
            log.IsSecure.Should().BeTrue();
            log.LookupStatus.Should().Be(AccessAuditLog.LookupDone);
            ipClient.Calls.Should().Be(1);
        }

        [Fact]
        public async Task TheSameIp_IsLookedUpOnce()
        {
            var (worker, services, repository, ipClient) = Build();
            var user = Guid.NewGuid();

            await worker.ProcessAsync(new SignInAuditEntry(user, "otp", "49.36.10.20", null), services, default);
            await worker.ProcessAsync(new SignInAuditEntry(user, "google", "49.36.10.20", null), services, default);

            repository.Rows.Should().HaveCount(2).And.OnlyContain(r => r.City == "Pune");
            ipClient.Calls.Should().Be(1);
        }

        [Fact]
        public async Task APrivateIp_IsKeptWithoutALookup()
        {
            var (worker, services, repository, ipClient) = Build();

            await worker.ProcessAsync(new SignInAuditEntry(Guid.NewGuid(), "otp", "127.0.0.1", null), services, default);

            repository.Rows.Single().LookupStatus.Should().Be(AccessAuditLog.LookupPrivateIp);
            ipClient.Calls.Should().Be(0);
        }

        private static (AccessAuditBackgroundService, IServiceProvider, FakeRepository, FakeIpClient) Build()
        {
            var repository = new FakeRepository();
            var ipClient = new FakeIpClient();
            var services = new ServiceCollection()
                .AddSingleton<IAccessAuditLogRepository>(repository)
                .AddSingleton<IIpReputationClient>(ipClient)
                .AddSingleton<IRazorpayClient>(new FakeRazorpay())
                .BuildServiceProvider();
            var worker = new AccessAuditBackgroundService(
                new AccessAuditQueue(),
                services.GetRequiredService<IServiceScopeFactory>(),
                new MemoryCache(new MemoryCacheOptions()),
                NullLogger<AccessAuditBackgroundService>.Instance);
            return (worker, services, repository, ipClient);
        }

        private class FakeRepository : IAccessAuditLogRepository
        {
            public List<AccessAuditLog> Rows { get; } = new();

            public Task AddAsync(AccessAuditLog log, CancellationToken cancellationToken)
            {
                Rows.Add(log);
                return Task.CompletedTask;
            }

            public Task UpdateAsync(AccessAuditLog log, CancellationToken cancellationToken) => Task.CompletedTask;

            public Task<AccessAuditLog?> GetByGatewayOrderIdAsync(string gatewayOrderId, CancellationToken cancellationToken) =>
                Task.FromResult(Rows.LastOrDefault(r => r.GatewayOrderId == gatewayOrderId));

            public Task<AccessAuditLog?> GetRecentLookupAsync(string ipAddress, DateTime since, CancellationToken cancellationToken) =>
                Task.FromResult<AccessAuditLog?>(null);
        }

        private class FakeIpClient : IIpReputationClient
        {
            public int Calls { get; private set; }
            public bool IsConfigured => true;

            public Task<IpReputation?> LookupAsync(string ipAddress, CancellationToken cancellationToken)
            {
                Calls++;
                return Task.FromResult<IpReputation?>(new IpReputation(
                    "Pune", "Maharashtra", "IN", "Reliance Jio", 18.52m, 73.85m,
                    "Wireless", false, false, false, false, 0, true));
            }
        }

        private class FakeRazorpay : IRazorpayClient
        {
            public string KeyId => "rzp_test";

            public Task<RazorpayOrderResult> CreateOrderAsync(decimal amount, string currency, string receipt, CancellationToken cancellationToken) =>
                throw new NotSupportedException();

            public bool VerifySignature(string orderId, string paymentId, string signature) => true;

            public Task<RazorpayPaymentMethod?> GetPaymentMethodAsync(string paymentId, CancellationToken cancellationToken) =>
                Task.FromResult<RazorpayPaymentMethod?>(new RazorpayPaymentMethod("card", "Visa credit card, HDFC"));
        }
    }
}
