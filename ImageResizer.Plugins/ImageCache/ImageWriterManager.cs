/* Copyright (c) 2014 Hieu Le */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ImageResizer.Plugins.ImageCache
{
    public class ImageWriterManager
    {
        private static readonly object sync = new object();

        private readonly Dictionary<string, ImageWriter> writers = new Dictionary<string, ImageWriter>();

        public static long maxBuffer = 1024*1024*8;

        public long QueuedBufferLength
        {
            get
            {
                lock (sync)
                {
                    return writers.Values.Where(value => value != null).Sum(value => value.BufferLength);
                }
            }
        }

        public delegate void ImageWriterDelegate(ImageWriter w);

        public bool Queue(ImageWriter imageWriter, ImageWriterDelegate writerDelegate)
        {
            lock (sync)
            {
                if (this.QueuedBufferLength + imageWriter.BufferLength > maxBuffer)
                {
                    return false;
                }

                if (writers.ContainsKey(GetImageWriterKey(imageWriter.PhysicalPath, imageWriter.ModifiedTime)))
                {
                    return false;
                }

                if (!ThreadPool.QueueUserWorkItem(delegate(object state)
                                                  {
                                                      var job = state as ImageWriter;
                                                      writerDelegate(job);
                                                  }, imageWriter))
                {
                    return false;
                }
                return true;
            }
        }

        public static string GetImageWriterKey(string physicalPath, DateTime modifiedTime)
        {
            return string.Format("{0}-{1}", physicalPath, modifiedTime.Ticks);
        }

        public ImageWriter Get(string key)
        {
            lock (sync)
            {
                ImageWriter imageWriter;
                if (this.writers.TryGetValue(key, out imageWriter))
                {
                    return imageWriter;
                }
                return null;
            }
        }

        public void Remove(string key)
        {
            lock (sync)
            {
                this.writers.Remove(key);
            }
        }
    }
}
