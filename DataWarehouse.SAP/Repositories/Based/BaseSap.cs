using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Context;
using DataWarehouse.SAP.Auth;
using DataWarehouse.SAP.Interfaces.Actors;
using DataWarehouse.SAP.Interfaces.Auth;
using DataWarehouse.SAP.Interfaces.Based;
using DataWarehouse.SAP.Models.Based;
using DataWarehouse.SAP.Repositories.Actors;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static DataWarehouse.SAP.Models.Actors.ItemSapModel;


namespace DataWarehouse.SAP.Repositories.Based
{
    public class BaseSap<T> : IBaseSap<T> where T : class
    {
        private readonly DataWarehouseDbContext _context;
        private readonly ISapAuthService _sapAuth;
        private readonly ISapConnectorFactory clientFactory;
        private readonly ILogger<BaseSap<T>> _logger;

        public BaseSap(
            DataWarehouseDbContext context,
            ISapAuthService sapAuth,
           ISapConnectorFactory clientFactory,
            ILogger<BaseSap<T>> logger)
        {
            _context = context;
            _sapAuth = sapAuth;
            this.clientFactory = clientFactory;
            _logger = logger;
        }
        public async Task<string> AddSapAsync(int sapId, string entityType, T entity)
        {
            var client = await clientFactory.Create(sapId);

            HttpResponseMessage? response = null;

            for (int attempt = 1; attempt <= 2; attempt++)
            {
                var sapSession = await _sapAuth.GetSessionIdAsync(sapId);

                // مهم جدًا: شيل أي Cookie قديمة
                client.DefaultRequestHeaders.Remove("Cookie");

                client.DefaultRequestHeaders.Add(
                    "Cookie",
                    $"B1SESSION={sapSession.SessionId};ROUTEID=.node1"
                );

                try
                {
                    var json = JsonSerializer.Serialize(entity, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    });

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    response = await client.PostAsync(entityType, content);

                    var responseBody = await response.Content.ReadAsStringAsync();

                    // Unauthorized → ReLogin مرة واحدة
                    if (response.StatusCode == HttpStatusCode.Unauthorized && attempt == 1)
                    {
                        _logger.LogWarning("SAP returned 401 on POST. Forcing re-login and retrying... Url={Url}", entityType);
                        await _sapAuth.ForceReLoginAsync(sapId);
                        continue;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError(
                            "SAP POST failed. Status={StatusCode} {Reason}. Url={Url}. Request={Request}. Response={Response}",
                            (int)response.StatusCode,
                            response.ReasonPhrase,
                            entityType,
                            json,
                            responseBody);

                        // ارمي exception فيها التفاصيل (مفيدة جداً)
                        throw new HttpRequestException(
                            $"SAP POST failed: {(int)response.StatusCode} {response.ReasonPhrase}\n" +
                            $"Url: {entityType}\n" +
                            $"Response: {responseBody}");
                    }

                    // ✅ نجاح
                    _logger.LogInformation("SAP POST success. Url={Url}. Response={Response}", entityType, responseBody);
                    break;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "HTTP POST to SAP failed! Url={Url}", entityType);
                    throw;
                }
            }

            // حماية إضافية
            if (response == null)
                throw new Exception("SAP POST failed: no response received");

            var body = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("SAP POST StatusCode: {StatusCode}", response.StatusCode);
            _logger.LogInformation("SAP POST Response Body: {Body}", body);

            if (string.IsNullOrWhiteSpace(body))
                _logger.LogWarning("SAP POST returned empty response body");

            // آخر تأكيد
            response.EnsureSuccessStatusCode();

