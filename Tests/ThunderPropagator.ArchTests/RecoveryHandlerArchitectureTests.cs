using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using ThunderPropagator.RecoveryHandler.MongoDb;
using ThunderPropagator.RecoveryHandler.Postgresql;
using ThunderPropagator.RecoveryHandler.Redis;
using ThunderPropagator.RecoveryHandler.SharedKernel;

namespace ThunderPropagator.ArchTests;

public class RecoveryHandlerArchitectureTests
{
    private const string MongoDbNamespace = "ThunderPropagator.RecoveryHandler.MongoDb";
    private const string PostgresqlNamespace = "ThunderPropagator.RecoveryHandler.Postgresql";
    private const string RedisNamespace = "ThunderPropagator.RecoveryHandler.Redis";
    private const string SharedKernelNamespace = "ThunderPropagator.RecoveryHandler.SharedKernel";

    public static TheoryData<Assembly, string> RecoveryHandlerAssemblies => new()
    {
        { typeof(MongoDbRecoveryHandlerExtensions).Assembly, MongoDbNamespace },
        { typeof(PostgresqlRecoveryHandlerExtensions).Assembly, PostgresqlNamespace },
        { typeof(RedisRecoveryHandlerExtensions).Assembly, RedisNamespace },
        { typeof(ThunderPropagatorExtensions).Assembly, SharedKernelNamespace },
    };

    public static TheoryData<Assembly, string[]> ForbiddenDependencies => new()
    {
        {
            typeof(MongoDbRecoveryHandlerExtensions).Assembly,
            [PostgresqlNamespace, RedisNamespace]
        },
        {
            typeof(PostgresqlRecoveryHandlerExtensions).Assembly,
            [MongoDbNamespace, RedisNamespace]
        },
        {
            typeof(RedisRecoveryHandlerExtensions).Assembly,
            [MongoDbNamespace, PostgresqlNamespace]
        },
        {
            typeof(ThunderPropagatorExtensions).Assembly,
            [MongoDbNamespace, PostgresqlNamespace, RedisNamespace]
        },
    };

    [Theory]
    [MemberData(nameof(ForbiddenDependencies))]
    public void Assemblies_WhenInspected_MustNotDependOnSiblingBackends(
        Assembly assembly,
        string[] forbiddenNamespaces)
    {
        // Arrange
        var types = Types.InAssembly(assembly);

        // Act
        var result = types.Should().NotHaveDependencyOnAny(forbiddenNamespaces).GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "backend implementations must remain independently deployable; failing types: {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Theory]
    [MemberData(nameof(RecoveryHandlerAssemblies))]
    public void Assemblies_WhenInspected_MustKeepTypesInExpectedNamespace(
        Assembly assembly,
        string expectedNamespace)
    {
        // Arrange
        var productionTypes = assembly.DefinedTypes
            .Where(type => !type.IsNested && !type.Name.StartsWith('<'))
            .ToArray();

        // Act
        var misplacedTypes = productionTypes
            .Where(type => type.Namespace is null ||
                           !type.Namespace.StartsWith(expectedNamespace, StringComparison.Ordinal))
            .Select(type => type.FullName)
            .ToArray();

        // Assert
        misplacedTypes.Should().BeEmpty();
    }

    [Fact]
    public void RecoveryHandlerAssemblies_WhenDependencyGraphIsInspected_MustBeAcyclic()
    {
        // Arrange
        var assemblies = RecoveryHandlerAssemblies
            .Select(row => (Assembly)row[0])
            .Distinct()
            .ToDictionary(assembly => assembly.GetName().Name!, StringComparer.Ordinal);

        // Act
        var hasCycle = assemblies.Keys.Any(name =>
            HasCycle(name, assemblies, [], []));

        // Assert
        hasCycle.Should().BeFalse("recovery-handler project references must form an acyclic graph");
    }

    private static bool HasCycle(
        string assemblyName,
        IReadOnlyDictionary<string, Assembly> assemblies,
        HashSet<string> visited,
        HashSet<string> path)
    {
        if (!path.Add(assemblyName))
            return true;

        if (!visited.Add(assemblyName))
        {
            path.Remove(assemblyName);
            return false;
        }

        var hasCycle = assemblies[assemblyName]
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(reference => reference is not null && assemblies.ContainsKey(reference))
            .Any(reference => HasCycle(reference!, assemblies, visited, path));

        path.Remove(assemblyName);
        return hasCycle;
    }
}
