using System;
using System.Diagnostics;
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
        /// <returns>
        /// The asynchronous result that yields a T.
        /// </returns>
        public static Task<T> RunAsync<T>(Func<T> method)
        {
            CancellationToken cancellationToken = new CancellationToken();
            return RunAsync(method, cancellationToken);
        }

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
