using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data;
using System.Data.SQLite;
using FRDB_SQLite;

namespace FRDB_SQLite
{
    public class ThreadBLL
    {
        #region 1. Fields

        private Thread _workerThread;
        private bool _connected;
        private System.Windows.Forms.MethodInvoker _task;
        public enum RunState { idle, running, canceling };
        private RunState _runState;// = RunState.idle;
        private String _connString;

        public SQLiteConnection Connection = new SQLiteConnection();
        public SQLiteDataAdapter Adapt = new SQLiteDataAdapter();

        #endregion

        #region 2. Properties

        public Thread WorkerThread
        {
            get { return _workerThread; }
            set { _workerThread = value; }
        }

        public Boolean Connected
        {
            get { return _connected; }
            set { _connected = value; }
        }

        public System.Windows.Forms.MethodInvoker Task
        {
            get { return _task; }
            set { _task = value; }
        }

        public RunState _RunState
        {
            get { return _runState; }
            set { _runState = value; }
        }

        public String ConnString
        {
            get { return _connString; }
            set { _connString = value; }
        }

        #endregion

        #region 3. Contructors

        public ThreadBLL(String connString)
        {
            this._workerThread = null;
            this._connected = false;
            this._task = null;
            this._runState = RunState.idle;
            this._connString = connString;
            this.Connection = new SQLiteConnection();
            this.Adapt = new SQLiteDataAdapter();
        }

        public ThreadBLL()
        {
            this._workerThread = null;
            this._connected = false;
            this._task = null;
            this._runState = RunState.idle;
            this._connString = String.Empty;///////////////////////important
            this.Connection = new SQLiteConnection();
            this.Adapt = new SQLiteDataAdapter();
        }

        #endregion

        #region 4. Methods

        public bool Connecting()
        {
            if (_connected)
            {
                return true;
            }

            RunOnWorker(new System.Windows.Forms.MethodInvoker(Connect), true);

            return _connected;
        }

        public void Connect()
        {
            if (_connected) return;

            try
            {
                if (Connection.State == ConnectionState.Closed)
                {
                    Connection.ConnectionString = this.ConnString;// Resource.ConnectionString;
                    Connection.Open();
                    _connected = true;
                }
            }
            catch (SQLiteException ex)
            {
                throw new Exception("ERROR:\n" + ex.Message);
                //MessageBox.Show(sqliteEx.Message);
            }
        }

        public void StartWorker()
        {
            do
            {
                // Wait for main thread wake up!
                //Thread.CurrentThread.Suspend();
                try
                {
                    Thread.Sleep(Timeout.Infinite);
                }
                catch (Exception) { }					// the wakeup call, ie Interrupt() will throw an exception
                // If doing nothing, Form thread will close
                if (_task == null) break;
                // On the contrary, do the task
                _task();
                _task = null;

            } while (true);
        }

        public void StopWorker()
        {
            WaitForWorker();
            //End of Thread
            _workerThread.Interrupt();//Stop thread when task is none
            _workerThread.Join();//Wait until end of thread
        }

        public void WaitForWorker()
        {
            while (_workerThread.ThreadState != ThreadState.WaitSleepJoin || _task != null)
            {
                Thread.Sleep(20);
            }
        }

        public void RunOnWorker(System.Windows.Forms.MethodInvoker method)
        {
            RunOnWorker(method, false);
        }

        public void RunOnWorker(System.Windows.Forms.MethodInvoker method, bool synchronous)
        {
            if (_task != null) 								// already doing something?
            {
                Thread.Sleep(100);					// give it 100ms to finish...
                if (_task != null) return;				// still not finished - cannot run new task
            }

            WaitForWorker();
            _task = method;
            _workerThread.Interrupt();

            if (synchronous)
            {
                WaitForWorker();
            }
        }

        /// <summary>
        /// Wait a query running synchronously (wait until query stop)
        /// This method is called when close a running query
        /// </summary>
        public void Canceling()
        {
            Cancel();

            if (_runState == RunState.running)
            {
                WaitForWorker();
                _runState = RunState.idle;
            }
        }

        public void Cancel()
        {
            if (_runState == RunState.running)
            {
                _runState = RunState.canceling;
                Thread cancelThread = new Thread(new ThreadStart(Adapt.SelectCommand.Cancel));
                cancelThread.Name = "DbClient Cancel Thread";
                cancelThread.Start();
                cancelThread.Join();
            }
        }

        public virtual void Dispose()
        {
            if (_connected)
            {
                Disconnecting();
            }

            StopWorker();
        }

        public void Disconnecting()
        {
            if (_runState == RunState.running)
            {
                Cancel();
            }

            if (_connected)
            {
                RunOnWorker(new System.Windows.Forms.MethodInvoker(Connection.Close), true);
            }
        }

        #endregion

        #region 5. Privates (None)

        #endregion
    }
}
