namespace PS.Model {
    public class Mail {
        public string Id { get; set; }
        public string UniqueId { get; set; }
        public bool New { get; set; }

        public Mail(string id, string uniqueId, bool newMessage) {
            Id = id;
            UniqueId = uniqueId;
            New = newMessage;
        }

        public override bool Equals(object obj) {
            // Check for null values and compare run-time types.
            if(obj == null || GetType() != obj.GetType())
                return false;

             Mail m = (Mail)obj;
            return Id.Equals(m.Id) && UniqueId.Equals(m.UniqueId);
        }

        public override int GetHashCode() {
            return 17 * Id.GetHashCode() + UniqueId.GetHashCode();
        }
    }
}