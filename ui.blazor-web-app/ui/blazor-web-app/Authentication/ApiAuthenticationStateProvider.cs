using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using ui.webapp.Services;

namespace ui.webapp.Authentication
{
    /// <summary>
    /// Manages the user's authentication state based on a JWT token.
    /// </summary>
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenManagerService _tokenManager;

        public ApiAuthenticationStateProvider(HttpClient httpClient, ITokenManagerService tokenManager)
        {
            _httpClient = httpClient;
            _tokenManager = tokenManager;
        }

        /// <summary>
        /// Gets the current authentication state of the user.
        /// </summary>
        /// <returns>A Task that resolves to the AuthenticationState.</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _tokenManager.GetTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())); // Anonymous user
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
        }

        /// <summary>
        /// Marks the user as authenticated, stores the token, and notifies the application of the state change.
        /// </summary>
        /// <param name="token">The JWT for the authenticated user.</param>
        public async Task MarkUserAsAuthenticated(string token)
        {
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
            await _tokenManager.SetTokenAsync(token);
            var authState = Task.FromResult(new AuthenticationState(claimsPrincipal));
            NotifyAuthenticationStateChanged(authState);
        }

        /// <summary>
        /// Marks the user as logged out, removes the token, and notifies the application of the state change.
        /// </summary>
        public async Task MarkUserAsLoggedOut()
        {
            await _tokenManager.RemoveTokenAsync();
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            _httpClient.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(authState);
        }

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            
            var jsonBytes = ParseBase64WithoutPadding(payload);
            
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs is null)
            {
                return claims;
            }

            keyValuePairs.TryGetValue(ClaimTypes.Role, out object? roles);

            if (roles != null)
            {
                if (roles.ToString()!.Trim().StartsWith("["))
                {
                    var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString()!);
                    if (parsedRoles is not null)
                    {
                        claims.AddRange(parsedRoles.Select(role => new Claim(ClaimTypes.Role, role)));
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()!));
                }
                keyValuePairs.Remove(ClaimTypes.Role);
            }

            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!)));
            
            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}