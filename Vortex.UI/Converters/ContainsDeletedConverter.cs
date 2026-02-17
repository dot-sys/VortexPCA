namespace Vortex.UI.Converters
{
    public class ContainsDeletedConverter : StringContainsConverterBase
    {
        protected override string SearchTerm => "Deleted";
    }
}
