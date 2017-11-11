using System;
using System.IO;
using System.Net;
using System.Xml;
using System.ComponentModel;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace MunApp.Common
{
    public static class Data
    {
        static Data()
        {
            CurrentMacAddress = string.Empty;
            Countries = new ObservableList<Country>(GetDefaultCountries());
            Countries.ItemAdded += (sender, e) =>
            {
                Countries.Sort(
                    delegate (Country c1, Country c2)
                    {
                        if(c1 == null || c2 == null) return 0;
                        return c1.Name.CompareTo(c2.Name);
                    }
                );
            };
        }

        public static string CommitteeName { get; set; }
        public static bool FirstRun { get; set; }
        public static string CurrentMacAddress { get; set; }
        public static ObservableList<Country> Countries { get; set; }
        public static string GSLAgenda { get; set; }
        public static string SSLAgenda { get; set; }
        public static bool RollCallDone { get; set; }
        public static string ExtraData { get; set; }
        public static int EncryptionCount { get { return 2; } }

        public static ActivationStatus Activate(string serialkey)
        {
            return ActivationStatus.Success;
            try {
                var url = @"http://singular.site40.net/activator.php";
                var nvc = new System.Collections.Specialized.NameValueCollection();
                nvc.Add("app_name", "MunApp");
                nvc.Add("serial_key", serialkey.ToUpper().Replace(" ", ""));
                nvc.Add("mac_addr", GetMacAddress().ToUpper());
                var client = new WebClient();
                var data = client.UploadValues(url, nvc);
                var res = System.Text.Encoding.ASCII.GetString(data);
                if (res.StartsWith("true"))
                    return ActivationStatus.Success;
                else
                    return ActivationStatus.Failure;
            }
            catch (WebException)
            {
                return ActivationStatus.UnableToConnect;
            }
        }

        public static string GetMacAddress()
        {
            string macAddresses = string.Empty;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddresses += nic.GetPhysicalAddress().ToString();
                    break;
                }
            }

            return macAddresses;
        }

        public static void Save()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineChars = Environment.NewLine;
            settings.NewLineOnAttributes = true;
            using (XmlWriter writer = XmlWriter.Create("Data.dat", settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Data");

                writer.WriteAttributeString("FirstRun", FirstRun.ToString());
                writer.WriteAttributeString("MacAddress", GetMacAddress());
                writer.WriteAttributeString("CommitteeName", CommitteeName);
                writer.WriteAttributeString("GSLAgenda", GSLAgenda);
                writer.WriteAttributeString("SSLAgenda", SSLAgenda);
                writer.WriteAttributeString("RollCallDone", RollCallDone.ToString());
                writer.WriteAttributeString("ExtraData", ExtraData);

                foreach (Country country in Countries)
                {
                    writer.WriteStartElement("Country");

                    writer.WriteAttributeString("Name", country.Name);
                    writer.WriteAttributeString("Flag", country.Flag);
                    writer.WriteAttributeString("Attendance", country.Attendance.ToString());
                    writer.WriteAttributeString("GSL", country.GSL.ToString());
                    writer.WriteAttributeString("SSL", country.SSL.ToString());

                    writer.WriteEndElement();
                }

                writer.WriteStartElement("Topics");

                foreach(Country country in Countries)
                {
                    foreach(Topic topic in country.MC)
                    {
                        writer.WriteStartElement("Topic");

                        writer.WriteAttributeString("Heading", topic.Heading);
                        writer.WriteAttributeString("Passed", topic.Passed.ToString());
                        //TODO report / log this incident
                        if (topic.ProposedBy != country) throw new InvalidDataException("The proposed country is not the same as the parent country. This incident must be reported.");
                        writer.WriteAttributeString("ProposedBy", topic.ProposedBy.Name);

                        writer.WriteStartElement("SpeakingCountries");

                        foreach (Country speaker in topic.SpeakingCountries)
                        {
                            writer.WriteElementString("Country", speaker.Name);
                        }

                        writer.WriteEndElement();
                        
                        writer.WriteEndElement();
                    }
                }

                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Close();
            }
        }

        public static void Load()
        {
            //TODO MUST making the xml called data.dat and encryption
            if (File.Exists("Data.dat"))
            {
                using (XmlReader reader = XmlReader.Create("Data.dat"))
                {
                    Countries.Clear();
                    while (reader.Read())
                    {
                        if(reader.NodeType == XmlNodeType.Element)
                        {
                            if (reader.Name == "Data")
                            {
                                FirstRun = bool.Parse(reader.GetAttribute("FirstRun"));
                                CurrentMacAddress = reader.GetAttribute("MacAddress");
                                CommitteeName = reader.GetAttribute("CommitteeName");
                                GSLAgenda = reader.GetAttribute("GSLAgenda");
                                SSLAgenda = reader.GetAttribute("SSLAgenda");
                                RollCallDone = bool.Parse(reader.GetAttribute("RollCallDone"));
                                ExtraData = reader.GetAttribute("ExtraData");
                            }
                            else if(reader.Name == "Country")
                            {
                                Country country = new Country();
                                country.Name = reader.GetAttribute("Name");
                                country.Flag = reader.GetAttribute("Flag");
                                switch(reader.GetAttribute("Attendance"))
                                {
                                    case "PresentAndVoting":
                                        country.Attendance = Attendance.PresentAndVoting;
                                        break;
                                    case "Present":
                                        country.Attendance = Attendance.Present;
                                        break;
                                    default:
                                    case "Absent":
                                        country.Attendance = Attendance.Absent;
                                        break;
                                }
                                country.GSL = int.Parse(reader.GetAttribute("GSL"));
                                country.SSL = int.Parse(reader.GetAttribute("SSL"));
                                if (!Countries.Contains(country))
                                    Countries.Add(country);
                            }
                            else if(reader.Name == "Topics")
                            {
                                using (XmlReader topicsReader = reader.ReadSubtree())
                                {
                                    while (topicsReader.Read())
                                    {
                                        if (topicsReader.NodeType == XmlNodeType.Element && topicsReader.Name == "Topic")
                                        {
                                            Topic topic = new Topic();
                                            topic.Heading = topicsReader.GetAttribute("Heading");
                                            topic.Passed = bool.Parse(topicsReader.GetAttribute("Passed"));
                                            topic.ProposedBy = FindCountry(topicsReader.GetAttribute("ProposedBy"));
                                            using (XmlReader speakersReader = topicsReader.ReadSubtree())
                                            {
                                                while (speakersReader.Read())
                                                {
                                                    if (speakersReader.NodeType == XmlNodeType.Element && speakersReader.Name == "Country")
                                                    {
                                                        topic.SpeakingCountries.Add(FindCountry(speakersReader.ReadElementContentAsString()));
                                                    }
                                                }
                                            }
                                            if (!topic.ProposedBy.MC.Contains(topic))
                                                topic.ProposedBy.MC.Add(topic);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                FirstRun = true;
                CurrentMacAddress = string.Empty;
                Countries = GetDefaultCountries();
                CommitteeName = "Committee";
                GSLAgenda = "GSL Agenda";
                SSLAgenda = "SSL Agenda";
                RollCallDone = false;
                ExtraData = string.Empty;
            }
        }

        public static bool CountryExists(string name)
        {
            foreach (Country country in Data.Countries)
            {
                if (country.Name.ToLower() == name.ToLower())
                    return true;
            }
            return false;
        }

        public static Country FindCountry(string name)
        {
            foreach (Country country in Data.Countries)
            {
                if (country.Name.ToLower() == name.ToLower())
                    return country;
            }
            return null;
        }

        public static ObservableList<string> GetCountryNames()
        {
            ObservableList<string> names = new ObservableList<string>();
            foreach(Country c in Countries)
            {
                names.Add(c.Name);
            }
            return names;
        }

        public static ObservableList<Country> GetPresentCountries()
        {
            ObservableList<Country> names = new ObservableList<Country>();
            foreach(Country c in Countries)
            {
                if (c.Attendance == Attendance.Present || c.Attendance == Attendance.PresentAndVoting)
                    names.Add(c);
            }
            return names;
        }

        public static ObservableList<Country> GetDefaultCountries()
        {
            string[] names = new string[]
            {
                "India",
                "USA",
                "UK"
            };
            string[] flags = new string[]
            {
                ".\\Flags\\India.jpg",
                ".\\Flags\\USA.jpg",
                ".\\Flags\\UK.png"
            };
            ObservableList<Country> list = new ObservableList<Country>();
            for (int i = 0; i < names.Length; i++ )
            {
                Country country = new Country();
                string name = names[i];
                country.Name = name;
                country.Flag = flags[i];
                list.Add(country);
            }
            return list;
        }

        public static string Encrypt(string to, int type)
        {
            if (to == null) return "";
            string original = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-=!@#$%^&*()_+\\|[]{};:\'\",<.>/?`~ ";
            string encrypted = "";
            string jumbled;
            switch (type)
            {
                case 0:
                    jumbled = original;
                    break;
                case 1:
                    jumbled = "`1234567890-=~!@#$%^&*()_+ qwertyuiopasdfghjklzxcvbnm[]\\;\',./QWERTYUIOPASDFGHJKLZXCVBNM{}|:\"<>?";
                    break;
                case 2:
                    jumbled = "~!@#$%^&*()_+`1234567890-=QWERTYUIOP{}|qwertyuiop[] \\asdfghjkl;\'ASDFGHJKL:\"zxcvbnm,./ZXCVBNM<>?";
                    break;
                default:
                    return "";
            }
            for (int i = 0; i < to.Length; i++)
            {
                char achar = to.ToCharArray()[i];
                for (int j = 0; j < original.Length; j++)
                {
                    if (achar == original.ToCharArray()[j])
                    {
                        encrypted += jumbled.ToCharArray()[j];
                        break;
                    }
                }
            }
            return encrypted;
        }

        public static string Decrypt(string to, int type)
        {
            if (to == null) return "";
            string original;
            string jumbled = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-=!@#$%^&*()_+\\|[]{};:\'\",<.>/?`~ ";
            switch (type)
            {
                case 0:
                    original = jumbled;
                    break;
                case 1:
                    original = "`1234567890-=~!@#$%^&*()_+ qwertyuiopasdfghjklzxcvbnm[]\\;\',./QWERTYUIOPASDFGHJKLZXCVBNM{}|:\"<>?";
                    break;
                case 2:
                    original = "~!@#$%^&*()_+`1234567890-=QWERTYUIOP{}|qwertyuiop[] \\asdfghjkl;\'ASDFGHJKL:\"zxcvbnm,./ZXCVBNM<>?";
                    break;
                default:
                    return "";
            }
            string decrypted = "";
            for (int i = 0; i < to.Length; i++)
            {
                char achar = to.ToCharArray()[i];
                for (int j = 0; j < original.Length; j++)
                {
                    if (achar == original.ToCharArray()[j])
                    {
                        decrypted += jumbled.ToCharArray()[j];
                        break;
                    }
                }
            }
            return decrypted;
        }
    }

    public enum Attendance { Present, PresentAndVoting, Absent }
    public enum ActivationStatus { Success, Failure, UnableToConnect }

    public class Topic : INotifyPropertyChanged
    {
        private string heading;
        private Country proposedBy;
        private bool passed;
        private List<Country> speakingCountries = new List<Country>();

        public event PropertyChangedEventHandler PropertyChanged;

        public string Heading
        {
            get
            {
                return heading;
            }
            set
            {
                if (heading != value)
                {
                    heading = value;
                    OnPropertyChanged("Heading");
                }
            }
        }
        public Country ProposedBy
        {
            get
            {
                return proposedBy;
            }
            set
            {
                if (proposedBy != value)
                {
                    proposedBy = value;
                    OnPropertyChanged("ProposedBy");
                }
            }
        }

        public bool Passed
        {
            get
            {
                return passed;
            }
            set
            {
                if(passed != value)
                {
                    passed = value;
                    OnPropertyChanged("Pass");
                }
            }
        }

        public List<Country> SpeakingCountries
        {
            get
            {
                return speakingCountries;
            }
            set
            {
                if(speakingCountries != value)
                {
                    speakingCountries = value;
                    OnPropertyChanged("SpeakingCountries");
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Country : INotifyPropertyChanged
    {
        private Attendance attendance;
        private string name;
        private int gsl;
        private List<Topic> mc;
        private int ssl;
        private string flag;

        public Country()
        {
            attendance = Attendance.Absent;
            mc = new List<Topic>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if(name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        public string Flag
        {
            get
            {
                return flag;
            }
            set
            {
                if(flag != value)
                {
                    flag = value;
                    OnPropertyChanged("Flag");
                }
            }
        }
        public int GSL
        {
            get
            {
                return gsl;
            }
            set
            {
                if(gsl != value)
                {
                    gsl = value;
                    OnPropertyChanged("GSL");
                }
            }
        }
        public List<Topic> MC
        {
            get
            {
                return mc;
            }
            set
            {
                if(mc != value)
                {
                    mc = value;
                    OnPropertyChanged("MC");
                }
            }
        }
        public int SSL
        {
            get
            {
                return ssl;
            }
            set
            {
                if(ssl != value)
                {
                    ssl = value;
                    OnPropertyChanged("SSL");
                }
            }
        }
        public Attendance Attendance
        {
            get
            {
                return attendance;
            }
            set
            {
                if(attendance != value)
                {
                    attendance = value;
                    OnPropertyChanged("Attendance");
                }
            }
        }
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
