using BadPractice.Cursoriam.Extension;
using BadPractice.Cursoriam.Inspector.Attributes;
using BadPractice.Cursoriam.Inspector.Editor.Logic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Drawer
{
    [LogicPriority(80)]
    internal sealed class ReferenceGuardDrawer : CursoriamInspectorDrawer<CursoriamInspectorField>
    {
        private readonly ReferenceGuardLogic _logic = new();

        protected override VisualElement CreateVisualElementInternal(CursoriamInspectorField element)
        {
            var isSuccess = _logic.InitializeContext(element);
            if (!isSuccess)
                return null;

            var panel = new VisualElement()
                .AsBoxPanel("Настройки Reference Guard");

            var titleLabel = panel.Q<Label>();
            titleLabel?.SetAlignSelf(Align.Center);

            // backup/restore
            var row1 = new VisualElement()
                .SetFlex(FlexDirection.Row)
                .SetJustifyContent(Justify.SpaceBetween)
                .SetMargin(bottom: 4f, right: 8f);

            var backupButton = new Button()
                .SetText("BACKUP")
                .SetHeight(22f)
                .SetWidth(new Length(50, LengthUnit.Percent))
                .SetFlexGrow(1f)
                .SetFontStyle(FontStyle.Bold)
                .SetMargin(right: 2f);
            row1.Add(backupButton);

            var restoreButton = new Button()
                .SetText("RESTORE")
                .SetHeight(22f)
                .SetWidth(new Length(50, LengthUnit.Percent))
                .SetFlexGrow(1f)
                .SetMargin(left: 2f)
                .SetFontStyle(FontStyle.Bold);
            row1.Add(restoreButton);

            panel.Add(row1);

            // toggle
            var row2 = new VisualElement()
                .SetFlex(FlexDirection.Row)
                .SetJustifyContent(Justify.SpaceBetween)
                .SetMargin(bottom: 4f, right: 8f);

            var autoBackupButton = new Button()
                .SetHeight(20f)
                .SetWidth(new Length(50, LengthUnit.Percent))
                .SetFlexGrow(1f)
                .SetMargin(right: 2f)
                .SetFontStyle(FontStyle.Bold);
            row2.Add(autoBackupButton);

            var autoRestoreButton = new Button()
                .SetHeight(20f)
                .SetWidth(new Length(50, LengthUnit.Percent))
                .SetFlexGrow(1f)
                .SetMargin(left: 2f)
                .SetFontStyle(FontStyle.Bold);
            row2.Add(autoRestoreButton);

            panel.Add(row2);

            void RefreshButtons()
            {
                restoreButton.SetEnabled(_logic.IsBackupExists);

                autoBackupButton
                    .SetText(_logic.IsAutoBackupActive ? "Авто: ВКЛЮЧЕНО" : "Авто: ВЫКЛЮЧЕНО")
                    .SetBackColor(_logic.IsAutoBackupActive ? COLOR_BTN_GREEN : COLOR_BTN_RED);

                autoRestoreButton
                    .SetText(_logic.IsAutoRestoreActive ? "Авто: ВКЛЮЧЕНО" : "Авто: ВЫКЛЮЧЕНО")
                    .SetBackColor(_logic.IsAutoRestoreActive ? COLOR_BTN_GREEN : COLOR_BTN_RED);
            }

            RefreshButtons();

            backupButton.SetAction(() =>
            {
                _logic.Backup();
                RefreshButtons();
            });

            restoreButton.SetAction(() =>
            {
                _logic.Restore();
                RefreshButtons();
            });

            autoBackupButton.SetAction(() =>
            {
                _logic.ToggleAutoBackup();
                _logic.Save();
                RefreshButtons();
            });

            autoRestoreButton.SetAction(() =>
            {
                _logic.ToggleAutoRestore();
                _logic.Save();
                RefreshButtons();
            });

            return panel;
        }

        protected override bool CanDrawInternal(CursoriamInspectorField element)
        {
            return base.CanDrawInternal(element) &&
                   element.Property != null &&
                   element.Member.IsDefined(typeof(ReferenceGuardAttribute), true) &&
                   !element.Member.IsDefined(typeof(SerializeReference), true);
        }

        public override bool IsDecorator => true;
    }
}