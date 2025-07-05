using Mango.Services.EmailAPI.Messaging;

namespace Mango.Services.EmailAPI.Extensions
{
    // Diese statische Klasse erweitert IApplicationBuilder um die Möglichkeit,
    // einen Azure Service Bus Consumer beim Starten und Stoppen der Anwendung zu verwalten.
    public static class ApplicationBuilderExtensions
    {
        // Statische Property, um die Instanz des ServiceBusConsumers zu speichern.
        private static IAzureServiceBusConsumer? ServiceBusConsumer { get; set; }

        // Erweiterungsmethode für IApplicationBuilder, um den ServiceBusConsumer zu registrieren.
        public static IApplicationBuilder UseAzureServiceBusConsumer(this IApplicationBuilder app)
        {
            // Holt die Instanz des ServiceBusConsumers aus dem DI-Container.
            ServiceBusConsumer = app.ApplicationServices.GetService<IAzureServiceBusConsumer>();
            // Holt das Lebenszyklus-Objekt der Anwendung.
            var hostApplicationLife = app.ApplicationServices.GetService<IHostApplicationLifetime>();

            // Registriert die Methode OnStart, die beim Starten der Anwendung ausgeführt wird.
            hostApplicationLife.ApplicationStarted.Register(OnStart);
            // Registriert die Methode OnStop, die beim Stoppen der Anwendung ausgeführt wird.
            hostApplicationLife.ApplicationStopping.Register(OnStop);

            return app;
        }

        // Wird beim Stoppen der Anwendung aufgerufen.
        private static void OnStop()
        {
            // Stoppt den ServiceBusConsumer (z.B. beendet das Empfangen von Nachrichten).
            ServiceBusConsumer.Stop();
        }

        // Wird beim Starten der Anwendung aufgerufen.
        private static void OnStart()
        {
            // Startet den ServiceBusConsumer (z.B. beginnt mit dem Empfangen von Nachrichten).
            ServiceBusConsumer.Start();
        }
    }
}
