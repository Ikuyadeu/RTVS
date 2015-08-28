﻿using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Workspace;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Application.Host
{
    internal class EditorInstanceFactory
    {
        public static IEditorInstance CreateEditorInstance(IWorkspaceItem workspaceItem, ITextBuffer textBuffer, ICompositionService cs)
        {
            var importComposer = new ContentTypeImportComposer<IEditorFactory>(cs);
            var factory = importComposer.GetImport(textBuffer.ContentType.TypeName);

            var documentFactoryImportComposer = new ContentTypeImportComposer<IEditorDocumentFactory>(EditorShell.CompositionService);
            var documentFactory = documentFactoryImportComposer.GetImport(textBuffer.ContentType.TypeName);

            // Debug.Assert(factory != null, String.Format("No editor factory found for content type {0}", textBuffer.ContentType.TypeName));
            if(factory != null) // may be null if file type only support colorization, like VBScript
                return factory.CreateEditorInstance(workspaceItem, textBuffer, documentFactory);

            return null;
        }
    }
}