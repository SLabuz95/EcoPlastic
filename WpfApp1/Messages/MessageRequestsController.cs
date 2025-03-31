using CommunityToolkit.Mvvm.Messaging;
using GitControl.View.Dialogs;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static GitControl.Extentions.Extensions;
using static GitControl.Messages.MessageRequestsController;
using GitControl.Utils;

namespace GitControl.Messages
{
    using TaskPendingRequestOperation = TaskOperation<PendingMessageRequest>;
    static class MessageRequestsController
    {
        
        //readonly static Action<object?> mainAction = new((request) =>
        //{
        //    if (request is PendingMessageRequest)
        //    {
        //        var pendingMessageRequest = (PendingMessageRequest)request;
        //        Task.Delay(5000).Wait();
        //        //pendingMessageRequest.cancellationToken.ThrowIfCancellationRequested();
        //        //Task.Delay(5000).Wait();
        //        //throw new Exception();
        //        Trace.Write(String.Format("Main: {0}", DateTimeOffset.Now.ToUnixTimeMilliseconds()));
        //    }

        //});
        //readonly static Action<Task, object?> continueAction = new((task, taskOperation) =>
        //{
        //    if (task is Task)
        //    {
        //        var t = (Task)task;
        //        var status = (PendingMessageRequest)t.AsyncState;
        //        if (t.Exception != null)
        //            t.Exception.Handle((e) =>
        //            {
        //                //Handle Exception and if OK, return true.
        //                Trace.Write(String.Format("Exception {0}", DateTimeOffset.Now.ToUnixTimeMilliseconds()));
        //                return true;
        //            });
        //        else
        //            Trace.Write(String.Format("No exception? {0}", DateTimeOffset.Now.ToUnixTimeMilliseconds()));

        //        //Trace.Write(String.Format("Status {0}", status.cancellationToken.IsCancellationRequested));
        //        Trace.Write(String.Format("Continue {0}", DateTimeOffset.Now.ToUnixTimeMilliseconds()));
        //    }

        //});
        //readonly static TaskTimeoutStrategy<TaskOperationControl<TaskOperation<PendingMessageRequest>>> TimeoutAction =
        //    new TaskTimeoutStrategy<TaskOperationControl<TaskPendingRequestOperation>>(
        //    (taskControl) =>
        //    {
        //        if (taskControl.TaskOperation.Exception != null)
        //        {
        //            Trace.Write(taskControl.Exception.ToString());
        //            //cts.Cancel();
        //            //Trace.Write(cts.Token.IsCancellationRequested ? "Yes" : "No");
        //        }
        //    }, 5000);
        //readonly static Action<Task> ExceptionAction = new((task) =>
        //{
        //    if (task.Exception != null)
        //    {
        //        Trace.Write(task.Exception.ToString());
        //        //cts.Cancel();
        //        //Trace.Write(cts.Token.IsCancellationRequested ? "Yes" : "No");
        //    }
        //});
        //readonly static Action<Task> CancelAction = new((task) =>
        //{
        //    if (task.Exception != null)
        //    {
        //        Trace.Write("Cancelled");
        //    }
        //});
        public class PendingMessageRequest
        {
            public DataRequestMessage RequestMessage { get; set; }
            public PendingMessageRequest(DataRequestMessage request)
            {
                RequestMessage = request;
            }
        };
       
        public class TaskKillStrategy 
        {
            private bool _executed = false;
            private TaskExceptionStrategy _onException = new();
            protected TaskExceptionStrategy OnException { get { return _onException; } set { _onException = value; } }
            public TaskKillStrategy() { }
            
