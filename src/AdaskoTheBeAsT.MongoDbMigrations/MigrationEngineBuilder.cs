using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using AdaskoTheBeAsT.MongoDbMigrations.Core;
using AdaskoTheBeAsT.MongoDbMigrations.Document;
using MongoDB.Driver;
using Renci.SshNet;

namespace AdaskoTheBeAsT.MongoDbMigrations;

/// <summary>
/// Builder for configuring and creating a <see cref="MigrationEngine"/>.
/// </summary>
public sealed class MigrationEngineBuilder
{
    private const string Localhost = "127.0.0.1";

    private SshConfig? _sshConfig;
    private SslSettings? _tlsSettings;

    /// <summary>
    /// Configures TLS settings for the MongoDB connection.
    /// </summary>
    /// <param name="certificate">The X509 certificate.</param>
    /// <returns>The builder for chaining.</returns>
    public MigrationEngineBuilder UseTls(X509Certificate2 certificate)
    {
        if (certificate == null)
        {
            throw new ArgumentNullException(nameof(certificate));
        }

        _tlsSettings = new SslSettings
        {
            ClientCertificates = [certificate],
        };

        return this;
    }

    /// <summary>
    /// Configures SSH tunnel with password authentication.
    /// </summary>
    /// <param name="sshAddress">SSH server address.</param>
    /// <param name="sshUser">SSH username.</param>
    /// <param name="sshPassword">SSH password.</param>
    /// <param name="mongoAddress">MongoDB server address.</param>
    /// <returns>The builder for chaining.</returns>
    public MigrationEngineBuilder UseSshTunnel(
        ServerAddressConfig sshAddress,
        string sshUser,
        string sshPassword,
        ServerAddressConfig mongoAddress)
    {
#pragma warning disable CC0022 // SshClient is disposed in MigrationEngine's Dispose method
        return EstablishConnectionViaSsh(
            new SshClient(sshAddress.Host, sshAddress.PortAsInt, sshUser, sshPassword),
            mongoAddress);
#pragma warning restore CC0022
    }

    /// <summary>
    /// Configures SSH tunnel with private key authentication.
    /// </summary>
    /// <param name="sshAddress">SSH server address.</param>
    /// <param name="sshUser">SSH username.</param>
    /// <param name="privateKeyFileStream">Private key file stream.</param>
    /// <param name="mongoAddress">MongoDB server address.</param>
    /// <param name="keyFilePassPhrase">Private key passphrase.</param>
    /// <returns>The builder for chaining.</returns>
    public MigrationEngineBuilder UseSshTunnel(
        ServerAddressConfig sshAddress,
        string sshUser,
        Stream privateKeyFileStream,
        ServerAddressConfig mongoAddress,
        string? keyFilePassPhrase = null)
    {
        using var keyFile = keyFilePassPhrase == null
            ? new PrivateKeyFile(privateKeyFileStream)
            : new PrivateKeyFile(privateKeyFileStream, keyFilePassPhrase);
#pragma warning disable CC0022 // SshClient is disposed in MigrationEngine's Dispose method
        return EstablishConnectionViaSsh(
            new SshClient(sshAddress.Host, sshAddress.PortAsInt, sshUser, keyFile),
            mongoAddress);
#pragma warning restore CC0022
    }

    /// <summary>
    /// Creates a migration engine using a connection string.
    /// The created engine will own and dispose the MongoDB client.
    /// </summary>
    /// <param name="connectionString">MongoDB connection string.</param>
    /// <param name="databaseName">Database name.</param>
    /// <param name="emulation">MongoDB emulation type.</param>
    /// <returns>A new migration engine.</returns>
    public MigrationEngine UseDatabase(
        string connectionString,
        string databaseName,
        MongoEmulation emulation = MongoEmulation.None)
    {
        var settings = MongoClientSettings.FromConnectionString(connectionString);
#pragma warning disable IDISP001 // Dispose created - ownership transferred to MigrationEngine
        var client = new MongoClient(settings);
#pragma warning restore IDISP001
        return CreateEngine(client, databaseName, emulation, ownsClient: true);
    }

    /// <summary>
    /// Creates a migration engine using an existing MongoDB client.
    /// The engine will NOT dispose the client - caller retains ownership.
    /// </summary>
    /// <param name="mongoClient">MongoDB client.</param>
    /// <param name="databaseName">Database name.</param>
    /// <param name="emulation">MongoDB emulation type.</param>
    /// <returns>A new migration engine.</returns>
    public MigrationEngine UseDatabase(
        IMongoClient mongoClient,
        string databaseName,
        MongoEmulation emulation = MongoEmulation.None)
    {
        if (mongoClient == null)
        {
            throw new ArgumentNullException(nameof(mongoClient));
        }

        return CreateEngine(mongoClient, databaseName, emulation, ownsClient: false);
    }

    private MigrationEngineBuilder EstablishConnectionViaSsh(SshClient client, ServerAddressConfig mongoAddress)
    {
        client.Connect();
        var forwardedPortLocal = new ForwardedPortLocal(Localhost, mongoAddress.Host, mongoAddress.Port);
        client.AddForwardedPort(forwardedPortLocal);
        forwardedPortLocal.Start();

        _sshConfig = new SshConfig(client, forwardedPortLocal, forwardedPortLocal.BoundPort, Localhost);

        return this;
    }

    private MigrationEngine CreateEngine(
        IMongoClient mongoClient,
        string databaseName,
        MongoEmulation emulation,
        bool ownsClient)
    {
        var database = mongoClient
            .SetTls(_tlsSettings)
            .SetSsh(_sshConfig)
            .GetDatabase(databaseName);

        return new MigrationEngine(mongoClient, database, emulation, ownsClient, _sshConfig);
    }
}
