using System;
using System.Threading;

namespace Bmf.Shared.Caching
{
    public class CacheOptions<TKey> : ICacheOptions where TKey : class
    {
        private readonly object _localDataLock = new object();

        private volatile TKey _data;

        private volatile bool _isReloading;

        private string _key;

        private string _subKey;

        public Func<TKey> DataLoadFunction;

        /// <summary>
        ///     die eigentlichen Daten
        /// </summary>
        public TKey Data
        {
            get
            {
                if (_data == null)
                {
                    lock (_localDataLock)
                    {
                        if (_data == null)
                        {
                            if (DataLoadFunction == null)
                                throw new NullReferenceException(
                                    "Es muss eine Ladefunktion zur delegate \"DataLoadFunction\" hinzugefügt werden");
                            _data = DataLoadFunction();
                        }
                    }
                }
                return _data;
            }
        }

        //resets the Element, and reloads it
        public bool Reset { get; set; }

        /// <summary>
        ///     Zu diesem Zeitpunkt erstellt
        /// </summary>
        public DateTime CreatedOn { get; } = DateTime.Now;

        /// <summary>
        ///     Letzter Zugriff auf Datenobjekt
        /// </summary>
        public DateTime LastAccess { get; set; }

        /// <summary>
        ///     Nach dieser Zeit wird das Objekt auf alle Fälle zerstört
        /// </summary>
        public TimeSpan MaxCachingTime { get; set; }

        /// <summary>
        ///     Objekt wird zerstört wenn nicht innerhalb von dieser Zeit wieder abgerufen
        ///     bzw. neu geladen
        /// </summary>
        public TimeSpan MaxOnHoldTime { get; set; }

        /// <summary>
        ///     Objekt ist abhängig von Session
        /// </summary>
        public bool SessionDependend { get; set; }

        /// <summary>
        ///     ProojektId (Guid) für Cache-Key
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        ///     UserId (Guid) für Cache-Key
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        ///     True = Wenn abgelaufen wird das ChachingObject neu aufgebaut.
        ///     False = Default.
        /// </summary>
        public bool ReloadAfterExpire { get; set; }

        public bool IsExpired
        {
            get
            {
                return (CreatedOn + MaxCachingTime < DateTime.Now)
                       || ((LastAccess + MaxOnHoldTime < DateTime.Now) && ReloadAfterExpire == false);
            }
        }

        public bool MustReload
        {
            get { return (DateTime.Now > LastAccess + MaxOnHoldTime) && ReloadAfterExpire; }
        }

        public string SubKey
        {
            get { return _subKey; }
            set
            {
                if (!string.IsNullOrEmpty(_subKey) && _subKey != value)
                    throw new Exception("Subkey darf nur initial mit einem Wert belegt werden");
                _subKey = value;
            }
        }

        public string Key
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_key))
                    return _key;
                _key = _subKey + "|";
                _key += ProjectId;
                if (SessionDependend)
                    _key += "|" + UserId;
                return _key;
            }
        }


        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ProjectId))
                throw new NullReferenceException("Das benötigte Feld \"ProjectId\" wurde nicht belegt.");
            if (string.IsNullOrWhiteSpace(SubKey))
                throw new NullReferenceException("Das benötigte Feld \"SubKey\" wurde nicht belegt.");
            if (SessionDependend && string.IsNullOrWhiteSpace(UserId))
                throw new NullReferenceException("Das benötigte Feld \"UserId\" wurde nicht belegt.");
        }

        public void ResetCreatetOn()
        {
            // Es werden erst die Daten neu geladen bevor das alte Objekt ausgetauscht wird.

            LastAccess = DateTime.Now;
            //merken, dass wir bereits neu laden
            if (_isReloading) return;
            _isReloading = true;

            //Daten neu Laden in eigenem Thread
            ThreadPool.QueueUserWorkItem(o =>
            {
                //Daten laden
                var reloaddata = DataLoadFunction();
                lock (_localDataLock)
                {
                    //Daten tauschen
                    _data = reloaddata;
                }
                //reload beenden
                _isReloading = false;
            });
        }

        /// <summary>
        ///     Gibt an, ob das objekt noch zur verfügung sthet
        /// </summary>
        public bool IsValid
        {
            get { return (CreatedOn + MaxCachingTime > DateTime.Now) && (LastAccess + MaxOnHoldTime > DateTime.Now); }
        }
    }
}