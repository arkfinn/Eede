﻿using Eede.Domain.Files;
using Eede.Domain.ImageBlenders;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Infrastructure.Pictures;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageBlenders
{
    [TestFixture]
    public class DirectImageBlenderTests
    {
        [Test]
        public void TestBlend()
        {
            Picture src = ReadPicture(@"ImageBlenders\test\blend.png");
            DirectImageBlender blender = new();
            Picture dst = ReadPicture(@"ImageBlenders\test\base.png");

            Picture result = dst.Blend(blender, src, new Position(0, 0));

            // result.ToImage().Save(@"ImageBlenders\test\direct_blend.png", ImageFormat.Png);
            Picture expected = ReadPicture(@"ImageBlenders\test\direct_blend.png");
            Assert.That(result.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        private Picture ReadPicture(string path)
        {
            return new PictureFileReader(new FilePath(path)).Read();
        }
    }
}
