using System;

namespace RustMyAdmin.Backend {

    using System.IO;

    public static class Extensions {

        public static DirectoryInfo RustMyAdminUserDir {
            get => new DirectoryInfo(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".rustmyadmin"
            ));
        }

        public static DirectoryInfo RustMyAdminCfgDir {
            get => RustMyAdminUserDir.CreateSubdirectory("cfg");
        }

        public static DirectoryInfo RustMyAdminLogDir {
            get => RustMyAdminUserDir.CreateSubdirectory("log");
        }

        public static DirectoryInfo RustMyAdminCryptDir {
            get => RustMyAdminUserDir.CreateSubdirectory("crypt");
        }

        public static FileInfo SaltFile {
            get => RustMyAdminCryptDir.CreateFile(".salt");
        }

        public static FileInfo ConfigFile {
            get => RustMyAdminCfgDir.CreateFile(".cfg.json");
        }

        static Extensions() {
            RustMyAdminUserDir.Create();
            RustMyAdminCfgDir.Create();
            RustMyAdminCryptDir.Create();
            RustMyAdminLogDir.Create();
        }

        public static FileInfo CreateFile(this DirectoryInfo dir, string fileName) {
            var file = new FileInfo(Path.Combine(dir.FullName, fileName));
            using (file.Create()) { }

            return file;
        }

    }
}

