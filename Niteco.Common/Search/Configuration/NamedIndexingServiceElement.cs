using System;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Configuration;

namespace Niteco.Common.Search.Configuration
{
	public class NamedIndexingServiceElement : ConfigurationElement
	{
		private X509Certificate2 clientCertificate;
		[ConfigurationProperty("name", IsRequired = true, IsKey = true)]
		public string Name
		{
			get
			{
				return (string)base["name"];
			}
			set
			{
				base["name"] = value;
			}
		}
		[ConfigurationProperty("baseUri", IsRequired = true)]
		public Uri BaseUri
		{
			get
			{
				return (Uri)base["baseUri"];
			}
			set
			{
				base["baseUri"] = value;
			}
		}
		[ConfigurationProperty("accessKey", IsRequired = true)]
		public string AccessKey
		{
			get
			{
				return (string)base["accessKey"];
			}
			set
			{
				base["accessKey"] = value;
			}
		}
		[ConfigurationProperty("certificate", IsRequired = false)]
		public CertificateReferenceElement Certificate
		{
			get
			{
				return (CertificateReferenceElement)base["certificate"];
			}
			set
			{
				base["certificate"] = value;
			}
		}
		[ConfigurationProperty("certificateAllowUntrusted", IsRequired = false, DefaultValue = false)]
		public bool CertificateAllowUntrusted
		{
			get
			{
				return (bool)base["certificateAllowUntrusted"];
			}
			set
			{
				base["certificateAllowUntrusted"] = value;
			}
		}
		internal X509Certificate2 GetClientCertificate()
		{
			CertificateReferenceElement certificate = this.Certificate;
			if (certificate == null || !certificate.ElementInformation.IsPresent)
			{
				return null;
			}
			if (this.clientCertificate != null)
			{
				return this.clientCertificate;
			}
			X509Store x509Store = new X509Store(certificate.StoreName, certificate.StoreLocation);
			try
			{
				x509Store.Open(OpenFlags.OpenExistingOnly);
				X509Certificate2Collection x509Certificate2Collection = x509Store.Certificates.Find(certificate.X509FindType, certificate.FindValue, false);
				if (x509Certificate2Collection.Count == 0)
				{
					throw new System.InvalidOperationException("Unable to find client certificate.");
				}
				this.clientCertificate = x509Certificate2Collection[0];
			}
			finally
			{
				x509Store.Close();
			}
			return this.clientCertificate;
		}
	}
}
