using System;

namespace Bmf.Shared.Esb.Types
{
    public class ProcessingInformation
    {
        private DateTime _createdOn;
        public DateTime CreatedOn
        {
            get { return _createdOn; }
            set { _createdOn = value;
            Notifiy("CreatedOn", value);
            }
        }

        private DateTime _startedOn;
        public DateTime StartedOn
        {
            get { return _startedOn; }
            set { _startedOn = value;
            Notifiy("StartedOn", value);
            }
        }

        private DateTime _finishedOn;
        public DateTime FinishedOn
        {
            get { return _finishedOn; }
            set { _finishedOn = value;
            Notifiy("FinishedOn", value);
            }
        }

        private bool _started;
        public bool Started
        {
            get { return _started; }
            set { _started = value;
            Notifiy("Started", value);
            }
        }

        private bool _finished;
        public bool Finished
        {
            get { return _finished; }
            set { _finished = value;
            Notifiy("Finished", value);
            }
        }

        private bool _error;
        public bool Error
        {
            get { return _error; }
            set { _error = value;
            Notifiy("Error", value);
            }
        }

        private void Notifiy(string property, object value)
        {
           // Console.WriteLine("Property {0} was set to value {1}", property, value);
        }
    }
}