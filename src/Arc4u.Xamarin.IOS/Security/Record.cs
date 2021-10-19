using Arc4u.Diagnostics;
using Foundation;
using Security;
using System;

namespace Arc4u.Security
{
    /// <summary>
    /// The class will let you store, remove and update data in a dedicated and secure store from the iOS device.
    /// But to wotk you need to add in the entitlements.plist file a section
    /// <plist version="1.0">
    //  <dict>
    //  	<key>keychain-access-groups</key>
    //  	<array>
    //  		<string>the identifier of the application defined in the properties of the iOS project.</string>
    //  	</array>
    //  </dict>
    //  </plist>
    // 
    // The entitlements.plist file can be empty but MUST be attach in the project!
    /// </summary>
    public class Record
    {
        private const string ServiceName = "arc4uService";

        public static string GetValue(string key)
        {
            var record = CreateSecRecord(key);

            try
            {
                var match = SecKeyChain.QueryAsRecord(record, out SecStatusCode resultCode);

                if (resultCode == SecStatusCode.Success)
                    return NSString.FromData(match.Generic, NSStringEncoding.UTF8);

            }
            catch (Exception)
            {
                RemoveRecord(record);
            }

            return String.Empty;
        }

        public static void SetValue(string key, string value)
        {
            var record = CreateSecRecord(key);

            if (String.IsNullOrEmpty(value))
            {
                if (!String.IsNullOrEmpty(GetValue(key)))
                    RemoveRecord(record);

                return;
            }

            // if the key already exists, remove it
            if (!String.IsNullOrEmpty(GetValue(key)))
                RemoveRecord(record);

            var result = SecKeyChain.Add(CreateRecordForNewKeyValue(key, value));
            if (result != SecStatusCode.Success)
            {
                throw new Exception(String.Format("Error adding record: {0}", result));
            }
        }

        public static bool Remove(string key)
        {
            var record = CreateSecRecord(key);

            return RemoveRecord(record); ;
        }

        private static SecRecord CreateRecordForNewKeyValue(string key, string value)
        {
            var record = CreateSecRecord(key);
            record.Generic = NSData.FromString(value, NSStringEncoding.UTF8);
            record.Accessible = SecAccessible.Always;

            return record;
        }

        private static SecRecord CreateSecRecord(string key)
        {
            return new SecRecord(SecKind.GenericPassword)
            {
                Account = key,
            };
        }

        private static bool RemoveRecord(SecRecord record)
        {
            try
            {
                var result = SecKeyChain.Remove(record);
                if (result != SecStatusCode.Success)
                {
                    Logger.Technical.From<Record>().Error($"Error removing record: {result}.").Log();
                }

                return result == SecStatusCode.Success;
            }
            catch (Exception ex)
            {
                Logger.Technical.From<Record>().Exception(ex).Log();
            }

            return false;

        }
    }
}
