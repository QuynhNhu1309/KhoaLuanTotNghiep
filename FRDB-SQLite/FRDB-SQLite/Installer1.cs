using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.IO;
using System.Security.Principal;


namespace FRDB_SQLite
{
    [RunInstaller(true)]
    public partial class Installer1 : System.Configuration.Install.Installer
    {
        public Installer1()
            : base()
        {
            InitializeComponent();
            AfterInstall += new InstallEventHandler(Installer1_AfterInstall);
        }

        void Installer1_AfterInstall(object sender, InstallEventArgs e)
        {
            //throw new NotImplementedException();
            String sFolder = Path.Combine(Path.GetDirectoryName(Context.Parameters["assemblypath"]), "lib");
            //String sUsername = "NT AUTHORITY\\Administrator";
            //String sUsername = System.Environment.UserDomainName + "\\Administrator";
            String sUsername = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();
            String account = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();
            //account = account.Remove(account.LastIndexOf("\\"));
            IdentityReference identity = new NTAccount(account);// + "\\Administrator");


            DirectoryInfo myDirectoryInfo = new DirectoryInfo(sFolder);
            DirectorySecurity myDirectorySecurity = myDirectoryInfo.GetAccessControl();
            myDirectorySecurity.AddAccessRule(new FileSystemAccessRule(identity,
                FileSystemRights.FullControl, AccessControlType.Allow));
            myDirectoryInfo.SetAccessControl(myDirectorySecurity);

            myDirectorySecurity.AddAccessRule(new FileSystemAccessRule(identity,
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.InheritOnly, AccessControlType.Allow));

            ClearReadOnly(myDirectoryInfo);
        }

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);
            RegistrationServices regSrv = new RegistrationServices();
            regSrv.RegisterAssembly(base.GetType().Assembly,
              AssemblyRegistrationFlags.SetCodeBase);
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            base.Uninstall(savedState);
            RegistrationServices regSrv = new RegistrationServices();
            regSrv.UnregisterAssembly(base.GetType().Assembly);
        }

        public void ClearReadOnly(DirectoryInfo parentDirectory)
        {
            if (parentDirectory != null)
            {
                parentDirectory.Attributes = FileAttributes.Normal;
                foreach (FileInfo fi in parentDirectory.GetFiles())
                {
                    fi.Attributes = FileAttributes.Normal;
                }
                foreach (DirectoryInfo di in parentDirectory.GetDirectories())
                {
                    ClearReadOnly(di);
                }
            }
        }
    }
}