            public TaskKillStrategy(TaskExceptionStrategy exceptionStrategy) { 
                OnException = exceptionStrategy?? throw new ArgumentException("Argument exceptionStrategy is null reference."); ;
            }
            public TaskKillStrategy(TaskKillStrategy obj) {
                _executed = obj._executed;
                OnException = obj.OnException.Clone();
            }
            public TaskKillStrategy Clone()
            {
                return new TaskKillStrategy(this);
            }
            protected virtual void ExecuteToOverride()
            {
                if (_executed) return;
                _executed = true;
                MessageBox.Show("Task kill strategy has not been implemented.\nIt's highly recommended to restart application.\nIf you want, you can wait for other tasks to finish.");
            }
            public void Execute()
            {
                try
                {
                    ExecuteToOverride();
                }
                catch (Exception exception)
                {
                    OnException.Execute(exception);
                }
            }

        }
        public class TaskTimeoutStrategy<T> 
        {
            private class TaskTimeoutStrategyException : Exception
            {
                public TaskTimeoutStrategyException() : 
                    base(String.Format("TaskTimeout occured. Application will be closed.")) {}
            }
            private int _timeout = 0;
            public TimeSpan Timeout { get { return new TimeSpan(0,0,0,0,_timeout); } }
            private Action<T> _onTimeout = new ((x) => throw new TaskTimeoutStrategyException());
            protected Action<T> OnTimeout { get { return _onTimeout; } set { _onTimeout = value; } }

            private TaskExceptionStrategy _onException = new();
            protected TaskExceptionStrategy OnException { get { return _onException; } set { _onException = value; } }
            public TaskTimeoutStrategy()
            {}
            public TaskTimeoutStrategy(Action<T> onTimeout, int timeout)
            {
                OnTimeout = onTimeout ?? throw new ArgumentException("Argument onTimeout is null reference.");
                if (timeout == 0)
                {
                    throw new ArgumentException("Timeout cannot be zero.");
                }
                _timeout = timeout;
            }
            public TaskTimeoutStrategy(TaskExceptionStrategy taskException, Action<T> onTimeout, int timeout)
            {
                OnTimeout = onTimeout ?? throw new ArgumentException("Argument onTimeout is null reference.");
                if (timeout == 0)
                {
                    throw new ArgumentException("Timeout cannot be zero.");
                }
                _timeout = timeout;
                OnException = taskException ?? throw new ArgumentException("Argument taskException is null reference.");
            }
            public TaskTimeoutStrategy(TaskTimeoutStrategy<T> obj)
            {
                _timeout = obj._timeout;
                OnException = obj.OnException.Clone();
                OnTimeout = (Action<T>) obj.OnTimeout.Clone();
            }
            public TaskTimeoutStrategy<T> Clone()
            {
                return new TaskTimeoutStrategy<T>(this);
            }
            public void Execute(T parameter)
            {
                try
                {
                    OnTimeout(parameter);
                }
                catch (Exception ex)
                {
                    OnException.Execute(ex);
                }
            }
        }
        public class TaskExceptionStrategy 
        {
            private class TaskExceptionStrategyException : Exception
            {
                public TaskExceptionStrategyException() :
                    base(String.Format("TaskException occured. Application will be closed."))
                { }
            }
            static private readonly TaskExceptionStrategy DefaultTaskExceptionStrategy = new ();
            //private static Action<Exception>? _onUnhandledException = new Action<Exception>(
            //    (Exception unhandled) =>
            //    {

            //    }
            //);
            //protected static Action<Exception>? OnUnhandledException { get { return _onUnhandledException; } set { _onUnhandledException = value; } }
            private Func<Exception, bool> _onException = new ((_) => throw new TaskExceptionStrategyException());
            protected Func<Exception, bool> OnException { get { return _onException; } set { _onException = value; } }

