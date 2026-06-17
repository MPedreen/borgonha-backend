using Borgonha.IntegrationTests.Infrastructure;
using Xunit;

namespace Borgonha.IntegrationTests;

[CollectionDefinition(nameof(BorgonhaCollection))]
public sealed class BorgonhaCollection : ICollectionFixture<BorgonhaWebAppFactory>;
