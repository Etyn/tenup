using SendGrid;
using SendGrid.Helpers.Mail;
using Tenup.Repositories.Entities;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Tenup.Services
{
    public class EmailService
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly List<string> _recipientEmails;

        public EmailService(ISendGridClient sendGridClient, IConfiguration configuration)
        {
            _sendGridClient = sendGridClient;
            _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@tenup.com";
            _fromName = configuration["SendGrid:FromName"] ?? "Tenup Notifications";
            
            // Liste des emails destinataires depuis la configuration
            var recipients = configuration["SendGrid:RecipientEmails"];
            _recipientEmails = !string.IsNullOrEmpty(recipients) 
                ? recipients.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>();
        }

        public async Task SendNewTournamentsEmailAsync(List<TournoiDb> nouveauxtournois)
        {
            if (!nouveauxtournois.Any() || !_recipientEmails.Any())
                return;

            var subject = $"Nouveaux tournois disponibles - {nouveauxtournois.Count} tournoi(s)";
            var htmlContent = GenerateHtmlContent(nouveauxtournois);
            var plainTextContent = GeneratePlainTextContent(nouveauxtournois);

            var from = new EmailAddress(_fromEmail, _fromName);
            var tos = _recipientEmails.Select(email => new EmailAddress(email)).ToList();

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(
                from, 
                tos, 
                subject, 
                plainTextContent, 
                htmlContent
            );

            var response = await _sendGridClient.SendEmailAsync(msg);
            
            // Log du statut de l'envoi
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                Console.WriteLine($"Email envoyé avec succès pour {nouveauxtournois.Count} nouveaux tournois");
            }
            else
            {
                Console.WriteLine($"Erreur lors de l'envoi de l'email: {response.StatusCode}");
            }
        }

        private string GenerateHtmlContent(List<TournoiDb> tournois)
        {
            var html = new StringBuilder();
            html.AppendLine("<html><body>");
            html.AppendLine("<h2>Nouveaux tournois de tennis disponibles</h2>");
            html.AppendLine($"<p>Nous avons trouvé <strong>{tournois.Count}</strong> nouveau(x) tournoi(s) :</p>");
            
            html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; width: 100%;'>");
            html.AppendLine("<thead>");
            html.AppendLine("<tr style='background-color: #f0f0f0;'>");
            html.AppendLine("<th>Nom du tournoi</th>");
            html.AppendLine("<th>Ville</th>");
            html.AppendLine("<th>Date début</th>");
            html.AppendLine("<th>Date fin</th>");
            html.AppendLine("<th>Club</th>");
            html.AppendLine("</tr>");
            html.AppendLine("</thead>");
            html.AppendLine("<tbody>");

            foreach (var tournoi in tournois.OrderBy(t => t.DateDebut))
            {
                html.AppendLine("<tr>");
                html.AppendLine($"<td><strong>{tournoi.Libelle}</strong></td>");
                html.AppendLine($"<td>{tournoi.VilleEngagement}</td>");
                html.AppendLine($"<td>{tournoi.DateDebut?.Date}</td>");
                html.AppendLine($"<td>{tournoi.DateFin?.Date}</td>");
                html.AppendLine($"<td>{tournoi.NomClub}</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");
            html.AppendLine("<br>");
            html.AppendLine("<p><em>Email envoyé automatiquement par le système Tenup</em></p>");
            html.AppendLine("</body></html>");

            return html.ToString();
        }

        private string GeneratePlainTextContent(List<TournoiDb> tournois)
        {
            var text = new StringBuilder();
            text.AppendLine("Nouveaux tournois de tennis disponibles");
            text.AppendLine("=====================================");
            text.AppendLine();
            text.AppendLine($"Nous avons trouvé {tournois.Count} nouveau(x) tournoi(s) :");
            text.AppendLine();

            foreach (var tournoi in tournois.OrderBy(t => t.DateDebut))
            {
                text.AppendLine($"• {tournoi.Libelle}");
                text.AppendLine($"  Ville: {tournoi.VilleEngagement}");
                text.AppendLine($"  Date début: {tournoi.DateDebut?.Date}");
                text.AppendLine($"  Date fin: {tournoi.DateFin?.Date}");
                text.AppendLine($"  Club: {tournoi.NomClub}");
                text.AppendLine();
            }

            text.AppendLine("Email envoyé automatiquement par le système Tenup");
            return text.ToString();
        }
    }
}