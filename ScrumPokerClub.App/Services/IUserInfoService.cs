namespace ScrumPokerClub
{
    /// <summary>
    /// Contract defining expected characteristics of an authenticated user.
    /// </summary>
    interface IUserInfoService
    {
        /// <summary>
        /// The friendly name of the user.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The email address of the user.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// The object identifier of the user.
        /// </summary>
        public string Identifier { get; }
    }
}
