using System;

namespace RustMyAdmin.Backend.Security {

    using ListShuffle;

    using System.Net.NetworkInformation;
    using System.Security.Cryptography;
    using System.Text;

    public class DataEncryptor<T> : IEncryptor where T : ICryptoTransform {

        public T Rng { get; private set; }

        public DataEncryptor(T rng) {
            Rng = rng;
            File.WriteAllBytes(Extensions.SaltFile.FullName, GenerateSalt());
        }

        public string Decrypt(byte[] encrypted) {
            throw new NotImplementedException();
        }

        public byte[] DecryptBytes(byte[] encrypted) {
            throw new NotImplementedException();
        }

        public byte[] Encrypt(string unencrypted) => EncryptBytes(Encoding.Default.GetBytes(unencrypted));

        public byte[] EncryptBytes(byte[] unencrypted) {
            using (var outputStream = new MemoryStream())
            using (var cryptStream = new CryptoStream(outputStream, Rng, CryptoStreamMode.Write))
            using (var sWriter = new StreamWriter(cryptStream)) {
                sWriter.Write(GenerateSalt());
                sWriter.Write(unencrypted);

                return outputStream.ToArray();
            }
        }

        public async Task<byte[]> GetBytesAsync(byte[] unencrypted) {
            using (var outputStream = new MemoryStream())
            using (var cryptStream = new CryptoStream(outputStream, Rng, CryptoStreamMode.Write)) {
                var salt = GenerateSalt();
                await cryptStream.WriteAsync(salt, 0, salt.Length);
                await cryptStream.WriteAsync(unencrypted, 0, unencrypted.Length);

                return outputStream.ToArray();
            }
        }

        public byte[] GenerateSalt() {
            if (Extensions.SaltFile.Exists) {
                return File.ReadAllBytes(Extensions.SaltFile.FullName);
            }

            var sysKey = Encoding.Default.GetBytes(Environment.MachineName + Environment.UserName);
            var macAddress = GetPhysicalAddresses().Select(x => x.GetAddressBytes());
            var uuid = new Guid().ToByteArray();

            var unifiedKey = sysKey.ToList();
            macAddress.ToList().ForEach(x => unifiedKey.AddRange(x));
            unifiedKey.AddRange(uuid);

            unifiedKey.Shuffle();
            return unifiedKey.ToArray();
        }

        public PhysicalAddress[] GetPhysicalAddresses() {
            return NetworkInterface.GetAllNetworkInterfaces().Select(x => x.GetPhysicalAddress()).ToArray();
        }

    }
}

