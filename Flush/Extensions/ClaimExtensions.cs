using System;
using System.Security.Claims;

namespace Flush.Extensions
{
    public static class ClaimExtensions
    {
        private static readonly int NAME = 0;
        private static readonly int ROOM = 1;

        public static string GetFlushUsername(this Claim claim)
        {
            if (claim.Type != ClaimTypes.NameIdentifier)
                throw new ArgumentException("Claim does not have the NameIdentifier type.");
            return claim.Value.Split('_')[NAME];
        }

        public static string GetFlushRoom(this Claim claim)
        {
            if (claim.Type != ClaimTypes.NameIdentifier)
                throw new ArgumentException("Claim does not have the NameIdentifier type.");
            return claim.Value.Split('_')[ROOM];
        }
    }
}
