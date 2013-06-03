using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Imposter.Model
{
    [DataContract]
    public class Profile
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "localDirectory")]
        public string LocalDirectory { get; set; }

        [DataMember(Name = "remoteUrl")]
        public string RemoteUrl { get; set; }

        [DataMember(Name = "decryptSsl")]
        public bool DecryptSsl { get; set; }

        [DataMember(Name = "port")]
        public int Port { get; set; }

        [DataMember(Name = "overrides")]
        public List<Override> Overrides { get; set; }

        [IgnoreDataMember]
        public static string DefaultName = " -- New Profile --";

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
