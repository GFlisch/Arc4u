using Android.Content;
using Android.Runtime;
using Java.Security;
using Javax.Crypto;
using Newtonsoft.Json;
using System;
using System.Text;

namespace Arc4u.Security
{
    /// <summary>
    /// AccountStore that uses a KeyStore of PrivateKeys protected by a fixed password
    /// in a private region of internal storage.
    /// </summary>
    public class SafeStore
    {
        Context context;
        KeyStore ks;
        KeyStore.PasswordProtection prot;

        static readonly object fileLock = new object();
        readonly string FileName;
        static char[] Password;

        /// <summary>
        /// Initializes a new instance of the <see cref="Xamarin.Auth.AndroidAccountStore"/> class
        /// with a KeyStore password provided by the application.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="password">KeyStore Password.</param>
        public SafeStore(Context context, string password, string fileName = "Arc4u.Secure.Store")
        {
            if (String.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException("password");
            }
            Password = password.ToCharArray();

            if (String.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("fileName");
            }
            FileName = fileName;

            this.context = context;

            ks = KeyStore.GetInstance(KeyStore.DefaultType);
            prot = new KeyStore.PasswordProtection(Password);

            try
            {
                lock (fileLock)
                {
                    if (!this.FileExists(context, FileName))
                    {
                        LoadEmptyKeyStore(Password);
                    }
                    else
                    {
                        using (var s = context.OpenFileInput(FileName))
                        {
                            ks.Load(s, Password);
                        }
                    }
                }
            }
            catch (Java.IO.FileNotFoundException)
            {
                LoadEmptyKeyStore(Password);
            }
        }

        protected void Save()
        {
            lock (fileLock)
            {
                using (var s = context.OpenFileOutput(FileName, FileCreationMode.Private))
                {
                    ks.Store(s, Password);
                    s.Flush();
                    s.Close();
                }
            }

            return;
        }

        class SecretData<T> : Java.Lang.Object, ISecretKey
        {
            byte[] bytes;
            public SecretData(T data)
            {
                var serializer = JsonConvert.SerializeObject(data);

                bytes = UnicodeEncoding.UTF8.GetBytes(serializer);

            }

            public byte[] GetEncoded()
            {
                return bytes;
            }
            public string Algorithm
            {
                get
                {
                    return "RAW";
                }
            }
            public string Format
            {
                get
                {
                    return "RAW";
                }
            }
        }

        static IntPtr id_load_Ljava_io_InputStream_arrayC;

        /// <summary>
        /// Work around Bug https://bugzilla.xamarin.com/show_bug.cgi?id=6766
        /// </summary>
        void LoadEmptyKeyStore(char[] password)
        {
            if (id_load_Ljava_io_InputStream_arrayC == IntPtr.Zero)
            {
                id_load_Ljava_io_InputStream_arrayC = JNIEnv.GetMethodID(ks.Class.Handle, "load", "(Ljava/io/InputStream;[C)V");
            }
            IntPtr intPtr = IntPtr.Zero;
            IntPtr intPtr2 = JNIEnv.NewArray(password);
            JNIEnv.CallVoidMethod(ks.Handle, id_load_Ljava_io_InputStream_arrayC, new JValue[]
            {
                new JValue (intPtr),
                new JValue (intPtr2)
            });
            JNIEnv.DeleteLocalRef(intPtr);
            if (password != null)
            {
                JNIEnv.CopyArray(intPtr2, password);
                JNIEnv.DeleteLocalRef(intPtr2);
            }
        }

        bool FileExists(Context context, String filename)
        {
            Java.IO.File file = context.GetFileStreamPath(filename);
            if (file == null || !file.Exists())
            {
                return false;
            }

            return true;
        }

        public bool DeleteStore()
        {
            Java.IO.File file = context.GetFileStreamPath(FileName);
            if (file == null || !file.Exists())
            {
                return true;
            }

            return file.Delete();
        }

        public T Get<T>(string key)
        {
            var aliases = ks.Aliases();
            while (aliases.HasMoreElements)
            {
                var alias = aliases.NextElement().ToString();
                if (alias.Equals(key.Trim(), StringComparison.InvariantCulture))
                {
                    if (ks.GetEntry(alias, prot) is KeyStore.SecretKeyEntry e)
                    {
                        var bytes = e.SecretKey.GetEncoded();
                        return JsonConvert.DeserializeObject<T>(UnicodeEncoding.UTF8.GetString(bytes));
                    }
                }
            }

            return default(T);
        }

        public void Save<T>(string key, T data)
        {
            var secretKey = new SecretData<T>(data);
            KeyStore.SecretKeyEntry entry = new KeyStore.SecretKeyEntry(secretKey);
            ks.SetEntry(key.Trim(), entry, prot);

            Save();
        }

        public void Delete(string key)
        {
            ks.DeleteEntry(key.Trim());
            Save();
        }

    }
}