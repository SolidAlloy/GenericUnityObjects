using System.Runtime.CompilerServices;
using GenericUnityObjects.Util;

[assembly: InternalsVisibleTo("GenericUnityObjects.EditorTests")]
[assembly: InternalsVisibleTo(Config.MenuItemsAssemblyName)]
[assembly: InternalsVisibleTo("Assembly-CSharp")] // TODO: remove after debugging