using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace XamarinAndroidEfCoreApp
{
    [Android.App.Application]
    public class App : Android.App.Application
    {
        public static long DbInitializationTime = 0;
        public static long DbFirstRequestTime = 0;
        public static long DbSecondRequestTime = 0;

        public App(IntPtr handle, Android.Runtime.JniHandleOwnership ownerShip) : base(handle, ownerShip) { }

        public override void OnCreate()
        {
            base.OnCreate();
            var databaseLocationPath = BaseContext.GetExternalFilesDir(null).Path;
            var databaseName = "ApplicationLog.db";

            // Uninstall application before use next line. It should speed up first run for large DbContext
            //CopyEmbeddedDatabase(databaseLocationPath, databaseName);

            var timer = new Stopwatch();
            timer.Start();
            InitializeDatabase(databaseLocationPath, databaseName);
            timer.Stop();

            DbInitializationTime = timer.ElapsedMilliseconds;

            timer.Restart();
            ExecuteInsertRequest(databaseLocationPath, databaseName);
            timer.Stop();
            DbFirstRequestTime = timer.ElapsedMilliseconds;
            timer.Restart();
            ExecuteInsertRequest(databaseLocationPath, databaseName);
            timer.Stop();
            DbSecondRequestTime = timer.ElapsedMilliseconds;
        }


        private void InitializeDatabase(string databaseLocationPath, string databaseName)
        {
            var logContext = GetDbContext(databaseLocationPath, databaseName);
            logContext.Database.EnsureCreated();
        }

        private void ExecuteInsertRequest(string databaseLocationPath, string databaseName)
        {
            var logContext = GetDbContext(databaseLocationPath, databaseName);
            logContext.Logs.Add(new Log { Level = "Debug", Timestamp = DateTime.UtcNow.ToLongDateString(), RenderedMessage = "Insert log" });
            logContext.SaveChanges();
        }

        private ApplicationLogDbContext GetDbContext(string databaseLocationPath, string databaseName)
        {
            var databaseConnectionString = $"Data Source={System.IO.Path.Combine(databaseLocationPath, databaseName)}";

            var options = new DbContextOptionsBuilder<ApplicationLogDbContext>()
                .UseSqlite(databaseConnectionString)
                .Options;

            return new ApplicationLogDbContext(options);
        }

        private void CopyEmbeddedDatabase(string databaseLocationPath, string databaseName)
        {
            var databaseFile = new Java.IO.File(databaseLocationPath, databaseName);
            if (databaseFile.Exists())
            {
                return;
            }

            var databaseFolder = new Java.IO.File(databaseFile.Parent);
            if (!databaseFolder.Exists())
            {
                databaseFolder.Mkdirs();
            }

            var destinationStream = new Java.IO.FileOutputStream(databaseFile);

            var buffer = new byte[16345];

            var databasePath = $"{typeof(App).Namespace}.{databaseName}";
            var databaseAssembly = System.Reflection.Assembly.GetAssembly(typeof(App));

            using (var stream = databaseAssembly.GetManifestResourceStream(databasePath))
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    destinationStream.Write(buffer, 0, read);
                }
            }

            destinationStream.Close();
        }
    }
}
