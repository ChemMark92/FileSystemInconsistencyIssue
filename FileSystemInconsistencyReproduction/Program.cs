#region Config

const string baseDir = @"/data";

string file = Path.Combine(baseDir, "test.txt");

#endregion

#region Execution

WaitTillNextRoundMinute();

for (int i = 0; i < 15; i++)
{
    if (i == 1)
        TryCreateFile();

    Console.WriteLine($"File {file} exists: {File.Exists(file)}, now: {DateTime.Now}");

    Thread.Sleep(1000);
}

#endregion

#region Helpers

void WaitTillNextRoundMinute()
{
    int nowMilliseconds = DateTime.Now.Millisecond;
    int nowSeconds = DateTime.Now.Second;

    int millisecondsToWait = 1000 - nowMilliseconds;
    int secondsToWait = 60 - nowSeconds;

    Thread.Sleep((secondsToWait) * 1000 + millisecondsToWait);
}

void TryCreateFile()
{
    try
    {
        File.WriteAllText(file, string.Empty);
    }
    catch
    {
    }
}

#endregion
