using DataWarehouse.SAP.Interfaces.Auth;
using DataWarehouse.SAP.Repositories.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Auth
{
    public class SapAuthService : ISapAuthService
    {
        private readonly ISapSessionCache _cache;
        private readonly ILogger<SapAuthService> logger;
        private readonly ISapSettingsCache _sapSettingsCache;
        private readonly ISapConnectorFactory clientFactory;

        public SapAuthService(
            ISapSessionCache cache, ILogger<SapAuthService> logger
            , ISapSettingsCache sapSettingsCache,
            ISapConnectorFactory clientFactory)
        {
            _cache = cache;
            this.logger = logger;
            _sapSettingsCache = sapSettingsCache;
            this.clientFactory = clientFactory;
        }


        public async Task<SapSession> GetSessionIdAsync(int sapId)
        {
            logger.LogWarning("Starting SAP Login process...");

            // 1️⃣ Check cache first
            var cached = _cache.Get(sapId);
            if (cached != null && cached.SessionTimeout > DateTime.UtcNow)
            {
                logger.LogInformation("Cache hit! SessionId: {SessionId}, ExpireAt: {ExpireAt}, now {now}",
                    cached.SessionId, cached.SessionTimeout, DateTime.UtcNow);
                return new SapSession() { SessionId = cached.SessionId ,SessionTimeout =cached.SessionTimeout};
            }

            // 2️⃣ Create HttpClient
            var client = await clientFactory.Create(sapId);

            // Optional: log the BaseAddress
            logger.LogInformation("Using SAP BaseAddress: {BaseAddress}", client.BaseAddress);

            var sapSettings = await _sapSettingsCache.GetOrSetAsync(sapId);

            var loginData = new
            {
                CompanyDB = sapSettings.CompanyDB,
                UserName = sapSettings.UserName,
                Password = sapSettings.Password
            };


            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            logger.LogInformation("content: {content}", content);

            // 4️⃣ Send POST request
            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync("Login", content);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP request to SAP failed!");
                throw;
            }

            // 5️⃣ Log status code & response body
            var body = await response.Content.ReadAsStringAsync();
            logger.LogInformation("SAP Login StatusCode: {StatusCode}", response.StatusCode);
            logger.LogInformation("SAP Login Response Body: {Body}", body);

            // 6️⃣ Ensure successful response
            response.EnsureSuccessStatusCode();

            // 7️⃣ Deserialize response
            var result = JsonSerializer.Deserialize<SapSessionLogin>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || string.IsNullOrWhiteSpace(result.SessionId))
            {
                throw new Exception("Failed to get SAP SessionId from response!");
            }

            // 8️⃣ Store in cache
            var session = new SapSession
            {
                SessionId = result.SessionId,
                SessionTimeout = DateTime.UtcNow.AddMinutes(result.SessionTimeout)
            };
            _cache.Set(sapId,session);

            logger.LogInformation("SAP SessionId cached successfully: {SessionId}", session.SessionId);

            // 9️⃣ Return session ID
            return session;
        }

        public async Task<SapSession> ForceReLoginAsync(int sapId)
        {
            logger.LogWarning("Forcing SAP Re-Login process...");

            // 1️⃣ Clear cached session
            _cache.Clear(sapId);
            logger.LogInformation("SAP session cache cleared");

            // 2️⃣ Create HttpClient
            var client = await clientFactory.Create(sapId);

            // Optional: log the BaseAddress
            logger.LogInformation("Using SAP BaseAddress: {BaseAddress}", client.BaseAddress);

            var sapSettings = await _sapSettingsCache.GetOrSetAsync(sapId);

            var loginData = new
            {
                CompanyDB = sapSettings.CompanyDB,
                UserName = sapSettings.UserName,
                Password = sapSettings.Password
            };




            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            logger.LogInformation("content: {content}", content);


            // 4️⃣ Send POST request
            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync("Login", content);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP request to SAP failed during force re-login!");
                throw;
            }

            // 5️⃣ Log status code & response body
            var body = await response.Content.ReadAsStringAsync();
            logger.LogInformation("SAP Re-Login StatusCode: {StatusCode}", response.StatusCode);
            logger.LogInformation("SAP Re-Login Response Body: {Body}", body);

            // 6️⃣ Ensure successful response
            response.EnsureSuccessStatusCode();

            // 7️⃣ Deserialize response
            var result = JsonSerializer.Deserialize<SapSessionLogin>(
                body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (result == null || string.IsNullOrWhiteSpace(result.SessionId))
            {
                throw new Exception("Failed to get SAP SessionId during force re-login!");
            }

            // 8️⃣ Store in cache
            var session = new SapSession
            {
                SessionId = result.SessionId,
                SessionTimeout = DateTime.UtcNow.AddMinutes(result.SessionTimeout)
            };

            _cache.Set(sapId, session);

            logger.LogInformation(
                "SAP Re-Login successful. New SessionId: {SessionId}, ExpireAt: {ExpireAt}",
                session.SessionId,
                session.SessionTimeout
            );

            // 9️⃣ Return session
            return session;
        }

        //// DTO للرد من SAP
        public class SapSessionLogin
        {
            public string SessionId { get; set; }
            public string Version { get; set; }
            public int SessionTimeout { get; set; }


        }


    }
}
