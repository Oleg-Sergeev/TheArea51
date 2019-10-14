using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable]
public class InvalidSaveOperationException : Exception
{
    private static string logPath = (UnityEngine.Application.isEditor ? UnityEngine.Application.dataPath : UnityEngine.Application.persistentDataPath) + "/Log.txt";

    public InvalidSaveOperationException() : base() {}
    public InvalidSaveOperationException(string message) : base(message)
    {
        string lastLogs = "";

        if (File.Exists(logPath)) lastLogs = File.ReadAllText(logPath);

        File.WriteAllText(logPath, $"{lastLogs}\n{message}");

        MyDebug.LogError(message);
    }
    public InvalidSaveOperationException(string message, Exception inner): base(message, inner) {}
    public InvalidSaveOperationException(int exceptionCode, Exception inner) : base(inner.Message, inner)
    {
        string lastLogs = "";
        
        string currentLog = $"*** Error {exceptionCode}: {inner.StackTrace} /***/ {inner.Message} ***\n";

        if (File.Exists(logPath)) lastLogs = File.ReadAllText(logPath);

        File.WriteAllText(logPath, $"{lastLogs}\n{currentLog}");

        MyDebug.LogError(currentLog);
    }

    protected InvalidSaveOperationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public override string ToString() => $"Log path - {logPath} *** {base.ToString()}";
}
