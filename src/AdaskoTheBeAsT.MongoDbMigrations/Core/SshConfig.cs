using Renci.SshNet;

namespace AdaskoTheBeAsT.MongoDbMigrations.Core;

/// <summary>
/// SSH configuration for tunneling.
/// </summary>
internal sealed class SshConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SshConfig"/> class.
    /// </summary>
    /// <param name="sshClient">SSH client.</param>
    /// <param name="forwardedPortLocal">Forwarded port.</param>
    /// <param name="boundPort">Bound port.</param>
    /// <param name="boundHost">Bound host.</param>
    public SshConfig(
        SshClient sshClient,
        ForwardedPortLocal forwardedPortLocal,
        uint boundPort,
        string boundHost)
    {
        SshClient = sshClient;
        ForwardedPortLocal = forwardedPortLocal;
        BoundPort = boundPort;
        BoundHost = boundHost;
    }

    /// <summary>
    /// Gets the SSH client.
    /// </summary>
    public SshClient SshClient { get; }

    /// <summary>
    /// Gets the forwarded port.
    /// </summary>
    public ForwardedPortLocal ForwardedPortLocal { get; }

    /// <summary>
    /// Gets the bound port.
    /// </summary>
    public uint BoundPort { get; }

    /// <summary>
    /// Gets the bound host.
    /// </summary>
    public string BoundHost { get; }
}
