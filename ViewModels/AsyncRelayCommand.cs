using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Learning_Management_System.ViewModels
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object?, Task> _execute;
        private readonly Predicate<object?>? _canExecute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<object?, Task> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute == null || _canExecute(parameter));
        }

        public async void Execute(object? parameter)
        {
            var logPath = @"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log";
            
            // #region agent log
            try 
            { 
                var logData = new 
                { 
                    sessionId = "debug-session", 
                    runId = "run1", 
                    hypothesisId = "A", 
                    location = "AsyncRelayCommand.cs:Execute", 
                    message = "AsyncRelayCommand.Execute entry", 
                    data = new 
                    { 
                        isExecuting = _isExecuting,
                        parameterType = parameter?.GetType().Name,
                        parameterValue = parameter?.ToString()
                    }, 
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() 
                }; 
                var json = JsonSerializer.Serialize(logData);
                File.AppendAllText(logPath, json + Environment.NewLine);
                Debug.WriteLine($"[LOG] AsyncRelayCommand.Execute: {json}");
            } 
            catch (Exception ex) 
            { 
                Debug.WriteLine($"[LOG ERROR] {ex.Message}");
            }
            // #endregion
            
            if (_isExecuting) 
            {
                // #region agent log
                try 
                { 
                    var logData = new 
                    { 
                        sessionId = "debug-session", 
                        runId = "run1", 
                        hypothesisId = "A", 
                        location = "AsyncRelayCommand.cs:Execute", 
                        message = "Already executing, returning", 
                        data = new { }, 
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() 
                    }; 
                    var json = JsonSerializer.Serialize(logData);
                    File.AppendAllText(logPath, json + Environment.NewLine);
                } 
                catch { }
                // #endregion
                return;
            }

            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();

            try
            {
                // #region agent log
                try 
                { 
                    var logData = new 
                    { 
                        sessionId = "debug-session", 
                        runId = "run1", 
                        hypothesisId = "A", 
                        location = "AsyncRelayCommand.cs:Execute", 
                        message = "Before calling async execute", 
                        data = new { }, 
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() 
                    }; 
                    var json = JsonSerializer.Serialize(logData);
                    File.AppendAllText(logPath, json + Environment.NewLine);
                    Debug.WriteLine($"[LOG] Before async execute: {json}");
                } 
                catch { }
                // #endregion
                
                await _execute(parameter);
                
                // #region agent log
                try 
                { 
                    var logData = new 
                    { 
                        sessionId = "debug-session", 
                        runId = "run1", 
                        hypothesisId = "A", 
                        location = "AsyncRelayCommand.cs:Execute", 
                        message = "After async execute completed", 
                        data = new { }, 
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() 
                    }; 
                    var json = JsonSerializer.Serialize(logData);
                    File.AppendAllText(logPath, json + Environment.NewLine);
                    Debug.WriteLine($"[LOG] After async execute: {json}");
                } 
                catch { }
                // #endregion
            }
            catch (Exception ex)
            {
                // #region agent log
                try 
                { 
                    var logData = new 
                    { 
                        sessionId = "debug-session", 
                        runId = "run1", 
                        hypothesisId = "A", 
                        location = "AsyncRelayCommand.cs:Execute", 
                        message = "Exception in async execute", 
                        data = new 
                        {
                            exceptionType = ex.GetType().Name,
                            message = ex.Message,
                            stackTrace = ex.StackTrace
                        }, 
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() 
                    }; 
                    var json = JsonSerializer.Serialize(logData);
                    File.AppendAllText(logPath, json + Environment.NewLine);
                    Debug.WriteLine($"[LOG] Exception: {json}");
                } 
                catch { }
                // #endregion
                
                Debug.WriteLine($"AsyncRelayCommand exception: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
