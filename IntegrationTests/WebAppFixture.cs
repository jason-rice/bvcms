﻿using IntegrationTests.Support;
using OpenQA.Selenium.Chrome;
using SharedTestFixtures;
using System;
using System.Net;
using System.Xml;

namespace IntegrationTests
{
    public class WebAppFixture : IDisposable
    {
        private IISExpress cmswebInstance;

        private static ChromeDriver SharedChromeDriver { get; set; }

        public WebAppFixture()
        {
            Settings.EnsureApplicationHostConfig();
            SetupWebConfig();
            cmswebInstance = IISExpress.Start(Settings.ApplicationHostConfig, "CMSWeb");
            Warmup();
        }

        private void SetupWebConfig()
        {
            var xml = DatabaseFixture.LoadWebConfig();
            var appSettings = xml.GetElementsByTagName("appSettings")[0];
            var list = appSettings.SelectNodes("add[@key='UseRuntimeSettingsCache']");
            if (list.Count == 0)
            {
                var setting = xml.CreateElement("add");
                var attr = xml.CreateAttribute("key");
                attr.Value = "UseRuntimeSettingsCache";
                setting.Attributes.Append(attr);
                attr = xml.CreateAttribute("value");
                attr.Value = "false";
                setting.Attributes.Append(attr);
                appSettings.AppendChild(setting);
                xml.Save(DatabaseFixture.FindWebConfigPath());
            }
        }

        private void CleanupWebConfig()
        {
            var xml = DatabaseFixture.LoadWebConfig();
            var appSettings = xml.GetElementsByTagName("appSettings")[0];
            var list = appSettings.SelectNodes("add[@key='UseRuntimeSettingsCache']");
            if (list.Count > 0)
            {
                appSettings.RemoveChild(list[0]);
                xml.Save(DatabaseFixture.FindWebConfigPath());
            }
        }

        private void Warmup()
        {
            var attempts = 0;
            while (attempts++ < 100)
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadString(Settings.RootUrl);
                        break;
                    }
                }
                catch { }
            }
        }

        public static ChromeDriver GetChromeDriver(bool shared)
        {
            if (shared && SharedChromeDriver != null)
            {
                return SharedChromeDriver;
            }
            ChromeDriver chromeDriver;
            ChromeOptions options = new ChromeOptions();
            // ChromeDriver is just AWFUL because every version or two it breaks unless you pass cryptic arguments
            //AGRESSIVE: options.setPageLoadStrategy(PageLoadStrategy.NONE); // https://www.skptricks.com/2018/08/timed-out-receiving-message-from-renderer-selenium.html
            options.AddArgument("ignore-certificate-errors");
            options.AddArgument("start-maximized"); // https://stackoverflow.com/a/26283818/1689770
            options.AddArgument("enable-automation"); // https://stackoverflow.com/a/43840128/1689770
            options.AddArgument("--no-sandbox"); //https://stackoverflow.com/a/50725918/1689770
            options.AddArgument("--disable-infobars"); //https://stackoverflow.com/a/43840128/1689770
            options.AddArgument("--disable-dev-shm-usage"); //https://stackoverflow.com/a/50725918/1689770
            options.AddArgument("--disable-browser-side-navigation"); //https://stackoverflow.com/a/49123152/1689770
            options.AddArgument("--disable-gpu"); //https://stackoverflow.com/questions/51959986/how-to-solve-selenium-chromedriver-timed-out-receiving-message-from-renderer-exc
            var chromedriverDir = Environment.GetEnvironmentVariable("ChromeDriverDir");
            if (string.IsNullOrEmpty(chromedriverDir))
            {
                chromeDriver = new ChromeDriver(options);
            }
            else
            {
                chromeDriver = new ChromeDriver(chromedriverDir, options, TimeSpan.FromSeconds(120));
            }

            if (shared)
            {
                SharedChromeDriver = chromeDriver;
            }
            return chromeDriver;
        }

        public void Dispose()
        {
            cmswebInstance?.Stop();
            cmswebInstance = null;
            SharedChromeDriver?.Quit();
            SharedChromeDriver = null;
            CleanupWebConfig();
        }
    }
}
