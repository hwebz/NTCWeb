using System;
using System.Timers;

namespace Niteco.Search.IndexingService
{
	public class TaskQueue
	{
		internal class QueueItem
		{
			internal Action Task
			{
				get;
				set;
			}
			internal System.DateTime Time
			{
				get;
				set;
			}
			internal QueueItem(Action task)
			{
				this.Task = task;
				this.Time = System.DateTime.Now;
			}
		}
		private readonly Timer _queueFlushTimer;
		private readonly System.Collections.Queue _queue = System.Collections.Queue.Synchronized(new System.Collections.Queue());
		private readonly double _timerInterval;
		private readonly System.TimeSpan _minQueueItemAge;
		private readonly string _queueName;
		public event System.EventHandler QueueProcessed;
		public int QueueLength
		{
			get
			{
				return this._queue.Count;
			}
		}
		public TaskQueue(string queueName, double timerInterval, System.TimeSpan minQueueItemAge)
		{
			this._queueName = queueName;
			this._timerInterval = timerInterval;
			this._minQueueItemAge = minQueueItemAge;
			this._queueFlushTimer = new Timer(this._timerInterval);
			this._queueFlushTimer.AutoReset = false;
			this._queueFlushTimer.Elapsed += new ElapsedEventHandler(this.Timer_Elapsed);
		}
		public void Enqueue(Action task)
		{
			this._queue.Enqueue(new TaskQueue.QueueItem(task));
			this._queueFlushTimer.Enabled = true;
		}
		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				while (this._queue.Count > 0 && ((TaskQueue.QueueItem)this._queue.Peek()).Time < System.DateTime.Now.Add(-this._minQueueItemAge))
				{
					try
					{
						((TaskQueue.QueueItem)this._queue.Dequeue()).Task();
					}
					catch (System.Exception ex)
					{
						IndexingServiceSettings.IndexingServiceServiceLog.Error(string.Format("An exception was thrown when task was invoked by TaskQueue: '{0}'. The message was: {1}. Stacktrace was: {2}", this._queueName, ex.Message, ex.StackTrace));
					}
				}
				this.OnQueueProcessed();
			}
			catch (System.Exception ex2)
			{
				IndexingServiceSettings.IndexingServiceServiceLog.Error(string.Format("An exception was thrown while processing TaskQueue: '{0}'. The message was: {1}. Stacktrace was: {2}", this._queueName, ex2.Message, ex2.StackTrace));
			}
			finally
			{
				if (this._queue.Count > 0)
				{
					this._queueFlushTimer.Enabled = true;
				}
			}
		}
		private void OnQueueProcessed()
		{
			if (this.QueueProcessed != null)
			{
				this.QueueProcessed(null, new System.EventArgs());
			}
		}
	}
}
