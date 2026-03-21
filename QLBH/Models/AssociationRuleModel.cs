namespace MyWebApp.Models
{
    public class AssociationRuleModel
    {
        public List<string> Antecedent { get; set; } = new();
        public List<string> Consequent { get; set; } = new();
        public double Confidence { get; set; }
        public double Lift { get; set; }
        public bool? Synthetic { get; set; }
    }
}


