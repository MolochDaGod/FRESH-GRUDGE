using System;

public class RAHR_Utils {
    public static long GetCurrentEpochTime() {
        return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
    }

    public static long AddDaysToCurrentEpochTime(int days) {
        DateTime dt = DateTime.Now.ToUniversalTime().AddDays(days);
        return (long)dt.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }

    public static string TimeRemaining(long futureEpochTime) {
        string timeLeft = "";
        long remainingSeconds = futureEpochTime - GetCurrentEpochTime();
        TimeSpan timeSpan = TimeSpan.FromSeconds(remainingSeconds);
        if(timeSpan.Days > 0) timeLeft += timeSpan.Days + "d ";
        if(timeSpan.Hours > 0) timeLeft += timeSpan.Hours + "h";
        if(timeSpan.Hours <= 0 && timeSpan.Days < 1) timeLeft += "<1h";
        return timeLeft;
    }
}