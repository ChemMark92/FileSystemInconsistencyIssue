using System.Xml;

#region Config

const string baseDir = @"/data";

const int delayBetweenDocumentUpdates = 50;
const int numberOfItemsInDocument = 10;

#endregion

#region Variables

FileStream lockStream = null;

#endregion

#region Execution

for (int i = 0; i < 10; i++)
{
    //We want all containers start execution at the same time
    WaitTillNextRoundMinute();

    string documentName = CreateDocumentName();
    //Here we perform File.Exists(document) and seems like result of execution is cached at some level below the application
    CheckDocumentExistence(documentName);
    //Here we try to load document, and if document have been already created by other container but File.Exists(document) already have cached 'false' value
    //we get FileNotFoundException (line 73) and should wait until cache is cleared (WaitForDocumentExistence line 133)
    PerformMultipleDocumentUpdates(documentName);
}

void CheckDocumentExistence(string documentName)
{
    File.Exists(GetDocumentPath(documentName));
}

void PerformMultipleDocumentUpdates(string documentName)
{
    for (var i = 1; i <= numberOfItemsInDocument; i++)
    {
        AcquireLock(documentName);
        AddItemToDocument(i, documentName);
        ReleaseLock();

        Delay(delayBetweenDocumentUpdates);
    }
}

#endregion

#region Document

string CreateDocumentName()
{
    return "containers_" + DateTime.Now.ToString("HHmm");
}

void AddItemToDocument(int index, string documentName)
{
    XmlDocument doc = LoadDocumentOrReturnEmptyIfNotExist(documentName);
    AppendItem(doc, index);
    SaveDocument(doc, documentName);
}

XmlDocument LoadDocumentOrReturnEmptyIfNotExist(string documentName)
{
    var doc = new XmlDocument();

    try
    {
        LoadDocument(doc, documentName);
    }
    catch (FileNotFoundException)
    {
        CheckVolumeInconsistency(documentName);

        SetEmptyDoc(doc);
    }

    return doc;
}

void LoadDocument(XmlDocument doc, string documentName)
{
    var documentPath = GetDocumentPath(documentName);
    doc.Load(documentPath);
}

void AppendItem(XmlDocument doc, int index)
{
    var element = doc.CreateElement("container");

    element.SetAttribute("name", Environment.MachineName);
    element.SetAttribute("index", index.ToString());

    doc.DocumentElement.AppendChild(element);
}

void SaveDocument(XmlDocument doc, string documentName)
{
    string documentPath = GetDocumentPath(documentName);
    using StreamWriter writer = new StreamWriter(documentPath, false, System.Text.Encoding.Unicode);
    writer.Write(doc.OuterXml);
    writer.Close();
}

void SetEmptyDoc(XmlDocument doc)
{
    doc.LoadXml("<containers/>");
}

string GetDocumentPath(string documentName)
{
    return Path.Combine(baseDir, documentName + ".xml");
}

string GetDocumentLockPath(string documentName)
{
    return Path.Combine(baseDir, documentName + ".lock");
}

#endregion

#region Consistency Check

void CheckVolumeInconsistency(string documentName)
{
    if (IsVolumeInconsistent(documentName))
    {
        WriteError($"The issue is reproduced: Contradiction between File.Exists (false) and Directory.GetFiles (contains the file). Check {documentName} for missing items (See README p.6 for details)");
        WaitForDocumentExistence(documentName);
    }
}

bool IsVolumeInconsistent(string documentName)
{
    string documentPath = GetDocumentPath(documentName);
    string[] foundFiles = Directory.GetFiles(baseDir);
    return foundFiles.Contains(documentPath) && !File.Exists(documentPath);
}

void WaitForDocumentExistence(string documentName)
{
    string documentPath = GetDocumentPath(documentName);

    int delayBetweenAttempts = 100;
    int maxWaitForFileTime = 10000;

    for (int i = 0; i < maxWaitForFileTime / delayBetweenAttempts; i++)
    {
        Delay(delayBetweenAttempts);
        if (File.Exists(documentPath))
        {
            WriteError($"File was found after {i * delayBetweenAttempts} milliseconds");
            return;
        }
    }

    WriteError($"File was not found after {maxWaitForFileTime} milliseconds wait");
}

#endregion

#region Lock

void AcquireLock(string lockName)
{
    int delayBetweenAttempts = 100;
    int maxWaitForLockTime = 50000;

    for (int i = 0; i < maxWaitForLockTime / delayBetweenAttempts; i++)
    {
        try
        {
            CreateLockStream(lockName);
            return;
        }
        catch
        {
            Delay(delayBetweenAttempts);
        }
    }

    ThrowException("Acquire lock failed");
}

void ReleaseLock()
{
    CloseLockStream();
}

void CreateLockStream(string documentName)
{
    string lockPath = GetDocumentLockPath(documentName);
    lockStream = new FileStream(lockPath, FileMode.Create, FileAccess.Write, FileShare.None);
}

void CloseLockStream()
{
    if (lockStream == null) return;
    lockStream.Close();
    lockStream = null;
}

#endregion

#region Helpers

void WaitTillNextRoundMinute()
{
    int nowMilliseconds = DateTime.Now.Millisecond;
    int nowSeconds = DateTime.Now.Second;

    int millisecondsToWait = 1000 - nowMilliseconds;
    int secondsToWait = 60 - nowSeconds;

    Delay((secondsToWait) * 1000 + millisecondsToWait);
}

void ThrowException(string message)
{
    throw new Exception(message);
}

void Delay(int milliseconds)
{
    Thread.Sleep(milliseconds);
}

void WriteError(string message)
{
    Console.Error.WriteLine(message);
}

#endregion