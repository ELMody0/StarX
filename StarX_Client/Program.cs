namespace StarX_Client;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException +=
            (s, e) => HandleFatal(e.Exception, "ThreadException");
        AppDomain.CurrentDomain.UnhandledException +=
            (s, e) => HandleFatal(e.ExceptionObject as Exception, "AppDomain");

        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }

    private static void HandleFatal(Exception? ex, string source)
    {
        try
        {
            if (ex == null) return;
            string msg = string.Format(
                "[{0}] {1}{2}{2}==== STACK TRACE ===={2}{3}",
                source,
                ex, Environment.NewLine,
                ex.StackTrace ?? "(no trace)");
            string logDir = Path.Combine(
                AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(logDir);
            File.AppendAllText(
                Path.Combine(logDir, "crash.log"),
                $"---- {DateTime.Now:yyyy-MM-dd HH:mm:ss} ----{Environment.NewLine}{msg}{Environment.NewLine}");
            MessageBox.Show(
                string.Format("خطأ غير متوقع: {0}{1}{1}تم حفظ التفاصيل في: logs\\crash.log",
                    ex.Message, Environment.NewLine),
                "StarX", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch { /* لا شيء يمنع الإغلاق */ }
    }    
}