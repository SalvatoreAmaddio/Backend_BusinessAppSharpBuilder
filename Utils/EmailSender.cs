using MimeKit;
using MailKit.Net.Smtp;
using Backend.Exceptions;

namespace Backend.Utils
{
    /// <summary>
    /// Provides a way to send emails.
    /// <para/>
    /// How to use:
    /// <code>
    ///Send an email through a Gmail account:
    /// EmailSender es = new("smtp.gmail.com", "myEmail@gmail.com", "My Name", "Test");
    /// es.AddReceiver("myFriend@gmail.com", "My Friend Name");
    /// es.Body = "Ciao!";
    /// es.Send();
    /// </code>
    /// </summary>
    public class EmailSender
    {
        private MimeMessage Message { get; set; } = new MimeMessage();

        /// <summary>
        /// Gets or sets the email of the sender.
        /// </summary>
        public string SenderEmail { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name associated with the sender's email.
        /// </summary>
        public string SenderName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email's body.
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email's subject.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// List of strings containing attachment file paths.
        /// </summary>
        private List<string> Attachments { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the host (email provider).
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a flag indicating whether the <see cref="Host"/> requires authentication to send an email.
        /// </summary>
        public bool AuthenticationRequired { get; set; } = true;

        /// <summary>
        /// Gets or sets the port to use.
        /// </summary>
        public int Port { get; set; } = 587;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSender"/> class.
        /// </summary>
        public EmailSender() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSender"/> class with the specified parameters.
        /// </summary>
        /// <param name="host">The email provider host.</param>
        /// <param name="senderEmail">The sender's email address.</param>
        /// <param name="senderName">The sender's name.</param>
        /// <param name="subject">The subject of the email.</param>
        public EmailSender(string host, string senderEmail, string senderName, string subject)
        {
            this.Host = host;
            this.SenderEmail = senderEmail;
            this.SenderName = senderName;
            this.Subject = subject;
        }

        /// <summary>
        /// Prepares the body of the email, including attachments.
        /// </summary>
        private MimeEntity BuildBody()
        {
            BodyBuilder bodyBuilder = new()
            {
                TextBody = Body
            };

            foreach (string attachmentPath in Attachments)
                bodyBuilder.Attachments.Add(attachmentPath);

            return bodyBuilder.ToMessageBody();
        }

        private Task<MimeEntity> BuildBodyAsync() => Task.FromResult(BuildBody());

        /// <summary>
        /// Adds a new file path to the attachment list. If the path does not exist, an exception is thrown.
        /// </summary>
        /// <param name="path">The attachment's file path.</param>
        /// <exception cref="Exception">Thrown when the path provided does not exist.</exception>
        public void AddAttachment(string path)
        {
            if (!File.Exists(path)) throw new Exception("The path provided does not exist.");
            Attachments.Add(path);
        }

        /// <summary>
        /// Adds a receiver to the email.
        /// </summary>
        /// <param name="receiverAddress">The email address of the receiver.</param>
        /// <param name="receiverName">The name associated with the email address of the receiver.</param>
        public void AddReceiver(string receiverAddress, string receiverName) => Message.To.Add(new MailboxAddress(receiverName, receiverAddress));

        /// <summary>
        /// Adds a CC to the email.
        /// </summary>
        /// <param name="ccAddress">The email address of the CC.</param>
        /// <param name="ccName">The name associated with the email address of the CC.</param>
        public void AddCC(string ccAddress, string ccName) => Message.Cc.Add(new MailboxAddress(ccName, ccAddress));

        /// <summary>
        /// Sends the email asynchronously.
        /// </summary>
        /// <returns>A string indicating the server response.</returns>
        /// <exception cref="InvalidReceiver">Thrown when no receiver is specified.</exception>
        /// <exception cref="InvalidSender">Thrown when the sender email or name is not specified.</exception>
        /// <exception cref="InvalidHost">Thrown when the host is not specified.</exception>
        /// <exception cref="CredentialFailure">Thrown when authentication credentials cannot be retrieved.</exception>
        public async Task<string> SendAsync()
        {
            if (Message.To.Count == 0) throw new InvalidReceiver();
            if (string.IsNullOrEmpty(SenderEmail) || string.IsNullOrEmpty(SenderName)) throw new InvalidSender();
            if (string.IsNullOrEmpty(Host)) throw new InvalidHost();

            Message.From.Add(new MailboxAddress(SenderName, SenderEmail));
            Message.Subject = Subject;

            Task<MimeEntity> goAndBuildBody = BuildBodyAsync();

            using (var client = new SmtpClient())
            {
                try
                {
                    Task goAndConnect = client.ConnectAsync(Host, Port, MailKit.Security.SecureSocketOptions.StartTls);

                    await Task.WhenAll(goAndBuildBody, goAndConnect);

                    if (AuthenticationRequired)
                    {
                        SysCredentailTargets.EmailApp = SenderEmail;
                        Credential? credential = CredentialManager.Get(SysCredentailTargets.EmailApp) ?? throw new CredentialFailure("Failed to retrieve credentials for authentication");
                        Encrypter encrypter = new(credential.Password, SysCredentailTargets.EmailAppEncrypterSecretKey, SysCredentailTargets.EmailAppEncrypterIV);
                        await client.AuthenticateAsync(SenderEmail, encrypter.Decrypt());
                    }

                    Message.Body = goAndBuildBody.Result;

                    string serverResponse = await client.SendAsync(Message);
                    await client.DisconnectAsync(true);
                    return serverResponse;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send email: {ex.Message}");
                    return "500";
                }
            }
        }

        /// <summary>
        /// Checks if the EmailApp's <see cref="Credential"/> object associated with this email exists on the local computer.
        /// </summary>
        /// <returns>True if the credential is found; otherwise, false.</returns>
        public bool CredentialCheck()
        {
            SysCredentailTargets.EmailApp = SenderEmail;
            Credential? credential = CredentialManager.Get(SysCredentailTargets.EmailApp);
            return credential != null;
        }

        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <returns>A string indicating the server response.</returns>
        /// <exception cref="InvalidReceiver">Thrown when no receiver is specified.</exception>
        /// <exception cref="InvalidSender">Thrown when the sender email or name is not specified.</exception>
        /// <exception cref="InvalidHost">Thrown when the host is not specified.</exception>
        /// <exception cref="CredentialFailure">Thrown when authentication credentials cannot be retrieved.</exception>
        public string Send()
        {
            if (Message.To.Count == 0) throw new InvalidReceiver();
            if (string.IsNullOrEmpty(SenderEmail) || string.IsNullOrEmpty(SenderName)) throw new InvalidSender();
            if (string.IsNullOrEmpty(Host)) throw new InvalidHost();

            Message.From.Add(new MailboxAddress(SenderName, SenderEmail));
            Message.Subject = Subject;

            Message.Body = BuildBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect(Host, Port, MailKit.Security.SecureSocketOptions.StartTls);

                    if (AuthenticationRequired)
                    {
                        SysCredentailTargets.EmailApp = SenderEmail;
                        Credential? credential = CredentialManager.Get(SysCredentailTargets.EmailApp);
                        if (credential == null) throw new CredentialFailure("Failed to retrieve credentials for authentication");
                        Encrypter encrypter = new(credential.Password, SysCredentailTargets.EmailAppEncrypterSecretKey, SysCredentailTargets.EmailAppEncrypterIV);
                        client.Authenticate(SenderEmail, encrypter.Decrypt());
                    }

                    string serverResponse = client.Send(Message);
                    client.Disconnect(true);
                    return serverResponse;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send email: {ex.Message}");
                    return "505";
                }
            }
        }
    }

}
