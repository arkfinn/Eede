using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Application.Tests.Pictures
{
    public class DrawingSessionProviderTests
    {
        [Test]
        public void Update_ShouldUpdateCurrentSession()
        {
            var provider = new DrawingSessionProvider();
            var newSession = new DrawingSession(Picture.CreateEmpty(new PictureSize(16, 16)));

            provider.Update(newSession);

            Assert.That(provider.CurrentSession, Is.SameAs(newSession));
        }

        [Test]
        public void Update_ShouldFireSessionChangedEvent()
        {
            var provider = new DrawingSessionProvider();
            var newSession = new DrawingSession(Picture.CreateEmpty(new PictureSize(16, 16)));

            DrawingSession? eventSession = null;
            provider.SessionChanged += (s) => eventSession = s;

            provider.Update(newSession);

            Assert.That(eventSession, Is.SameAs(newSession));
        }
    }
}