            return body;
        }


        public async Task<bool> AddPatchSapAsync(int sapId,string entityType, T barCodeCollection)
        {
            var client =await clientFactory.Create(sapId);

            for (int attempt = 1; attempt <= 2; attempt++)
            {
                var session = await _sapAuth.GetSessionIdAsync(sapId);

                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders.Add(
                    "Cookie",
                    $"B1SESSION={session.SessionId};ROUTEID=.node1"
                );

                var payload = new
                {
                    ItemBarCodeCollection = barCodeCollection
                };

                var json = JsonSerializer.Serialize(
                    payload,
                    new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    }
                );

                var request = new HttpRequestMessage(HttpMethod.Patch, entityType)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                _logger.LogInformation("SAP PATCH URL: {Url}", client.BaseAddress + entityType);
                _logger.LogInformation("SAP PATCH BODY: {Json}", json);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                // Unauthorized → Retry مرة واحدة
                if (response.StatusCode == HttpStatusCode.Unauthorized && attempt == 1)
                {
                    _logger.LogWarning("SAP 401 → Re-login and retry");
                    await _sapAuth.ForceReLoginAsync(sapId);
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "SAP PATCH failed. Status: {Status}, Body: {Body}",
                        response.StatusCode,
                        responseBody
                    );

                    throw new Exception(
                        $"SAP Error ({response.StatusCode}): {responseBody}"
                    );
                }

                _logger.LogInformation(
                    "SAP PATCH success. Status: {Status}, Body: {Body}",
                    response.StatusCode,
                    responseBody
                );

                return true; // ✅ نجاح
            }

            return false;
        }

        public async Task DeleteSap(int sapId,string entityType)
        {
            var client = await clientFactory.Create(sapId);
            HttpResponseMessage? response = null;

            for (int attempt = 1; attempt <= 2; attempt++)
            {
                var sapSession = await _sapAuth.GetSessionIdAsync(sapId);

                // شيل أي Cookie قديمة
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders.Add(
                    "Cookie",
                    $"B1SESSION={sapSession.SessionId};ROUTEID=.node1"
                );

                try
                {
                    _logger.LogInformation("Sending DELETE to SAP: {EntityType}", entityType);

                    response = await client.DeleteAsync(entityType);

                    // Unauthorized → ReLogin مرة واحدة
                    if (response.StatusCode == HttpStatusCode.Unauthorized && attempt == 1)
                    {
                        _logger.LogWarning("SAP returned 401 on DELETE. Forcing re-login and retrying...");
                        await _sapAuth.ForceReLoginAsync(sapId);
                        continue;
                    }

                    response.EnsureSuccessStatusCode();
                    break; // ✅ نجاح
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "HTTP DELETE to SAP failed!");
                    throw;
                }
            }

            if (response == null)
                throw new Exception("SAP DELETE failed: no response received");

            var body = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("SAP DELETE StatusCode: {StatusCode}", response.StatusCode);
            _logger.LogInformation("SAP DELETE Response Body: {Body}", body);

            response.EnsureSuccessStatusCode();
        }
        public async Task<string> GetAllSap(int sapId, string entityType)
        {
            var client = await clientFactory.Create(sapId);

            // بناء URL النهائي
            var baseUrl = client.BaseAddress?.ToString() ?? throw new Exception("SAP BaseAddress is null");
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            var fullUrl = new Uri(baseUrl + entityType.TrimStart('/'));

            HttpResponseMessage? response = null;

            for (int attempt = 1; attempt <= 2; attempt++)
            {
                var sapSession = await _sapAuth.GetSessionIdAsync(sapId);

                using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
                req.Headers.Add("Cookie", $"B1SESSION={sapSession.SessionId}");

                var sw = System.Diagnostics.Stopwatch.StartNew();

                response = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
                sw.Stop();
                _logger.LogInformation("SAP headers in {ms} ms. Status={status}", sw.ElapsedMilliseconds, (int)response.StatusCode);

                if (response.StatusCode == HttpStatusCode.Unauthorized && attempt == 1)
                {
                    _logger.LogWarning("SAP returned 401. Forcing re-login and retrying...");
                    await _sapAuth.ForceReLoginAsync(sapId);
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError("SAP Error {status}. Url={url}. Body={body}", (int)response.StatusCode, fullUrl, err);
                    response.EnsureSuccessStatusCode();
                }

                sw.Restart();
                var body = await response.Content.ReadAsStringAsync();
                sw.Stop();
                _logger.LogInformation("SAP body read in {ms} ms. Length={len}", sw.ElapsedMilliseconds, body?.Length ?? 0);

                if (string.IsNullOrWhiteSpace(body))
                    throw new Exception("SAP returned empty response body");

                return body;
            }

            throw new Exception("SAP request failed after retries");
        }

        public async Task<string> GetAllSapPrivate(int sapId, string entityType)
        {
            var client = await clientFactory.Create(sapId);

            var baseUrl = client.BaseAddress?.ToString() ?? throw new Exception("SAP BaseAddress is null");
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            var fullUrl = new Uri(baseUrl + entityType.TrimStart('/'));

            HttpResponseMessage? response = null;

            for (int attempt = 1; attempt <= 2; attempt++)
            {
                var sapSession = await _sapAuth.GetSessionIdAsync(sapId);

                using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
                req.Headers.Add("Cookie", $"B1SESSION={sapSession.SessionId}");

                req.Headers.Accept.Clear();
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                req.Headers.AcceptCharset.Clear();
                req.Headers.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
                req.Headers.AcceptCharset.Add(new StringWithQualityHeaderValue("windows-1256"));

                var sw = System.Diagnostics.Stopwatch.StartNew();
                response = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
                sw.Stop();

                _logger.LogInformation("SAP headers in {ms} ms. Status={status}", sw.ElapsedMilliseconds, (int)response.StatusCode);

                if (response.StatusCode == HttpStatusCode.Unauthorized && attempt == 1)
                {
                    _logger.LogWarning("SAP returned 401. Forcing re-login and retrying...");
                    await _sapAuth.ForceReLoginAsync(sapId);
                    continue;
                }

                var bytes = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "<null>";
                _logger.LogInformation("SAP Content-Type: {ct}", contentType);

                // Decode with best encoding (THIS is the key fix)
                var body = DecodeSmart(bytes, response.Content.Headers.ContentType?.CharSet);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("SAP Error {status}. Url={url}. Body={body}", (int)response.StatusCode, fullUrl, body);
                    response.EnsureSuccessStatusCode();
                }

                if (string.IsNullOrWhiteSpace(body))
                    throw new Exception("SAP returned empty response body");

                return body;
            }

            throw new Exception("SAP request failed after retries");
        }

        private static string DecodeSmart(byte[] bytes, string? charsetFromHeader)
        {
            if (bytes is null || bytes.Length == 0) return string.Empty;

            // 1) If server specified charset, trust it first
            if (!string.IsNullOrWhiteSpace(charsetFromHeader))
            {
                try
                {
                    var enc = Encoding.GetEncoding(charsetFromHeader.Trim().Trim('"'));
                    return enc.GetString(bytes).TrimStart('\uFEFF');
                }
                catch
                {
                    // ignore and fallback to heuristics
                }
            }

            // 2) Try UTF-8 strictly
            string utf8;
            try
            {
                utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true)
                    .GetString(bytes)
                    .TrimStart('\uFEFF');
            }
            catch
            {
                utf8 = string.Empty;
            }

            // 3) Try Windows-1256 (Arabic)
            var cp1256 = Encoding.GetEncoding(1256).GetString(bytes).TrimStart('\uFEFF');

            // 4) Choose the one that "looks" most Arabic
            return ArabicScore(cp1256) >= ArabicScore(utf8) ? cp1256 : utf8;
        }

        private static int ArabicScore(string? s)
        {
            if (string.IsNullOrEmpty(s)) return -1;

            // Count Arabic letters range + Arabic presentation forms (broad)
            int score = 0;
            foreach (var ch in s)
            {
                if ((ch >= '\u0600' && ch <= '\u06FF') || (ch >= '\u0750' && ch <= '\u077F') ||
                    (ch >= '\u08A0' && ch <= '\u08FF') || (ch >= '\uFB50' && ch <= '\uFDFF') ||
                    (ch >= '\uFE70' && ch <= '\uFEFF'))
                    score++;
            }

            // Penalize obvious mojibake patterns a bit
            if (s.Contains('�')) score -= 10;

            return score;
        }
      
        //public async Task<string> GetAllSap(int sapId, string entityType)
        //{
        //    var client = await clientFactory.Create(sapId);

        //    HttpResponseMessage? response = null;

        //    for (int attempt = 1; attempt <= 2; attempt++)
        //    {
        //        var sapSession = await _sapAuth.GetSessionIdAsync(sapId);

        //         مهم جدًا: شيل أي Cookie قديمة
        //        client.DefaultRequestHeaders.Remove("Cookie");
        //        client.DefaultRequestHeaders.Add("Cookie", $"B1SESSION={sapSession.SessionId}");

        //        try
        //        {
        //            response = await client.GetAsync(entityType);

        //             Unauthorized في أول محاولة → ReLogin
        //            if (response.StatusCode == HttpStatusCode.Unauthorized && attempt == 1)
        //            {
        //                _logger.LogWarning("SAP returned 401. Forcing re-login and retrying...");
        //                await _sapAuth.ForceReLoginAsync(sapId);
        //                continue;
        //            }

        //            response.EnsureSuccessStatusCode();
        //            break; // ✅ نجاح
        //        }
        //        catch (HttpRequestException ex)
        //        {
        //            _logger.LogError(ex, "HTTP request to SAP failed!");
        //            throw;
        //        }
        //    }

        //     ✅ حماية إضافية
        //    if (response == null)
        //        throw new Exception("SAP request failed: no response received");

        //    var body = await response.Content.ReadAsStringAsync();

        //    if (string.IsNullOrWhiteSpace(body))
        //        throw new Exception("SAP returned empty response body");

        //    return body;
        //}

        public Task<int> GetByIdSap(int sapId,string entityType, string id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateSap(int sapId,string entityType, T entity, string id)
        {
            throw new NotImplementedException();
        }
    
    }
}
