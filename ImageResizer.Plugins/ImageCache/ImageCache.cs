/* Copyright (c) 2014 Hieu Le */
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Hosting;
using ImageResizer.Caching;
using ImageResizer.Configuration;
using ImageResizer.Plugins.Basic;
using log4net;

namespace ImageResizer.Plugins.ImageCache
{
    public class ImageCache : IPlugin, ICache
    {
        private const string AppDataPathKey = "[appDataPath]";

        private static readonly ILog logger = LogManager.GetLogger(typeof(ImageCache));

        public bool Enabled { get; set; }

        public string CacheDir { get; set; }

        private string physicalCacheDir;

        public string PhysicalCacheDir
        {
            get
            {
                if (string.IsNullOrEmpty(this.physicalCacheDir))
                {
                    if (Path.IsPathRooted(this.CacheDir))
                    {
                        this.physicalCacheDir = this.CacheDir;
                    }
                    else
                    {
                        if (this.CacheDir.StartsWith(AppDataPathKey))
                        {
                            string appDataBasePath = this.GetAppDataBasePath().TrimEnd('\\');
                            this.physicalCacheDir = Environment.ExpandEnvironmentVariables(appDataBasePath + "\\" + this.CacheDir.Substring(AppDataPathKey.Length).Trim('\\'));
                            if (!Path.IsPathRooted(this.physicalCacheDir))
                            {
                                this.physicalCacheDir = Path.Combine(HostingEnvironment.ApplicationPhysicalPath ?? AppDomain.CurrentDomain.BaseDirectory, this.physicalCacheDir);
                            }
                        }
                        else if (this.CacheDir.StartsWith("~"))
                        {
                            this.physicalCacheDir = HostingEnvironment.MapPath(this.CacheDir);
                        }
                        else
                        {
                            this.physicalCacheDir = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, this.CacheDir);
                        }
                    }
                }

                return this.physicalCacheDir;
            }
        }

        private readonly ImageWriterManager imageWriterManager = new ImageWriterManager();

        public IPlugin Install(Config c)
        {
            this.Enabled = c.get("imagecache.enabled", false);
            this.CacheDir = c.get("imagecache.cachedir", "imagecache");
            c.Plugins.add_plugin(this);
            return this;
        }

        public bool Uninstall(Config c)
        {
            c.Plugins.remove_plugin(this);
            return true;
        }

        public bool CanProcess(HttpContext current, IResponseArgs e)
        {
            if (((ResizeSettings)e.RewrittenQuerystring).Cache == ServerCacheMode.No)
            {
                return false;
            }

            return true;
        }

        public void Process(HttpContext current, IResponseArgs e)
        {
            var modifiedTime = e.HasModifiedDate ? e.GetModifiedDateUTC() : DateTime.MinValue;
            var hash = DateTime.MinValue.Equals(modifiedTime) ? this.GetHash(e.RequestKey) : this.GetHash(string.Format("{0}|={1}", e.RequestKey, modifiedTime));

            var subDir = string.Format("{0}{1}{2}", this.PhysicalCacheDir, Path.DirectorySeparatorChar, this.GetSubDir(hash));

            if (!Directory.Exists(subDir))
            {
                Directory.CreateDirectory(subDir);
            }

            var physicalPath = string.Format("{0}{1}{2}.{3}", subDir, Path.DirectorySeparatorChar, this.GetHashString(hash), e.SuggestedExtension);

            Stream stream;
            if (File.Exists(physicalPath))
            {
                var data = File.ReadAllBytes(physicalPath);
                stream = new MemoryStream(data);
                ((ResponseArgs)e).ResizeImageToStream = s => ((MemoryStream)stream).WriteTo(s);
                current.RemapHandler(new NoCacheHandler(e));
                if (logger.IsDebugEnabled)
                {
                    logger.Debug("Get image from cached file: " + physicalPath);
                }
            }
            else
            {
                var memoryStream = new MemoryStream(4096);
                e.ResizeImageToStream(memoryStream);
                memoryStream.Position = 0;

                var imageWriterKey = ImageWriterManager.GetImageWriterKey(physicalPath, modifiedTime);
                var imageWriter = this.imageWriterManager.Get(imageWriterKey);

                if (imageWriter == null)
                {
                    imageWriter = new ImageWriter(physicalPath, memoryStream, modifiedTime);
                    this.imageWriterManager.Queue(imageWriter, delegate(ImageWriter job)
                    {
                        try
                        {
                            if (logger.IsDebugEnabled)
                            {
                                logger.Debug("Begin writing image to file: " + job.PhysicalPath);
                            }
                            using (
                                var fileStream = new FileStream(job.PhysicalPath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                job.GetReadonlyStream().WriteTo(fileStream);
                                fileStream.Flush();
                            }
                            if (logger.IsDebugEnabled)
                            {
                                logger.Debug("End writing image to file: " + job.PhysicalPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (logger.IsErrorEnabled)
                            {
                                logger.Error(ex);
                            }
                        }
                        finally
                        {
                            this.imageWriterManager.Remove(imageWriterKey);
                        }
                    });
                }

                stream = imageWriter.GetReadonlyStream();
            }

            if (stream != null)
            {
                ((ResponseArgs)e).ResizeImageToStream = s => ((MemoryStream)stream).WriteTo(s);
                current.RemapHandler(new NoCacheHandler(e));
            }
        }

        private byte[] GetHash(string input)
        {
            var md5 = SHA256.Create();
            return md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
        }

        private string GetHashString(byte[] hash)
        {
            var sb = new StringBuilder(hash.Length);
            foreach (var b in hash)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        private string GetSubDir(byte[] hash)
        {
            return EncodeBase16(new[] { (byte)(hash[0] >> 3) });
        }

        private string EncodeBase16(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x", NumberFormatInfo.InvariantInfo).PadLeft(2, '0'));
            }
            return sb.ToString();
        }

        private string GetAppDataBasePath()
        {
            object section = ConfigurationManager.GetSection("episerver.framework");
            if (section != null)
            {
                object value = section.GetType().GetProperty("AppData").GetValue(section, null);
                if (value != null)
                {
                    return value.GetType().GetProperty("BasePath").GetValue(value, null) as string;
                }
            }
            return null;
        }
    }
}
