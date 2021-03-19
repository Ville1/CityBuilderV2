using System;
using System.Collections.Generic;
using System.Diagnostics;

public class DiagnosticsManager {
    private static readonly int MAX_HISTORY = 50;

    private static DiagnosticsManager instance;

    public bool Verbose { get; set; }

    private Dictionary<Type, DiagnosticsData> data;
    private bool log;

    private DiagnosticsManager()
    {
        Verbose = false;
    }

    public static DiagnosticsManager Instance
    {
        get {
            if(instance == null) {
                instance = new DiagnosticsManager();
            }
            return instance;
        }
    }


    public bool Log
    {
        get {
            return log;
        }
        set {
            if(log == value) {
                return;
            }
            log = value;
            data = log ? new Dictionary<Type, DiagnosticsData>() : null;
        }
    }

    public void Message(Type parent, string message)
    {
        if (!Log || !Verbose) {
            return;
        }
        CustomLogger.Instance.Debug(string.Format("{0} -> {1}", parent.ToString(), message));
    }

    public void Start(Type parent, string key, string message)
    {
        if (!Log) {
            return;
        }
        if (!data.ContainsKey(parent)) {
            data.Add(parent, new DiagnosticsData());
        }
        if (data[parent].Watches.ContainsKey(key)) {
            CustomLogger.Instance.Error(string.Format("Watch with key: {0} already exists", key));
            data[parent].Watches.Remove(key);
        }
        data[parent].Watches.Add(key, new WatchContainer() {
            Key = key,
            Message = message,
            Watch = Stopwatch.StartNew()
        });
    }

    public void End(Type parent, string key)
    {
        if (!Log) {
            return;
        }
        if (!data.ContainsKey(parent) || !data[parent].Watches.ContainsKey(key)) {
            CustomLogger.Instance.Error("Start should be called first");
            return;
        }
        DiagnosticsData diagnostics = data[parent];
        WatchContainer container = diagnostics.Watches[key];
        container.Watch.Stop();
        if (Verbose) {
            CustomLogger.Instance.Debug(string.Format("{0} -> {1} in {2}ms", parent.ToString(), container.Message, container.Watch.ElapsedMilliseconds));
        }

        if (!diagnostics.History.ContainsKey(key)) {
            diagnostics.History.Add(key, new List<long>() { container.Watch.ElapsedMilliseconds });
        } else {
            diagnostics.History[key].Add(container.Watch.ElapsedMilliseconds);
            while (diagnostics.History.Count >= MAX_HISTORY) {
                diagnostics.History[key].RemoveAt(diagnostics.History.Count - 1);
            }
        }
        if (!diagnostics.Peak.ContainsKey(key)) {
            diagnostics.Peak.Add(key, container.Watch.ElapsedMilliseconds);
        } else if(container.Watch.ElapsedMilliseconds > diagnostics.Peak[key]) {
            diagnostics.Peak[key] = container.Watch.ElapsedMilliseconds;
        }
        if (!diagnostics.Total.ContainsKey(key)) {
            diagnostics.Total.Add(key, container.Watch.ElapsedMilliseconds);
        } else {
            diagnostics.Total[key] += container.Watch.ElapsedMilliseconds;
        }

        diagnostics.Watches.Remove(key);
    }

    public void Print()
    {
        if (!Log) {
            return;
        }
        foreach(KeyValuePair<Type, DiagnosticsData> pair in data) {
            ConsoleManager.Instance.Run_Command(string.Format("echo {0}", pair.Key.ToString()));
            foreach(KeyValuePair<string, List<long>> history in pair.Value.History) {
                if(history.Value.Count == 0) {
                    continue;
                }
                long total = 0;
                foreach(long time in history.Value) {
                    total += time;
                }
                ConsoleManager.Instance.Run_Command(string.Format("echo {0}: {1}ms (p: {2}, t: {3})", history.Key, total / history.Value.Count, pair.Value.Peak[history.Key], pair.Value.Total[history.Key]));
            }
        }
    }

    private class DiagnosticsData
    {
        public Dictionary<string, WatchContainer> Watches { get; set; }
        public Dictionary<string, List<long>> History { get; set; }
        public Dictionary<string, long> Peak { get; set; }
        public Dictionary<string, long> Total { get; set; }

        public DiagnosticsData()
        {
            Watches = new Dictionary<string, WatchContainer>();
            History = new Dictionary<string, List<long>>();
            Peak = new Dictionary<string, long>();
            Total = new Dictionary<string, long>();
        }
    }

    private class WatchContainer
    {
        public string Key { get; set; }
        public string Message { get; set; }
        public Stopwatch Watch { get; set; }
    }
}
