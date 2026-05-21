using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Eede.Tests.ArchTests
{
    [TestFixture]
    public class ArchitectureTests
    {
        private static readonly Assembly DomainAssembly = typeof(Eede.Domain.Palettes.Palette).Assembly;
        private static readonly Assembly ApplicationAssembly = typeof(Eede.Application.UseCase.Updates.CheckUpdateUseCase).Assembly;
        private static readonly Assembly InfrastructureAssembly = typeof(Eede.Infrastructure.Updates.VelopackUpdateService).Assembly;
        private static readonly Assembly PresentationAssembly = typeof(Eede.Presentation.ViewModels.Pages.MainViewModel).Assembly;

        [Test]
        public void DomainShouldNotDependOnOtherLayers()
        {
            var prohibitedAssemblies = new[] { ApplicationAssembly, InfrastructureAssembly, PresentationAssembly };
            var referencedAssemblies = DomainAssembly.GetReferencedAssemblies();

            foreach (var prohibited in prohibitedAssemblies)
            {
                var prohibitedName = prohibited.GetName().Name;
                var isDependent = referencedAssemblies.Any(r => r.Name == prohibitedName);
                Assert.That(isDependent, Is.False, $"Domain should not depend on {prohibitedName}");
            }
        }

        [Test]
        public void ApplicationShouldNotDependOnInfrastructureOrPresentation()
        {
            var prohibitedAssemblies = new[] { InfrastructureAssembly, PresentationAssembly };
            var referencedAssemblies = ApplicationAssembly.GetReferencedAssemblies();

            foreach (var prohibited in prohibitedAssemblies)
            {
                var prohibitedName = prohibited.GetName().Name;
                var isDependent = referencedAssemblies.Any(r => r.Name == prohibitedName);
                Assert.That(isDependent, Is.False, $"Application should not depend on {prohibitedName}");
            }
        }

        [Test]
        public void InfrastructureShouldNotDependOnPresentation()
        {
            var prohibitedAssemblies = new[] { PresentationAssembly };
            var referencedAssemblies = InfrastructureAssembly.GetReferencedAssemblies();

            foreach (var prohibited in prohibitedAssemblies)
            {
                var prohibitedName = prohibited.GetName().Name;
                var isDependent = referencedAssemblies.Any(r => r.Name == prohibitedName);
                Assert.That(isDependent, Is.False, $"Infrastructure should not depend on {prohibitedName}");
            }
        }
    }
}
