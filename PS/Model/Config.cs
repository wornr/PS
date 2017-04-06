using System.Xml.Serialization;

namespace PS.Model {
    [XmlRoot("config")]
    public class Config {
        [XmlElement("POP3")]
        public Pop3Config Incoming;

        [XmlElement("SMTP")]
        public SmtpConfig Outgoing;
    }

    public class MailConfig {
        [XmlElement("Host")]
        public string Host { get; set; }

        [XmlElement("Port")]
        public int Port { get; set; }

        [XmlElement("SSL")]
        public bool SSL { get; set; }

        [XmlElement("Username")]
        public string Username { get; set; }

        [XmlElement("Password")]
        public string Password { get; set; }
    }

    public class Pop3Config : MailConfig {
        [XmlElement("Interval")]
        public int Interval { get; set; }
    }

    public class SmtpConfig : MailConfig {
        [XmlElement("MailAddress")]
        public string MailAddress { get; set; }
    }
}