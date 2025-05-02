using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailService
{
    public virtual async Task SendActivationEmail(string recipientEmail, string activationLink)
    {
        var message = new MailMessage
        {
            From = new MailAddress("nejla.cajdin@gmail.com"),
            Subject = "Aktivirajte svoj račun na aplikaciji Šapica",
            Body = $"Kliknite sljedeći link kako biste aktivirali svoj račun: <a href='{activationLink}'>Aktiviraj</a>",
            IsBodyHtml = true
        };
        message.To.Add(recipientEmail);

        using var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("nejla.cajdin@gmail.com", "kljtdhkptcdtmwxw"), // App Password
            EnableSsl = true
        };

        try
        {
            await smtpClient.SendMailAsync(message);
            Console.WriteLine("Activation email sent successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email sending failed: {ex.Message}");
            throw; 
        }
    }
}
