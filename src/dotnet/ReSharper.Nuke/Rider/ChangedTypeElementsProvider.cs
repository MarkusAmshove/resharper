// Copyright 2019 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/resharper/blob/master/LICENSE

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.changes;
using JetBrains.Application.Threading;
using JetBrains.DataFlow;
using JetBrains.Diagnostics;
using JetBrains.DocumentManagers;
using JetBrains.DocumentManagers.impl;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Threading;
using JetBrains.Util;

namespace ReSharper.Nuke.Rider
{
    [PsiComponent]
    internal class ChangedTypeElementsProvider
    {
        private static readonly TimeSpan s_updateInterval = TimeSpan.FromMilliseconds(value: 1000);

        private readonly Lifetime _myLifetime;
        private readonly IPsiServices _myServices;
        private readonly IShellLocks _myShellLocks;
        private readonly DocumentManager _myDocumentManager;
        private readonly ConcurrentDictionary<IProjectFile, TextRange> _myChangedRanges;
        private readonly GroupingEvent _myDocumentChangedEvent;

        public ChangedTypeElementsProvider(
            Lifetime lifetime,
            IShellLocks shellLocks,
            ChangeManager changeManager,
            DocumentManager documentManager,
            IPsiServices services)
        {
            _myServices = services;
            _myLifetime = lifetime;
            _myShellLocks = shellLocks;
            _myDocumentManager = documentManager;
            _myChangedRanges = new ConcurrentDictionary<IProjectFile, TextRange>();

            changeManager.Changed2.Advise(lifetime, OnChange);
            _myDocumentChangedEvent = shellLocks.CreateGroupingEvent(
                lifetime,
                $"{nameof(ChangedTypeElementsProvider)}::DocumentChanged",
                s_updateInterval,
                OnProcessChangesEx);

            TypeElementsChanged = new Signal<IReadOnlyCollection<ITypeElement>>(lifetime, nameof(ChangedTypeElementsProvider));
            _myLifetime.OnTermination(TypeElementsChanged);
        }

        public ISignal<IReadOnlyCollection<ITypeElement>> TypeElementsChanged { get; }

        private void OnChange(ChangeEventArgs e)
        {
            var change = e.ChangeMap.GetChange<ProjectFileDocumentChange>(_myDocumentManager.ChangeProvider);
            if (change == null)
                return;

            lock (_myChangedRanges)
            {
                var changeRange = new TextRange(change.StartOffset, change.StartOffset + change.OldLength);
                _myChangedRanges.AddOrUpdate(change.ProjectFile, changeRange, (file, range) => changeRange.Join(range));
            }

            _myDocumentChangedEvent.FireIncoming();
        }

        private void OnProcessChangesEx()
        {
            using (_myShellLocks.UsingReadLock())
            {
                _myServices.Files.CommitAllDocumentsAsync(
                    () =>
                    {
                        var the = new InterruptableReadActivityThe(_myLifetime, _myShellLocks, () => false) { FuncRun = Invalidate };
                        the.DoStart();
                    },
                    () => { });
            }
        }

        private void Invalidate()
        {
            var changes = GetChanges();

            try
            {
                var allChangedTypeElements = new List<ITypeElement>();

                foreach (var changedRange in changes)
                {
                    var sourceFile = changedRange.Key.ToSourceFile().NotNull("sourceFile != null");
                    var declaredElements = _myServices.Symbols.GetTypesAndNamespacesInFile(sourceFile);
                    var declarations = declaredElements.SelectMany(x => x.GetDeclarationsIn(sourceFile));
                    var changedTypeElements = declarations
                        .Where(x => changedRange.Value.ContainedIn(x.GetDocumentRange().TextRange))
                        .Select(x => x.DeclaredElement)
                        .OfType<ITypeElement>();

                    allChangedTypeElements.AddRange(changedTypeElements);
                }

                TypeElementsChanged.Fire(allChangedTypeElements);
            }
            catch (OperationCanceledException)
            {
                ReAddChanges(changes);
            }
        }

        private KeyValuePair<IProjectFile, TextRange>[] GetChanges()
        {
            // TODO: try-finally?
            KeyValuePair<IProjectFile, TextRange>[] changes;
            lock (_myChangedRanges)
            {
                changes = _myChangedRanges.Where(x => x.Key.IsValid() && x.Value.IsValid).ToArray();
                _myChangedRanges.Clear();
            }

            return changes;
        }

        private void ReAddChanges(KeyValuePair<IProjectFile, TextRange>[] changes)
        {
            lock (_myChangedRanges)
            {
                foreach (var pair in changes)
                    _myChangedRanges.AddOrUpdate(pair.Key, pair.Value, (file, range) => pair.Value.Join(range));
            }
        }
    }
}