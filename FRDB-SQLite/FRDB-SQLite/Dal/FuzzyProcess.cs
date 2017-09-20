using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace FRDB_SQLite
{
    public class FuzzyProcess
    {
        private string[] systemFS = new string[] {"young", "middle", "old" };
        #region Continuous
        public List<ConFS> GenerateAllConFS(string path)
        {
            List<ConFS> result = new List<ConFS>();
            try
            {
                string[] filePaths = Directory.GetFiles(path, "*.conFS");
                foreach (var item in filePaths)
                {
                    result.Add(ReadEachConFS(item));
                }

                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }

        public ConFS ReadEachConFS(string path)//Also content the path and name
        {
            ConFS result = new ConFS();
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    String str = reader.ReadLine();
                    String[] list = str.Split(',');

                    //Only 4 members
                    if (list.Length == 4)
                    {
                        result.Name = FuzzySetName(path);
                        result.Bottom_Left = Double.Parse(list[0]);
                        result.Top_Left = Double.Parse(list[1]);
                        result.Top_Right = Double.Parse(list[2]);
                        result.Bottom_Right = Double.Parse(list[3]);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private String FuzzySetName(string path)
        {
            String result = "";
            if (path.Contains("\\") && path.Contains("."))
            {
                int flash = path.LastIndexOf("\\") + 1;
                int point = path.LastIndexOf(".");
                result = path.Substring(flash, point - flash);
            }
            return result;
        }

        private int DeleteFS(string path)//path here mean also content name
        {
            try
            {
                int result = -1;

                if (File.Exists(path))
                {
                    //Add access control for directory
                    //AddDirectorySecurity(path, "Everyone", FileSystemRights.FullControl, AccessControlType.Allow);
                    //AddFileSecurity(path, System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString() 
                    //    , FileSystemRights.FullControl, AccessControlType.Allow);
                    File.Delete(path);
                    result = 1;
                }

                return result;
            }
            catch
            {
                return -1;
            }
        }

        public int DeleteList(List<String> list)
        {
            try
            {
                int result = -1;
                int i = 0;

                foreach (var item in list)
                {
                    result = DeleteFS(item);
                    i++;
                }

                if (i == list.Count) return 1;
                else return -1;
            }
            catch
            {
                return -1;
            }
        }

        private int CreateFS(string path, string content, string newName)//name must be *.conFS or *.disFS
        {//path here not contains file name
            try
            {
                int result = -1;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    if (!File.Exists(path + newName))
                    {
                        //AddDirectorySecurity(path, System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString()
                        //    , FileSystemRights.FullControl, AccessControlType.Allow);
                        //using (StreamWriter writer = new StreamWriter(path + newName))
                        //{disfs
                        //    writer.WriteLine(content);
                        //    writer.Close();
                        //    result = 1;
                        //}
                        using (FileStream fs = new FileStream(path + newName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            StreamWriter tw = new StreamWriter(fs);
                            tw.WriteLine(content);
                            tw.Flush();
                            result = 1;
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public int UpdateFS(string path, string content, string oldName)//path here not contains file name
        {
            try
            {
                int result = -1;
                if (!Directory.Exists(path))

                    Directory.CreateDirectory(path);
                {
                    //Add access control for directory
                    //AddDirectorySecurity(path, System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString()
                    //    , FileSystemRights.FullControl, AccessControlType.Allow);
                    //Delete File
                    result = DeleteFS(path + oldName);
                    //Create file with the same name;
                    //using (StreamWriter writer = new StreamWriter(path + oldName))//oldName means newName :)
                    //{
                    //    writer.WriteLine(content);
                    //    writer.Close();
                    //    result = 1;
                    //}
                    using (FileStream fs = new FileStream(path + oldName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        StreamWriter tw = new StreamWriter(fs);
                        tw.WriteLine(content);
                        tw.Flush();
                        result = 1;
                    }

                    //Add access control for file
                    //AddFileSecurity(path + oldName, System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString()
                    //    , FileSystemRights.FullControl, AccessControlType.Allow);
                }

                //DirectoryInfo di = new DirectoryInfo(path);

                //DirectorySecurity ds = di.GetAccessControl();

                //foreach (AccessRule rule in ds.GetAccessRules(true, true, typeof(NTAccount)))
                //{
                //    System.Windows.Forms.MessageBox.Show(
                //                  rule.IdentityReference.Value.ToString());
                //    System.Windows.Forms.MessageBox.Show(
                //                rule.AccessControlType.ToString());
                //}

                return result;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }//Delete and Re Add file 
        #endregion
        #region DisFS
        
        public List<DisFS> GenerateAllDisFS(string path)//just file path, not contains filename
        {
            List<DisFS> result = new List<DisFS>();
            try
            {
                string[] filePaths = Directory.GetFiles(path, "*.disFS");
                foreach (var item in filePaths)
                {
                    result.Add(ReadEachDisFS(item));
                }

                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }

        public DisFS ReadEachDisFS(string path)
        {
            DisFS result = new DisFS();
            try
            {
                List<String> list = new List<string>();
                using (StreamReader reader = new StreamReader(path))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)//Only have two lines
                    {
                        list.Add(line); // Add to list.
                    }
                }

                result.Name = FuzzySetName(path);
                result.V = list[0];
                result.M = list[1];
                result.ValueSet = SplitString(list[0]);
                result.MembershipSet = SplitString(list[1]);

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public int UpdateFS(string path, List<String> contents, string oldName)
        {
            try
            {
                int result = -1;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                {
                    //Add access control for directory
                    //AddDirectorySecurity(path, (System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString())
                    //    , FileSystemRights.FullControl, AccessControlType.Allow);

                    //Delete File
                    result = DeleteFS(path + oldName);
                    //Create file with the same name;
                    ////using (StreamWriter writer = new StreamWriter(path + oldName))//oldName means newName :)
                    ////{
                    ////    foreach (var item in contents)
                    ////    {
                    ////        writer.WriteLine(item);
                    ////    }

                    ////    writer.Close();
                    ////    result = 1;
                    ////}
                    using (FileStream fs = new FileStream(path + oldName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        StreamWriter tw = new StreamWriter(fs);
                        foreach (var item in contents)
                        {
                            tw.WriteLine(item);
                        }
                        tw.Flush();
                        result = 1;
                    }
                    //Add access control for file
                    //AddFileSecurity(path + oldName, System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString()
                    //    , FileSystemRights.FullControl, AccessControlType.Allow);
                }

                return result;
            }
            catch (Exception ex)
            {
                return -1;
            }
        } 
        #endregion

        public Boolean Exists(string path)
        {
            bool result = false;
            try
            {
                if (File.Exists(path))
                    result = true;
                
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #region Checking Security
        // Adds an ACL entry on the specified file for the specified account. 
        public void AddFileSecurity(string fileName, string account,
            FileSystemRights rights, AccessControlType controlType)
        {

            // Get a FileSecurity object that represents the 
            // current security settings.
            FileSecurity fSecurity = File.GetAccessControl(fileName);

            // Add the FileSystemAccessRule to the security settings.
            fSecurity.AddAccessRule(new FileSystemAccessRule(account,
                rights, controlType));

            // Set the new access settings.
            File.SetAccessControl(fileName, fSecurity);

        }

        // Removes an ACL entry on the specified file for the specified account. 
        public void RemoveFileSecurity(string fileName, string account,
            FileSystemRights rights, AccessControlType controlType)
        {

            // Get a FileSecurity object that represents the 
            // current security settings.
            FileSecurity fSecurity = File.GetAccessControl(fileName);

            // Remove the FileSystemAccessRule from the security settings.
            fSecurity.RemoveAccessRule(new FileSystemAccessRule(account,
                rights, controlType));

            // Set the new access settings.
            File.SetAccessControl(fileName, fSecurity);

        }

        public void AddDirectorySecurity(string path, string account,
            FileSystemRights rights, AccessControlType controlType)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            DirectorySecurity ds = new DirectorySecurity();
            try
            {
                ds = info.GetAccessControl(AccessControlSections.All);
            }
            catch
            {
                ds = info.GetAccessControl();
            }
            //ds.AddAccessRule(new FileSystemAccessRule(new NTAccount(System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString()),
            //    rights, controlType));

            ds.AddAccessRule(new FileSystemAccessRule(account,
                rights, controlType));

            Directory.SetAccessControl(path, ds);
            
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

        private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }

        public void RemoveReadOnly()
        { 
            
        }

        #endregion

        private List<Double> SplitString(String str)
        {
            List<Double> result = new List<double>();

            ///Remove "{", "}" and ","
            String tmp = str.Replace("{", "");
            tmp = tmp.Replace("}", "");
            Char[] seperator = { ',' };
            String[] values = tmp.Split(seperator);

            ///Add value to list after remove unesessary
            foreach (var value in values)
            {
                result.Add(Convert.ToDouble(value));
            }

            return result;
        }
    }

    public class ConFS 
    {
        public String Name { get; set; }
        public Double Bottom_Left { get; set; }
        public Double Top_Left { get; set; }
        public Double Top_Right { get; set; }
        public Double Bottom_Right { get; set; }

        public Double GetMembershipAt(Double value)//value is a point on Ox
        {
            Double result = -1;

            //Validate the value is between two point which limit trapezoidal on Ox.
            if ((value >= this.Bottom_Left && value <= this.Top_Left) || (value >= this.Top_Left && value <= this.Bottom_Left) ||
                (value >= this.Top_Right && value <= this.Bottom_Right) || (value >= this.Bottom_Right && value <= this.Top_Right) ||
                value >= this.Top_Left && value <= this.Top_Right)
            {
                if (value == this.Bottom_Left || value == this.Bottom_Right)
                {
                    result = 0;
                }
                else if (value >= this.Top_Left && value <= this.Top_Right)
                {
                    result = 1;
                }
                else
                {
                    //A (_Bottom_Left, 0)
                    //B (_Top_Left, 1)
                    //C (Top_Right, 1)
                    //D (Bottom_Right, 0)

                    Double OxAB = Math.Abs(this.Top_Left - this.Bottom_Left);
                    Double OxCD = Math.Abs(this.Bottom_Right - this.Top_Right);
                    Double OxValueLeft = Math.Abs(value - this.Bottom_Left);
                    Double OxValueRight = Math.Abs(value - this.Bottom_Right);

                    //The value is on the left side of the trapezoidal between Top_Right and Bottom_Right
                    if ((value > this.Bottom_Left && value < this.Top_Left) || (value > this.Top_Right && value < this.Bottom_Right))
                    {
                        //Base on congruent triangles
                        result = 1 * OxValueLeft / OxAB;
                    }

                    //The value is on the right side of the trapezoidal between _Bottom_Left and _Top_Left
                    if ((value > this.Top_Left && value < this.Bottom_Right) || (value > this.Bottom_Right && value < this.Top_Right))
                    {
                        //Base on congruent triangles
                        result = 1 * OxValueRight / OxCD;
                    }
                }
            }

            return Math.Round(result, 4);
        }
        //edit-----
        public ConFS()
        {
            this.Bottom_Left = new Double() ;
            this.Bottom_Right=new Double();
            this.Name = string.Empty;
            this.Top_Left= new Double();
            this.Top_Right = new Double(); ;
        }
        public ConFS(String name,Double bottom_left,Double top_left,Double top_right,Double bottom_right)
        {
            this.Bottom_Left = bottom_left;
            this.Bottom_Right = bottom_right;
            this.Name = name;
            this.Top_Left = top_left;
            this.Top_Right = top_right ;
        }
        //----
    }

    public class DisFS 
    {
        public String Name { get; set; }
        public String V { get; set; }
        public String M { get; set; }
        public List<Double> ValueSet { get; set; }
        public List<Double> MembershipSet { get; set; }

        public Double GetMembershipAt(Double value)
        {
            Double result = 0;
            int i = 0;

            foreach (var item in this.ValueSet)
            {
                if (item == value)
                {
                    result = MembershipSet[i];
                    break;
                }
                else
                {
                    i++;
                }
            }

            return result;
        }

        public Boolean IsMember(Double value)
        {
            foreach (Double item in this.ValueSet)
            {
                if (value == item)
                {
                    return true;
                }
            }

            return false;
        }

        public Double GetMinValue()
        {
            Double result = Double.MaxValue;

            for (int i = 0; i < this.ValueSet.Count; i++)
            {
                if (ValueSet[i] <= result)
                    result = ValueSet[i];
            }

            return result;
        }

        public Double GetMaxValue()
        {
            Double result = Double.MinValue;

            for (int i = 0; i < this.ValueSet.Count; i++)
            {
                if (ValueSet[i] >= result)
                    result = ValueSet[i];
            }

            return result;
        }

        //edit----
       
        public DisFS ()
        {
            this.Name = String.Empty;
            this.V = String.Empty;
            this.M = String.Empty;
            this.ValueSet = new List<double>();
            this.MembershipSet = new List<double>();
        }
        public DisFS (Double uValue)
        {
            this.Name = String.Empty;
            this.V = String.Empty;
            this.M = String.Empty;
            this.ValueSet = new List<double>();
            this.ValueSet.Add(uValue);
            this.MembershipSet = new List<double>();
            this.MembershipSet.Add(1);
        }
        public DisFS (String name, String v, String m, List<Double> valueset, List<Double> membershipset)
        {
            this.Name = name;
            this.V = v;
            this.M = m;
            this.ValueSet = valueset;
            this.MembershipSet = membershipset;
        }
        //------
    }
}
