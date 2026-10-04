using AnvilClicker.Data;
using NUnit.Framework;
using UnityEditor;

namespace AnvilClicker.Tests
{
    /// <summary>Runs the validator against the real project data, so a bad asset fails CI instead of the game.</summary>
    public class DatabaseValidationTests
    {
        const string DatabasePath = "Assets/_Project/ScriptableObjects/GameDatabase.asset";

        [Test]
        public void ProjectDatabase_HasNoErrors()
        {
            var database = AssetDatabase.LoadAssetAtPath<GameDatabase>(DatabasePath);
            Assert.That(database, Is.Not.Null, $"Missing {DatabasePath}. Run Tools/Anvil Clicker/Setup/Run All.");

            var errors = GameDatabaseValidator.Validate(database);

            Assert.That(errors, Is.Empty, string.Join("\n", errors));
        }

        [Test]
        public void ProjectDatabase_HasShopContent()
        {
            var database = AssetDatabase.LoadAssetAtPath<GameDatabase>(DatabasePath);

            Assert.That(database.UpgradeAssets, Is.Not.Empty);
            Assert.That(database.ApprenticeAssets, Is.Not.Empty);
        }
    }
}
