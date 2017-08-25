/* Copyright (c) 2014 Hieu Le */
using System;
using System.IO;

namespace ImageResizer.Plugins.ImageCache
{
    public class ImageWriter
    {
        private readonly string physicalPath;

        public string PhysicalPath
        {
            get
            {
                return this.physicalPath;
            }
        }

        private readonly MemoryStream data;

        public long DataLength
        {
            get { return this.data.Length; }
        }

        private readonly DateTime modifiedTime;

        public DateTime ModifiedTime
        {
            get
            {
                return this.modifiedTime;
            }
        }

        public long BufferLength
        {
            get { return this.data.Capacity; }
        }

        public ImageWriter(string physicalPath, MemoryStream data, DateTime modifiedTime)
        {
            this.physicalPath = physicalPath;
            this.data = data;
            this.modifiedTime = modifiedTime;
        }

        public MemoryStream GetReadonlyStream()
        {
            return new MemoryStream(this.data.GetBuffer(), 0, (int)this.data.Length, false, true);
        }
    }
}
