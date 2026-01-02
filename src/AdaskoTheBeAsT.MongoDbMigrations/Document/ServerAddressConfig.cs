using System;

namespace AdaskoTheBeAsT.MongoDbMigrations.Document;

public class ServerAddressConfig
{
    private string _host = string.Empty;
    private uint _port = 0;

    public string Host
    {
        get
        {
            return _host;
        }

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            _host = value;
        }
    }

    public uint Port
    {
        get
        {
            return _port;
        }

        set
        {
            if (value > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Port number must be less or equal 65535");
            }

            _port = value;
        }
    }

    public int PortAsInt
    {
        get { return (int)_port; }
    }
}
