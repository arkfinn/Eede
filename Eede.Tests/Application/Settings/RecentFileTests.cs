using Eede.Application.Settings;
using NUnit.Framework;
using System;

namespace Eede.Application.Tests.Settings
{
    [TestFixture]
    public class RecentFileTests
    {
        [Test]
        public void DefaultConstructor_SetsExpectedDefaultValues()
        {
            var recentFile = new RecentFile();

            Assert.That(recentFile.Path, Is.EqualTo(string.Empty));
            Assert.That(recentFile.LastAccessed, Is.EqualTo(default(DateTime)));
        }

        [Test]
        public void PathProperty_CanBeSetAndRetrieved()
        {
            var expectedPath = "/some/test/path.png";
            var recentFile = new RecentFile
            {
                Path = expectedPath
            };

            Assert.That(recentFile.Path, Is.EqualTo(expectedPath));
        }

        [Test]
        public void LastAccessedProperty_CanBeSetAndRetrieved()
        {
            var expectedDate = new DateTime(2023, 10, 25, 12, 34, 56);
            var recentFile = new RecentFile
            {
                LastAccessed = expectedDate
            };

            Assert.That(recentFile.LastAccessed, Is.EqualTo(expectedDate));
        }
    }
}
