namespace SeasbrokerWebAPI.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<SqlServerIntegrationFixture>
{
    public const string Name = "SqlServer Integration Tests";
}
