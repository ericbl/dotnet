using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Helper
{
    /// <summary>
    /// Global utilities.
    /// </summary>
    public static class Utils
    {
        #region StackFrame
        /// <summary>
        /// Gets the calling method in the n frame before the current one (default = 1)
        /// </summary>
        /// <param name="n">
        /// The Calling Method Number n.
        /// </param>
        /// <returns>
        /// The calling method
        /// </returns>
        public static MethodBase GetCallingMethod(int n = 1)
        {
            // get call stack
            var stackTrace = new StackTrace();

            // get calling method name
            var result = stackTrace.GetFrame(n).GetMethod();
            if (result.Name == "Invoke")
            {
                // The stacktrace behaves differently if the app is started from Visual Studio (VS) or from the Filesytem (FS)
                // In FS, the trace is shifted with Invoke/SyncInvoke, thus get the previous frame (-1,-2, -3, -4 always return SyncInvokeXXX!)
                result = stackTrace.GetFrame(n - 1).GetMethod();
            }

            return result;
        }

        /// <summary>
        /// Gets the method name from site.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns>The method name from the ex.TargetSite.Name property</returns>
        public static string GetMethodNameFromSite(Exception ex)
        {
            MethodBase site = ex.TargetSite;
            return site == null ? null : site.Name;
        }

        /// <summary>
        /// Returns the method name of the caller.
        /// </summary>
        /// <returns>Name of the calling method</returns>
        public static string GetCallingMethodName()
        {
            StackFrame sf = new StackFrame(1, false);
            return sf.GetMethod().Name;
        }

        /// <summary>
        /// Returns the method name of the caller.
        /// </summary>
        /// <param name="n">The Calling Method Number n.</param>
        /// <returns>
        /// Name of the calling method
        /// </returns>
        public static string GetCallingMethodNameOnStackTrace(int n = 1)
        {
            return GetCallingMethod(n).Name;
        }
        #endregion

        #region Cmd line and computer
        /// <summary>
        /// Start a process to run the given command.
        /// </summary>
        /// <param name="cmdLineArgs">The command line arguments.</param>
        /// <returns>Redirected standard output of the command</returns>
        public static string ShellProcessCommandLine(string cmdLineArgs)
        {
            var sb = new StringBuilder();
            var pSpawn = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = System.IO.Path.GetTempPath(),
                    FileName = "cmd.exe",
                    CreateNoWindow = true,
                    Arguments = cmdLineArgs,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            pSpawn.OutputDataReceived += (sender, args) => sb.AppendLine(args.Data);
            pSpawn.Start();
            pSpawn.BeginOutputReadLine();
            pSpawn.WaitForExit();

            return sb.ToString();
        }

        /// <summary>
        /// Determines whether the computerName matchs the local computer.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <returns>
        ///   <c>true</c> if the computerName matchs the local computer; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLocalComputer(string computerName)
        {
            return computerName.Contains(Environment.MachineName);
        }
        #endregion

        #region EnsureStaticReference
        /// <summary>
        /// Ensures the static reference: will copy the .NET dll in the starting folder.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <exception cref="Exception">This code is used to ensure that the compiler will include assembly</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "calling typeof is the essence of this method")]
        public static void EnsureStaticReference<T>()
        {
            var dummy = typeof(T);
            if (dummy == null)
                throw new Exception("This code is used to ensure that the compiler will include assembly");
        }
        #endregion

        #region RunAsync, DatesWithSameYearAndMonth
        /// <summary>
        /// Executes the asynchronous operation.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="method">The method.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The asynchronous result that yields a T.
        /// </returns>
        public static Task<T> RunAsync<T>(Func<T> method, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(method);
        }

        /// <summary>
        /// Check if the given dates have the same year and month. They might have a different day!
        /// </summary>
        /// <param name="date1">The date1.</param>
        /// <param name="date2">The date2.</param>
        /// <returns>date1.Year == date2.Year AND date1.Month == date2.Month</returns>
        public static bool DatesWithSameYearAndMonth(DateTime date1, DateTime date2)
        {
            date1 = date1.Date;
            date2 = date2.Date;
            return date1.Year == date2.Year && date1.Month == date2.Month;
        }
        #endregion
    }
}
