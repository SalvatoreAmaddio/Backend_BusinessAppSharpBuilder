using MimeKit;
using MailKit.Net.Smtp;
using Backend.Exceptions;

namespace Backend.Utils
{
    /// <summary>
    /// This class provide a way of sending emails.<para/>
    /// How to use:
    /// <code>
    /// //Send an email trhough a gmail account:
    /// EmailSender es = new("smtp.gmail.com", "myEmail@gmail.com", "My Name","Test");
    /// es.AddReceiver("myFriend@gmail.com", "My Friend Name");
    /// es.Body = "Ciao!";
    /// es.Send();
    /// </code>
    /// </summary>
    public class EmailSender
    {
        private MimeMessage Message { get; set; } = new MimeMessage();

        /// <summary>
        /// Gets and Sets the email of the sender.
        /// </summary>
        public string SenderEmail { get; set; } = string.Empty;

        /// <summary>
        /// Gets and Sets the name associated with the Sender's email
        /// </summary>
        public string SenderName { get; set;} = string.Empty;

        /// <summary>
        /// Gets and Sets the email's body
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Gets and Sets the email's subject
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// List of strings containing Attachment's file paths.
        /// </summary>        
        private List<string> Attachments { get; set; } = [];

        /// <summary>
        /// Gets and Sets the host (email provider)
        /// </summary>
        public string Host { get; set; } = string.Empty; //"smtp.gmail.com";

        /// <summary>
        /// Gets and Sets a flag telling if the <see cref="Host"/> requires authentication to send an email.
        /// </summary>
        public bool AuthenticationRequired { get; set; } = true;

        /// <summary>
        /// Gets and Sets the Port to use.
        /// </summary>
        public int Port { get; set; } = 587;

        public EmailSender() { }

        public EmailSender(string host, string senderEmail, string senderName, string subject)
        {
            this.Host = host;
            this.SenderEmail = senderEmail;
            this.SenderName = senderName;
            this.Subject = subject;
        }
        private Task<MimeEntity> BuildBodyAsync() 
        {
            return Task.FromResult(BuildBody());
        }

        /// <summary>
        /// Prepare the body of the email, attachments included.
        /// </summary>
        private MimeEntity BuildBody() 
        {
            BodyBuilder bodyBuilder = new()
            {
                TextBody = Body
            };

            foreach(string attachmentPath in Attachments)
                bodyBuilder.Attachments.Add(attachmentPath);

            return bodyBuilder.ToMessageBody();
        }

        /// <summary>
        /// Add a new file's path to the Attachment list. If the path does not exist, it will throw an Exception.
        /// </summary>
        /// <param name="path">the Attachment's file path</param>
        /// <exception cref="Exception"></exception>
        public void AddAttachment(string path)
        {
            if (!File.Exists(path)) throw new Exception("The path provided does not exist.");
            Attachments.Add(path);
        }

        /// <summary>
        /// Add a receiver.
        /// </summary>
        /// <param name="receiverAddress">the email address of the receiver</param>
        /// <param name="receiverName">the name associated to the email address of the receiver</param>
        public void AddReceiver(string receiverAddress, string receiverName) => Message.To.Add(new MailboxAddress(receiverName, receiverAddress));


        /// <summary>
        /// Add a CC.
        /// </summary>
        /// <param name="ccAddress">the email address of the CC</param>
        /// <param name="ccName">the name associated to the email address of the CC</param>
        public void AddCC(string ccAddress, string ccName) => Message.Cc.Add(new MailboxAddress(ccName, ccAddress));

        /// <summary>
        /// Send the email Asynchronously.
        /// </summary>
        /// <returns>A string which tells the Server Response.</returns>
        /// <exception cref="InvalidReceiver"></exception>
        /// <exception cref="InvalidSender"></exception>
        /// <exception cref="InvalidHost"></exception>
        /// <exception cref="CredentialFailure"></exception>
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
                    Task goAndConnect = client.ConnectAsync(Host, 587, MailKit.Security.SecureSocketOptions.StartTls);

                    await Task.WhenAll(goAndBuildBody, goAndConnect);

                    if (AuthenticationRequired)
                    {
                        SysCredentailTargets.EmailApp = SenderEmail;
                        Credential? credential = CredentialManager.Get(SysCredentailTargets.EmailApp) ?? throw new CredentialFailure("Failed to retrieve credentials for authentication");
                        Encrypter encrypter = new(credential.Password, SysCredentailTargets.EmailAppEncrypterKey, SysCredentailTargets.EmailAppEncrypterIV);
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
        /// Check if the EmailApp's <see cref="Credential"/> object associated to this email exist in the local computer.
        /// </summary>
        /// <returns>true if found</returns>
        public bool CredentialCheck() 
        {
            SysCredentailTargets.EmailApp = SenderEmail;
            Credential? credential = CredentialManager.Get(SysCredentailTargets.EmailApp);
            return credential != null;
        }

        /// <summary>
        /// Send the email.
        /// </summary>
        /// <returns>A string which tells the Server Response.</returns>
        /// <exception cref="InvalidReceiver"></exception>
        /// <exception cref="InvalidSender"></exception>
        /// <exception cref="InvalidHost"></exception>
        /// <exception cref="CredentialFailure"></exception>
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
                    client.Connect(Host, 587, MailKit.Security.SecureSocketOptions.StartTls);

                    if (AuthenticationRequired) 
                    {
                        SysCredentailTargets.EmailApp = SenderEmail;
                        Credential? credential = CredentialManager.Get(SysCredentailTargets.EmailApp);
                        if (credential == null) throw new CredentialFailure("Failed to retrieve credentials for authentication");
                        Encrypter encrypter = new(credential.Password, SysCredentailTargets.EmailAppEncrypterKey, SysCredentailTargets.EmailAppEncrypterIV);
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
