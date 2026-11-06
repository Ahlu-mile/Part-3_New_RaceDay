namespace RaceDay.API.Security
{
    /// <summary>
    /// Central place for the session keys used to store the authenticated
    /// user's identity after login (see AuthController.Login).
    /// </summary>
    public static class SessionKeys
    {
        public const string UserId = "UserId";
        public const string Role = "Role";
        public const string FullName = "FullName";
    }
}
