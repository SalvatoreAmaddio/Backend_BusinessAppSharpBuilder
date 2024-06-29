namespace Backend.Model
{
    /// <summary>
    /// This interface defines a set of properties and methods that a User class should implement.
    /// </summary>
    public interface IUser
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// Gets or sets the username of the user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password of the user.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user credentials should be remembered.
        /// If set to true, the <see cref="Login(string?)"/> method will call <see cref="SaveCredentials"/>.
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        /// Gets the number of remaining attempts allowed for login.
        /// </summary>
        public int Attempts { get; }

        /// <summary>
        /// Gets the target string to be used when storing <see cref="Utils.Credential"/> objects.
        /// A Target is a string that represents the credential's unique identifier.
        /// </summary>
        public string Target { get; }

        /// <summary>
        /// Attempts to log in with the specified password.
        /// If <see cref="RememberMe"/> is set to true, <see cref="SaveCredentials"/> is called.
        /// If the login is not successful, the <see cref="Attempts"/> property is decremented by 1.
        /// If <see cref="Attempts"/> reaches zero, this method should throw an exception if called again.
        /// </summary>
        /// <param name="pwd">The password to be checked against.</param>
        /// <returns>true if the login attempt is successful; otherwise, false.</returns>
        /// <exception cref="Exception">Thrown if <see cref="Attempts"/> is zero and another login attempt is made.</exception>
        public bool Login(string? pwd);

        /// <summary>
        /// Deletes the <see cref="Utils.Credential"/> object that was stored by <see cref="SaveCredentials"/>.
        /// </summary>
        public void Logout();

        /// <summary>
        /// Stores a <see cref="Utils.Credential"/> object on the local computer.
        /// This method is called within the <see cref="Login(string?)"/> method if the <see cref="RememberMe"/> property is set to true.
        /// </summary>
        public void SaveCredentials();

        /// <summary>
        /// Resets the <see cref="Attempts"/> property to its original value.
        /// </summary>
        public void ResetAttempts();
    }

}
