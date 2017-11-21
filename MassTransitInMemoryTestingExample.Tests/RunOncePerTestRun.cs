using System;
using System.IO;
using log4net.Config;
using NUnit.Framework;

namespace MassTransitInMemoryTestingExample.Tests
{
    [SetUpFixture]
    public class RunOncePerTestRun
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            XmlConfigurator.Configure(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config")));
        }
    }
}
