using System;

namespace Bmf.Shared.Caching
{
    public interface ICacheOptions
    {
        /// <summary>
        ///     Zu diesem Zeitpunkt erstellt
        /// </summary>
        DateTime CreatedOn { get; }

        /// <summary>
        ///     Letzter Zugriff auf Datenobjekt
        /// </summary>
        DateTime LastAccess { get; set; }

        /// <summary>
        ///     Nach dieser Zeit wird das Objekt auf alle Fälle zerstört
        /// </summary>
        TimeSpan MaxCachingTime { get; set; }

        /// <summary>
        ///     Objekt wird zerstört, wenn nicht innerhalb von dieser Zeit wieder abgerufen
        /// </summary>
        TimeSpan MaxOnHoldTime { get; set; }

        /// <summary>
        ///     Gibt an, ob das objekt noch zur verfügung sthet
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        ///     Gibt an, dass das Objekt nicht mehr gültig ist und komplett gelöscht werden muss.
        /// </summary>
        bool IsExpired { get; }

        /// <summary>
        ///     Gibt an, dass das Objekt neu geladen werden muss
        /// </summary>
        bool MustReload { get; }

        /// <summary>
        ///     Objekt ist abhängig von Session
        /// </summary>
        bool SessionDependend { get; set; }

        /// <summary>
        ///     ProojektId (Guid) für Cache-Key
        /// </summary>
        string ProjectId { get; set; }

        /// <summary>
        ///     UserId (Guid) für Cache-Key
        /// </summary>
        string UserId { get; set; }

        /// <summary>
        ///     True = Wenn abgelaufen wird das ChachingObject neu aufgebaut.
        ///     False = Default.
        /// </summary>
        bool ReloadAfterExpire { get; set; }

        string SubKey { get; set; }
        string Key { get; }
        bool Reset { get; set; }

        /// <summary>
        ///     Bereinig das Objekt für ein neuladen.
        /// </summary>
        void ResetCreatetOn();

        void Validate();
    }
}