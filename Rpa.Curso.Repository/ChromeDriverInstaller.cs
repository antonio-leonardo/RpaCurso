using Rpa.Curso.CrossCutting;
using HtmlAgilityPack;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Rpa.Curso.Repository
{
    public class ChromeDriverInstaller : LoggingBase
    {
        private static readonly HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://chromedriver.storage.googleapis.com/")
        };

        public Task Install() => Install(null, false);
        public Task Install(string chromeVersion) => Install(chromeVersion, false);
        public Task Install(bool forceDownload) => Install(null, forceDownload);

        public async Task Install(string chromeVersion, bool forceDownload)
        {
            string fullChromeVersion = chromeVersion;
            Log.Info("Instruções de https://chromedriver.chromium.org/downloads/version-selection");
            Log.Info("Primeiro, encontrar qual versão do Chrome está sendo utilizada. Vamos supor que que tenha a 72.0.3626.81...");
            if (chromeVersion == null)
            {
                chromeVersion = await GetChromeVersion();
            }

            Log.Info("Pega o número da versão do Chrome, remove a última parte...");
            chromeVersion = chromeVersion.Substring(0, chromeVersion.LastIndexOf('.'));

            Log.Info("... e concatena com a URL de resultado para https://chromedriver.storage.googleapis.com/LATEST_RELEASE_.");

            Log.Info("Por exemplo, com a versão do Chrome 72.0.3626.81, você vai Get URL \"https://chromedriver.storage.googleapis.com/LATEST_RELEASE_72.0.3626\"");
            var chromeDriverVersionResponse = await httpClient.GetAsync($"LATEST_RELEASE_{chromeVersion}");
            if (!chromeDriverVersionResponse.IsSuccessStatusCode)
            {
                string zipName = null;
                string driverName = null;
                string osName = null;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    zipName = "chromedriver-win32.zip";
                    driverName = "chromedriver.exe";
                    osName = "win32";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    zipName = "chromedriver-linux64.zip";
                    driverName = "chromedriver";
                    osName = "linux64";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    zipName = "chromedriver-mac-x64.zip";
                    driverName = "chromedriver";
                    osName = "mac-x64";
                }
                else
                {
                    Log.Error("Seu sistema operacional não é suportado");
                    throw new PlatformNotSupportedException("Seu sistema operacional não é suportado");
                }
                Log.Info("Define a URL antiga para download");
                string downlodableUrl = "https://edgedl.me.gvt1.com/edgedl/chrome/chrome-for-testing/" + fullChromeVersion + "/" + osName + "/" + zipName;
                string chromelabsUrl = "https://googlechromelabs.github.io/chrome-for-testing/";

                Log.Info("Get a versão mais atualizada");
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(chromelabsUrl);
                HtmlNode divNode = doc.DocumentNode.SelectSingleNode("//div[@class='table-wrapper summary']");
                HtmlNode tableNode = divNode.SelectSingleNode("//table");
                HtmlNode tbodyNode = divNode.SelectSingleNode("//tbody");
                HtmlNode trNode = divNode.SelectSingleNode("//tr[@class='status-ok']");
                HtmlNode thNode = divNode.SelectSingleNode("//th");
                HtmlNode aNode = divNode.SelectSingleNode("//a[contains(text(), 'Stable')]");
                HtmlNode customNode = aNode.SelectSingleNode("//parent::th/following-sibling::td[1]/code");
                string currentChromeVersion = customNode.InnerText;

                Log.Info("Define a URL real de download");
                string downlodableRealUrl = "https://storage.googleapis.com/chrome-for-testing-public/" + currentChromeVersion + "/" + osName + "/" + zipName;
                if (this.ExistsDownloadableFile(downlodableUrl).Item1)
                {
                    this.DownloadZipFile(downlodableUrl, driverName);
                }
                else if (this.ExistsDownloadableFile(downlodableRealUrl).Item1)
                {
                    this.DownloadZipFile(downlodableRealUrl, driverName);
                }
                else if (chromeDriverVersionResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    Log.Error($"Versão do ChromeDriver não encontrada para a versão do Chrome {chromeVersion}");
                    throw new Exception($"Versão do ChromeDriver não encontrada para a versão do Chrome {chromeVersion}");
                }
                else
                {
                    Log.Error($"Falha na requisição para o encontrar o ChromeDriver com status code: {chromeDriverVersionResponse.StatusCode}, motivo: {chromeDriverVersionResponse.ReasonPhrase}");
                    throw new Exception($"Falha na requisição para o encontrar o ChromeDriver com status code: {chromeDriverVersionResponse.StatusCode}, motivo: {chromeDriverVersionResponse.ReasonPhrase}");
                }
            }
            else
            {
                var chromeDriverVersion = await chromeDriverVersionResponse.Content.ReadAsStringAsync();

                string zipName;
                string driverName;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    zipName = "chromedriver_win32.zip";
                    driverName = "chromedriver.exe";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    zipName = "chromedriver_linux64.zip";
                    driverName = "chromedriver";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    zipName = "chromedriver_mac64.zip";
                    driverName = "chromedriver";
                }
                else
                {
                    Log.Error("Seu sistema operacional não é suportado");
                    throw new PlatformNotSupportedException("Seu sistema operacional não é suportado");
                }

                string targetPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                targetPath = Path.Combine(targetPath, driverName);
                if (!forceDownload && File.Exists(targetPath))
                {
                    using var process = Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = targetPath,
                            ArgumentList = { "--version" },
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                        }
                    );
                    string existingChromeDriverVersion = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    process.Kill(true);

                    // expected output is something like "ChromeDriver 88.0.4324.96 (68dba2d8a0b149a1d3afac56fa74648032bcf46b-refs/branch-heads/4324@{#1784})"
                    // the following line will extract the version number and leave the rest
                    existingChromeDriverVersion = existingChromeDriverVersion.Split(" ")[1];
                    if (chromeDriverVersion == existingChromeDriverVersion)
                    {
                        return;
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception($"Failed to execute {driverName} --version");
                    }
                }

                //   Use the URL created in the last step to retrieve a small file containing the version of ChromeDriver to use. For example, the above URL will get your a file containing "72.0.3626.69". (The actual number may change in the future, of course.)
                //   Use the version number retrieved from the previous step to construct the URL to download ChromeDriver. With version 72.0.3626.69, the URL would be "https://chromedriver.storage.googleapis.com/index.html?path=72.0.3626.69/".
                var driverZipResponse = await httpClient.GetAsync($"{chromeDriverVersion}/{zipName}");
                if (!driverZipResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"ChromeDriver download request failed with status code: {driverZipResponse.StatusCode}, reason phrase: {driverZipResponse.ReasonPhrase}");
                }

                // this reads the zipfile as a stream, opens the archive, 
                // and extracts the chromedriver executable to the targetPath without saving any intermediate files to disk
                using (var zipFileStream = await driverZipResponse.Content.ReadAsStreamAsync())
                using (var zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Read))
                using (var chromeDriverWriter = new FileStream(targetPath, FileMode.Create))
                {
                    var entry = zipArchive.GetEntry(driverName);
                    using Stream chromeDriverStream = entry.Open();
                    await chromeDriverStream.CopyToAsync(chromeDriverWriter);
                }

                // on Linux/macOS, you need to add the executable permission (+x) to allow the execution of the chromedriver
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    using var process = Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = "chmod",
                            ArgumentList = { "+x", targetPath },
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                        }
                    );
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    process.Kill(true);

                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception("Failed to make chromedriver executable");
                    }
                }
            }
        }

        private void DownloadZipFile(string url, string driverName)
        {
            string targetPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            targetPath = Path.Combine(targetPath, driverName);
            HttpClient httpClient = null;
            Stream zipFileStream = null;
            ZipArchive zipArchive = null;
            FileStream chromeDriverWriter = null;
            Stream chromeDriverStream = null;
            try
            {
                httpClient = new HttpClient();
                zipFileStream = httpClient.GetStreamAsync(url).GetAwaiter().GetResult();
                zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Read);
                chromeDriverWriter = new FileStream(targetPath, FileMode.Create);
                ZipArchiveEntry entry = zipArchive.Entries.ToList().Where(x => x.Name == driverName).FirstOrDefault();
                chromeDriverStream = entry.Open();
                chromeDriverStream.CopyToAsync(chromeDriverWriter).GetAwaiter().GetResult();

                // on Linux/macOS, you need to add the executable permission (+x) to allow the execution of the chromedriver
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    using var process = Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = "chmod",
                            ArgumentList = { "+x", targetPath },
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                        }
                    );
                    string error = process.StandardError.ReadToEndAsync().GetAwaiter().GetResult();
                    process.WaitForExitAsync().GetAwaiter().GetResult();
                    process.Kill(true);

                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception("Failed to make chromedriver executable");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Throws an error during downloadable file trying.", ex);
            }
            finally
            {
                if (null != chromeDriverStream)
                {
                    chromeDriverStream.Dispose();
                }
                if (null != chromeDriverWriter)
                {
                    chromeDriverWriter.Dispose();
                }
                if (null != zipArchive)
                {
                    zipArchive.Dispose();
                }
                if (null != zipFileStream)
                {
                    zipFileStream.Dispose();
                }
                if (null != httpClient)
                {
                    httpClient.Dispose();
                }
            }
        }

        private Tuple<bool, string> ExistsDownloadableFile(string fileLocation)
        {
            bool isValidURL = true;
            string data = string.Empty;
            using (WebClient client = new WebClient())
            {
                try
                {
                    data = client.DownloadString(fileLocation);
                }
                catch (WebException weX)
                {
                    isValidURL = !isValidURL;
                    data = weX.Message;
                }
            }
            return Tuple.Create<bool, string>(isValidURL, data);
        }

        public async Task<string> GetChromeVersion()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string chromePath = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe", null, null);
                if (chromePath == null)
                {
                    throw new Exception("Google Chrome not found in registry");
                }

                var fileVersionInfo = FileVersionInfo.GetVersionInfo(chromePath);
                return fileVersionInfo.FileVersion;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    using var process = Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = "google-chrome",
                            ArgumentList = { "--product-version" },
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                        }
                    );
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    process.Kill(true);

                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception(error);
                    }

                    return output;
                }
                catch (Exception ex)
                {
                    throw new Exception("An error occurred trying to execute 'google-chrome --product-version'", ex);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                try
                {
                    using var process = Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
                            ArgumentList = { "--version" },
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                        }
                    );
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    process.Kill(true);

                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception(error);
                    }

                    output = output.Replace("Google Chrome ", "");
                    return output;
                }
                catch (Exception ex)
                {
                    throw new Exception($"An error occurred trying to execute '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome --version'", ex);
                }
            }
            else
            {
                throw new PlatformNotSupportedException("Your operating system is not supported.");
            }
        }
    }
}