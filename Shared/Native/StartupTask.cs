using Microsoft.Win32.TaskScheduler;
using Serilog;
using WorkLifeBalance.Shared.Data;

namespace WorkLifeBalance.Shared.Native
{
    public class StartupTask
    {
        private readonly DataStorageFeature _dataStorage;

        public StartupTask(DataStorageFeature dataStorage)
        {
            _dataStorage = dataStorage;
        }

        private AppSettingsData Settings => _dataStorage.Settings;

        public void Create()
        {
            try
            {
                using var taskService = new TaskService();
                Delete();

                var definition = taskService.NewTask();
                definition.RegistrationInfo.Description = $"Run {Settings.AppName} at windows startup as administrator to start recording activity.";
                definition.Triggers.Add(new LogonTrigger());
                definition.Actions.Add(new ExecAction(Settings.AppExePath, null, Settings.AppDirectory));
                definition.Settings.StopIfGoingOnBatteries = false;
                definition.Settings.StartWhenAvailable = true;
                definition.Settings.RestartInterval = TimeSpan.FromMinutes(1);
                definition.Settings.RestartCount = 3;
                definition.Settings.ExecutionTimeLimit = TimeSpan.Zero;
                definition.Principal.RunLevel = TaskRunLevel.Highest;

                taskService.RootFolder.RegisterTaskDefinition(Settings.AppName, definition);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create the startup task");
            }
        }

        public void Regenerate()
        {
            using var taskService = new TaskService();
            if (taskService.FindTask(Settings.AppName) != null)
                Create();
        }

        public void Delete()
        {
            using var taskService = new TaskService();
            if (taskService.FindTask(Settings.AppName) != null)
                taskService.RootFolder.DeleteTask(Settings.AppName);
        }
    }
}