            private TaskExceptionStrategy _exceptionStrategy = DefaultTaskExceptionStrategy;
            protected TaskExceptionStrategy ExceptionStrategy { get { return _exceptionStrategy; } set { _exceptionStrategy = value; } }
            public TaskExceptionStrategy() { }
            public TaskExceptionStrategy(Func<Exception, bool> onException)
            {
                OnException = onException ?? throw new ArgumentException("Argument onException is null reference.");
            }
            public TaskExceptionStrategy(TaskExceptionStrategy taskExceptionStrategy, Func<Exception, bool> onException)
            {
                OnException = onException ?? throw new ArgumentException("Argument onException is null reference.");
                ExceptionStrategy = taskExceptionStrategy ?? throw new ArgumentException("Argument taskExceptionStrategy is null reference.");
            }
            public TaskExceptionStrategy(TaskExceptionStrategy obj)
            {
                OnException = (Func<Exception, bool>)obj.OnException.Clone();
                ExceptionStrategy = obj.ExceptionStrategy.Clone();
            }
            public TaskExceptionStrategy Clone()
            {
                return new TaskExceptionStrategy(this);
            }
            public bool Execute(Exception exception)
            {
                try
                {
                    if (OnException != null)
                        return OnException(exception);
                    return false;
                }
                catch (Exception internalException)
                {
                    if(ExceptionStrategy != null)
                    {
                        ExceptionStrategy.Execute(internalException);
                        return false;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
        public class TaskCancelStrategy<T>
        {

            private Action<T>? _onCancelled = null;
            protected Action<T>? OnCancelled { get { return _onCancelled; } set { _onCancelled = value; } }

            private TaskExceptionStrategy _onException = new();
            protected TaskExceptionStrategy OnException { get { return _onException; } set { _onException = value; } }
            public TaskCancelStrategy(Action<T>? onCancelled = null)
            {
                OnCancelled = onCancelled;
            }
            public TaskCancelStrategy(TaskExceptionStrategy onException, Action<T>? onCancelled = null)
            {
                OnCancelled = onCancelled;
                OnException = onException ?? throw new ArgumentException("Argument onException is null reference.");
            }
            public TaskCancelStrategy(TaskCancelStrategy<T> obj)
            {
                OnException = obj.OnException.Clone();
                OnCancelled = obj.OnCancelled != null? (Action<T>?)obj.OnCancelled.Clone() : null;
            }
            public TaskCancelStrategy<T> Clone()
            {
                return new TaskCancelStrategy<T>(this);
            }
            public void Execute(T parameter)
            {
                try
                {
                    if (OnCancelled != null)
                        OnCancelled(parameter);
                }
                catch (Exception exception)
                {
                    OnException.Execute(exception);
                }
            }
        }

        public class TaskOperationControl<TTaskOperation> where TTaskOperation : TaskOperationBase
        {
            private readonly TTaskOperation _taskOperation;
            public TTaskOperation TaskOperation { get { return (TTaskOperation)_taskOperation; } }
            public TaskOperationControl(TTaskOperation taskOperation) { 
                _taskOperation = taskOperation;
            }
            public void Cancel()
            {
                TaskOperation.TokenSource.Cancel();
            }
            public bool IsCancelRequested()
            {
                return TaskOperation.TokenSource.Token.IsCancellationRequested;
            }
        }

        public abstract class TaskOperationBase
        {
            private readonly CancellationTokenSource _cts = new();
            public CancellationTokenSource TokenSource { get { return _cts; } }

            private TaskKillStrategy _taskKillStrategy = new ();
            protected TaskKillStrategy TaskKillStrategy { get { return _taskKillStrategy; } set { _taskKillStrategy = value; } }

            private Task _task;
            protected Task Task { get { return _task; } set { _task = value; } }
            abstract public Task Start();
            public void Kill()
            {
                try
                {
                    TaskKillStrategy.Execute();
                }
                catch (Exception exception)
                {
                }
            }
            public async void Wait()
            {
                await Task;
            }
            public void Cancel()
            {
                TokenSource.Cancel();
            }
            public bool IsCancelRequested()
            {
                return TokenSource.IsCancellationRequested;
            }
        }
        public abstract class TaskOperationBase<Derived> : TaskOperationBase 
            where Derived : class            
        {
            private TaskTimeoutStrategy<Derived> _onTimeout = new ();
            protected TaskTimeoutStrategy<Derived> OnTimeout { get { return _onTimeout; } set { _onTimeout = value; } }

            private TaskCancelStrategy<Derived> _onCancelled = new ();
            protected TaskCancelStrategy<Derived> OnCancelled { get { return _onCancelled; } set { _onCancelled = value; } }

            private TaskExceptionStrategy _onException = new ();
            protected TaskExceptionStrategy OnException { get { return _onException; } set { _onException = value; } }

            public TaskOperationBase(TaskTimeoutStrategy<Derived> onTimeout, TaskCancelStrategy<Derived> onCancelled, TaskExceptionStrategy onException)
            {
                OnTimeout = onTimeout;
                OnCancelled = onCancelled;
                OnException = onException;
            }
            override public Task Start()
            {
                if (Task == null)
                {
                    throw new NullReferenceException("Configure Task before you call base.Start (TaskOperationBase) method");

                }
                if(Task.Status != TaskStatus.Created) {
                    throw new Exception("Trying to start already running task.");
                }

                var MainTask = Task;
                Task.Start();
                if(OnTimeout != null)
                {
                    MainTask = MainTask.WaitAsync(OnTimeout.Timeout);
                }
                return MainTask.ContinueWith((task) =>
                {
                    if (task != null)
                    {     
                        // Exception
                        if (task.IsFaulted)
                        {
                            Exception innerEx = task.Exception?.GetBaseException();
                            if (innerEx is TimeoutException)
                            {
                                if (OnTimeout != null)
                                    OnTimeout.Execute(this as Derived);
                            }
                            else
                            {
                                if(innerEx is TaskCanceledException && innerEx.InnerException is TimeoutException)
                                {
                                    // User internal timeout exception - handle as exception - unwrap it
                                    innerEx = innerEx.InnerException;
                                }
                                try
                                {
                                    OnException.Execute(innerEx);
                                }
                                catch (Exception unhandledExceptions) // Catch unhandled
                                {
                                    throw;
                                    // Catch or not (global unhandled exception or not)
                                }
                            }
                        }

                        if (task.IsCanceled)
                        {
                            if(OnCancelled != null)
                                OnCancelled.Execute(this as Derived);
                        }

                    }
                    else
                    {
                        throw new Exception("Critical exception: task is null in ContinueWith function");
                    }
                });
            }
        }
        public class TaskOperation : TaskOperationBase<TaskOperationControl<TaskOperation>>
        {

            public readonly Action<TaskOperationControl<TaskOperation>> MainAction;
            public TaskOperation(Action<TaskOperationControl<TaskOperation>> mainAction,
                                TaskTimeoutStrategy<TaskOperationControl<TaskOperation>> onTimeout,
                                TaskCancelStrategy<TaskOperationControl<TaskOperation>> onCancelled,
                                TaskExceptionStrategy onException)
                : base(onTimeout, onCancelled, onException)
            {
                MainAction = mainAction;
            }
            override public Task Start()
            {
                // Prepare task first
                Task = new Task(() =>
                {
                    try
                    {
                        MainAction(new TaskOperationControl<TaskOperation>(this));
                    }
                    catch(TimeoutException e)// To wrap up timeout exception and distingish it from my timeout waiting for taskOperation
                    {
                        throw new TaskCanceledException("User task inner timeout", e);
                    } 
                });
                return base.Start();
            }
        }
        public class TaskOperation<T> : TaskOperationBase<TaskOperationControl<TaskOperation<T>>>
        {
            public readonly Action<TaskOperationControl<TaskOperation<T>>> MainAction;
            public T MainActionParameter;

            public TaskOperation(Action<TaskOperationControl<TaskOperation<T>>> mainAction,
                                T mainActionParameter,
                                TaskTimeoutStrategy<TaskOperationControl<TaskOperation<T>>> onTimeout,
                                TaskCancelStrategy<TaskOperationControl<TaskOperation<T>>> onCancelled,
                                TaskExceptionStrategy onException)
                : base(onTimeout, onCancelled, onException)
            {
                MainAction = mainAction;
                MainActionParameter = mainActionParameter;
            }
            override public Task Start()
            {
                // Prepare task first
                Task = new Task(() =>
                {
                    try
                    {
                        MainAction(new TaskOperationControl<TaskOperation<T>>(this));
                    }
                    catch (TimeoutException e)// To wrap up timeout exception and distingish it from my timeout waiting for taskOperation
                    {
                        throw new TaskCanceledException("User task inner timeout", e);
                    }
                });
               return base.Start();
            }
        }

        //public class TaskExternalOperation
        //{
        //    private readonly Action<object?>? MainAction = null;
        //    private readonly Action<Task>? OnTimeout = null;
        //    private readonly Action<Task>? OnException = null;
        //    private readonly Action<Task>? OnCancelled = null;

        //    public TaskExternalOperation( Action<Task>? onTimeout = null, Action<Task>? onException = null, Action<Task>? onCancelled = null)
        //    {                
        //        OnTimeout = onTimeout;
        //        OnException = onException;
        //        OnCancelled = onCancelled;
        //    }
        //}
        //public class ConfigurableTask<T>
        //{
        //    private TaskOperation taskOperation;
        //    ConfigurableTask(Action<object?> mainAction,
        //                        T mainActionParameter,
        //                        Action<Task>? onTimeout = null,
        //                        Action<Task>? onCancelled = null,
        //                        Action<Task>? onException = null)
        //    {
        //        //taskOperation = new TaskOperation(
        //        //    mainAction,
        //        //    mainActionParameter,
        //        //    onTimeout,
        //        //    onException,
        //        //    onCancelled
        //        //    );
        //    }
        //}
        public class OperationsBasedTask
        {
            private int Index = 0;
            private readonly TaskOperationBase[] taskOperations;
            private readonly Action<Task> action;
            private Task task;
            public OperationsBasedTask(TaskOperationBase[] taskOperations)
            {
                this.taskOperations = taskOperations;
                action = (task) =>
                {
                    if (task != null && task.IsCompletedSuccessfully)
                    {
                        Index++;
                        CurrentOperation.Start()
                        .WaitAsync(default(CancellationToken))
                        .ContinueWith(action);
                    }
                };
            }

            private TaskOperationBase _currentOperation { get; set; }
            public TaskOperationBase CurrentOperation { get { return _currentOperation; } }

            public void Start()
            {
                _currentOperation = taskOperations[Index++];
                if(CurrentOperation != null)
                {
                    task = CurrentOperation.Start()
                        .WaitAsync(default(CancellationToken))
                        .ContinueWith(action);
                }
            }
        }

        readonly static Action<object?> RequestMainAction = new((request) =>
        {
            if (request is PendingMessageRequest)
            {
                
                //WeakReferenceMessenger.Default.Send(pendingMessageRequest.requestMessage);
                while(true)
                {
                    Trace.Write(String.Format("Main: {0}", DateTimeOffset.Now.ToUnixTimeMilliseconds()));
                    Task.Delay(1000).Wait();
                   //pendingMessageRequest.cancellationToken.ThrowIfCancellationRequested();
                }
               // pendingMessageRequest.cancellationToken.ThrowIfCancellationRequested();
                Task.Delay(1000).Wait();
                //throw new Exception();
                Trace.Write(String.Format("Main: {0}", DateTimeOffset.Now.ToUnixTimeMilliseconds()));
            }
            else
            {
                if (request == null)
                    throw new ArgumentNullException("PendingMessageRequest is null");
                else
                    throw new InvalidCastException("PendingMessageRequest: incorrect type provided");
            }

        });

        public class MessageRequestTask : OperationsBasedTask
        {            
            // 1. Send request and wait for reception confirmation and wait for response
            // On exception (depends on current state perform recovery actions)
            // 2. Process response
            public MessageRequestTask(DataRequestMessage request) 
                : base(new TaskOperation[] { 
                    //new TaskOperation<PendingMessageRequest>(
                    //    RequestMainAction,
                    //    new PendingMessageRequest(request),
                    //    TimeoutAction,
                    //    ExceptionAction,
                    //    CancelAction)  
                })
            {                
            }

        }

        public class RequestControl
        {
            private MessageRequestTask? Task { get; set; } = null;
            public RequestControl(MessageRequestTask requestTask ) {
                Task = requestTask;
                Task.Start();
            }
        }
        
        public static RequestControl SendRequest(DataRequestMessage request, Action OnResponseAction, Action? preAction = null, Action? postAction = null, Action? OnTimeoutAction = null)
        {
            
            //var PendingMessageRequest = new PendingMessageRequest(request);

            return new RequestControl(new MessageRequestTask(request));
            
        }

    }
}
