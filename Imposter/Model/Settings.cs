using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;

namespace Imposter.Model
{
    [DataContract]
    public class ImposterSettings
    {
        [DataMember(Name = "profiles")]
        public List<Profile> Profiles { get; set; }

        public static ImposterSettings Load()
        {
            try
            {
                if (File.Exists("settings.json"))
                {
                    var settingsJson = File.ReadAllText("settings.json");
                    var json = new DataContractJsonSerializer(typeof(ImposterSettings));
                    var stream = new MemoryStream(Encoding.UTF8.GetBytes(settingsJson));
                    return (Model.ImposterSettings)json.ReadObject(stream);
                }
                else
                {
                    try
                    {
                        File.WriteAllText("settings.json", "{ \"profiles\": [] }");
                        return Load();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("A problem was encountered while attempting to create the settings file. Detail: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A problem was encountered while attempting to load settings. Detail: " + ex.Message);
            }

            return null;
        }

        public void Save()
        {
            try
            {
                // Remove all 'blank' settings
                this.Profiles = this.Profiles.Where(p => p.Name != Profile.DefaultName).ToList();

                var serializer = new DataContractJsonSerializer(this.GetType());

                using (var ms = new MemoryStream())
                {
                    serializer.WriteObject(ms, this);
                    byte[] json = ms.ToArray();
                    File.WriteAllText("settings.json", Encoding.UTF8.GetString(json, 0, json.Length));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A problem was encountered while attempting to save settings. Detail: " + ex.Message);
            }
        }
    }
}
