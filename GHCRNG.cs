using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;

class GHCRNG
{
    private readonly string profileURL = "https://github.com/rachitdhar";

    private bool ExecuteCommand(string cmdCommand, string repositoryPath)
    {
        try
        {
            using (Process process = new Process())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "git";
                startInfo.Arguments = cmdCommand;
                startInfo.WorkingDirectory = repositoryPath;
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
            return true;
        }
        catch { return false; }
    }
    private async Task<string> GetProfilePageHtml()
    {
        string result = "";
        var options = new ChromeOptions();
        options.AddArgument("--headless"); // run chrome in headless mode (no GUI)

        using IWebDriver driver = new ChromeDriver(options);
        try
        {
            driver.Navigate().GoToUrl(profileURL);
            await Task.Delay(3000);
            result = driver.PageSource;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in getting profile page HTML: {ex.Message}");
        }
        finally
        {
            driver.Quit();
        }
        return result;
    }

    private List<int> GetContributionLevels(string html)
    {
        string pattern = @"(contribution-graph-legend-level-)(\d+)";
        MatchCollection matches = Regex.Matches(html, pattern);
        List<int> result = new List<int>();

        foreach (Match match in matches)
            result.Add(int.Parse(match.Groups[2].Value));
        return result;
    }

    private long GenerateRandomNumber(List<int> contributionsList, TimeSpan delay)
    {
        string delayNum = delay.Ticks.ToString();
        string listNumbers = delayNum + string.Join("", contributionsList.FindAll(x => x != 0));
        SHA256 sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(listNumbers));
        
        string decimalString = "";
        foreach (byte b in bytes)
            decimalString += b.ToString("D3");  // converting byte to 3-digit decimal representation
        return long.Parse(decimalString.Substring(Math.Max(decimalString.Length - 10, 0)));
    }

    public async static Task Main()
    {
        GHCRNG app = new GHCRNG();
        long randomNum = 0;
        string? repositoryPath = Console.ReadLine();
        try
        {
            if (string.IsNullOrEmpty(repositoryPath)) return;
            string[] lines = await File.ReadAllLinesAsync(repositoryPath + "/README.md");
            if (!lines[lines.Length - 1].Contains("GHCRNG")) lines.Append("Program GHCRNG Initialized");

            while (true)
            {
                TimeSpan delay = TimeSpan.FromHours((int)(randomNum % 24));
                await Task.Delay(delay);
                string pageHtml = await app.GetProfilePageHtml();
                List<int> contributionLevelsList = app.GetContributionLevels(pageHtml);
                randomNum = app.GenerateRandomNumber(contributionLevelsList, delay);

                lines[lines.Length - 1] = $"{DateTime.UtcNow.ToString()} | New Random Number generated from GHCRNG: {randomNum}";
                await File.WriteAllLinesAsync(repositoryPath + "/README.md", lines);

                string commitMessage = $"Program GHCRNG adding the next random number: {randomNum}";
                if (app.ExecuteCommand("add .", repositoryPath))
                    if (app.ExecuteCommand($"commit -m \"{commitMessage}\"", repositoryPath))
                        app.ExecuteCommand("push origin main", repositoryPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Guess it didn't work. -- {ex}");
        }
        return;
    }
}