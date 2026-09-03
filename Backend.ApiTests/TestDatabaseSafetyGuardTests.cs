using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class TestDatabaseSafetyGuardTests
{
    [TestCase("LMS")]
    [TestCase("master")]
    [TestCase("tempdb")]
    [TestCase("model")]
    [TestCase("msdb")]
    [TestCase("")]
    [TestCase("Production")]
    public void EnsureAllowedDatabaseName_RejectsEveryNonTestDatabase(string databaseName)
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            TestDatabaseSafetyGuard.EnsureAllowedDatabaseName(databaseName));

        Assert.That(exception!.Message, Is.EqualTo(TestDatabaseSafetyGuard.UnsafeDatabaseMessage));
    }

    [Test]
    public void EnsureAllowedDatabaseName_AllowsOnlyLmsTestPrefix()
    {
        Assert.DoesNotThrow(() => TestDatabaseSafetyGuard.EnsureAllowedDatabaseName("LMS_TEST_TASK7C"));
    }
}
