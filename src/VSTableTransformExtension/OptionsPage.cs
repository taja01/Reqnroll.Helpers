using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace VSTableTransformExtension
{
    internal class OptionsPage : DialogPage
    {
        [Category("Headers")]
        [DisplayName("First Column Header")]
        [Description("Header for the first column of the transformed table")]
        public string FirstHeader { get; set; } = "property";

        [Category("Headers")]
        [DisplayName("Second Column Header")]
        [Description("Header for the second column of the transformed table")]
        public string SecondHeader { get; set; } = "value";
    }
}
