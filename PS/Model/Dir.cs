using System.Collections.Generic;

namespace PS.Model {
    public class Dir {
        public Type Type { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public string ModifiedDate { get; set; }
        

        public List<Dir> Children { get; set; }
    }
}