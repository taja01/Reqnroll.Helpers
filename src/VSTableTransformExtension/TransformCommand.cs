using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;

namespace VSTableTransformExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class TransformCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("f470b6fc-0b1c-4a6f-84b3-0aa8a181ad0d");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private TransformCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static TransformCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in TransformCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new TransformCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Get the selected text:
            DTE dte = (DTE)Package.GetGlobalService(typeof(DTE));
            Document activeDocument = dte.ActiveDocument;
            TextDocument textDocument = (TextDocument)activeDocument.Object("");

            var startPoint = textDocument.Selection.TopPoint.CreateEditPoint();
            var endPoint = textDocument.Selection.BottomPoint.CreateEditPoint();
            var selectedText = startPoint.GetText(endPoint);

            // Write your logic here to transform the selected text:
            var lines = selectedText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (lines.Length == 2 && lines.First().Count(x => x == '|') > 2)
            {
                var page = (OptionsPage)package.GetDialogPage(typeof(OptionsPage));
                var firstHeader = page.FirstHeader;
                var secondHeader = page.SecondHeader;

                var properties = lines[0].Split('|');
                var values = lines[1].Split('|');

                for (var i = 0; i < properties.Length; i++)
                {
                    properties[i] = properties[i].Trim();
                    values[i] = values[i].Trim();
                }

                // Transform lines into a dictionary
                var dict = new Dictionary<string, string>();
                for (var i = 0; i < properties.Length; i++)
                {
                    dict[properties[i]] = values[i];
                }

                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"| {firstHeader} | {secondHeader} |");
                foreach (var pair in dict.OrderBy(x => x.Key))
                {
                    if (pair.Key.Length > 0)
                    {
                        stringBuilder.AppendLine($"| {pair.Key} | {pair.Value} |");
                    }
                }

                // Replace the selected text with the transformed one
                startPoint.ReplaceText(endPoint, stringBuilder.ToString(), (int)vsEPReplaceTextOptions.vsEPReplaceTextAutoformat);
            }
            else if (lines.Count() > 2 && lines.First().Count(c => c == '|') == 3)
            {
                var dict = new Dictionary<string, string>();
                foreach (var line in lines.Skip(1))
                {
                    var cells = line.Split('|');
                    if (cells.Length == 4)
                    {
                        dict[cells[1].Trim()] = cells[2].Trim();
                    }
                }

                var transformedText = $"|{string.Join("|", dict.Keys)}|\n|{string.Join("|", dict.Values)}|";

                // Replace the selected text with the transformed text
                startPoint.ReplaceText(endPoint, transformedText, (int)vsEPReplaceTextOptions.vsEPReplaceTextAutoformat);
            }
            else
            {
                // Show an error message
                var message = "Selected text doesn't match expected format.";
                var title = "Table Transform";
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    message,
                    title,
                    OLEMSGICON.OLEMSGICON_WARNING,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }
    }
}
