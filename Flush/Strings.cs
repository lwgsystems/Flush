namespace Flush
{
    public static class Strings
    {
        public static class Configuration
        {
            public static readonly string IdentityDb = @"IdentityDb";
            public static readonly string FlushDb = @"FlushDb";
            public static readonly string JwtAuthentication = @"JwtAuthentication";
            public static readonly string JwtIssuer = @"Flush";
            public static readonly string JwtAudience = @"Flush";
            public static readonly string AccessToken = @"access_token";
            public static readonly string SessionHubEndpoint = @"/session";
            public static readonly string ApplicationBaseUri = @"/app";
            public static readonly string ErrorEndpoint = @"/Error";
        }
        public static class SessionHub
        {
            public static readonly string PlayerPurged = @"PlayerPurged";
        }
    }
}
