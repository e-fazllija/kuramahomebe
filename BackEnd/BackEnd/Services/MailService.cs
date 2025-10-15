using Azure.Core;
using BackEnd.Interfaces;
using BackEnd.Models.InputModels;
using BackEnd.Models.MailModels;
using BackEnd.Models.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BackEnd.Services;

public class MailService : IMailService
{
    private readonly MailOptions _mailSettings;
    public MailService(IOptions<MailOptions> mailSettings)
    {
        _mailSettings = mailSettings.Value;
    }

    public async Task SendEmailAsync(MailRequest mailRequest)
    {
        var email = new MimeMessage();
        email.Sender = MailboxAddress.Parse("info@thinkhome.it");
        email.From.Add(InternetAddress.Parse("info@thinkhome.it"));
        email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
        if (mailRequest.CcEmail.Count > 0)
        {
            foreach (var item in mailRequest.CcEmail)
            {
                email.Cc.Add(InternetAddress.Parse(item));
            }
        }
        email.Subject = mailRequest.Subject;
        var builder = new BodyBuilder();
        if (mailRequest.Attachments != null)
        {
            byte[] fileBytes;
            foreach (var file in mailRequest.Attachments)
            {
                if (file.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        fileBytes = ms.ToArray();
                    }
                    builder.Attachments.Add(file.FileName, fileBytes, MimeKit.ContentType.Parse(file.ContentType));
                }
            }
        }
        builder.HtmlBody = mailRequest.Body;
        email.Body = builder.ToMessageBody();
        using var smtp = new SmtpClient();
        smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.SslOnConnect);
        smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
        await smtp.SendAsync(email);
        smtp.Disconnect(true);
    }

    public async Task SendEvaluationRequestAsync(SendRequestModel mailRequest)
    {
        try
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse("info@thinkhome.it");
            email.From.Add(InternetAddress.Parse("info@thinkhome.it"));
            email.To.Add(MailboxAddress.Parse("info@thinkhome.it"));

            email.Subject = $"Richiesta di valutazione, richiesta di {mailRequest.FirstName} {mailRequest.LastName}";
            var builder = new BodyBuilder();

            string body = $"<strong>Nome:</strong> {mailRequest.FirstName}, <strong>Cognome:</strong> {mailRequest.LastName}, <strong>Email:</strong> {mailRequest.FromEmail}, <strong>Tel - Cell:</strong> {mailRequest.Phone} - {mailRequest.MobilePhone} <br><br>" +
                $"<strong>Dati immobile:</strong> <br>" +
                $"<strong>Contratto:</strong> {mailRequest.RequestType}<br>" +
                $"<strong>Tipologia:</strong> {mailRequest.PropertyType}<br>" +
                $"<strong>Provincia:</strong> {mailRequest.Province}<br>" +
                $"<strong>Località:</strong> {mailRequest.Location}<br>" +
                $"<strong>Indirizzo:</strong> {mailRequest.Address ?? "Non specificato"}<br>" +
                $"<strong>Numero vani:</strong> {mailRequest.NumberRooms}<br>" +
                $"<strong>Numero camere:</strong> {mailRequest.NumberBedRooms}<br>" +
                $"<strong>Numero servizi:</strong> {mailRequest.NumberServices}<br>" +
                $"<strong>Metri quadri:</strong> {mailRequest.MQ} <br>" +
                $"<strong>Giardino:</strong> {(mailRequest.Garden ? "Si" : "No")}<br>" +
                $"<strong>Terrazzo:</strong> {(mailRequest.Terrace ? "Si" : "No")}<br>" +
                $"<strong>Ascensore:</strong> {(mailRequest.Lift ? "Si" : "No")}<br>" +
                $"<strong>Arredato:</strong> {(mailRequest.Furnished ? "Si" : "No")}<br>" +
                $"<strong>Ricaldamento:</strong> {mailRequest.Heating}<br>" +
                $"<strong>Poto auto:</strong> {mailRequest.Box}<br>" +
                $"<strong>Canone mensile / prezzo:</strong> {mailRequest.Price}<br>" +
                $"<strong>Informazioni supplementari:</strong> {mailRequest.Information}<br><br>" +
                $"<strong>Messaggio:</strong><br><br>";

            builder.HtmlBody = body += mailRequest.Body;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.SslOnConnect);
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task SendWorkWithUsRequestAsync(SendRequestModel mailRequest)
    {
        try
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse("info@thinkhome.it");
            email.From.Add(InternetAddress.Parse("info@thinkhome.it"));
            email.To.Add(MailboxAddress.Parse("info@thinkhome.it"));

            email.Subject = $"Lavora con noi, richiesta di {mailRequest.FirstName} {mailRequest.LastName}";
            var builder = new BodyBuilder();

            string body =
                $"<strong>Nome:</strong> {mailRequest.FirstName}, <strong>Cognome:</strong> {mailRequest.LastName}, <strong>Email:</strong> {mailRequest.FromEmail}, <strong>Tel - Cell:</strong> {mailRequest.Phone} - {mailRequest.MobilePhone} <br><br>" +
                $"<strong>Messaggio:</strong><br><br>";
            builder.HtmlBody = body += mailRequest.Body;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.SslOnConnect);
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task SendRequestAsync(SendRequestModel mailRequest)
    {
        try
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse("info@thinkhome.it");
            email.From.Add(InternetAddress.Parse("info@thinkhome.it"));
            email.To.Add(MailboxAddress.Parse("info@thinkhome.it"));

            email.Subject = $"Nuova richiesta di {mailRequest.FirstName} {mailRequest.LastName}";
            var builder = new BodyBuilder();

            string body = $"Invio di una richiesta di valutazione da:<br>" +
                $"<strong>Nome:</strong> {mailRequest.FirstName}, <strong>Cognome:</strong> {mailRequest.LastName}, <strong>Email:</strong> {mailRequest.FromEmail}, <strong>Tel - Cell:</strong> {mailRequest.Phone} - {mailRequest.MobilePhone} <br><br>" +
                $"<strong>Dati immobile:</strong> <br>" +
                $"<strong>Contratto:</strong> {mailRequest.RequestType}<br>" +
                $"<strong>Tipologia:</strong> {mailRequest.PropertyType}<br>" +
                $"<strong>Provincia:</strong> {mailRequest.Province}<br>" +
                $"<strong>Località:</strong> {mailRequest.Location}<br>" +
                $"<strong>Indirizzo:</strong> {mailRequest.Address ?? "Non specificato"}<br>" +
                $"<strong>Numero vani:</strong> {mailRequest.NumberRooms}<br>" +
                $"<strong>Numero camere:</strong> {mailRequest.NumberBedRooms}<br>" +
                $"<strong>Numero servizi:</strong> {mailRequest.NumberServices}<br>" +
                $"<strong>Metri quadri:</strong> {mailRequest.MQ} <br>" +
                $"<strong>Giardino:</strong> {(mailRequest.Garden ? "Si" : "No")}<br>" +
                $"<strong>Terrazzo:</strong> {(mailRequest.Terrace ? "Si" : "No")}<br>" +
                $"<strong>Ascensore:</strong> {(mailRequest.Lift ? "Si" : "No")}<br>" +
                $"<strong>Arredato:</strong> {(mailRequest.Furnished ? "Si" : "No")}<br>" +
                $"<strong>Ricaldamento:</strong> {mailRequest.Heating}<br>" +
                $"<strong>Poto auto:</strong> {mailRequest.Box}<br>" +
                $"<strong>Canone mensile / prezzo:</strong> {mailRequest.Price}<br>" +
                $"<strong>Informazioni supplementari:</strong> {mailRequest.Information}<br><br>" +
                $"<strong>Messaggio:</strong><br><br>";
            builder.HtmlBody = body += mailRequest.Body;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.SslOnConnect);
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task InformationRequestAsync(SendRequestModel mailRequest)
    {
        try
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse("info@thinkhome.it");
            email.From.Add(InternetAddress.Parse("info@thinkhome.it"));
            email.To.Add(MailboxAddress.Parse("info@thinkhome.it"));

            email.Subject = $"Informazioni per immobile Cod. 00{mailRequest.Information}, richiesta di {mailRequest.FirstName} {mailRequest.LastName}";
            var builder = new BodyBuilder();

            string body =
                $"<strong>Nome:</strong> {mailRequest.FirstName}, <strong>Cognome:</strong> {mailRequest.LastName}, <strong>Email:</strong> {mailRequest.FromEmail}, <strong>Tel - Cell:</strong> {mailRequest.Phone} - {mailRequest.MobilePhone} <br><br>" +
                $"<strong>Messaggio:</strong><br><br>";
            builder.HtmlBody = body += mailRequest.Body;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.SslOnConnect);
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}