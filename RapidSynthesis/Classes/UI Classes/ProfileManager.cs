using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// SAVE FILE NAME FORMAT:
// FIRST LINE: Name of DEFAULT profile (When any profie is selected, it is set as default, thus automatically loads next time app is run) (with DFLT: before)
// NEXT LINE: Name of PROFILE (with NAME: before)
// NEXT LINE: Profile ToString() Result (with PRFL: before)
// REPEAT

namespace RapidSynthesis
{
    static class ProfileManager
    {
        #region Properties and Consts
        private static Dictionary<string, Profile> Profiles { get; set; }
        private static string Directory { get; set; }
        public static string DefaultProfile { get; set; }

        private const string DEFAULT = "DFLT:";
        private const string PROFILE = "PRFL:";
        private const string SPLITTER = ":::";
        private const string FileName = @"\Profiles.txt";
        #endregion

        #region Constructors
        static ProfileManager()
        {
            Profiles = new Dictionary<string, Profile>();
            DefaultProfile = "";

            string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            Directory = Path.GetDirectoryName(path).Replace(@"file:\", "") + FileName;
        }
        #endregion

        #region Internal Store and Read Methods
        private static void SaveProfilesToSystem(string newDefault = "")
        {
            string content = DEFAULT;
            content += (String.IsNullOrEmpty(newDefault)) ? DefaultProfile + "\n" : newDefault += "\n";
            foreach (string name in Profiles.Keys)
            {
                content += PROFILE + name + SPLITTER;
                content += Profiles[name].ToString() + "\n";
            }
            WriteToFile(content);
        }

        private static void ReadProfilesFromSystem()
        {
            Profiles = new Dictionary<string, Profile>();
            DefaultProfile = "";

            var input = ReadFromFile();
            foreach (var line in input)
            {
                if (line.StartsWith(DEFAULT))
                {
                    var newLine = line.Replace(DEFAULT, "");
                    DefaultProfile = newLine;
                } 
                else if (line.StartsWith(PROFILE))
                {
                    var newLine = line.Replace(PROFILE, "");
                    var splits = newLine.Split(new string[] { ":::" }, StringSplitOptions.None);
                    var name = splits[0];
                    var profileString = splits[1];
                    var newProfile = new Profile(profileString);
                    Profiles.Add(name, newProfile);
                }
            }
        }
        #endregion

        #region Save Profile
        public static void SaveProfile(string name, Profile profile) 
        {
            VerifyLegalName(name);
            if (Profiles.ContainsKey(name))
                Profiles[name] = profile;
            else
                Profiles.Add(name, profile);
            DefaultProfile = name;
            SaveProfilesToSystem();
        }
        #endregion

        #region Load Profiles
        public static List<string> GetProfilesList()
        {
            ReadProfilesFromSystem();
            return Profiles.Keys.ToList();
        }

        public static Profile LoadProfile(string profileName)
        {
            if (Profiles.ContainsKey(profileName))
            {
                // Update default and save to system
                DefaultProfile = profileName;
                SaveProfilesToSystem();
                return Profiles[profileName];
            }
            return null;
        }
        #endregion

        #region Delete Profile From System
        public static void DeleteProfile(string profileName)
        {
            if (Profiles.ContainsKey(profileName))
            {
                Profiles.Remove(profileName);
                if (DefaultProfile == profileName)
                {
                    if (Profiles.Count > 0)
                        DefaultProfile = Profiles.Keys.First();
                    else
                        DefaultProfile = "";
                }
                SaveProfilesToSystem();
            }
        }
        #endregion

        #region File Save and Read Methods
        private static void WriteToFile(string content) 
        {
            File.WriteAllText(Directory, content);
        }

        private static List<string> ReadFromFile()
        {
            List<string> output;
            try
            {
                output = File.ReadLines(Directory).ToList();
            }
            catch (FileNotFoundException)
            {
                output = new List<string>();
            }
            return output;
        }
        #endregion

        #region Name Check
        private static void VerifyLegalName(string name)
        {
            // If any new limitations have to be added to the name, have them throw an InvalidProdileNameException
            if (name.Contains(SPLITTER))
                throw new InvalidProfileNameException();
        }

        public static bool VerifyDefaultProfile()
        {
            if (String.IsNullOrEmpty(DefaultProfile))
                return false;
            return Profiles.ContainsKey(DefaultProfile);
        }
        #endregion
    }
}
