﻿using CommandLine;
using CommandLine.Text;

namespace Piranha {
    class PiranhaCommands {
        [VerbOption("remove-all-references", HelpText = "Remove all references from the assembly")]
        public RemoveAllReferencesCommand RemoveAllReferences { get; set; }

        [VerbOption("remove-all-resources", HelpText = "Remove all assembly resources")]
        public RemoveAllResourcesCommand RemoveAllResources { get; set; }

        [VerbOption("mark-all-references-retargetable", HelpText = "Marks all references as retargetable")]
        public MarkAllReferencesRetargetableCommand MarkAllReferencesRetargetable { get; set; }

        [VerbOption("set-target-framework", HelpText = "Set TargetFramework attribute of the assembly")]
        public SetTargetFrameworkCommand SetTargetFramework { get; set; }

        [VerbOption("list-used-types", HelpText = "List all types used by the assembly")]
        public ListUsedTypesCommand ListUsedTypes { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb) {
            return HelpText.AutoBuild(this, verb);
        }
    }
}
