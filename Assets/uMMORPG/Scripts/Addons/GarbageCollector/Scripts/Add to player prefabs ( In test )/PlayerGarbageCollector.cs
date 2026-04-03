using System;
using UnityEngine;

public class PlayerGarbageCollector : MonoBehaviour
{
    [Range(1, 9999)] public int delayGCInMinutes = 5;
#if _CLIENT
    private void Start()
    {
        InvokeRepeating(nameof(GarbageCollectorStart), (delayGCInMinutes*60), (delayGCInMinutes*60));
    }

    private void GarbageCollectorStart()
    {
        long before = GC.GetTotalMemory(true);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long after = GC.GetTotalMemory(true);

        GameLog.LogMessage("Garbage Collector called : (before : " + FormatBytes(before) +") (after: "+ FormatBytes(after) +") : freed memory : " + FormatBytes(before - after));

    }

    private static string FormatBytes(long bytes)
    {
        string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
        int i;
        double dblSByte = bytes;
        for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
        {
            dblSByte = bytes / 1024.0;
        }

        return String.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
    }
#endif
}
