using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Config;
using PiroPiro.Contract;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.IO;

namespace PiroPiro.Selenium
{
    public class SeleniumBrowserFactory : BrowserFactory
    {
        protected override Browser DoCreate(Configuration cfg)
        {
            if (cfg == null)
            {
                cfg = new Configuration();
            }

            string name = cfg.Get("PiroPiro.Selenium.Driver");

            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("PiroPiro.Selenium.Driver setting is required");
            }

            string remoteAddress = cfg.Get("PiroPiro.Selenium.Remote.Address", false);

            RemoteWebDriver driver = null;
            bool isLocal = true;

            if (string.IsNullOrEmpty(remoteAddress))
            {
                // use a browser in the local machine

                switch (name.ToLower())
                {
                    case "ie":
                    case "internetexplorer":
                    case "internet explorer":
                        driver = new InternetExplorerDriver();
                        break;
                    case "firefox":
                    case "ff":
                        FirefoxProfile ffp = new FirefoxProfile
                        {
                            AcceptUntrustedCertificates = true,
                            AlwaysLoadNoFocusLibrary = true,
                            EnableNativeEvents = true,
                        };
                        string authDomains = cfg.Get("PiroPiro.TrustedAuthDomains", false);
                        if (string.IsNullOrEmpty(authDomains))
                        {
                            authDomains = "localhost";
                        }
                        ffp.SetPreference("browser.sessionstore.enabled", false);
                        ffp.SetPreference("browser.sessionstore.resume_from_crash", false);
                        ffp.SetPreference("network.automatic-ntlm-auth.trusted-uris", authDomains);
                        driver = new FirefoxDriver(ffp);
                        break;
                    case "chrome":
                        driver = new ChromeDriver(GetChromeDriverDir(cfg));
                        break;
                    default:
                        throw new Exception("Selenium Driver not supported: " + name);
                }
            }
            else
            {
                // use a browser in a remote machine
                isLocal = false;

                string version = cfg.Get("PiroPiro.Selenium.Remote.Version", false);
                if (string.IsNullOrEmpty(version))
                {
                    version = null;
                }
                string platform = cfg.Get("PiroPiro.Selenium.Remote.Platform", false);
                if (string.IsNullOrEmpty(platform))
                {
                    platform = "Any";
                }

                if (name.ToLower().Trim() == "htmlunit" && string.IsNullOrWhiteSpace(version))
                {
                    // tell htmlunit by default to behave like firefox
                    version = "firefox";
                }

                DesiredCapabilities cap = new OpenQA.Selenium.Remote.DesiredCapabilities(name, version, new Platform((PlatformType)Enum.Parse(typeof(PlatformType), platform)));

                // enable javascript by default
                cap.IsJavaScriptEnabled = true;

                if (name.ToLower().Trim() == "firefox")
                {
                    // if firefox, add some preferences

                    string authDomains = cfg.Get("PiroPiro.TrustedAuthDomains", false);
                    if (string.IsNullOrEmpty(authDomains))
                    {
                        authDomains = "localhost";
                    }

                    FirefoxProfile ffp = new FirefoxProfile();
                    ffp.SetPreference("browser.sessionstore.enabled", false);
                    ffp.SetPreference("browser.sessionstore.resume_from_crash", false);
                    ffp.SetPreference("network.automatic-ntlm-auth.trusted-uris", authDomains);

                    cap.SetCapability(FirefoxDriver.ProfileCapabilityName, ffp.ToBase64String());
                }

                driver = new RemoteWebDriver(new Uri(remoteAddress), cap);
            }

            return new SeleniumBrowser(driver, isLocal, cfg);

        }

        public static string GetChromeDriverDir(Configuration cfg)
        {
            string chromeDriverDir = cfg.Get("PiroPiro.Selenium.ChromeDriverDir", false);
            if (string.IsNullOrEmpty(chromeDriverDir))
            {
                #region FindChromeDriverExe

                chromeDriverDir = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(SeleniumBrowserFactory)).Location);
                while (!File.Exists(Path.Combine(chromeDriverDir, "chromedriver.exe")))
                {
                    if (chromeDriverDir == "" || chromeDriverDir == Path.GetPathRoot(chromeDriverDir))
                    {
                        chromeDriverDir = null;
                        break;
                    }
                    chromeDriverDir = Path.GetDirectoryName(chromeDriverDir);
                }
                #endregion
                if (!string.IsNullOrEmpty(chromeDriverDir))
                {
                    cfg.Set("PiroPiro.Selenium.ChromeDriverDir", chromeDriverDir);
                }
                else
                {
                    chromeDriverDir = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(SeleniumBrowserFactory)).Location);
                    throw new Exception("chromediver.exe not found in \"" + chromeDriverDir + "\", use ChromeDriverDir setting to specify a path");
                }
            }
            return chromeDriverDir;
        }

        public SeleniumBrowserFactory(Configuration cfg = null)
            : base(cfg)
        {
        }
    }
}
