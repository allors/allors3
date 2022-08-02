namespace Allors.Database.Domain
{
    using System.Linq;
    using System.Text;
    using Allors.Database.Meta;

    public partial class EmailPolicy
    {
        private readonly ITransaction transaction;
        private readonly MetaPopulation m;

        public EmailPolicy(ITransaction transaction)
        {
            this.transaction = transaction;
            this.m = transaction.Database.Services.Get<MetaPopulation>();
        }

        public void Immediate()
        {
            var persons = new People(this.transaction).Extent();
            persons.Filter.AddEquals(this.m.Person.EmailFrequency, new EmailFrequencies(this.transaction).Immediate);

            foreach (Person person in persons)
            {
                this.ProcessNotifications(person, "Aviaco Notifications");
            }

            this.transaction.Derive();
            this.transaction.Commit();
        }

        public void Daily()
        {
            var persons = new People(this.transaction).Extent();
            persons.Filter.AddEquals(this.m.Person.EmailFrequency, new EmailFrequencies(this.transaction).Daily);

            foreach (Person person in persons)
            {
                this.ProcessNotifications(person, "Aviaco Notifications - Daily Digest");
            }

            this.transaction.Derive();
            this.transaction.Commit();
        }

        public void Weekly()
        {
            var persons = new People(this.transaction).Extent();
            persons.Filter.AddEquals(this.m.Person.EmailFrequency, new EmailFrequencies(this.transaction).Weekly);

            foreach (Person person in persons)
            {
                this.ProcessNotifications(person, "Aviaco Notifications - Weekly Digest");
            }

            this.transaction.Derive();
            this.transaction.Commit();
        }

        private void ProcessNotifications(Person person, string subject)
        {
            var notifications = person.NotificationList.UnconfirmedNotifications.Where(v => v.ShouldEmail).ToArray();

            if (notifications.Length > 0)
            {
                var emailMessage = new EmailMessageBuilder(this.transaction)
                    .WithRecipient(person)
                    .WithSubject(subject).Build();

                var body = new StringBuilder();

                body.Append("<h1>Overview</h1>\n");
                body.Append($"<p>You have {notifications.Length} new notifications.</p>\n");

                body.Append("<ul>\n");
                foreach (var notification in notifications)
                {
                    body.Append($"<li>{notification.Title}</li>\n");
                }

                body.Append("</ul>\n");

                body.Append("<h1>Detail</h1>\n");

                foreach (var notification in notifications)
                {
                    body.Append($"<p>{notification.Description}<p>\n");
                    body.Append("<hr>\n");
                }

                emailMessage.Body = body.ToString();

                foreach (var notification in notifications)
                {
                    notification.EmailMessage = emailMessage;
                }
            }
        }
    }
}
