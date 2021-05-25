using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Mail;
using System.Text;
using Refactored.Email.Models;

#if NET45
using System.Net.Configuration;
#endif

namespace Refactored.Email.Extensions {
	internal static class MailMessageExtensions {





		


	
		internal static MailAddressCollection AddAddresses(this MailAddressCollection collection, object addresses) {
			if (addresses is MailAddressCollection addressCollection) {
				foreach (MailAddress address in addressCollection)
					collection.Add(address);
			} else {
				if (!(addresses is string) || string.IsNullOrEmpty(addresses.ToString())) {
					return collection;
				}

				string strAddresses = addresses.ToString();

				foreach (string address in strAddresses.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries)) {
					collection.Add(new MailAddress(address));
				}
			}

			return collection;
		}

		public static MailAddress FormatAddress(object address) {
			if (address == null) {
				return null;
			}

			return (address as MailAddress) ?? new MailAddress(address.ToString());
		}



	}
}
